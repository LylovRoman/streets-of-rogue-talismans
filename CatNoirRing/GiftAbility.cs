using System.Collections.Generic;
using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class GiftAbility : CustomAbility
    {
        public const string Id = "Gift";

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

            PigEffect? form = PigEffect.Find(agent);
            if (form == null || !form.Charge)
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            if (!Gift.Give(agent, form))
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.Charge = false;
            form.BeginCountdown();
            Count = 1;
            gc.audioHandler.Play(agent, "ChooseReward");
        }
    }

    static class Gift
    {
        public static bool Give(Agent caster, PigEffect form)
        {
            Agent? person = Cataclysm.AimedCreature(caster);
            if (person == null || !Person(person) || person.relationships == null || person.relationships.RelList == null)
                return false;

            Befriend(person);
            form.Accept(person);
            return true;
        }

        static bool Person(Agent? agent)
        {
            if (agent == null || agent.dead || agent.ghost || agent.inhuman || agent.objectAgent)
                return false;
            if (agent.mechEmpty || agent.killerRobot || agent.copBot || agent.butlerBot)
                return false;
            return !MouseEffect.IsClone(agent);
        }

        static void Befriend(Agent person)
        {
            List<Relationship> list = person.relationships.RelList;
            List<Relationship> copy = new List<Relationship>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Relationship rel = list[i];
                if (rel != null && rel.agent != null && rel.agent != person)
                    copy.Add(rel);
            }

            for (int i = 0; i < copy.Count; i++)
                Calm(person, copy[i]);

            person.opponent = null;
            person.hasOpponent = false;
            person.inCombat = false;
            person.inFleeCombat = false;
            person.lastHitByAgent = null;
            DropBattle(person);
            if (person.statusEffects != null && person.statusEffects.hasTrait("Enraged"))
                person.statusEffects.RemoveTrait("Enraged");
        }

        static void Calm(Agent person, Relationship rel)
        {
            rel.relBeforeRage = "Friendly";
            rel.relBeforeRageCode = relStatus.Friendly;
            rel.relHateBeforeRage = 0f;
            person.relationships.SetRelInitial(rel.agent, "Friendly");
            rel.relType = "Friendly";
            rel.relTypeCode = relStatus.Friendly;
            rel.initialRelType = "Friendly";
            rel.initialRelTypeCode = relStatus.Friendly;
            rel.relHate = 0f;
            rel.relStrikes = 0;
            rel.didHostile = false;
            rel.didAnger = false;
            rel.saidStrike1 = false;
            rel.saidStrike2 = false;
            rel.annoyedCountdown = 0f;
            rel.secretHate = false;
            rel.mechHate = false;
            rel.damageDone = 0;
            Pull(person.relationships.hateList, rel.agent);
            Pull(person.relationships.loyalList, rel.agent);
            Pull(person.relationships.alignedList, rel.agent);
            Pull(person.relationships.alignedLoyalList, rel.agent);
        }

        static void Pull(List<Agent>? list, Agent agent)
        {
            if (list != null && list.Contains(agent))
                list.Remove(agent);
        }

        static void DropBattle(Agent person)
        {
            if (person.brain == null || person.brain.Goals == null)
                return;

            List<Goal> goals = person.brain.Goals;
            for (int i = goals.Count - 1; i >= 0; i--)
            {
                Goal goal = goals[i];
                if (goal != null && goal.goalCode == goalType.Battle)
                    goals.RemoveAt(i);
            }
        }
    }
}
