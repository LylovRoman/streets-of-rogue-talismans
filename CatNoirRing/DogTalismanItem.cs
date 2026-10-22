using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(ItemId)]
    [ItemCategories(RogueCategories.Usable, RogueCategories.Weird, RogueCategories.Technology)]
    public class DogTalismanItem : CustomItem, IItemUsable
    {
        public const string ItemId = "DogTalisman";

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

            if (DogEffect.Find(agent) != null)
            {
                agent.statusEffects.RemoveStatusEffect(DogEffect.EffectId);
                return true;
            }

            if (Time.time < DogEffect.ReadyAt(agent))
                return false;

            agent.statusEffects.AddStatusEffect(DogEffect.EffectId);
            return true;
        }

        public override CustomTooltip GetCountString()
        {
            if (Owner == null)
                return default;

            float left = DogEffect.ReadyAt(Owner) - Time.time;
            if (left <= 0.05f)
                return default;

            return new CustomTooltip(Mathf.CeilToInt(left), Color.red);
        }
    }
}
