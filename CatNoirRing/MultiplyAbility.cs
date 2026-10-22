using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class MultiplyAbility : CustomAbility
    {
        public const string Id = "Multiplication";

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

            MouseEffect? form = MouseEffect.Find(agent);
            if (form == null || !form.BeginMultiply())
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            gc.audioHandler.Play(agent, "AgentShrink");
        }
    }
}
