using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class ShellAbility : CustomAbility
    {
        public const string Id = "TurtleShell";

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

            TurtleEffect? form = TurtleEffect.Find(agent);
            if (form == null)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.ToggleShell();
            gc.audioHandler.Play(agent, form.ShellUp ? "Spawn" : "SelectItem");
        }
    }
}
