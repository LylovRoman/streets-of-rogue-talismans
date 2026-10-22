using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class TimeStopAbility : CustomAbility
    {
        public const string Id = "TimeStop";

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

            RabbitEffect? form = RabbitEffect.Find(agent);
            if (form == null || !form.Charge || !TimeStop.Begin(agent))
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
            gc.audioHandler.Play(agent, "Teleport");
        }
    }
}
