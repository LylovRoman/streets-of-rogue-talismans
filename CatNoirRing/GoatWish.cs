using HarmonyLib;
using RogueLibsCore;

namespace CatNoirRing
{
    static class GoatWish
    {
        internal static int WishAgentId;
        static bool wishPersonal;
        static int opening;
        static bool hiding;

        public static bool Open(Agent agent)
        {
            if (agent == null || agent.mainGUI == null)
                return false;

            GameController gc = agent.gc;
            if (gc != null && gc.loadLevel != null && gc.loadLevel.switchingLevel)
                return false;

            bool personal = gc != null && (gc.fourPlayerMode || gc.coopMode);
            opening++;
            try
            {
                if (personal)
                    agent.mainGUI.ShowScrollingMenuPersonal("FreeItems", agent);
                else
                    agent.mainGUI.ShowScrollingMenu("FreeItems", agent, agent);
            }
            finally
            {
                opening--;
            }

            WishAgentId = agent.GetInstanceID();
            wishPersonal = personal;
            return true;
        }

        internal static void Dismiss(Agent agent)
        {
            if (agent == null || WishAgentId != agent.GetInstanceID())
                return;

            Hide(agent);
            if (WishAgentId == agent.GetInstanceID())
                WishAgentId = 0;
        }

        static bool TryGive(Agent agent, Unlock? unlock)
        {
            if (unlock == null || string.IsNullOrEmpty(unlock.unlockName))
                return false;

            InvDatabase? inventory = agent.inventory;
            InvDatabase? pocket = agent.agentInvDatabase != null ? agent.agentInvDatabase : inventory;
            if (inventory == null || pocket == null)
                return false;

            DropIn(inventory, pocket, unlock.unlockName);
            if (unlock.unlockName == "BombMaker" && !pocket.HasItem("BombTrigger"))
                DropIn(inventory, pocket, "BombTrigger");
            return true;
        }

        static void DropIn(InvDatabase inventory, InvDatabase pocket, string name)
        {
            InvItem item = new InvItem();
            item.invItemName = name;
            item.SetupDetails(false);
            item.invItemCount = item.initCount;
            inventory.DontPlayPickupSounds(true);
            pocket.AddItemOrDrop(item);
            inventory.DontPlayPickupSounds(false);
        }

        static void Hide(Agent agent)
        {
            if (hiding || agent == null)
                return;

            MainGUI? gui = agent.mainGUI;
            if (gui == null && agent.gc != null)
                gui = agent.gc.mainGUI;
            if (gui == null)
                return;

            hiding = true;
            try
            {
                if (wishPersonal)
                    gui.HideScrollingMenuPersonal();
                else
                    gui.HideScrollingMenu();
            }
            finally
            {
                hiding = false;
            }
        }

        static void Play(Agent agent, string sound)
        {
            GameController gc = agent.gc;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(agent, sound);
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.PushedButton))]
        static class Choice
        {
            static bool Prefix(ScrollingMenu __instance, ButtonHelper myButton)
            {
                if (__instance == null || __instance.menuType != "FreeItems")
                    return true;

                Agent agent = __instance.agent;
                if (agent == null || agent.GetInstanceID() != WishAgentId)
                    return true;

                GoatEffect? form = GoatEffect.Find(agent);
                if (form == null || !form.Charge)
                {
                    Dismiss(agent);
                    return false;
                }

                Unlock? unlock = myButton != null ? myButton.scrollingButtonUnlock : null;
                if (!TryGive(agent, unlock))
                {
                    Play(agent, "CantDo");
                    return false;
                }

                form.Charge = false;
                form.BeginCountdown();
                GenesisAbility.Spend(agent);
                Play(agent, "UseItemTeleporter");
                Hide(agent);
                if (WishAgentId == agent.GetInstanceID())
                    WishAgentId = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.CloseScrollingMenu))]
        static class Closed
        {
            static void Postfix(ScrollingMenu __instance)
            {
                if (opening > 0 || __instance == null || __instance.menuType != "FreeItems")
                    return;

                Agent agent = __instance.agent;
                if (agent == null || agent.GetInstanceID() != WishAgentId)
                    return;

                WishAgentId = 0;
            }
        }
    }
}
