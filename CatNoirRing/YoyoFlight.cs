using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class YoyoFlight
    {
        class Flight
        {
            public Item? Body;
            public Agent? Thrower;
            public Vector3 Origin;
            public float ThrownAt;
            public float Farthest;
            public bool Returning;
            public bool Caught;
        }

        static readonly Dictionary<int, Flight> Flights = new Dictionary<int, Flight>();

        public static void Tick(GameController gc)
        {
            if (gc != null && gc.itemList != null)
            {
                for (int i = 0; i < gc.itemList.Count; i++)
                    Track(gc.itemList[i]);
            }

            List<int> finished = new List<int>();
            foreach (KeyValuePair<int, Flight> pair in Flights)
            {
                if (Pull(pair.Value))
                    finished.Add(pair.Key);
            }

            for (int i = 0; i < finished.Count; i++)
                Flights.Remove(finished[i]);
        }

        public static void Vanish(Agent owner)
        {
            List<Item> flying = new List<Item>();
            HashSet<int> seen = new HashSet<int>();
            foreach (KeyValuePair<int, Flight> pair in Flights)
            {
                Item? body = pair.Value.Body;
                if (body != null && (body.thrower == owner || body.owner == owner))
                    flying.Add(body);
            }

            GameController gc = owner.gc;
            if (gc != null && gc.itemList != null)
            {
                for (int i = 0; i < gc.itemList.Count; i++)
                {
                    Item item = gc.itemList[i];
                    if (IsYoyo(item) && (item.thrower == owner || item.owner == owner))
                        flying.Add(item);
                }
            }

            for (int i = 0; i < flying.Count; i++)
            {
                Item item = flying[i];
                if (item == null || !seen.Add(item.GetInstanceID()))
                    continue;
                Flights.Remove(item.GetInstanceID());
                item.DestroyMe();
            }
        }

        static bool IsYoyo(Item? item)
        {
            return IsWeapon(item, LadybugYoyoItem.ItemId)
                || IsWeapon(item, TrompoItem.ItemId)
                || IsWeapon(item, DogBallItem.ItemId)
                || IsWeapon(item, UproarItem.ItemId);
        }

        static bool IsReturningName(string? name)
        {
            return name == LadybugYoyoItem.ItemId || name == TrompoItem.ItemId || name == DogBallItem.ItemId || name == UproarItem.ItemId;
        }

        static bool Harmless(Item? item)
        {
            return IsWeapon(item, DogBallItem.ItemId) || IsWeapon(item, UproarItem.ItemId);
        }

        static string WeaponId(Item flying)
        {
            if (IsWeapon(flying, TrompoItem.ItemId))
                return TrompoItem.ItemId;
            if (IsWeapon(flying, DogBallItem.ItemId))
                return DogBallItem.ItemId;
            if (IsWeapon(flying, UproarItem.ItemId))
                return UproarItem.ItemId;
            return LadybugYoyoItem.ItemId;
        }

        static bool IsWeapon(Item? item, string itemId)
        {
            if (item == null)
                return false;
            if (item.invItem != null && item.invItem.invItemName == itemId)
                return true;

            string objectName = item.name;
            return objectName == itemId
                || objectName.StartsWith(itemId + "(", System.StringComparison.Ordinal);
        }

        static Agent? HitAgent(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
                return null;

            Agent struck = collision.collider.GetComponent<Agent>();
            if (struck != null)
                return struck;

            Transform parent = collision.collider.transform.parent;
            if (parent == null)
                return null;

            struck = parent.GetComponent<Agent>();
            if (struck != null || parent.parent == null)
                return struck;

            return parent.parent.GetComponent<Agent>();
        }

        [HarmonyPatch(typeof(InvDatabase), nameof(InvDatabase.ThrowWeapon), typeof(bool), typeof(bool), typeof(float), typeof(string))]
        static class ArmThrow
        {
            static bool Prefix(InvDatabase __instance)
            {
                Agent? agent = __instance.agent;
                InvItem? weapon = agent != null && agent.inventory != null ? agent.inventory.equippedWeapon : null;
                if (agent == null || weapon == null || !IsReturningName(weapon.invItemName))
                    return true;

                return !HasFlying(agent);
            }

            static void Postfix(Item __result)
            {
                if (!IsYoyo(__result) || __result.tr == null)
                    return;

                __result.canHitOwner = false;
                if (Harmless(__result))
                    __result.throwDamage = 0;
                IgnoreThrower(__result, __result.thrower != null ? __result.thrower : __result.owner);
                Agent? thrower = __result.thrower != null ? __result.thrower : __result.owner;
                Flights[__result.GetInstanceID()] = new Flight
                {
                    Body = __result,
                    Thrower = thrower,
                    Origin = __result.tr.position,
                    ThrownAt = Time.time,
                };
                if (thrower != null)
                    TakeFromHand(thrower, __result);
            }
        }

        static void Track(Item item)
        {
            if (!IsYoyo(item) || item.tr == null || (item.go != null && !item.go.activeInHierarchy))
                return;

            int id = item.GetInstanceID();
            if (Flights.ContainsKey(id))
                return;

            Agent? who = item.thrower != null ? item.thrower : item.owner;
            if (who == null)
                return;

            Flights[id] = new Flight
            {
                Body = item,
                Thrower = who,
                Origin = item.tr.position,
                ThrownAt = Time.time,
            };
        }

        static bool Pull(Flight flight)
        {
            Item? item = flight.Body;
            if (item == null || item.tr == null)
                return true;

            Agent? thrower = flight.Thrower != null ? flight.Thrower : item.thrower;
            if (thrower == null || thrower.dead || thrower.tr == null)
                return false;

            if (!HoldingForm(thrower))
            {
                item.DestroyMe();
                return true;
            }

            if (flight.Caught)
                return true;

            item.canHitOwner = false;
            IgnoreThrower(item, thrower);
            float traveled = Vector2.Distance(item.tr.position, flight.Origin);
            if (traveled > flight.Farthest)
                flight.Farthest = traveled;
            if (!flight.Returning && (flight.Farthest >= LadybugYoyoItem.AimReach || Time.time >= flight.ThrownAt + 1.36f))
                flight.Returning = true;
            if (!flight.Returning)
            {
                Fly(item, Away(item, flight, thrower), 12f);
                return false;
            }

            Vector3 target = thrower.tr.position;
            float remaining = Vector2.Distance(item.tr.position, target);
            if (remaining <= 1.15f && flight.Returning && flight.Farthest >= 0.8f)
            {
                flight.Caught = true;
                item.DestroyMe();
                Catch(thrower);
                return true;
            }

            Fly(item, (Vector2)target - (Vector2)item.tr.position, 12f);

            return false;
        }

        [HarmonyPatch(typeof(Item), "OnCollisionEnter2D")]
        static class Bounce
        {
            static bool Prefix(Item __instance, Collision2D other)
            {
                if (!IsYoyo(__instance))
                    return true;

                Agent? struck = HitAgent(other);
                if (struck == null)
                    return true;
                if (struck == __instance.thrower || struck == __instance.owner)
                    return false;
                if (!Harmless(__instance))
                    return true;

                IgnoreThrower(__instance, struck);
                Agent? thrower = __instance.thrower != null ? __instance.thrower : __instance.owner;
                if (IsWeapon(__instance, UproarItem.ItemId))
                    MonkeyEffect.Tag(thrower, struck);
                else
                    DogEffect.Tag(thrower, struck);
                return false;
            }

            static void Postfix(Item __instance, Collision2D other)
            {
                if (!IsYoyo(__instance))
                    return;

                int id = __instance.GetInstanceID();
                if (!Flights.TryGetValue(id, out Flight flight))
                    return;

                Agent? struck = HitAgent(other);
                if (struck != null && (struck == flight.Thrower || struck == __instance.thrower || struck == __instance.owner))
                    return;
                if (Time.time < flight.ThrownAt + 0.2f || flight.Farthest < 0.8f)
                    return;

                flight.Returning = true;
                __instance.canHitOwner = false;
            }
        }

        static Vector2 Away(Item item, Flight flight, Agent thrower)
        {
            Vector2 away = (Vector2)item.tr.position - (Vector2)flight.Origin;
            if (away.sqrMagnitude > 0.01f)
                return away;
            if (item.rb != null && item.rb.velocity.sqrMagnitude > 0.01f)
                return item.rb.velocity;
            return thrower.tr != null ? (Vector2)thrower.tr.up : Vector2.up;
        }

        static void Fly(Item item, Vector2 direction, float speed)
        {
            if (item.tr == null || direction.sqrMagnitude < 0.0001f)
                return;

            Vector2 step = direction.normalized * (speed * Time.deltaTime);
            item.tr.position += (Vector3)step;
            item.beingThrown = true;
            if (item.rb == null)
                return;

            item.rb.isKinematic = false;
            item.rb.velocity = direction.normalized * speed;
        }

        static void IgnoreThrower(Item item, Agent? thrower)
        {
            Collider2D yoyo = item.itemCollider != null ? item.itemCollider : item.myCollider2D;
            if (yoyo == null || thrower == null)
                return;

            Collider2D[] body = thrower.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < body.Length; i++)
            {
                if (body[i] != null)
                    Physics2D.IgnoreCollision(yoyo, body[i], true);
            }
        }

        static bool HasFlying(Agent agent)
        {
            foreach (KeyValuePair<int, Flight> pair in Flights)
            {
                Flight flight = pair.Value;
                if (flight.Body == null)
                    continue;
                if (flight.Thrower == agent || flight.Body.thrower == agent || flight.Body.owner == agent)
                    return true;
            }

            return false;
        }

        static void TakeFromHand(Agent thrower, Item flying)
        {
            InvDatabase inventory = thrower.inventory;
            if (inventory == null)
                return;

            string itemId = WeaponId(flying);
            InvItem held = inventory.FindItem(itemId);
            if (held == null || held == flying.invItem)
                return;

            held.cantDrop = false;
            inventory.DestroyItem(held, true);
        }

        static bool HoldingForm(Agent thrower)
        {
            return LadybugEffect.Find(thrower) != null || BeeEffect.Find(thrower) != null || DogEffect.Find(thrower) != null || MonkeyEffect.Find(thrower) != null;
        }

        static string? FormWeapon(Agent thrower)
        {
            if (BeeEffect.Find(thrower) != null)
                return TrompoItem.ItemId;
            if (DogEffect.Find(thrower) != null)
                return DogBallItem.ItemId;
            if (MonkeyEffect.Find(thrower) != null)
                return UproarItem.ItemId;
            if (LadybugEffect.Find(thrower) != null)
                return LadybugYoyoItem.ItemId;
            return null;
        }

        static bool Catch(Agent thrower)
        {
            if (thrower.inventory == null)
                return false;

            string? itemId = FormWeapon(thrower);
            if (itemId == null)
                return false;

            InvItem weapon = thrower.inventory.FindItem(itemId);
            if (weapon == null)
                weapon = thrower.inventory.AddItem(itemId, 1);
            if (weapon == null)
                return false;

            weapon.throwDamage = itemId == DogBallItem.ItemId || itemId == UproarItem.ItemId ? 0 : CatNoirPlugin.YoYoDamage.Value;
            weapon.throwDistance = LadybugYoyoItem.AimReach;
            weapon.invItemCount = 1;
            thrower.inventory.EquipWeapon(weapon, true);
            return true;
        }
    }
}
