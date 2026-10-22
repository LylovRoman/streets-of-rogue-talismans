using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class RoosterWish
    {
        const string Menu = "Achievements";

        static readonly string[] Blocked =
        {
            "Werewolf",
            "WerewolfEffect",
            "Zombify",
            "ZombieSpirit",
            "EveryoneHatesZombie",
            "Gorilla",
            "Vampire",
            "Ghost",
            "Cop2",
        };

        internal static int WishAgentId;
        static int tookFrame = -1;
        static int tookButton;
        static int openingAgentId;
        static bool wishPersonal;
        static int opening;
        static bool hiding;
        static readonly List<Unlock> offered = new List<Unlock>();

        public static bool Open(Agent agent)
        {
            if (agent == null || agent.mainGUI == null || agent.statusEffects == null)
                return false;

            GameController gc = agent.gc;
            if (gc == null || gc.sessionDataBig == null || gc.sessionDataBig.traitUnlocksCharacterCreation == null)
                return false;
            if (gc.loadLevel != null && gc.loadLevel.switchingLevel)
                return false;

            if (WishAgentId == agent.GetInstanceID() && !Shown(agent))
                Abandon(agent);

            offered.Clear();
            Collect(agent);
            if (offered.Count == 0)
                return false;

            wishPersonal = gc.fourPlayerMode || gc.coopMode;
            openingAgentId = agent.GetInstanceID();
            WishAgentId = 0;
            ReadLevel(agent, out int skill, out int session);
            opening++;
            bool opened = false;
            try
            {
                if (wishPersonal)
                    agent.mainGUI.ShowScrollingMenuPersonal(Menu, agent);
                else
                    agent.mainGUI.ShowScrollingMenu(Menu, agent, agent);
                opened = Shown(agent);
            }
            catch (System.Exception exception)
            {
                opened = false;
                Debug.LogWarning("Сублимация не открыла список черт.");
                Debug.LogWarning(exception);
            }
            finally
            {
                opening--;
                openingAgentId = 0;
                WriteLevel(agent, skill, session);
                if (opened)
                    WishAgentId = agent.GetInstanceID();
                else
                    Abandon(agent);
            }

            return opened && WishAgentId == agent.GetInstanceID();
        }

        internal static void Dismiss(Agent agent)
        {
            if (agent == null || WishAgentId != agent.GetInstanceID())
                return;

            Abandon(agent);
        }

        static void Collect(Agent agent)
        {
            List<Unlock> source = agent.gc.sessionDataBig.traitUnlocksCharacterCreation;
            for (int i = 0; i < source.Count; i++)
            {
                Unlock unlock = source[i];
                if (Allowed(agent, unlock))
                    offered.Add(unlock);
            }
        }

        static bool Allowed(Agent agent, Unlock unlock)
        {
            if (unlock == null || unlock.removal || unlock.isUpgrade || unlock.cost3 <= 0)
                return false;
            if (string.IsNullOrEmpty(unlock.unlockName) || unlock.unlockType != "Trait")
                return false;
            if (BlockedName(unlock.unlockName))
                return false;

            StatusEffects effects = agent.statusEffects;
            return effects != null && !effects.hasTrait(unlock.unlockName);
        }

        static bool BlockedName(string name)
        {
            for (int i = 0; i < Blocked.Length; i++)
            {
                if (Blocked[i] == name)
                    return true;
            }

            return false;
        }

        static void Fill(ScrollingMenu menu)
        {
            if (menu.buttonsData == null || menu.smallTraitList == null || offered.Count == 0)
                return;

            ButtonData? template = menu.buttonsData.Count > 0 ? menu.buttonsData[0] : null;
            menu.smallTraitList.Clear();
            menu.buttonsData.Clear();
            for (int i = 0; i < offered.Count; i++)
            {
                Unlock unlock = offered[i];
                menu.smallTraitList.Add(unlock);
                ButtonData data = new ButtonData();
                data.isActive = true;
                data.scrollingMenu = menu;
                data.agent = menu.agent;
                data.scrollingButtonNum = i;
                data.scrollingButtonUnlock = unlock;
                data.scrollingButtonType = unlock.unlockName;
                data.scrollingButtonAgentName = unlock.unlockName;
                data.scrollingButtonConfigName = "";
                data.scrollingButtonLevelFeeling = "";
                data.buttonText = Label(menu, unlock.unlockName);
                data.interactable = true;
                data.resizeTextForBestFit = true;
                data.canvasGroupAlpha = 1;
                data.highlightedSprite = menu.solidObjectButton;
                if (template != null)
                {
                    data.mainGUI = template.mainGUI;
                    data.worldSpaceGUI = template.worldSpaceGUI;
                    data.canvas = template.canvas;
                    data.hasCanvas = template.hasCanvas;
                    data.menuType = template.menuType;
                }
                else if (menu.agent != null)
                {
                    data.mainGUI = menu.agent.mainGUI;
                    data.worldSpaceGUI = menu.agent.worldSpaceGUI;
                    data.menuType = "ScrollingMenu";
                }

                menu.buttonsData.Add(data);
            }

            menu.numButtons = offered.Count;
            if (menu.yOffset > 0f)
                menu.totalSize = menu.numButtons * menu.yOffset;
            if (menu.content != null)
            {
                RectTransform rect = menu.content.GetComponent<RectTransform>();
                if (rect != null)
                    rect.sizeDelta = new Vector2(rect.rect.width, menu.totalSize);
            }

            menu.timeOnMenu = 1f;
            menu.canPressButtons = true;
            menu.refreshing = true;
            if (menu.scrollerController != null)
            {
                menu.scrollerController.menuType = "ScrollingMenu";
                menu.scrollerController.scrollingMenu = menu;
                menu.scrollerController.RefreshMenuData(true);
            }
            if (menu.scrollBar != null)
                menu.scrollBar.value = 1f;
            ShowPane(menu);
        }

        static string Label(ScrollingMenu menu, string trait)
        {
            NameDB? names = menu.gc != null ? menu.gc.nameDB : null;
            string label = names != null ? names.GetName(trait, "StatusEffect") : "";
            if (string.IsNullOrEmpty(label) || label.StartsWith("E_"))
                label = names != null ? names.GetName(trait, "Trait") : "";
            if (string.IsNullOrEmpty(label) || label.StartsWith("E_"))
                return trait;
            return label;
        }

        static bool Opening(ScrollingMenu menu)
        {
            return opening > 0
                && menu != null
                && menu.menuType == Menu
                && menu.agent != null
                && menu.agent.GetInstanceID() == openingAgentId;
        }

        static string Picked(ButtonHelper? button)
        {
            if (button == null)
                return "";

            int index = button.scrollingButtonNum;
            if (index >= 0 && index < offered.Count)
            {
                Unlock row = offered[index];
                if (row != null && !string.IsNullOrEmpty(row.unlockName))
                    return row.unlockName;
            }

            Unlock? unlock = button.scrollingButtonUnlock;
            if (unlock != null && !string.IsNullOrEmpty(unlock.unlockName))
                return unlock.unlockName;
            return button.scrollingButtonType ?? "";
        }

        static bool Ours(ScrollingMenu menu)
        {
            return menu != null
                && menu.menuType == Menu
                && menu.agent != null
                && WishAgentId != 0
                && menu.agent.GetInstanceID() == WishAgentId;
        }

        static MainGUI? Gui(Agent agent)
        {
            if (agent == null)
                return null;
            if (agent.mainGUI != null)
                return agent.mainGUI;
            return agent.gc != null ? agent.gc.mainGUI : null;
        }

        static bool Shown(Agent agent)
        {
            MainGUI? gui = Gui(agent);
            if (gui == null || agent == null)
                return false;

            bool open = wishPersonal ? gui.openedScrollingMenuPersonal : gui.openedScrollingMenu;
            if (!open && agent.gc != null && agent.gc.mainGUI != null)
                open = wishPersonal ? agent.gc.mainGUI.openedScrollingMenuPersonal : agent.gc.mainGUI.openedScrollingMenu;
            if (!open)
                return false;

            ScrollingMenu menu = wishPersonal ? gui.scrollingMenuPersonalScript : gui.scrollingMenuScript;
            if (menu == null || menu.menuType != Menu || menu.agent == null)
                return false;
            if (menu.agent.GetInstanceID() != agent.GetInstanceID())
                return false;

            GameObject root = wishPersonal ? gui.scrollingMenuPersonal : gui.scrollingMenu;
            if (root != null)
            {
                Canvas canvas = root.GetComponent<Canvas>();
                if (canvas != null && !canvas.enabled)
                    return false;
            }

            return menu.buttonsData != null && menu.buttonsData.Count > 0;
        }

        static void Abandon(Agent? agent)
        {
            if (agent == null)
            {
                WishAgentId = 0;
                return;
            }

            MainGUI? gui = Gui(agent);
            bool open = gui != null && (wishPersonal ? gui.openedScrollingMenuPersonal : gui.openedScrollingMenu);
            if (open && gui != null)
            {
                if (wishPersonal)
                    gui.justClosedScrollingMenuPersonal = false;
                else
                    gui.justClosedScrollingMenu = false;
                try
                {
                    Hide(agent);
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning("Сублимация не закрыла список черт.");
                    Debug.LogWarning(exception);
                }
            }

            ClearFlags(agent);
            if (WishAgentId == agent.GetInstanceID())
                WishAgentId = 0;
        }

        static void ClearFlags(Agent agent)
        {
            MainGUI? gui = Gui(agent);
            ScrollingMenu? menu = gui != null
                ? (wishPersonal ? gui.scrollingMenuPersonalScript : gui.scrollingMenuScript)
                : null;
            if (menu != null)
                menu.canPressButtons = true;

            if (gui != null)
            {
                gui.openedScrollingMenu = false;
                gui.openedScrollingMenuPersonal = false;
                GameObject root = wishPersonal ? gui.scrollingMenuPersonal : gui.scrollingMenu;
                if (root != null)
                {
                    try
                    {
                        gui.SetInterfaceActive(wishPersonal ? "ScrollingMenuPersonal" : "ScrollingMenu", false);
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            GameController gc = agent.gc;
            if (gc == null)
                return;
            if (gc.mainGUI != null)
                gc.mainGUI.openedScrollingMenu = false;
            if (gc.playerControl != null)
                gc.playerControl.SetCantPressGameplayButtons("All", 0, 0);
        }

        static void ReadLevel(Agent agent, out int skill, out int session)
        {
            skill = agent.skillPoints != null ? agent.skillPoints.levelsGained : 0;
            session = 0;
            GameController gc = agent.gc;
            int slot = agent.isPlayer;
            if (gc == null || gc.sessionData == null || gc.sessionData.levelsGained == null)
                return;
            if (slot < 0 || slot >= gc.sessionData.levelsGained.Length)
                return;
            session = gc.sessionData.levelsGained[slot];
        }

        static void WriteLevel(Agent agent, int skill, int session)
        {
            if (agent.skillPoints != null)
                agent.skillPoints.levelsGained = skill;

            GameController gc = agent.gc;
            int slot = agent.isPlayer;
            if (gc == null || gc.sessionData == null || gc.sessionData.levelsGained == null)
                return;
            if (slot < 0 || slot >= gc.sessionData.levelsGained.Length)
                return;
            gc.sessionData.levelsGained[slot] = session;
        }

        static void Hide(Agent agent)
        {
            if (hiding || agent == null)
                return;

            MainGUI? gui = Gui(agent);
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

        internal static bool Take(ButtonHelper button)
        {
            if (button == null || WishAgentId == 0)
                return false;

            int id = button.GetInstanceID();
            if (tookFrame == Time.frameCount && tookButton == id)
                return true;

            Agent agent = button.agent;
            ScrollingMenu menu = button.scrollingMenu;
            if (agent == null && menu != null)
                agent = menu.agent;
            if (agent == null || agent.GetInstanceID() != WishAgentId || !Shown(agent))
                return false;

            int index = button.dataIndex;
            if (index < 0 || index >= offered.Count)
                index = button.scrollingButtonNum;
            if (index < 0 || index >= offered.Count || offered[index] == null)
                return false;

            tookFrame = Time.frameCount;
            tookButton = id;
            if (menu != null)
            {
                menu.timeOnMenu = 1f;
                menu.canPressButtons = true;
            }

            RoosterEffect? form = RoosterEffect.Find(agent);
            if (form == null || !form.Charge || form.Granted)
            {
                Abandon(agent);
                return true;
            }

            string name = offered[index].unlockName ?? "";
            if (string.IsNullOrEmpty(name) || !form.TryGrant(name))
            {
                Play(agent, "CantDo");
                return true;
            }

            form.Charge = false;
            form.BeginCountdown();
            SublimationAbility.Spend(agent);
            Play(agent, "ClickButton");
            Abandon(agent);
            return true;
        }

        [HarmonyPatch(typeof(ButtonHelper), nameof(ButtonHelper.PressedScrollingMenuButton))]
        static class ClickGate
        {
            static bool Prefix(ButtonHelper __instance)
            {
                if (__instance == null)
                    return true;
                if (Take(__instance) || PeacockWish.Take(__instance))
                    return false;

                ScrollingMenu? menu = __instance.scrollingMenu;
                if (menu == null || !Ours(menu))
                    return true;

                menu.timeOnMenu = 1f;
                menu.canPressButtons = true;
                return true;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), "FindTrait")]
        [HarmonyPriority(Priority.First)]
        static class KeepLevel
        {
            static bool Prefix(ScrollingMenu __instance, ref Unlock __result)
            {
                if (!Opening(__instance) || offered.Count == 0)
                    return true;

                __result = offered[0];
                return false;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.OpenScrollingMenu))]
        [HarmonyPriority(Priority.Last)]
        static class Opened
        {
            static void Postfix(ScrollingMenu __instance)
            {
                if (!Opening(__instance))
                    return;

                Fill(__instance);
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.PushedButton))]
        [HarmonyPriority(Priority.First)]
        static class Choice
        {
            static bool Prefix(ScrollingMenu __instance, ButtonHelper myButton)
            {
                if (!Ours(__instance))
                    return true;

                __instance.canPressButtons = true;
                Agent agent = __instance.agent;
                RoosterEffect? form = RoosterEffect.Find(agent);
                if (form == null || !form.Charge || form.Granted)
                {
                    Abandon(agent);
                    return false;
                }

                string name = Picked(myButton);
                if (string.IsNullOrEmpty(name))
                    return true;
                if (!form.TryGrant(name))
                {
                    __instance.canPressButtons = true;
                    Play(agent, "CantDo");
                    return false;
                }

                form.Charge = false;
                form.BeginCountdown();
                SublimationAbility.Spend(agent);
                Play(agent, "ClickButton");
                Abandon(agent);
                return false;
            }
        }

        static string RowName(ButtonHelper? button)
        {
            if (button == null)
                return "";

            Unlock? unlock = button.scrollingButtonUnlock;
            if (unlock != null && !string.IsNullOrEmpty(unlock.unlockName))
                return unlock.unlockName;

            int index = button.dataIndex;
            if (index < 0 || index >= offered.Count)
                index = button.scrollingButtonNum;
            if (index >= 0 && index < offered.Count && offered[index] != null && !string.IsNullOrEmpty(offered[index].unlockName))
                return offered[index].unlockName;
            return button.scrollingButtonType ?? "";
        }

        static void ShowPane(ScrollingMenu menu)
        {
            BindText(menu);
            if (menu.scrollViewDetails != null)
                menu.scrollViewDetails.gameObject.SetActive(true);
            if (menu.details != null)
                menu.details.SetActive(true);
            if (menu.detailsTitle != null)
                menu.detailsTitle.gameObject.SetActive(true);
            if (menu.detailsText != null)
                menu.detailsText.gameObject.SetActive(true);
            if (menu.scrollBarDetails != null)
                menu.scrollBarDetails.gameObject.SetActive(true);
        }

        static void BindText(ScrollingMenu menu)
        {
            if (menu.detailsTitle != null && menu.detailsText != null)
                return;

            Transform[] nodes = menu.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < nodes.Length; i++)
            {
                Transform node = nodes[i];
                if (node == null)
                    continue;
                if (menu.detailsTitle == null && node.name == "DetailsTitle")
                    menu.detailsTitle = node.GetComponent<UnityEngine.UI.Text>();
                if (menu.detailsText == null && node.name == "DetailsText")
                    menu.detailsText = node.GetComponent<UnityEngine.UI.Text>();
            }
        }

        static bool Describe(ScrollingMenu menu, ButtonHelper button)
        {
            string trait = RowName(button);
            if (string.IsNullOrEmpty(trait) || menu.detailsTitle == null || menu.detailsText == null)
                return false;

            ShowPane(menu);
            NameDB? names = menu.gc != null ? menu.gc.nameDB : null;
            string title = names != null ? names.GetName(trait, "StatusEffect") : trait;
            if (string.IsNullOrEmpty(title) || title.StartsWith("E_"))
                title = names != null ? names.GetName(trait, "Trait") : trait;
            string body = names != null ? names.GetName(trait, "Description") : "";
            if (string.IsNullOrEmpty(title) || title.StartsWith("E_"))
                title = trait;
            menu.detailsTitle.text = title;
            menu.detailsText.text = string.IsNullOrEmpty(body) || body.StartsWith("E_") ? trait : body;
            return true;
        }

        static void RefreshDetails(ScrollingMenu menu)
        {
            ButtonHelper? cell = null;
            if (menu.agent != null && menu.agent.mainGUI != null && menu.agent.mainGUI.curSelected != null)
            {
                UnityEngine.UI.Selectable selected = menu.agent.mainGUI.curSelected;
                cell = selected.GetComponent<ButtonHelper>();
                if (cell == null)
                    cell = selected.GetComponentInParent<ButtonHelper>();
            }

            if (cell == null)
            {
                ButtonHelper[] cells = menu.GetComponentsInChildren<ButtonHelper>(false);
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i] != null && cells[i].scrollingHighlighted)
                    {
                        cell = cells[i];
                        break;
                    }
                }
            }

            if (cell != null)
                Describe(menu, cell);
        }

        [HarmonyPatch(typeof(ButtonHelper), "OnPointerEnter")]
        static class Hover
        {
            static void Postfix(ButtonHelper __instance)
            {
                if (__instance == null || __instance.scrollingMenu == null || !Ours(__instance.scrollingMenu))
                    return;

                Describe(__instance.scrollingMenu, __instance);
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.ShowDetails))]
        [HarmonyPriority(Priority.First)]
        static class Details
        {
            static bool Prefix(ScrollingMenu __instance, ButtonHelper myButton)
            {
                if (!Ours(__instance) || myButton == null)
                    return true;

                if (!Describe(__instance, myButton))
                    return true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), nameof(ScrollingMenu.CloseScrollingMenu))]
        static class Closed
        {
            static void Postfix(ScrollingMenu __instance)
            {
                if (opening > 0 || hiding || !Ours(__instance))
                    return;

                Agent agent = __instance.agent;
                WishAgentId = 0;
                if (agent != null)
                    ClearFlags(agent);
                else
                    __instance.canPressButtons = true;
            }
        }

        [HarmonyPatch(typeof(ScrollingMenu), "Update")]
        static class Cancel
        {
            static void Postfix(ScrollingMenu __instance)
            {
                if (opening > 0 || hiding || !Ours(__instance))
                    return;

                Agent agent = __instance.agent;
                if (agent == null || !Shown(agent))
                {
                    Abandon(agent);
                    return;
                }

                __instance.canPressButtons = true;
                RefreshDetails(__instance);
                GameController gc = agent.gc;
                if (gc == null || gc.playerControl == null)
                    return;
                if (!gc.playerControl.pressButtonDown("UICancel", agent))
                    return;

                Abandon(agent);
            }
        }
    }
}
