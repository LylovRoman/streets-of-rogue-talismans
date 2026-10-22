using System;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class ButterflyAim
    {
        const int ReachTiles = 15;

        static InvItem? token;
        static int closedFrame = -1;

        public static bool Press(Agent agent)
        {
            if (agent == null)
                return false;
            if (closedFrame == Time.frameCount)
                return true;
            if (IsOpen(agent))
            {
                Close(agent, true);
                return true;
            }

            return Open(agent);
        }

        public static void Dismiss(Agent? agent)
        {
            if (agent == null || !IsOpen(agent))
                return;

            Close(agent, false);
        }

        static bool Open(Agent agent)
        {
            if (agent.isPlayer <= 0 || agent.mainGUI == null || agent.mainGUI.invInterface == null)
                return false;

            InvInterface inv = agent.mainGUI.invInterface;
            InvItem item = Token(agent);
            try
            {
                inv.ShowTarget(item);
            }
            catch (Exception)
            {
                try
                {
                    if (inv.showingTarget)
                        inv.HideTarget();
                }
                catch (Exception)
                {
                }

                return false;
            }

            if (!inv.showingTarget || agent.mainGUI.targetItem != item)
                return false;

            if (inv.cursorTextString3 != null)
                inv.cursorTextString3.text = Label(agent);
            return true;
        }

        static void Close(Agent agent, bool sound)
        {
            if (agent.mainGUI != null && agent.mainGUI.invInterface != null && agent.mainGUI.showingTarget)
                agent.mainGUI.invInterface.HideTarget();
            if (sound)
                Play(agent, "BeginCombine");
        }

        static bool Consider(InvItem item, PlayfieldObject other, string combineType)
        {
            Agent? caster = item != null ? item.agent : null;
            if (caster == null)
                return false;

            Agent? victim = other as Agent;
            if (victim == null || !Accept(caster, victim))
                return false;
            if (combineType != "Combine")
                return true;

            return Confirm(caster, victim);
        }

        static bool Confirm(Agent caster, Agent victim)
        {
            ButterflyEffect? form = ButterflyEffect.Find(caster);
            if (form == null || !form.Charge)
                return false;
            if (!Akuma.Turn(victim))
                return false;

            form.Charge = false;
            form.Note(victim);
            form.BeginCountdown();
            Play(caster, "Spawn");
            if (caster.mainGUI != null && caster.mainGUI.invInterface != null && IsOpen(caster))
                caster.mainGUI.invInterface.HideTarget();
            return true;
        }

        static bool Accept(Agent caster, Agent victim)
        {
            if (victim == null || victim == caster || caster.tr == null || victim.tr == null)
                return false;
            if (!Akuma.CanTarget(victim))
                return false;

            return Vector2.Distance(caster.tr.position, victim.tr.position) <= Reach();
        }

        static float Reach()
        {
            float tile = Consts.tileSize > 0.01f ? Consts.tileSize : 0.64f;
            return tile * ReachTiles;
        }

        static InvItem Token(Agent agent)
        {
            if (token == null)
                token = new InvItem();

            token.invItemName = ButterflyAbility.Id;
            token.itemType = "Tool";
            token.agent = agent;
            token.invInterface = agent.mainGUI.invInterface;
            return token;
        }

        static bool IsOpen(Agent agent)
        {
            return token != null
                && agent.mainGUI != null
                && agent.mainGUI.showingTarget
                && agent.mainGUI.targetItem == token;
        }

        static bool Owns(InvItem item)
        {
            return token != null && ReferenceEquals(item, token);
        }

        static void NoteClosed(InvInterface inv)
        {
            if (inv == null || !inv.showingTarget || token == null)
                return;

            MainGUI gui = inv.mainGUI;
            if (gui != null && gui.targetItem == token)
                closedFrame = Time.frameCount;
        }

        static void Play(Agent agent, string name)
        {
            GameController gc = agent.gc != null ? agent.gc : GameController.gameController;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(agent, name);
        }

        static string Label(Agent agent)
        {
            GameController gc = agent.gc != null ? agent.gc : GameController.gameController;
            string language = gc != null && gc.sessionDataBig != null ? gc.sessionDataBig.gameLanguage : "";
            if (language == "russian")
                return "Акума";

            return "Akuma";
        }

        [HarmonyPatch(typeof(InvItem), nameof(InvItem.TargetObject), typeof(PlayfieldObject), typeof(string))]
        static class ItemTarget
        {
            [HarmonyPriority(Priority.First + 100)]
            static bool Prefix(InvItem __instance, PlayfieldObject otherObject, string combineType, ref bool __result)
            {
                if (!Owns(__instance))
                    return true;

                __result = Consider(__instance, otherObject, combineType);
                return false;
            }
        }

        [HarmonyPatch(typeof(InvInterface), nameof(InvInterface.HideTarget))]
        static class HideWatch
        {
            static void Prefix(InvInterface __instance)
            {
                NoteClosed(__instance);
            }
        }
    }
}
