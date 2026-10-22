using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class AmokAbility : CustomAbility
    {
        public const string Id = "Amok";

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

            PeacockEffect? form = PeacockEffect.Find(agent);
            if (form == null)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (form.HasAlly)
            {
                form.DismissAlly();
                gc.audioHandler.Play(agent, "ClickButton");
                return;
            }

            if (PeacockWish.IsOpen(agent))
                return;

            if (!form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!PeacockWish.Open(agent))
                gc.audioHandler.Play(agent, "CantDo");
        }

        internal static void Spend(Agent agent)
        {
            InvItem? item = agent.inventory != null ? agent.inventory.equippedSpecialAbility : null;
            AmokAbility? hook = item != null ? item.GetHook<AmokAbility>() : null;
            if (hook != null)
                hook.Count = 1;
        }
    }
}
