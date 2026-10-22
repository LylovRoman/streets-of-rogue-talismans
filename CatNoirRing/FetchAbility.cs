using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class FetchAbility : CustomAbility
    {
        public const string Id = "Fetch";

        public override void OnAdded()
        {
            if (Owner != null)
                Owner.specialAbilityAttack = true;
        }

        public override void OnPressed()
        {
            Agent? agent = Owner;
            if (agent == null)
                return;

            DogEffect? form = DogEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!DogFetch.Take(agent, form))
                return;

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
        }
    }

    static class DogFetch
    {
        public static bool Take(Agent thief, DogEffect form)
        {
            Agent? victim = form.Marked;
            GameController gc = thief.gc != null ? thief.gc : GameController.gameController;
            if (victim == null || victim.dead || victim.ghost || victim.invisible || victim.inventory == null || thief.inventory == null)
            {
                if (victim != null && (victim.dead || victim.ghost))
                    form.ClearMark();
                Deny(thief, gc);
                return false;
            }

            bool playerStealing = thief.isPlayer > 0;
            InvItem stolen = victim.inventory.StealItem(playerStealing, true);
            if (stolen == null)
            {
                if (gc != null && gc.spawnerMain != null)
                    gc.spawnerMain.SpawnStatusText(thief, "ItemPickup", "Empty", "Item");
                Deny(thief, gc);
                return false;
            }

            if (!thief.inventory.hasEmptySlotForItem(stolen))
            {
                if (gc != null && gc.serverPlayer)
                    thief.inventory.PlayerFullResponse(thief);
                Deny(thief, gc);
                return false;
            }

            victim.MustStopOperating();
            Buzz(gc, thief, victim);
            thief.inventory.PlayPickupSound(thief, stolen);
            if (gc != null && gc.percentChance(10))
                thief.SayDialogue("Yoink");

            if (stolen.isArmorHead && stolen.equipped)
                victim.inventory.UnequipArmorHead();
            if (stolen.invItemName == "MayorHat" && victim.isMayor && gc != null && gc.ending != null)
                gc.ending.SetMayorTurnover("StoleHat");

            thief.inventory.DontPlayPickupSounds(true);
            bool moved = victim.isPlayer == 0
                ? TakeWhole(thief, victim, stolen)
                : TakePortion(thief, victim, stolen);
            thief.inventory.DontPlayPickupSounds(false);
            if (!moved)
            {
                Deny(thief, gc);
                return false;
            }

            Notice(thief, victim, gc);
            form.ClearMark();
            return true;
        }

        static bool TakeWhole(Agent thief, Agent victim, InvItem stolen)
        {
            InvItem added = thief.inventory.AddItem(stolen);
            if (added == null)
                return false;

            victim.inventory.DestroyItem(stolen);
            added.ShowPickingUpText(thief);
            return true;
        }

        static bool TakePortion(Agent thief, Agent victim, InvItem stolen)
        {
            int count = Portion(stolen);
            InvItem copy = new InvItem();
            copy.invItemName = stolen.invItemName;
            copy.SetupDetails(false);
            copy.invItemCount = count;
            if (stolen.maxAmmo > copy.maxAmmo)
                copy.maxAmmo = stolen.maxAmmo;
            if (copy.contents != null)
                copy.contents.Clear();
            if (stolen.contents != null && copy.contents != null)
            {
                for (int i = 0; i < stolen.contents.Count; i++)
                    copy.contents.Add(stolen.contents[i]);
            }

            InvItem added = thief.inventory.AddItem(copy);
            if (added == null)
                return false;

            added.doSpill = true;
            victim.inventory.SubtractFromItemCount(stolen, count);
            added.ShowPickingUpText(thief);
            return true;
        }

        static int Portion(InvItem stolen)
        {
            int count = stolen.invItemCount;
            if (count <= 0)
                return count;

            int code = (int)stolen.weaponCode;
            if (code == 1 || code == 3 || stolen.isArmor || stolen.isArmorHead)
                return count;

            return Random.Range(1, count + 1);
        }

        static void Notice(Agent thief, Agent victim, GameController? gc)
        {
            if (victim.isPlayer == 0)
                victim.didPickpocket = true;

            if (victim.isPlayer == 0 && victim.pickpocketedTimes < 3)
            {
                string rel = victim.relationships != null ? victim.relationships.GetRel(thief) : "";
                if (rel != "Aligned")
                {
                    victim.pickpocketedTimes++;
                    if (gc != null && gc.stats != null)
                        gc.stats.AddToStat(thief, "StoleItems", 1);
                    if (thief.skillPoints != null)
                        thief.skillPoints.AddPoints("PickpocketPoints");
                }
            }

            if (thief.statusEffects != null && thief.statusEffects.hasTrait("AgentsDontSeePickpocket"))
                return;
            if (gc == null)
                return;

            gc.EnforcerAlert(thief, 0f, Vector2.zero, 0);
            if (thief.relationships == null || thief.tr == null || gc.spawnerMain == null)
                return;
            if (thief.relationships.HurtFriendCheck(victim, false))
                gc.spawnerMain.SpawnNoise(thief.tr.position, 0f, null, null, thief);
        }

        static void Buzz(GameController? gc, Agent thief, Agent victim)
        {
            if (gc == null || gc.playerControl == null)
                return;

            gc.playerControl.Vibrate(thief.isPlayer, 0.15f, 0.2f);
            if (victim.isPlayer > 0)
                gc.playerControl.Vibrate(victim.isPlayer, 0.15f, 0.2f);
        }

        static void Deny(Agent agent, GameController? gc)
        {
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(agent, "CantDo");
        }
    }
}
