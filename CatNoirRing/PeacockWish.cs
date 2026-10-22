using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class PeacockWish
    {
        const string Menu = "Achievements";

        static readonly string[] Offer =
        {
            "Alien",
            "Assassin",
            "Athlete",
            "Bartender",
            "Bouncer",
            "Businessman",
            "ButlerBot",
            "Cannibal",
            "Clerk",
            "Comedian",
            "Cop",
            "Cop2",
            "CopBot",
            "Courier",
            "Demolitionist",
            "Doctor",
            "DrugDealer",
            "Firefighter",
            "Gangbanger",
            "GangbangerB",
            "Ghost",
            "Gorilla",
            "Guard",
            "Guard2",
            "Hacker",
            "Hobo",
            "Mafia",
            "Mayor",
            "MechEmpty",
            "MechFilled",
            "MechPilot",
            "Musician",
            "OfficeDrone",
            "ResistanceLeader",
            "Robot",
            "RobotPlayer",
            "Scientist",
            "ShapeShifter",
            "Shopkeeper",
            "Slave",
            "Slavemaster",
            "Soldier",
            "Thief",
            "UpperCruster",
            "Vampire",
            "Werewolf",
            "WerewolfB",
            "Worker",
            "Wrestler",
            "Zombie",
        };

        internal static int WishAgentId;
        static int tookFrame = -1;
        static int tookButton;
        static int openingAgentId;
        static bool wishPersonal;
        static int opening;
        static bool hiding;
        static readonly List<string> offered = new List<string>();
        static readonly HashSet<string> seen = new HashSet<string>();

        public static bool IsOpen(Agent agent)
        {
            if (opening > 0 || agent == null || WishAgentId == 0 || WishAgentId != agent.GetInstanceID())
                return false;
            if (Shown(agent))
                return true;

            Abandon(agent);
            return false;
        }

        public static bool Open(Agent agent)
        {
            if (agent == null || agent.mainGUI == null)
                return false;

            GameController gc = agent.gc;
            if (gc == null)
                return false;
            if (gc.loadLevel != null && gc.loadLevel.switchingLevel)
                return false;

            if (WishAgentId == agent.GetInstanceID() && !Shown(agent))
                Abandon(agent);

            offered.Clear();
            seen.Clear();
            Collect(agent);
            if (offered.Count == 0)
                return false;

            wishPersonal = gc.fourPlayerMode || gc.coopMode;
            openingAgentId = agent.GetInstanceID();
            WishAgentId = 0;
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
                Debug.LogWarning("Амок не открыл список классов.");
                Debug.LogWarning(exception);
            }
            finally
            {
                opening--;
                openingAgentId = 0;
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
            for (int i = 0; i < Offer.Length; i++)
            {
                string name = Offer[i];
                if (!seen.Add(name))
                    continue;
                offered.Add(name);
            }
        }

        static string Label(ScrollingMenu menu, string className)
        {
            NameDB? names = menu.gc != null ? menu.gc.nameDB : null;
            string label = names != null ? names.GetName(className, "Agent") : "";
            if (string.IsNullOrEmpty(label) || label.StartsWith("E_"))
                return className;
            return label;
        }

        static void Fill(ScrollingMenu menu)
        {
            if (menu.buttonsData == null || offered.Count == 0)
                return;

            ButtonData? template = menu.buttonsData.Count > 0 ? menu.buttonsData[0] : null;
            if (menu.smallTraitList != null)
                menu.smallTraitList.Clear();
            menu.buttonsData.Clear();
            for (int i = 0; i < offered.Count; i++)
            {
                string className = offered[i];
                Unlock unlock = new Unlock(className, "Agent", true);
                if (menu.smallTraitList != null)
                    menu.smallTraitList.Add(unlock);
                ButtonData data = new ButtonData();
                data.isActive = true;
                data.scrollingMenu = menu;
                data.agent = menu.agent;
                data.scrollingButtonNum = i;
                data.scrollingButtonUnlock = unlock;
                data.scrollingButtonType = className;
                data.scrollingButtonAgentName = className;
                data.scrollingButtonConfigName = "";
                data.scrollingButtonLevelFeeling = "";
                data.buttonText = Label(menu, className);
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

            if (menu.titleText != null)
            {
                NameDB? names = menu.gc != null ? menu.gc.nameDB : null;
                string title = names != null ? names.GetName("CharacterSelectScrolling", "Interface") : "";
                if (!string.IsNullOrEmpty(title) && !title.StartsWith("E_"))
                    menu.titleText.text = title;
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
            if (index >= 0 && index < offered.Count && !string.IsNullOrEmpty(offered[index]))
                return offered[index];

            Unlock? unlock = button.scrollingButtonUnlock;
            if (unlock != null && !string.IsNullOrEmpty(unlock.unlockName))
                return unlock.unlockName;
            return button.scrollingButtonAgentName ?? "";
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
                    Debug.LogWarning("Амок не закрыл список классов.");
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
            if (index < 0 || index >= offered.Count || string.IsNullOrEmpty(offered[index]))
                return false;

            tookFrame = Time.frameCount;
            tookButton = id;

            PeacockEffect? form = PeacockEffect.Find(agent);
            if (form == null || !form.Charge || form.HasAlly)
            {
                Abandon(agent);
                return true;
            }

            if (!form.TrySummon(offered[index]))
            {
                Play(agent, "CantDo");
                return true;
            }

            form.Charge = false;
            form.BeginCountdown();
            AmokAbility.Spend(agent);
            Play(agent, "ClickButton");
            Abandon(agent);
            return true;
        }

        [HarmonyPatch(typeof(UnityEngine.UI.Button), "OnPointerClick")]
        static class Pointer
        {
            static void Postfix(UnityEngine.UI.Button __instance)
            {
                if (__instance == null)
                    return;

                ButtonHelper helper = __instance.GetComponent<ButtonHelper>();
                if (helper == null)
                    helper = __instance.GetComponentInParent<ButtonHelper>();
                if (helper == null)
                    return;

                if (RoosterWish.Take(helper))
                    return;
                Take(helper);
            }
        }

        [HarmonyPatch(typeof(ButtonHelper), nameof(ButtonHelper.PressedScrollingMenuButton))]
        static class ClickGate
        {
            static void Prefix(ButtonHelper __instance)
            {
                ScrollingMenu? menu = __instance != null ? __instance.scrollingMenu : null;
                if (menu == null || !Ours(menu))
                    return;

                menu.timeOnMenu = 1f;
                menu.canPressButtons = true;
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
                PeacockEffect? form = PeacockEffect.Find(agent);
                if (form == null || !form.Charge || form.HasAlly)
                {
                    Abandon(agent);
                    return false;
                }

                string name = Picked(myButton);
                if (string.IsNullOrEmpty(name))
                    return true;
                if (!form.TrySummon(name))
                {
                    __instance.canPressButtons = true;
                    Play(agent, "CantDo");
                    return false;
                }

                form.Charge = false;
                form.BeginCountdown();
                AmokAbility.Spend(agent);
                Play(agent, "ClickButton");
                Abandon(agent);
                return false;
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

                Unlock? unlock = myButton.scrollingButtonUnlock;
                string className = unlock != null ? unlock.unlockName : myButton.scrollingButtonAgentName;
                if (string.IsNullOrEmpty(className))
                    return false;

                NameDB? names = __instance.gc != null ? __instance.gc.nameDB : null;
                if (names == null || __instance.detailsTitle == null || __instance.detailsText == null)
                    return false;

                string title = names.GetName(className, "Agent");
                if (string.IsNullOrEmpty(title) || title.StartsWith("E_"))
                    title = className;
                __instance.detailsTitle.text = title;
                __instance.detailsText.text = names.GetName(className, "Description");
                if (__instance.details != null)
                    __instance.details.SetActive(true);
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
