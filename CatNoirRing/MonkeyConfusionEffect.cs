using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class MonkeyConfusionEffect : CustomEffect
    {
        public const string EffectId = "MonkeyConfusion";

        internal bool released;

        public static int Seconds()
        {
            return Mathf.Max(1, CatNoirPlugin.ConfusionDuration.Value);
        }

        public override int GetEffectTime()
        {
            return Seconds();
        }

        public override int GetEffectHate()
        {
            return 0;
        }

        public override void OnAdded()
        {
            released = false;
            Effect.infiniteTime = false;
            CurrentTime = Seconds();
        }

        public override void OnRefreshed()
        {
            released = false;
            Effect.infiniteTime = false;
            CurrentTime = Seconds();
        }

        public override void OnRemoved()
        {
            released = true;
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            if (!e.IsFirstTick)
                CurrentTime--;
        }

        public static bool Affects(Agent? agent)
        {
            MonkeyConfusionEffect? effect = Find(agent);
            return effect != null && !effect.released;
        }

        public static MonkeyConfusionEffect? Find(Agent? agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                MonkeyConfusionEffect? effect = list[i].GetHook<MonkeyConfusionEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }
    }

    static class ConfusionWalk
    {
        const float WalkPace = 0.35f;
        const float StumblePace = 0.12f;

        struct Sample
        {
            public bool Active;
            public Vector2 Pending;
        }

        static readonly FieldInfo MovementAgent = AccessTools.Field(typeof(Movement), "agent");
        static readonly FieldInfo CombatAgent = AccessTools.Field(AccessTools.TypeByName("Combat"), "agent");

        static Sample Begin(Agent? agent)
        {
            if (agent == null || agent.rb == null || !MonkeyConfusionEffect.Affects(agent))
                return default;

            return new Sample
            {
                Active = true,
                Pending = agent.rb.totalForce,
            };
        }

        static void End(Agent? agent, Sample sample)
        {
            if (!sample.Active || agent == null || agent.rb == null || !MonkeyConfusionEffect.Affects(agent))
                return;

            Vector2 added = agent.rb.totalForce - sample.Pending;
            if (added.sqrMagnitude < 0.000001f)
                return;

            float pace = Stumble(agent) ? StumblePace : WalkPace;
            agent.rb.totalForce = sample.Pending - added * pace;
        }

        static bool Stumble(Agent agent)
        {
            float phase = Mathf.Repeat(Time.time * 1.6f + agent.agentID * 0.17f, 1f);
            return phase < 0.3f;
        }

        static Agent? Read(object body, FieldInfo field)
        {
            if (body == null || field == null)
                return null;

            return field.GetValue(body) as Agent;
        }

        [HarmonyPatch(typeof(Movement), nameof(Movement.PlayerMovement))]
        static class PlayerMove
        {
            static void Prefix(Movement __instance, out Sample __state)
            {
                __state = Begin(Read(__instance, MovementAgent));
            }

            static void Postfix(Movement __instance, Sample __state)
            {
                End(Read(__instance, MovementAgent), __state);
            }
        }

        [HarmonyPatch(typeof(PathfindingAI), nameof(PathfindingAI.FixedUpdatePathfindingAI))]
        static class NpcMove
        {
            static void Prefix(PathfindingAI __instance, out Sample __state)
            {
                __state = Begin(__instance != null ? __instance.agent : null);
            }

            static void Postfix(PathfindingAI __instance, Sample __state)
            {
                End(__instance != null ? __instance.agent : null, __state);
            }
        }

        [HarmonyPatch]
        static class CombatMove
        {
            static MethodBase TargetMethod()
            {
                return AccessTools.Method(AccessTools.TypeByName("Combat"), "FixedUpdateCombat");
            }

            static void Prefix(Component __instance, out Sample __state)
            {
                __state = Begin(Read(__instance, CombatAgent));
            }

            static void Postfix(Component __instance, Sample __state)
            {
                End(Read(__instance, CombatAgent), __state);
            }
        }
    }
}
