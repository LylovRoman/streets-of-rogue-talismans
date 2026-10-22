using System.Collections.Generic;
using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Weapons, RogueCategories.Melee, RogueCategories.NPCsCantPickUp)]
    public class JumpRopeItem : CustomItem
    {
        public const string ItemId = "JumpRope";
        public static readonly HashSet<InvItem> Live = new HashSet<InvItem>();

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponMelee;
            Item.isWeapon = true;
            Item.weaponCode = weaponType.WeaponMelee;
            Item.meleeDamage = CatNoirPlugin.StaffDamage.Value;
            Item.initCount = 100;
            Item.rewardCount = 200;
            Item.maxAmmo = 100;
            Item.hierarchy = 4f;
            Item.shadowOffset = 1;
            Item.stackable = false;
            Item.goesInToolbar = true;
            Item.noCountText = true;
            Item.cantDrop = true;
            Item.thiefCantSteal = true;
            Item.notInLoadoutMachine = true;
            Item.cantBeCloned = true;
            Item.itemValue = 20;
            Item.hitSoundType = "Normal";
        }
    }
}
