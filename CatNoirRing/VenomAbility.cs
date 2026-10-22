using HarmonyLib;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class VenomAbility : CustomAbility
    {
        public const string Id = "Venom";

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

            BeeEffect? form = BeeEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!Venom.Sting(agent))
                return;

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
            gc.audioHandler.Play(agent, "Paralyzer");
        }
    }

    static class Venom
    {
        public static bool Sting(Agent agent)
        {
            Agent? creature = Cataclysm.AimedCreature(agent);
            if (creature == null || creature.statusEffects == null)
                return false;

            creature.statusEffects.AddStatusEffect("Paralyzed", Mathf.Max(1, CatNoirPlugin.ParalysisDuration.Value));
            StrikeAlarm.Personal(agent, creature);
            return true;
        }

        public static bool Holds(Agent? agent)
        {
            return agent != null && !agent.dead && agent.statusEffects != null && agent.statusEffects.hasStatusEffect("Paralyzed");
        }
    }

    [HarmonyPatch(typeof(Melee), nameof(Melee.CheckAttack))]
    static class ParalyzedMelee
    {
        static bool Prefix(Melee __instance, ref bool __result)
        {
            if (!Venom.Holds(__instance.agent))
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Melee), nameof(Melee.Attack), typeof(bool))]
    [HarmonyPatch(typeof(Melee), nameof(Melee.Attack), typeof(PlayfieldObject))]
    static class ParalyzedSwing
    {
        static bool Prefix(Melee __instance)
        {
            return !Venom.Holds(__instance.agent);
        }
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.CheckAttack))]
    static class ParalyzedGun
    {
        static bool Prefix(Gun __instance, ref bool __result)
        {
            if (!Venom.Holds(__instance.agent))
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Gun), nameof(Gun.Shoot), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPatch(typeof(Gun), nameof(Gun.Shoot), typeof(bool), typeof(bool), typeof(bool), typeof(int))]
    [HarmonyPatch(typeof(Gun), nameof(Gun.Shoot), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(string))]
    static class ParalyzedShot
    {
        static bool Prefix(Gun __instance)
        {
            return !Venom.Holds(__instance.agent);
        }
    }

    [HarmonyPatch(typeof(StatusEffects), nameof(StatusEffects.PressedSpecialAbility))]
    static class ParalyzedSpecial
    {
        static bool Prefix(StatusEffects __instance, ref bool __result)
        {
            if (__instance == null || !Venom.Holds(__instance.agent))
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(MeleeHitbox), "OnTriggerEnter2D")]
    static class ParalyzedHit
    {
        static bool Prefix(MeleeHitbox __instance, Collider2D other)
        {
            if (other == null || __instance == null || __instance.myMelee == null)
                return true;

            return !Venom.Holds(__instance.myMelee.agent);
        }
    }
}
