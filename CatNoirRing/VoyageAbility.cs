using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class VoyageAbility : CustomAbility
    {
        public const string Id = "Voyage";

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

            HorseEffect? form = HorseEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            HorseVoyage.Open(agent);
        }
    }
}
