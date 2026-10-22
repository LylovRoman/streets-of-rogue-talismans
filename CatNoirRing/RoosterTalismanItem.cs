using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Usable, RogueCategories.Weird, RogueCategories.Technology)]
    public class RoosterTalismanItem : CustomItem, IItemUsable
    {
        public const string ItemId = "RoosterTalisman";

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

            if (RoosterEffect.Find(agent) != null)
            {
                agent.statusEffects.RemoveStatusEffect(RoosterEffect.EffectId);
                return true;
            }

            if (Time.time < RoosterEffect.ReadyAt(agent))
                return false;

            agent.statusEffects.AddStatusEffect(RoosterEffect.EffectId);
            return true;
        }

        public override CustomTooltip GetCountString()
        {
            if (Owner == null)
                return default;

            float left = RoosterEffect.ReadyAt(Owner) - Time.time;
            if (left <= 0.05f)
                return default;

            return new CustomTooltip(Mathf.CeilToInt(left), Color.red);
        }
    }
}
