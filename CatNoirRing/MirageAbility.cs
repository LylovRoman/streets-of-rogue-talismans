using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class MirageAbility : CustomAbility
    {
        public const string Id = "Mirage";

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

            FoxEffect? form = FoxEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!Mirage.Summon(agent))
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.Charge = false;
            form.BeginCountdown();
            form.KeepSuit();
            Count = 1;
        }
    }

    static class Mirage
    {
        public static bool Summon(Agent agent)
        {
            GameController gc = agent.gc;
            if (gc == null || gc.spawnerMain == null || agent.tr == null)
                return false;

            Agent hologram = gc.spawnerMain.SpawnAgent(agent.tr.position, agent, "Hologram", "Hologram", agent);
            return hologram != null && hologram != agent;
        }
    }
}
