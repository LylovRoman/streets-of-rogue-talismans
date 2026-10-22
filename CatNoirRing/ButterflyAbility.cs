using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class ButterflyAbility : CustomAbility
    {
        public const string Id = "Akuma";

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

            ButterflyEffect? form = ButterflyEffect.Find(agent);
            if (form == null)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!form.Charge)
            {
                if (!form.RecallVictim())
                {
                    gc.audioHandler.Play(agent, "CantDo");
                    return;
                }

                gc.audioHandler.Play(agent, "Teleport");
                return;
            }

            if (!ButterflyAim.Press(agent))
                gc.audioHandler.Play(agent, "CantDo");
        }
    }
}
