using System.Collections.Generic;
using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    public class DragonArmItem : CustomItem
    {
        public const string ItemId = "DragonArm";

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.isWeapon = true;
            Item.stackable = false;
            Item.goesInToolbar = true;
            Item.cantDrop = true;
            Item.thiefCantSteal = true;
            Item.cantBeCloned = true;
            Item.notInLoadoutMachine = true;
            Item.dontAutomaticallySelect = true;
            Item.initCount = 1;
            Item.rewardCount = 1;
            Item.itemValue = 0;
        }
    }

    static class DragonArms
    {
        public const string WaterTrait = "StrongerWaterCannon";
        public const string RateMod = "RateOfFireMod";
        const int StreamAmmo = 4000;

        public enum Kind
        {
            None,
            Wind,
            Water,
            Taser,
        }

        public static readonly HashSet<InvItem> Live = new HashSet<InvItem>();

        public static Kind Next(Kind kind)
        {
            switch (kind)
            {
                case Kind.Wind: return Kind.Water;
                case Kind.Water: return Kind.Taser;
                default: return Kind.Wind;
            }
        }

        public static bool IsFormWeapon(InvItem item)
        {
            if (item == null)
                return false;
            if (Live.Contains(item) || item.invItemName == DragonArmItem.ItemId)
                return true;
            if (item.invItemName != "LeafBlower" && item.invItemName != "WaterCannon" && item.invItemName != "Taser")
                return false;

            return item.cantDrop && item.cantBeCloned && item.notInLoadoutMachine && item.thiefCantSteal;
        }

        public static bool Give(Agent agent, Kind kind)
        {
            InvDatabase? inventory = agent != null ? agent.inventory : null;
            if (inventory == null || kind == Kind.None)
                return false;

            Clear(inventory);
            InvItem created = inventory.AddItem(DragonArmItem.ItemId, 1);
            if (created == null)
                return false;

            Dress(created, kind);
            Live.Add(created);
            inventory.EquipWeapon(created, true);
            return true;
        }

        public static void Clear(InvDatabase inventory)
        {
            if (inventory == null || inventory.InvItemList == null)
                return;

            List<InvItem> doomed = new List<InvItem>();
            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (IsFormWeapon(item))
                    doomed.Add(item);
            }

            for (int i = 0; i < doomed.Count; i++)
                Drop(inventory, doomed[i]);
        }

        public static void Strip(InvDatabase inventory)
        {
            Clear(inventory);
        }

        public static void Feed(InvDatabase inventory, Kind kind)
        {
            InvItem? arm = Find(inventory);
            if (arm == null || kind == Kind.None)
                return;

            if (kind == Kind.Taser)
            {
                arm.rapidFire = false;
                arm.longerRapidFire = false;
                arm.rechargeAmount = 1;
                arm.invItemCount = 1;
                arm.rechargingItem = false;
                arm.noRefills = true;
                if (arm.contents != null && !arm.contents.Contains(RateMod))
                    arm.contents.Add(RateMod);
                return;
            }

            arm.rapidFire = true;
            arm.noCountText = true;
            arm.rechargeAmount = 0;
            arm.rechargingItem = false;
            if (arm.invItemCount < StreamAmmo / 2)
                arm.invItemCount = StreamAmmo;
        }

        public static void SyncWater(Agent agent, bool water, bool keepForever)
        {
            if (agent == null || agent.statusEffects == null)
                return;

            bool has = agent.statusEffects.hasTrait(WaterTrait);
            if (water)
            {
                if (!has)
                    agent.statusEffects.AddTrait(WaterTrait);
                return;
            }

            if (!keepForever && has)
                agent.statusEffects.RemoveTrait(WaterTrait);
        }

        public static InvItem? RealItem(InvDatabase inventory, string name)
        {
            if (inventory == null || inventory.InvItemList == null || string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (item == null || item.invItemName != name || IsFormWeapon(item))
                    continue;
                return item;
            }

            return null;
        }

        public static void KeepInHand(Agent agent)
        {
            DragonEffect? form = DragonEffect.Find(agent);
            InvDatabase? inventory = agent != null ? agent.inventory : null;
            if (form == null || form.Arm == Kind.None || inventory == null)
                return;

            InvItem? arm = Find(inventory);
            if (arm != null)
                inventory.EquipWeapon(arm, true);
        }

        static InvItem? Find(InvDatabase inventory)
        {
            if (inventory == null || inventory.InvItemList == null)
                return null;

            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (IsFormWeapon(item))
                    return item;
            }

            return null;
        }

        static void Dress(InvItem arm, Kind kind)
        {
            if (arm.Categories != null)
                arm.Categories.Clear();

            arm.invItemName = Vanilla(kind);
            arm.SetupDetails(false);
            arm.cantDrop = true;
            arm.thiefCantSteal = true;
            arm.cantBeCloned = true;
            arm.notInLoadoutMachine = true;
            arm.dontAutomaticallySelect = true;
            arm.dontSelectNPC = true;
            arm.stackable = false;
            arm.goesInToolbar = true;
            arm.isWeapon = true;
            arm.weaponCode = weaponType.WeaponProjectile;
            arm.itemType = ItemTypes.WeaponProjectile;
            arm.characterExclusive = false;
            arm.characterExclusiveSpecificCharacter = "";
            arm.cantDropSpecificCharacter = "";
            arm.questItem = false;
            arm.itemValue = 0;
            arm.noRefills = true;
            arm.rechargingItem = false;

            if (kind == Kind.Taser)
            {
                arm.rapidFire = false;
                arm.longerRapidFire = false;
                arm.shortRangeProjectile = false;
                arm.rechargeAmount = 1;
                arm.invItemCount = 1;
                arm.initCount = 1;
                arm.doesNoDamage = true;
                arm.noCountText = false;
                if (arm.contents != null && !arm.contents.Contains(RateMod))
                    arm.contents.Add(RateMod);
                return;
            }

            arm.rapidFire = true;
            arm.shortRangeProjectile = true;
            arm.longerRapidFire = true;
            arm.doesNoDamage = true;
            arm.noCountText = true;
            arm.rechargeAmount = 0;
            arm.invItemCount = StreamAmmo;
            arm.initCount = StreamAmmo;
        }

        static string Vanilla(Kind kind)
        {
            switch (kind)
            {
                case Kind.Water: return "WaterCannon";
                case Kind.Taser: return "Taser";
                default: return "LeafBlower";
            }
        }

        static void Drop(InvDatabase inventory, InvItem item)
        {
            Live.Remove(item);
            item.cantDrop = false;
            inventory.DestroyItem(item, true);
        }
    }
}
