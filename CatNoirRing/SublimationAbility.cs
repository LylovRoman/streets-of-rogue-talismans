using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class SublimationAbility : CustomAbility
    {
        public const string Id = "Sublimation";

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

            RoosterEffect? form = RoosterEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!RoosterWish.Open(agent))
                gc.audioHandler.Play(agent, "CantDo");
        }

        internal static void Spend(Agent agent)
        {
            InvItem? item = agent.inventory != null ? agent.inventory.equippedSpecialAbility : null;
            SublimationAbility? hook = item != null ? item.GetHook<SublimationAbility>() : null;
            if (hook != null)
                hook.Count = 1;
        }
    }
}
