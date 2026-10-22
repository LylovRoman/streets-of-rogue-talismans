using System.Collections.Generic;
using RogueLibsCore;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnNextLevel)]
    public class DogMarkEffect : CustomEffect
    {
        public const string EffectId = "DogMark";

        public override int GetEffectTime()
        {
            return 9999;
        }

        public override int GetEffectHate()
        {
            return 0;
        }

        public override void OnAdded()
        {
            Effect.infiniteTime = true;
            CurrentTime = 9999;
        }

        public override void OnRemoved()
        {
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            Effect.infiniteTime = true;
            if (CurrentTime < 9999)
                CurrentTime = 9999;
        }

        public static DogMarkEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                DogMarkEffect? effect = list[i].GetHook<DogMarkEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public static void Apply(Agent agent)
        {
            if (agent == null || agent.statusEffects == null || Find(agent) != null)
                return;

            agent.statusEffects.AddStatusEffect(EffectId);
        }

        public static void Lift(Agent agent)
        {
            if (agent == null || agent.statusEffects == null || Find(agent) == null)
                return;

            agent.statusEffects.RemoveStatusEffect(EffectId);
        }
    }
}
