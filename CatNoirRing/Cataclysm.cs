using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    public enum StrikeResult
    {
        Miss,
        Spent,
        Pending,
    }

    public static class Cataclysm
    {
        public static StrikeResult TryStrike(Agent agent)
        {
            if (agent.melee == null || agent.tr == null)
                return StrikeResult.Miss;

            GameController gc = GameController.gameController;
            if (gc == null || gc.tileInfo == null)
                return StrikeResult.Miss;

            Vector2 aim = Facing(agent);
            gc.audioHandler.Play(agent, "SwingWeaponLarge");
            Agent? creature = FindCreature(gc, agent, aim);
            if (creature != null)
                return StrikeCreature(agent, creature) ? StrikeResult.Spent : StrikeResult.Miss;

            return BreakWall(gc, agent, aim) ? StrikeResult.Spent : StrikeResult.Miss;
        }

        static bool StrikeCreature(Agent agent, Agent creature)
        {
            float damage = Mathf.Max(1, CatNoirPlugin.CataclysmDamage.Value);
            creature.justHitByAgent = agent;
            creature.justHitByAgent2 = agent;
            creature.lastHitByAgent = agent;
            if (creature.statusEffects != null)
                creature.statusEffects.ChangeHealth(-damage, agent);
            if (creature.dead || creature.health <= 0f)
                creature.killedByAgent = agent;
            StrikeAlarm.Raise(agent, creature, 10.8f, StrikeAlarm.Loud);
            GameController gc = GameController.gameController;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(creature, "MeleeHitAgentLarge");
            if (gc != null && agent.tr != null)
                gc.ScreenShake(0.45f, 220f, agent.tr.position, agent);
            return true;
        }

        public static Agent? AimedCreature(Agent agent)
        {
            GameController gc = GameController.gameController;
            if (gc == null || agent.tr == null)
                return null;
            return FindCreature(gc, agent, Facing(agent));
        }

        static Agent? FindCreature(GameController gc, Agent agent, Vector2 aim)
        {
            List<Agent>? agents = gc.agentList;
            if (agents == null)
                return null;

            float tile = TileSize();
            float reach = tile * 2.4f;
            Agent? best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < agents.Count; i++)
            {
                Agent other = agents[i];
                if (other == null || other == agent || other.dead || other.tr == null || MouseEffect.IsClone(other))
                    continue;

                Vector2 delta = (Vector2)other.tr.position - (Vector2)agent.tr.position;
                float distance = delta.magnitude;
                if (distance < 0.05f || distance > reach || distance >= bestDistance)
                    continue;
                if (Vector2.Dot(delta / distance, aim) < 0.2f)
                    continue;

                best = other;
                bestDistance = distance;
            }

            return best;
        }

        public static Vector2 Facing(Agent agent)
        {
            return AimOf(agent);
        }

        public static bool StopsClout(Agent cause, Vector3 point)
        {
            GameController gc = GameController.gameController;
            if (gc == null || gc.tileInfo == null)
                return false;

            TileData tileData = gc.tileInfo.GetTileData(point);
            if (tileData == null)
                return true;
            if (tileData.destroyedWall || tileData.wallMaterial == wallMaterialType.None)
                return false;
            if (tileData.wallMaterial == wallMaterialType.Border || !IsCataclysmWall(tileData.wallMaterial))
                return true;

            int key = tileData.posX * 46337 + tileData.posY;
            if (!openedThisStep.Add(key))
                return false;

            SmashTile(gc, cause, point, tileData, false);
            return false;
        }

        public static void BeginCloutStep()
        {
            openedThisStep.Clear();
        }

        static readonly HashSet<int> openedThisStep = new HashSet<int>();

        static bool BreakWall(GameController gc, Agent agent, Vector2 aim)
        {
            float tile = TileSize();
            Vector3 origin = agent.tr.position;
            float[] steps = { 0.35f, 0.75f, 1.15f, 1.6f, 2.05f };
            for (int i = 0; i < steps.Length; i++)
            {
                Vector3 point = origin + (Vector3)(aim * (tile * steps[i]));
                TileData tileData = gc.tileInfo.GetTileData(point);
                if (tileData == null || tileData.destroyedWall || !IsCataclysmWall(tileData.wallMaterial))
                    continue;

                SmashTile(gc, agent, point, tileData, true);
                return true;
            }

            return false;
        }

        static void SmashTile(GameController gc, Agent agent, Vector3 point, TileData tileData, bool shake)
        {
            float tile = TileSize();
            BoxCollider2D? face = tileData.wallCollider;
            BoxCollider2D? side = tileData.wallColliderUnreachable;
            BoxCollider2D? nearby = face == null && side == null ? NearbyWallCollider(gc, point, tile) : null;
            wallMaterialType material = tileData.wallMaterial;
            gc.tileInfo.DestroyWallTileAtPosition(
                point.x,
                point.y,
                Vector3.zero,
                true,
                agent,
                false,
                true,
                false,
                agent,
                false);
            OpenCollider(face);
            OpenCollider(side);
            OpenCollider(nearby);
            if (gc.audioHandler != null)
                gc.audioHandler.Play(agent, material == wallMaterialType.Glass ? "WallDestroyGlass" : "WallDestroy");
            if (shake && agent.tr != null)
                gc.ScreenShake(0.25f, 160f, agent.tr.position, agent);
        }

        static BoxCollider2D? NearbyWallCollider(GameController gc, Vector3 point, float tile)
        {
            float limit = tile * 0.8f;
            float bestDistance = limit * limit;
            BoxCollider2D? best = null;
            Vector3[] offsets =
            {
                new Vector3(tile, 0f, 0f),
                new Vector3(-tile, 0f, 0f),
                new Vector3(0f, tile, 0f),
                new Vector3(0f, -tile, 0f),
            };
            for (int i = 0; i < offsets.Length; i++)
            {
                TileData neighbor = gc.tileInfo.GetTileData(point + offsets[i]);
                if (neighbor == null)
                    continue;
                best = CloserCollider(neighbor.wallCollider, point, best, ref bestDistance);
                best = CloserCollider(neighbor.wallColliderUnreachable, point, best, ref bestDistance);
            }

            return best;
        }

        static BoxCollider2D? CloserCollider(BoxCollider2D? collider, Vector3 point, BoxCollider2D? best, ref float bestDistance)
        {
            if (collider == null)
                return best;

            float distance = ((Vector2)collider.bounds.ClosestPoint(point) - (Vector2)point).sqrMagnitude;
            if (distance >= bestDistance)
                return best;

            bestDistance = distance;
            return collider;
        }

        static void OpenCollider(BoxCollider2D? collider)
        {
            if (collider == null)
                return;

            GameObject body = collider.gameObject;
            if (body == null)
                return;

            Component[] parts = body.GetComponents<Component>();
            for (int i = 0; i < parts.Length; i++)
            {
                Component part = parts[i];
                if (part == null || part.GetType().Name != "GraphUpdateScene")
                    continue;

                System.Reflection.MethodInfo apply = part.GetType().GetMethod("Apply");
                if (apply != null)
                    apply.Invoke(part, null);
            }

            collider.enabled = false;
            body.SetActive(false);
        }

        static bool IsCataclysmWall(wallMaterialType material)
        {
            switch (material)
            {
                case wallMaterialType.Normal:
                case wallMaterialType.Wood:
                case wallMaterialType.Glass:
                case wallMaterialType.Steel:
                case wallMaterialType.Border:
                case wallMaterialType.Door:
                case wallMaterialType.DoorOpen:
                case wallMaterialType.Window:
                case wallMaterialType.Bars:
                case wallMaterialType.BarbedWire:
                case wallMaterialType.LockdownWall:
                    return true;
                default:
                    return false;
            }
        }

        static Vector2 AimOf(Agent agent)
        {
            string direction = string.IsNullOrEmpty(agent.playerDir) ? agent.previousPlayerDir : agent.playerDir;
            switch (direction)
            {
                case "E": return Vector2.right;
                case "W": return Vector2.left;
                case "N": return Vector2.up;
                case "S": return Vector2.down;
                case "NE": return new Vector2(1f, 1f).normalized;
                case "NW": return new Vector2(-1f, 1f).normalized;
                case "SE": return new Vector2(1f, -1f).normalized;
                case "SW": return new Vector2(-1f, -1f).normalized;
            }

            float rad = agent.tr.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 angled = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            return angled.sqrMagnitude > 0.01f ? angled.normalized : Vector2.up;
        }

        static float TileSize()
        {
            return Consts.tileSize > 0.01f ? Consts.tileSize : 0.64f;
        }
    }

        static class StaffCombatPatches
        {
        static bool DisguisedMelee(InvItem weapon)
        {
            return SteelStaffItem.Live.Contains(weapon) || JumpRopeItem.Live.Contains(weapon);
        }

        [HarmonyPatch(typeof(Melee), nameof(Melee.Attack), typeof(bool))]
        static class SwingLikeBaton
        {
            static void Prefix(Melee __instance, ref string __state)
            {
                InvItem? weapon = __instance.agent != null && __instance.agent.inventory != null
                    ? __instance.agent.inventory.equippedWeapon
                    : null;
                if (weapon == null || weapon.invItemName != SteelStaffItem.ItemId)
                    return;

                __state = weapon.invItemName;
                weapon.invItemName = "PoliceBaton";
            }

            static void Postfix(Melee __instance, string __state)
            {
                if (__state == null || __instance.agent == null || __instance.agent.inventory == null)
                    return;

                InvItem? weapon = __instance.agent.inventory.equippedWeapon;
                if (weapon == null || weapon.invItemName != "PoliceBaton" || DisguisedMelee(weapon))
                    return;

                if (__instance.attackAnimPlaying)
                    BatonSwing.Hold(weapon, __state);
                else
                    weapon.invItemName = __state;
            }
        }

    [HarmonyPatch(typeof(Melee), nameof(Melee.MeleeLateUpdate))]
    static class BatonSwing
    {
        static InvItem? weapon;
        static string? originalName;

        public static void Hold(InvItem item, string name)
        {
            weapon = item;
            originalName = name;
        }

        public static bool IsHeld(InvItem? item)
        {
            return item != null && item == weapon;
        }

        static void Postfix(Melee __instance)
        {
            if (weapon == null || originalName == null)
                return;
            if (weapon.agent == null || weapon.agent.melee != __instance)
                return;
            if (__instance.attackAnimPlaying)
                return;

            weapon.invItemName = originalName;
            weapon = null;
            originalName = null;
        }
    }

        [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.GetSpriteIdByName), typeof(string))]
        static class StaffSprite
        {
            static void Prefix(ref string name)
            {
                if (name == SteelStaffItem.ItemId || name == JumpRopeItem.ItemId)
                    name = SteelStaffItem.SwingSprite;
            }
        }

        [HarmonyPatch(typeof(InvDatabase), nameof(InvDatabase.DepleteMelee), typeof(int))]
        static class KeepStaffCharges
        {
            static bool Prefix(InvDatabase __instance)
            {
                InvItem? weapon = __instance.equippedWeapon;
                return weapon == null || !DisguisedMelee(weapon);
            }
        }

        [HarmonyPatch(typeof(InvDatabase), nameof(InvDatabase.DepleteMelee), typeof(int), typeof(InvItem))]
        static class KeepStaffItemCharges
        {
            static bool Prefix(InvItem myMelee)
            {
                return myMelee == null || !DisguisedMelee(myMelee);
            }
        }
    }
}

