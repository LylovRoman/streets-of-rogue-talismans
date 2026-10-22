using System.Collections.Generic;
using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class ResistanceAbility : CustomAbility
    {
        public const string Id = "Resistance";

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

            OxEffect? form = OxEffect.Find(agent);
            if (form == null)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            Resistance.Cleanse(agent);
            form.BeginCountdown();
            form.KeepSuit();
            gc.audioHandler.Play(agent, "UseFood");
        }
    }

    static class Resistance
    {
        public static void Cleanse(Agent agent)
        {
            StatusEffects? effects = agent.statusEffects;
            if (effects == null)
                return;

            List<StatusEffect>? list = effects.StatusEffectList;
            if (list != null)
            {
                List<string> cured = new List<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    StatusEffect effect = list[i];
                    if (effect == null || string.IsNullOrEmpty(effect.statusEffectName))
                        continue;
                    if (effect.statusEffectName == OxEffect.EffectId)
                        continue;
                    if (!effects.IsAntidoteEffect(effect.statusEffectName))
                        continue;
                    cured.Add(effect.statusEffectName);
                }

                for (int i = 0; i < cured.Count; i++)
                    effects.RemoveStatusEffect(cured[i]);
            }

            effects.AddStatusEffect("BlockDebuffs");
        }
    }
}
