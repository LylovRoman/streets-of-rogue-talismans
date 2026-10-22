using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class ConfusionAbility : CustomAbility
    {
        public const string Id = "Confusion";

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

            MonkeyEffect? form = MonkeyEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!MonkeyUproar.Release(agent, form))
                return;

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
        }
    }

    static class MonkeyUproar
    {
        public static bool Release(Agent caster, MonkeyEffect form)
        {
            Agent? victim = form.Marked;
            GameController? game = caster.gc != null ? caster.gc : GameController.gameController;
            if (victim == null || victim.dead || victim.ghost || victim.statusEffects == null)
            {
                if (victim != null && (victim.dead || victim.ghost))
                    form.ClearMark();
                Deny(caster, game);
                return false;
            }

            form.ClearMark();
            int seconds = MonkeyConfusionEffect.Seconds();
            victim.statusEffects.AddStatusEffect(MonkeyConfusionEffect.EffectId, seconds);
            if (game != null && game.audioHandler != null)
                game.audioHandler.Play(victim, "Slip");
            if (game != null && game.playerControl != null)
                game.playerControl.Vibrate(caster.isPlayer, 0.15f, 0.2f);
            return true;
        }

        static void Deny(Agent agent, GameController? game)
        {
            if (game != null && game.audioHandler != null)
                game.audioHandler.Play(agent, "CantDo");
        }
    }
}
