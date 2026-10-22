using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class CataclysmAbility : CustomAbility
    {
        public const string Id = "Cataclysm";

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

            CatNoirEffect? form = CatNoirEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (Cataclysm.TryStrike(agent) != StrikeResult.Spent)
                return;

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
        }
    }
}
