using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Usable, RogueCategories.Weird, RogueCategories.Technology)]
    public class DragonTalismanItem : CustomItem, IItemUsable
    {
        public const string ItemId = "DragonTalisman";

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.itemValue = 180;
            Item.initCount = 1;
            Item.rewardCount = 1;
            Item.stackable = false;
            Item.goesInToolbar = true;
            Item.cantBeCloned = false;
        }

        public bool UseItem()
        {
            Agent? agent = Owner;
            if (agent == null || agent.dead || agent.statusEffects == null)
                return false;

            if (DragonEffect.Find(agent) != null)
            {
                agent.statusEffects.RemoveStatusEffect(DragonEffect.EffectId);
                return true;
            }

            if (Time.time < DragonEffect.ReadyAt(agent))
                return false;

            agent.statusEffects.AddStatusEffect(DragonEffect.EffectId);
            return true;
        }

        public override CustomTooltip GetCountString()
        {
            if (Owner == null)
                return default;

            float left = DragonEffect.ReadyAt(Owner) - Time.time;
            if (left <= 0.05f)
                return default;

            return new CustomTooltip(Mathf.CeilToInt(left), Color.red);
        }
    }
}
