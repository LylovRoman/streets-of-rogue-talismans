using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class LuckyCharmAbility : CustomAbility
    {
        public const string Id = "LuckyCharm";

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

            LadybugEffect? form = LadybugEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!LuckyCharm.Grant(agent))
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
            gc.audioHandler.Play(agent, "ChooseReward");
        }
    }

    static class LuckyCharm
    {
        public static bool Grant(Agent agent)
        {
            InvDatabase? inventory = agent.inventory;
            GameController gc = agent.gc;
            if (inventory == null || gc == null || gc.rnd == null)
                return false;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                string name = gc.rnd.RandomSelect("DefaultChallenge", "Items");
                if (string.IsNullOrEmpty(name) || name == LadybugYoyoItem.ItemId || name == LadybugEarringsItem.ItemId)
                    continue;
                if (name == SteelStaffItem.ItemId || name == CatNoirRingItem.ItemId || name == SnakeTalismanItem.ItemId)
                    continue;
                if (name == DogTalismanItem.ItemId || name == DogBallItem.ItemId)
                    continue;
                if (name == MonkeyTalismanItem.ItemId || name == UproarItem.ItemId)
                    continue;
                if (name == DragonTalismanItem.ItemId || name == DragonArmItem.ItemId)
                    continue;
                if (name == OxTalismanItem.ItemId || name == PeacockTalismanItem.ItemId || name == FoxTalismanItem.ItemId || name == PigTalismanItem.ItemId || name == TurtleTalismanItem.ItemId || name == ButterflyTalismanItem.ItemId || name == RabbitTalismanItem.ItemId)
                    continue;

                InvItem added = inventory.AddItem(name, 1);
                if (added == null)
                    continue;

                if (added.rewardCount > added.invItemCount)
                    added.invItemCount = added.rewardCount;
                return true;
            }

            return false;
        }
    }
}
