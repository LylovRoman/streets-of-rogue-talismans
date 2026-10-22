using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class Clout
    {
        const float TilesFlung = 32f;
        const float FlightSeconds = 0.8f;

        static readonly List<Flight> flights = new List<Flight>();

        public static bool TryStrike(Agent agent)
        {
            if (agent.tr == null)
                return false;

            Agent? creature = Cataclysm.AimedCreature(agent);
            if (creature == null || creature.tr == null || creature.disappeared || creature.statusEffects == null)
                return false;

            GameController gc = GameController.gameController;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(agent, "SwingWeaponLarge");

            float damage = Mathf.Max(1f, CatNoirPlugin.CloutDamage.Value);
            creature.justHitByAgent = agent;
            creature.justHitByAgent2 = agent;
            creature.lastHitByAgent = agent;
            creature.statusEffects.ChangeHealth(-damage, agent);
            if (creature.dead || creature.health <= 0f)
                creature.killedByAgent = agent;
            StrikeAlarm.Raise(agent, creature, 10.8f, StrikeAlarm.Loud);
            if (gc != null && gc.audioHandler != null && creature != null)
                gc.audioHandler.Play(creature, "MeleeHitAgentLarge");
            if (gc != null && agent.tr != null)
                gc.ScreenShake(0.75f, 380f, agent.tr.position, agent);

            if (creature == null || creature.tr == null)
                return true;

            Vector2 aim = Cataclysm.Facing(agent);
            if (aim.sqrMagnitude < 0.01f && agent.tr != null)
            {
                Vector2 away = (Vector2)creature.tr.position - (Vector2)agent.tr.position;
                aim = away.sqrMagnitude > 0.01f ? away.normalized : Vector2.up;
            }

            Launch(agent, creature, aim);
            return true;
        }

        static void Launch(Agent attacker, Agent victim, Vector2 direction)
        {
            if (victim.tr == null)
                return;

            direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.up;
            for (int i = 0; i < flights.Count; i++)
            {
                if (flights[i].Victim != victim)
                    continue;

                flights[i].Attacker = attacker;
                flights[i].Direction = direction;
                flights[i].Remaining = Reach();
                flights[i].Last = victim.tr.position;
                Hold(flights[i], victim);
                return;
            }

            Flight flight = new Flight();
            flight.Victim = victim;
            flight.Attacker = attacker;
            flight.Direction = direction;
            flight.Remaining = Reach();
            flight.Speed = Reach() / FlightSeconds;
            flight.Last = victim.tr.position;
            if (victim.agentCollider != null)
                flight.Circle = victim.agentCollider.enabled;
            if (victim.agentColliderNormal != null)
                flight.Normal = victim.agentColliderNormal.enabled;
            if (victim.agentColliderDead != null)
                flight.DeadBox = victim.agentColliderDead.enabled;

            if (victim.movement != null)
            {
                victim.movement.KnockbackSpecificVelocity(direction * 8f, 360f, attacker);
                Rigidbody2D body = victim.movement.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    flight.Drag = body.drag;
                    flight.HasDrag = true;
                    body.drag = 0f;
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }
            }

            Hold(flight, victim);
            flights.Add(flight);
        }

        static void Advance()
        {
            if (flights.Count == 0)
                return;

            Cataclysm.BeginCloutStep();
            float tile = Tile();
            for (int i = flights.Count - 1; i >= 0; i--)
            {
                Flight flight = flights[i];
                Agent victim = flight.Victim;
                if (victim == null || victim.tr == null)
                {
                    flights.RemoveAt(i);
                    continue;
                }

                Vector2 from = victim.tr.position;
                if ((from - flight.Last).sqrMagnitude > tile * tile * 9f)
                {
                    Finish(flight);
                    flights.RemoveAt(i);
                    continue;
                }

                float step = Mathf.Min(flight.Speed * Time.deltaTime, tile * 1.25f);
                if (step > flight.Remaining)
                    step = flight.Remaining;
                if (step < 0.001f)
                    continue;

                Vector2 next = from + flight.Direction * step;
                Agent cause = flight.Attacker != null ? flight.Attacker : victim;
                if (Closed(cause, from, next, flight.Direction))
                {
                    Finish(flight);
                    flights.RemoveAt(i);
                    continue;
                }

                Closed(cause, next, next + flight.Direction * (tile * 0.95f), flight.Direction);
                Place(victim, next);
                Hold(flight, victim);
                flight.Last = next;
                flight.Remaining -= step;
                if (flight.Remaining > 0.001f)
                    continue;

                Finish(flight);
                flights.RemoveAt(i);
            }
        }

        static bool Closed(Agent cause, Vector2 from, Vector2 to, Vector2 direction)
        {
            float span = (to - from).magnitude;
            int steps = Mathf.Max(1, Mathf.CeilToInt(span / (Tile() * 0.34f)));
            Vector2 side = new Vector2(-direction.y, direction.x) * 0.22f;
            for (int i = 1; i <= steps; i++)
            {
                Vector2 at = Vector2.Lerp(from, to, i / (float)steps);
                if (Cataclysm.StopsClout(cause, at)
                    || Cataclysm.StopsClout(cause, at + side)
                    || Cataclysm.StopsClout(cause, at - side))
                    return true;
            }

            return false;
        }

        static void Hold(Flight flight, Agent victim)
        {
            if (victim.agentCollider != null)
                victim.agentCollider.enabled = false;
            if (victim.agentColliderNormal != null)
                victim.agentColliderNormal.enabled = false;
            if (victim.agentColliderDead != null)
                victim.agentColliderDead.enabled = false;

            Rigidbody2D? body = victim.movement != null ? victim.movement.GetComponent<Rigidbody2D>() : null;
            if (body != null)
            {
                body.drag = 0f;
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        static void Finish(Flight flight)
        {
            Agent victim = flight.Victim;
            if (victim == null)
                return;

            bool down = victim.dead || victim.knockedOut;
            if (victim.agentCollider != null)
                victim.agentCollider.enabled = flight.Circle && !down;
            if (victim.agentColliderNormal != null)
                victim.agentColliderNormal.enabled = flight.Normal && !down;
            if (victim.agentColliderDead != null)
                victim.agentColliderDead.enabled = down || flight.DeadBox;

            Rigidbody2D? body = victim.movement != null ? victim.movement.GetComponent<Rigidbody2D>() : null;
            if (body != null)
            {
                if (flight.HasDrag)
                    body.drag = flight.Drag;
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        static void Place(Agent agent, Vector2 position)
        {
            Rigidbody2D? body = agent.movement != null ? agent.movement.GetComponent<Rigidbody2D>() : null;
            if (body != null)
            {
                body.position = position;
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            if (agent.tr != null)
                agent.tr.position = position;
        }

        static float Reach()
        {
            return TilesFlung * Tile();
        }

        static float Tile()
        {
            return Consts.tileSize > 0.01f ? Consts.tileSize : 0.64f;
        }

        sealed class Flight
        {
            public Agent Victim = null!;
            public Agent? Attacker;
            public Vector2 Direction;
            public Vector2 Last;
            public float Remaining;
            public float Speed;
            public float Drag;
            public bool HasDrag;
            public bool Circle;
            public bool Normal;
            public bool DeadBox;
        }

        [HarmonyPatch(typeof(Updater), nameof(Updater.LateUpdateAgents))]
        static class FlightPatch
        {
            static void Postfix()
            {
                Advance();
            }
        }
    }
}
