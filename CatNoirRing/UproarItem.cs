using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Weapons, RogueCategories.Melee, RogueCategories.NPCsCantPickUp)]
    public class UproarItem : CustomItem
    {
        public const string ItemId = "Uproar";

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponThrown;
            Item.isWeapon = true;
            Item.weaponCode = weaponType.WeaponThrown;
            Item.meleeDamage = 0;
            Item.throwDamage = 0;
            Item.throwDistance = LadybugYoyoItem.AimReach;
            Item.initCount = 1;
            Item.rewardCount = 1;
            Item.stackable = false;
            Item.goesInToolbar = true;
            Item.noCountText = true;
            Item.cantDrop = true;
            Item.thiefCantSteal = true;
            Item.notInLoadoutMachine = true;
            Item.cantBeCloned = true;
            Item.itemValue = 40;
        }
    }
}
