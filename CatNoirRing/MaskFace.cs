using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatNoirRing
{
    static class MaskFace
    {
        public struct Memory
        {
            public Agent Who;
            public string RelType;
            public int Hate;
            public int Strikes;
            public bool DidHostile;
            public bool DidAnger;
            public bool WasOpponent;
            public bool SaidStrike1;
            public bool SaidStrike2;
        }

        static readonly List<Dictionary<int, Memory>> faces = new List<Dictionary<int, Memory>>();

        public static void Remember(Agent owner, Dictionary<int, Memory> civilian)
        {
            civilian.Clear();
            Each(owner, (other, rel) => civilian[other.agentID] = Capture(owner, other, rel));
        }

        public static void Pardon(Agent person, Dictionary<int, Memory> civilian)
        {
            if (person == null)
                return;

            Memory calm = new Memory
            {
                Who = person,
                RelType = "Friendly",
                Hate = 0,
                Strikes = 0,
                DidHostile = false,
                DidAnger = false,
                WasOpponent = false,
                SaidStrike1 = false,
                SaidStrike2 = false,
            };
            if (civilian != null)
                civilian[person.agentID] = calm;

            for (int i = 0; i < faces.Count; i++)
            {
                Dictionary<int, Memory> face = faces[i];
                if (face != null)
                    face[person.agentID] = calm;
            }
        }

        public static void Wear(Agent owner, Dictionary<int, Memory> mask, Dictionary<int, Memory> civilian)
        {
            Register(mask);
            Prune(mask);
            Each(owner, (other, rel) =>
            {
                bool saved = mask.TryGetValue(other.agentID, out Memory memory) && memory.Who == other;
                if (LiveGrudge(other, owner, rel) && Witnessed(owner, other, rel))
                {
                    mask[other.agentID] = Capture(owner, other, rel);
                    return;
                }

                if (saved)
                    Write(owner, other, memory);
                else if (LiveGrudge(other, owner, rel))
                    Calm(owner, other, rel);
            });
        }

        public static void Save(Agent owner, Dictionary<int, Memory> mask)
        {
            Register(mask);
            Prune(mask);
            Each(owner, (other, rel) => mask[other.agentID] = Capture(owner, other, rel));
        }

        public static void Restore(Agent owner, Dictionary<int, Memory> civilian)
        {
            Each(owner, (other, rel) =>
            {
                if (LiveGrudge(other, owner, rel) && Witnessed(owner, other, rel))
                {
                    civilian[other.agentID] = Capture(owner, other, rel);
                    return;
                }

                if (civilian.TryGetValue(other.agentID, out Memory memory))
                    Write(owner, other, memory);
            });
            civilian.Clear();
        }

        static void Each(Agent owner, Action<Agent, Relationship> visit)
        {
            GameController gc = owner.gc;
            if (gc == null || gc.agentList == null)
                return;

            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (other == null || other == owner || other.relationships == null)
                    continue;

                Relationship rel = other.relationships.GetRelationship(owner);
                if (rel != null)
                    visit(other, rel);
            }
        }

        static Memory Capture(Agent owner, Agent other, Relationship rel)
        {
            return new Memory
            {
                Who = other,
                RelType = rel.relType,
                Hate = rel.relHate <= 0f ? 0 : Mathf.CeilToInt(rel.relHate),
                Strikes = rel.relStrikes,
                DidHostile = rel.didHostile,
                DidAnger = rel.didAnger,
                WasOpponent = other.opponent == owner,
                SaidStrike1 = rel.saidStrike1,
                SaidStrike2 = rel.saidStrike2,
            };
        }

        static void Write(Agent owner, Agent other, Memory memory)
        {
            if (!string.IsNullOrEmpty(memory.RelType))
                other.relationships.SetRel(owner, memory.RelType);

            Relationship rel = other.relationships.GetRelationship(owner);
            if (rel == null)
                return;

            rel.relHate = memory.Hate;
            rel.relStrikes = memory.Strikes;
            rel.didHostile = memory.DidHostile;
            rel.didAnger = memory.DidAnger;
            rel.saidStrike1 = memory.SaidStrike1;
            rel.saidStrike2 = memory.SaidStrike2;
            if (memory.WasOpponent)
                other.opponent = owner;
            else if (other.opponent == owner && !IsGrudge(memory.RelType, memory.Hate))
                other.opponent = null;
        }

        static void Calm(Agent owner, Agent other, Relationship rel)
        {
            string calm = string.IsNullOrEmpty(rel.initialRelType) ? "Neutral" : rel.initialRelType;
            other.relationships.SetRel(owner, calm);
            rel = other.relationships.GetRelationship(owner);
            if (rel != null)
            {
                rel.relHate = 0f;
                rel.relStrikes = 0;
                rel.didHostile = false;
                rel.didAnger = false;
            }

            if (other.opponent == owner)
                other.opponent = null;
        }

        static void Register(Dictionary<int, Memory> face)
        {
            if (face != null && !faces.Contains(face))
                faces.Add(face);
        }

        static void Prune(Dictionary<int, Memory> mask)
        {
            List<int> gone = new List<int>();
            foreach (KeyValuePair<int, Memory> pair in mask)
            {
                if (pair.Value.Who == null)
                    gone.Add(pair.Key);
            }

            for (int i = 0; i < gone.Count; i++)
                mask.Remove(gone[i]);
        }

        static bool LiveGrudge(Agent other, Agent owner, Relationship rel)
        {
            return other.opponent == owner || IsGrudge(rel.relType, rel.relHate);
        }

        static bool Witnessed(Agent owner, Agent other, Relationship rel)
        {
            Movement? eyes = other != null ? other.movement : null;
            if (eyes == null || owner == null)
                return false;

            return eyes.HasLOSAgent(owner) || eyes.HasLOSAgent360(owner);
        }

        static bool IsGrudge(string relType, float hate)
        {
            return hate > 0f || relType == "Hateful" || relType == "Annoyed";
        }
    }
}
