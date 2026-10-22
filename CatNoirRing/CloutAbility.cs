using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class CloutAbility : CustomAbility
    {
        public const string Id = "Clout";

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

            TigerEffect? form = TigerEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!Clout.TryStrike(agent))
                return;

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
        }
    }
}
