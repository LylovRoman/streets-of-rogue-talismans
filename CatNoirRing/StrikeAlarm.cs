using UnityEngine;

namespace CatNoirRing
{
    static class StrikeAlarm
    {
        // Игра слышит только эти громкости: 0, 0.2, 0.5, 0.7, 0.8, 1, 2, 3, 4.
        // 4 — самая дальняя, около 24 клеток.
        public const float Loud = 4f;

        public static void Personal(Agent attacker, Agent victim)
        {
            if (attacker == null || victim == null)
                return;

            MakeHostile(victim, attacker);
            GameController gc = GameController.gameController;
            if (gc != null && victim.tr != null)
                gc.EnforcerAlertAttack(attacker, victim, 0f, victim.tr.position);

            if (gc == null || gc.agentList == null || victim.tr == null)
                return;

            const float reach = 1.92f;
            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (other == null || other == attacker || other == victim || other.dead || other.tr == null || other.following == attacker)
                    continue;
                if (MouseEffect.IsClone(other) || !SideOf(victim, other))
                    continue;
                if (Vector2.Distance(other.tr.position, victim.tr.position) > reach)
                    continue;
                if (other.movement != null && !other.movement.HasLOSAgent360(victim))
                    continue;
                if (other.relationships != null)
                {
                    string towardAttacker = other.relationships.GetRel(attacker);
                    if (towardAttacker == "Loyal" || towardAttacker == "Submissive")
                        continue;
                }

                MakeHostile(other, attacker);
            }
        }

        public static void Raise(Agent attacker, Agent victim, float alertRadius, float noiseVolume)
        {
            if (attacker == null || victim == null)
                return;

            MakeHostile(victim, attacker);
            if (attacker.relationships != null)
                attacker.relationships.SetRel(victim, "Hateful", true);
            GameController gc = GameController.gameController;
            if (gc != null && victim.tr != null)
            {
                gc.EnforcerAlertAttack(attacker, victim, alertRadius, victim.tr.position);
                if (gc.spawnerMain != null)
                    gc.spawnerMain.SpawnNoise(victim.tr.position, noiseVolume, attacker, "Normal", attacker);
            }
            else if (gc != null)
            {
                gc.EnforcerAlertAttack(attacker, victim, alertRadius);
            }

            if (attacker.relationships != null)
                attacker.relationships.HurtFriendCheck(victim, false);

            if (gc == null || gc.agentList == null)
                return;

            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (other == null || other == attacker || other == victim || other.dead || other.following == attacker)
                    continue;
                if (MouseEffect.IsClone(other) || !SideOf(victim, other))
                    continue;
                if (other.relationships != null)
                {
                    string towardAttacker = other.relationships.GetRel(attacker);
                    if (towardAttacker == "Loyal" || towardAttacker == "Submissive")
                        continue;
                }

                MakeHostile(other, attacker);
            }
        }

        static bool SideOf(Agent person, Agent other)
        {
            if (other.following == person || other.employer == person || person.following == other || person.employer == other)
                return true;

            return Friendly(other, person) || Friendly(person, other);
        }

        static bool Friendly(Agent from, Agent to)
        {
            if (from.relationships == null)
                return false;

            string rel = from.relationships.GetRel(to);
            return rel == "Aligned" || rel == "Loyal" || rel == "Friendly" || rel == "Submissive";
        }

        static void MakeHostile(Agent npc, Agent attacker)
        {
            if (npc.relationships == null)
                return;

            npc.justHitByAgent = attacker;
            npc.justHitByAgent2 = attacker;
            npc.lastHitByAgent = attacker;
            npc.relationships.SetRel(attacker, "Hateful", true);
            npc.relationships.AddRelHate(attacker, 5);
            npc.relationships.AddStrikes(attacker, 1);
            Relationship? rel = npc.relationships.GetRelationship(attacker);
            if (rel != null)
            {
                rel.relType = "Hateful";
                rel.didHostile = true;
                rel.didAnger = true;
                if (rel.relHate < 5f)
                    rel.relHate = 5f;
            }

            npc.opponent = attacker;
        }
    }
}
