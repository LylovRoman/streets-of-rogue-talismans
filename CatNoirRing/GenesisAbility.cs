using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class GenesisAbility : CustomAbility
    {
        public const string Id = "Genesis";

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

            GoatEffect? form = GoatEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!GoatWish.Open(agent))
                gc.audioHandler.Play(agent, "CantDo");
        }

        internal static void Spend(Agent agent)
        {
            InvItem? item = agent.inventory != null ? agent.inventory.equippedSpecialAbility : null;
            GenesisAbility? hook = item != null ? item.GetHook<GenesisAbility>() : null;
            if (hook != null)
                hook.Count = 1;
        }
    }
}
