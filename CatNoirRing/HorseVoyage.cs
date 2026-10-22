using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class HorseVoyage
    {
        static readonly string[] Bypass =
        {
            "CantTeleportIndoors",
            "CantTeleportHatefulClose",
            "CantTeleportDangerousLevel",
            "CantTeleportDangerousLevel2",
            "CantTeleportLockdown",
            "CantTeleportMayor",
            "CantTeleportRescuing",
            "CantTeleportTrait",
            "CantTeleportCustom",
        };

        internal static int BypassAgentId;
        internal static int QuestDepth;

        public static void Open(Agent agent)
        {
            if (agent == null || agent.mainGUI == null)
                return;

            GameController gc = agent.gc;
            if (gc != null && gc.loadLevel != null && gc.loadLevel.switchingLevel)
            {
                Refuse(agent);
                return;
            }

            agent.mainGUI.ShowScrollingMenu("Floors", agent, agent);
        }

        internal static bool TryFloorLevel(GameController gc, string? type, out int level)
        {
            level = 0;
            if (gc == null || string.IsNullOrEmpty(type))
                return false;

            bool quick = gc.challenges != null && gc.challenges.Contains("QuickGame");
            switch (type)
            {
                case "Floor1":
                    level = 1;
                    return true;
                case "Floor2":
                    level = quick ? 3 : 4;
                    return true;
                case "Floor3":
                    level = quick ? 5 : 7;
                    return true;
                case "Floor4":
                    level = quick ? 7 : 10;
                    return true;
                case "Floor5":
                    level = quick ? 9 : 13;
                    return true;
                default:
                    return false;
            }
        }

        internal static void Depart(ScrollingMenu? menu, int level)
        {
            GameController? gc = menu != null ? menu.gc : null;
            Agent? agent = menu != null ? menu.agent : null;
            MainGUI? gui = agent != null ? agent.mainGUI : null;
            if (gui == null && gc != null)
                gui = gc.mainGUI;
            if (gui != null)
                gui.HideScrollingMenu();

            if (gc == null || gc.loadLevel == null || gc.sessionDataBig == null || gc.loadLevel.switchingLevel)
            {
                Refuse(agent);
                return;
            }

            if (gc.sessionDataBig.curLevel == level)
            {
                Refuse(agent);
                return;
            }

            gc.sessionDataBig.curLevel = level - 1;
            gc.sessionDataBig.curLevelEndless = level - 1;
            if (gc.audioHandler != null && agent != null)
                gc.audioHandler.Play(agent, "Teleport");
            gc.loadLevel.NextLevel();
        }

        static string SpendNote(Agent agent)
        {
            GameController gc = agent.gc != null ? agent.gc : GameController.gameController;
            string language = gc != null && gc.sessionDataBig != null ? gc.sessionDataBig.gameLanguage : "";
            if (language == "russian")
                return "Активная способность лошади позволит телепортироваться и будет израсходована. Костюм спадёт через 15 секунд.";

            return "The horse's ability allows this teleport and will be used. The suit then lasts 15 seconds.";
        }

        static bool Bypassed(string result)
        {
            if (string.IsNullOrEmpty(result))
                return false;

            for (int i = 0; i < Bypass.Length; i++)
            {
                if (Bypass[i] == result)
                    return true;
            }

            return false;
        }

        static void Refuse(Agent? agent)
        {
            GameController gc = agent != null ? agent.gc : GameController.gameController;
            if (gc != null && gc.audioHandler != null && agent != null)
                gc.audioHandler.Play(agent, "CantDo");
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.CanTeleport))]
        static class CanTeleportGate
        {
            static void Prefix(Agent __instance, ref bool sayDialogue, ref bool __state)
            {
                __state = sayDialogue;
                HorseEffect? form = HorseEffect.Find(__instance);
                if (form != null && form.Charge)
                    sayDialogue = false;
            }

            static void Postfix(Agent __instance, bool __state, ref string __result)
            {
                HorseEffect? form = HorseEffect.Find(__instance);
                if (form == null || !form.Charge)
                    return;

                if (Bypassed(__result))
                {
                    BypassAgentId = __instance.GetInstanceID();
                    __result = "Yes";
                    return;
                }

                if (BypassAgentId == __instance.GetInstanceID())
                    BypassAgentId = 0;

                if (!__state || string.IsNullOrEmpty(__result) || __result == "Yes")
                    return;

                __instance.SayDialogue(__result);
                if (__instance.gc != null && __instance.gc.audioHandler != null)
                    __instance.gc.audioHandler.Play(__instance, "CantDo");
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.TeleportToObjective))]
        static class ObjectiveDepth
        {
            static void Prefix()
            {
                QuestDepth++;
            }

            static void Postfix()
            {
                if (QuestDepth > 0)
                    QuestDepth--;
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.TeleportToObject))]
        static class ObjectDepth
        {
            static void Prefix()
            {
                QuestDepth++;
            }

            static void Postfix()
            {
                if (QuestDepth > 0)
                    QuestDepth--;
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.Teleport), typeof(Vector3), typeof(bool), typeof(bool))]
        static class SpendCharge
        {
            static void Prefix(Agent __instance, ref bool __state)
            {
                __state = __instance.finishedLevel
                    || __instance.finishedLevelImmediate
                    || __instance.finishedLevelImmediateMult
                    || __instance.teleporting;
            }

            static void Postfix(Agent __instance, bool __state)
            {
                if (__state || QuestDepth <= 0 || BypassAgentId != __instance.GetInstanceID())
                    return;

                BypassAgentId = 0;
                HorseEffect? form = HorseEffect.Find(__instance);
                if (form == null || !form.Charge)
                    return;

                form.Charge = false;
                form.BeginCountdown();
            }
        }

        [HarmonyPatch(typeof(QuestSheet), nameof(QuestSheet.SetupInstructionText))]
        static class TeleportCaption
        {
            static void Postfix(QuestSheet __instance)
            {
                Agent agent = __instance.openedByPlayer;
                if (agent == null || BypassAgentId != agent.GetInstanceID())
                    return;

                HorseEffect? form = HorseEffect.Find(agent);
                if (form == null || !form.Charge || __instance.instructionText1 == null)
                    return;

                string note = SpendNote(agent);
                string current = __instance.instructionText1.text ?? "";
                if (current.Contains(note))
                    return;

                __instance.instructionText1.text = string.IsNullOrEmpty(current) ? note : current + "\n" + note;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.PushedButton))]
        static class FloorChoice
        {
            static bool Prefix(ScrollingMenu __instance, ButtonHelper myButton)
            {
                if (__instance == null || __instance.menuType != "Floors")
                    return true;

                HorseEffect? form = HorseEffect.Find(__instance.agent);
                if (form == null || !form.Charge)
                    return true;

                string? type = myButton != null ? myButton.scrollingButtonType : null;
                if (!TryFloorLevel(__instance.gc, type, out int level))
                    return true;

                Depart(__instance, level);
                return false;
            }
        }
    }
}
