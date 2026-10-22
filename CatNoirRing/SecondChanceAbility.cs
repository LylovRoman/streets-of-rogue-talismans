using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class SecondChanceAbility : CustomAbility
    {
        public const string Id = "SecondChance";

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

            SnakeEffect? form = SnakeEffect.Find(agent);
            if (form == null)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!form.HasMark)
            {
                if (!form.MarkWorld())
                {
                    gc.audioHandler.Play(agent, "CantDo");
                    return;
                }

                form.BeginCountdown();
                gc.audioHandler.Play(agent, "Spawn");
                return;
            }

            form.Rewind();
            gc.audioHandler.Play(agent, "Teleport");
        }
    }
}
