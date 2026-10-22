using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Usable, RogueCategories.Weird, RogueCategories.Technology)]
    public class LadybugEarringsItem : CustomItem, IItemUsable
    {
        public const string ItemId = "LadybugEarrings";

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

            if (LadybugEffect.Find(agent) != null)
            {
                agent.statusEffects.RemoveStatusEffect(LadybugEffect.EffectId);
                return true;
            }

            if (Time.time < LadybugEffect.ReadyAt(agent))
                return false;

            agent.statusEffects.AddStatusEffect(LadybugEffect.EffectId);
            return true;
        }

        public override CustomTooltip GetCountString()
        {
            if (Owner == null)
                return default;

            float left = LadybugEffect.ReadyAt(Owner) - Time.time;
            if (left <= 0.05f)
                return default;

            return new CustomTooltip(Mathf.CeilToInt(left), Color.red);
        }
    }
}
