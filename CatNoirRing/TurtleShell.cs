using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class TurtleShell
    {
        public const float Radius = 1.7f;

        const float Step = 0.08f;
        static readonly Color FieldColor = new Color(0.55f, 1f, 0.62f, 0.9f);
        static readonly HashSet<int> echoed = new HashSet<int>();
        static GameObject? bubble;
        static tk2dSprite? field;
        static Texture2D? ring;
        static bool bubbleWarned;
        static bool sending;
        static float nextSound;
        static Vector2 anchor;
        static int anchorId;
        static bool haveAnchor;

        public static bool IsUp(Agent? agent)
        {
            if (agent == null)
                return false;

            TurtleEffect? form = TurtleEffect.Find(agent);
            return form != null && form.ShellUp;
        }

        public static void Show(Agent owner)
        {
            if (owner == null || owner.tr == null)
                return;

            EnsureBubble(owner);
            if (bubble == null)
                return;

            Follow(owner);
            bubble.SetActive(true);
        }

        public static void Hide(Agent owner)
        {
            if (bubble != null)
                bubble.SetActive(false);
        }

        public static void Tick(GameController? gc)
        {
            PruneEchoes(gc);
            if (gc == null || gc.playerAgent == null)
            {
                if (bubble != null)
                    bubble.SetActive(false);
                haveAnchor = false;
                return;
            }

            Agent owner = gc.playerAgent;
            if (!IsUp(owner) || owner.tr == null)
            {
                if (bubble != null)
                    bubble.SetActive(false);
                haveAnchor = false;
                return;
            }

            bool tracked = haveAnchor && anchorId == owner.GetInstanceID();
            Vector2 before = tracked ? anchor : (Vector2)owner.tr.position;
            Show(owner);
            KeepAway(owner, before, tracked);
            Follow(owner);
            TurnItems(owner, gc);
            anchor = owner.tr.position;
            anchorId = owner.GetInstanceID();
            haveAnchor = true;
        }

        public static void BounceBullet(Bullet bullet)
        {
            if (bullet == null || bullet.destroying || bullet.fakeDestroyed || bullet.tr == null)
                return;

            Agent? owner = Near(bullet.tr.position);
            if (owner == null || owner.tr == null)
                return;

            Vector2 center = owner.tr.position;
            Vector2 pos = bullet.tr.position;
            Vector2 offset = pos - center;
            Vector2 heading = Facing(bullet);
            if (offset.sqrMagnitude < 0.0025f)
            {
                if (bullet.agent == owner)
                    return;
                offset = heading.sqrMagnitude > 0.01f ? -heading : Vector2.up;
            }

            float dist = offset.magnitude;
            Vector2 normal = offset / dist;
            float step = Mathf.Max(0.05f, bullet.speed * Time.deltaTime);
            Vector2 next = pos + heading * step;
            bool incoming = Vector2.Dot(heading, normal) < -0.05f;
            bool inside = dist <= Radius && incoming;
            bool crossing = dist > Radius && (next - center).sqrMagnitude <= Radius * Radius && incoming;
            if (!inside && !crossing)
                return;
            if (crossing && owner.movement != null && !owner.movement.HasLOSPosition360FourDirection(bullet.tr.position))
                return;
            if (bullet.agent == owner && dist < Radius * 0.55f)
                return;

            Vector2 bounce = Vector2.Reflect(heading, normal);
            if (Vector2.Dot(bounce, normal) < 0.2f)
                bounce = normal;
            if (bounce.sqrMagnitude < 0.01f)
                bounce = normal;
            bounce.Normalize();

            Vector2 parked = center + normal * (Radius + 0.2f);
            Vector3 world = new Vector3(parked.x, parked.y, bullet.tr.position.z);
            bullet.tr.position = world;
            float angle = Mathf.Atan2(-bounce.x, bounce.y) * Mathf.Rad2Deg;
            bullet.tr.eulerAngles = new Vector3(0f, 0f, angle);
            if (bullet.rb != null)
            {
                bullet.rb.position = parked;
                bullet.rb.rotation = angle;
            }

            bullet.particleStoppingPoint = world + (Vector3)(bounce * 14f);
            bullet.agent = owner;
            bullet.explosionOwner = owner;
            Play(owner, "BulletHitWall");
        }

        static void KeepAway(Agent owner, Vector2 before, bool tracked)
        {
            GameController gc = owner.gc;
            if (gc == null || gc.agentList == null || owner.tr == null)
                return;

            Vector2 center = owner.tr.position;
            if (tracked && (center - before).sqrMagnitude <= 1.25f * 1.25f && Presses(owner, gc, center, before))
            {
                StopHero(owner, before);
                center = owner.tr.position;
            }

            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (!Fighter(owner, other) || other.tr == null)
                    continue;

                Vector2 pos = other.tr.position;
                Vector2 offset = pos - center;
                float dist = offset.magnitude;
                if (dist >= Radius || dist < 0.001f)
                    continue;
                if (Separated(owner, other, dist))
                    continue;

                Vector2 normal = offset / dist;
                float push = Mathf.Min(Radius - dist + 0.08f, 0.4f);
                Vector2 landed = Farthest(gc, other, pos, normal, push);
                bool room = (landed - pos).sqrMagnitude >= 0.0001f
                    && !Blocked(gc, other, landed + normal * Step, BodyRadius(other));
                Shove(other, landed, normal, room);
            }
        }

        static bool Presses(Agent owner, GameController gc, Vector2 center, Vector2 before)
        {
            if ((center - before).sqrMagnitude < 0.000001f || gc.agentList == null)
                return false;

            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (!Fighter(owner, other) || other.tr == null)
                    continue;

                Vector2 pos = other.tr.position;
                float dist = (pos - center).magnitude;
                float was = (pos - before).magnitude;
                if (dist >= Radius || dist >= was - 0.0005f)
                    continue;
                if (Separated(owner, other, dist))
                    continue;
                if (dist < 0.001f)
                    return true;

                Vector2 normal = (pos - center) / dist;
                float push = Mathf.Min(Radius - dist + 0.08f, 0.4f);
                Vector2 landed = Farthest(gc, other, pos, normal, push);
                if ((landed - pos).sqrMagnitude < 0.0004f)
                    return true;
            }

            return false;
        }

        static bool Fighter(Agent owner, Agent other)
        {
            if (other == null || other == owner || other.tr == null)
                return false;
            if (other.dead || other.sleepKnockedOut || other.disappeared || other.ghost)
                return false;
            if (other.opponent == owner || owner.opponent == other)
                return true;
            if (!other.inCombat || other.relationships == null)
                return false;
            if (other.opponent != null)
                return false;

            Relationship rel = other.relationships.GetRelationship(owner);
            return rel != null && rel.relTypeCode == relStatus.Hostile;
        }

        static bool Separated(Agent owner, Agent other, float dist)
        {
            return dist > 0.75f && owner.movement != null && !owner.movement.HasLOSPosition360FourDirection(other.tr.position);
        }

        static Vector2 Farthest(GameController gc, Agent body, Vector2 from, Vector2 normal, float distance)
        {
            float radius = BodyRadius(body);
            Vector2 best = from;
            float walked = 0f;
            while (walked + 0.001f < distance)
            {
                float next = walked + Step;
                if (next > distance)
                    next = distance;
                Vector2 at = from + normal * next;
                if (Blocked(gc, body, at, radius))
                    break;

                best = at;
                walked = next;
            }

            return best;
        }

        static bool Blocked(GameController gc, Agent body, Vector2 at, float radius)
        {
            if (HitsWall(gc, at, radius, out bool door) || (door && ShutDoor(gc, at, radius)))
                return true;

            Movement movement = body.movement;
            if (movement == null)
                return false;

            return !movement.HasLOSPosition360FourDirection(new Vector3(at.x, at.y, body.tr.position.z));
        }

        static bool HitsWall(GameController gc, Vector2 center, float radius, out bool door)
        {
            door = false;
            TileInfo tiles = gc.tileInfo;
            if (tiles == null)
                return false;

            bool wall = false;
            if (WallPoint(tiles, center, ref door))
                wall = true;

            const int slices = 8;
            for (int i = 0; i < slices; i++)
            {
                float ang = i * (Mathf.PI * 2f / slices);
                Vector2 rim = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
                if (WallPoint(tiles, rim, ref door))
                    wall = true;
            }

            float probe = radius + 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 side = center;
                if (i == 0)
                    side.x += probe;
                else if (i == 1)
                    side.x -= probe;
                else if (i == 2)
                    side.y += probe;
                else
                    side.y -= probe;

                TileData data = tiles.GetTileData(side);
                if (data == null)
                    continue;

                NoteDoor(data, ref door);
                if (data.wallMaterial == wallMaterialType.Door || data.wallMaterial == wallMaterialType.DoorOpen)
                    continue;
                if (Blocks(data) && ColliderOverlaps(data, center, radius))
                    wall = true;
            }

            return wall;
        }

        static bool WallPoint(TileInfo tiles, Vector2 point, ref bool door)
        {
            TileData data = tiles.GetTileData(point);
            if (data == null)
                return false;

            NoteDoor(data, ref door);
            return Blocks(data);
        }

        static void NoteDoor(TileData data, ref bool door)
        {
            if (data.wallMaterial == wallMaterialType.Door || data.wallMaterial == wallMaterialType.DoorOpen)
                door = true;
        }

        static bool Blocks(TileData data)
        {
            if (data.destroyedWall)
                return false;

            wallMaterialType material = data.wallMaterial;
            if (material == wallMaterialType.None || material == wallMaterialType.DoorOpen)
                return false;
            if (material == wallMaterialType.Window && data.destroyedWindow)
                return false;
            return true;
        }

        static bool ColliderOverlaps(TileData data, Vector2 center, float radius)
        {
            return BoxOverlaps(data.wallCollider, center, radius) || BoxOverlaps(data.wallColliderUnreachable, center, radius);
        }

        static bool BoxOverlaps(BoxCollider2D box, Vector2 center, float radius)
        {
            if (box == null || !box.enabled || !box.gameObject.activeInHierarchy)
                return false;

            return CircleHits(box, center, radius);
        }

        // Open door does not use wallCollider. A closed door's own BoxCollider2D is enabled.
        static bool ShutDoor(GameController gc, Vector2 center, float radius)
        {
            List<ObjectReal> list = gc.objectRealListActive;
            if (list == null || list.Count == 0)
                list = gc.objectRealList;
            if (list == null)
                return false;

            float reach = radius + 0.9f;
            float reachSq = reach * reach;
            for (int i = 0; i < list.Count; i++)
            {
                ObjectReal real = list[i];
                if (real == null || !real.isDoor || real.destroyed || real.tr == null)
                    continue;

                if (((Vector2)real.tr.position - center).sqrMagnitude > reachSq)
                    continue;

                BoxCollider2D box = real.objectCollider;
                if (box == null || !box.enabled || !box.gameObject.activeInHierarchy)
                    continue;
                if (CircleHits(box, center, radius))
                    return true;
            }

            return false;
        }

        static bool CircleHits(BoxCollider2D box, Vector2 center, float radius)
        {
            Bounds bounds = box.bounds;
            float nearestX = Mathf.Clamp(center.x, bounds.min.x, bounds.max.x);
            float nearestY = Mathf.Clamp(center.y, bounds.min.y, bounds.max.y);
            float dx = center.x - nearestX;
            float dy = center.y - nearestY;
            return dx * dx + dy * dy <= radius * radius;
        }

        static float BodyRadius(Agent agent)
        {
            CircleCollider2D circle = agent.agentCollider;
            if (circle == null)
                return 0.22f;

            Vector3 scale = circle.transform.lossyScale;
            float span = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            if (span < 0.01f)
                span = 1f;
            return Mathf.Clamp(circle.radius * span, 0.12f, 0.45f);
        }

        static void Shove(Agent other, Vector2 next, Vector2 normal, bool room)
        {
            Rigidbody2D body = Mover(other);
            Vector2 velocity = body != null ? body.velocity : Vector2.zero;
            float inward = Vector2.Dot(velocity, -normal);
            if (inward > 0f)
                velocity += normal * inward;
            if (!room)
            {
                float outward = Vector2.Dot(velocity, normal);
                if (outward > 0f)
                    velocity -= normal * outward;
            }

            other.tr.position = new Vector3(next.x, next.y, other.tr.position.z);
            Plant(other.rigidBody2D, next, velocity);
            if (body != null && body != other.rigidBody2D)
                Plant(body, next, velocity);
        }

        static void StopHero(Agent owner, Vector2 before)
        {
            Vector2 into = (Vector2)owner.tr.position - before;
            owner.tr.position = new Vector3(before.x, before.y, owner.tr.position.z);
            Rigidbody2D body = Mover(owner);
            Hold(owner.rigidBody2D, before, into);
            if (body != null && body != owner.rigidBody2D)
                Hold(body, before, into);
        }

        static Rigidbody2D Mover(Agent agent)
        {
            if (agent.movement != null)
            {
                Rigidbody2D found = agent.movement.GetComponent<Rigidbody2D>();
                if (found != null)
                    return found;
            }

            return agent.rigidBody2D;
        }

        static void Plant(Rigidbody2D body, Vector2 next, Vector2 velocity)
        {
            if (body == null)
                return;

            body.position = next;
            body.velocity = velocity;
        }

        static void Hold(Rigidbody2D body, Vector2 position, Vector2 into)
        {
            if (body == null)
                return;

            body.position = position;
            if (into.sqrMagnitude < 0.000001f)
                return;

            Vector2 dir = into.normalized;
            Vector2 velocity = body.velocity;
            float forward = Vector2.Dot(velocity, dir);
            if (forward > 0f)
                velocity -= dir * forward;
            body.velocity = velocity;
        }

        static void TurnItems(Agent owner, GameController gc)
        {
            if (gc.itemList == null || owner.tr == null)
                return;

            for (int i = 0; i < gc.itemList.Count; i++)
                DeflectItem(gc.itemList[i], owner, false);
        }

        static void DeflectItem(Item item, Agent owner, bool hitBody)
        {
            if (item == null || !item.beingThrown || item.tr == null || owner.tr == null)
                return;

            Vector2 center = owner.tr.position;
            Vector2 offset = (Vector2)item.tr.position - center;
            if (offset.sqrMagnitude < 0.0025f)
                offset = Vector2.up;
            float dist = offset.magnitude;
            if (!hitBody && dist > Radius)
                return;
            if (!hitBody && dist > 0.75f && owner.movement != null && !owner.movement.HasLOSPosition360FourDirection(item.tr.position))
                return;

            Vector2 normal = offset / dist;
            Vector2 velocity = item.rb != null ? item.rb.velocity : Vector2.zero;
            bool own = item.thrower == owner || item.owner == owner;
            if (own && dist < Radius * 0.55f && !hitBody)
                return;
            if (!hitBody && velocity.sqrMagnitude > 0.04f && Vector2.Dot(velocity, normal) >= -0.05f)
                return;

            Vector2 bounce = velocity.sqrMagnitude > 0.25f ? Vector2.Reflect(velocity, normal) : normal * 9f;
            if (Vector2.Dot(bounce, normal) < 0.35f)
                bounce = normal * Mathf.Max(9f, velocity.magnitude);
            Vector2 parked = center + normal * (Radius + 0.28f);
            item.tr.position = new Vector3(parked.x, parked.y, item.tr.position.z);
            if (item.rb != null)
            {
                item.rb.position = parked;
                item.rb.velocity = bounce;
            }

            if (!own)
            {
                item.thrower = owner;
                item.owner = owner;
                item.hitCauser = owner;
            }

            item.canHitOwner = false;
            item.beingThrown = true;
            Play(owner, "BulletHitWall");
        }

        static void Echo(Explosion boom, Agent owner)
        {
            if (boom == null || owner == null)
                return;
            if (!echoed.Add(boom.GetInstanceID()))
                return;
            Strike(boom.agent, owner, Mathf.Max(1, boom.damage));
        }

        static void Strike(Agent? attacker, Agent owner, float damage)
        {
            if (attacker == null || attacker == owner || attacker.dead || attacker.statusEffects == null)
                return;

            float amount = Mathf.Max(1f, damage);
            sending = true;
            try
            {
                attacker.justHitByAgent = owner;
                attacker.justHitByAgent2 = owner;
                attacker.lastHitByAgent = owner;
                attacker.statusEffects.ChangeHealth(-amount, owner);
                if (attacker.dead || attacker.health <= 0f)
                    attacker.killedByAgent = owner;
            }
            finally
            {
                sending = false;
            }

            Play(owner, "MeleeHitAgentLarge");
        }

        static Agent? Near(Vector2 pos)
        {
            GameController gc = GameController.gameController;
            Agent owner = gc != null ? gc.playerAgent : null!;
            if (!IsUp(owner) || owner.tr == null)
                return null;
            float reach = Radius + 1.6f;
            if (((Vector2)owner.tr.position - pos).sqrMagnitude > reach * reach)
                return null;
            return owner;
        }

        static Vector2 Facing(Bullet bullet)
        {
            float z = bullet.tr.eulerAngles.z * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Sin(z), Mathf.Cos(z));
        }

        static Agent? AgentOf(GameObject? body)
        {
            if (body == null)
                return null;

            Agent agent = body.GetComponent<Agent>();
            if (agent != null)
                return agent;

            ObjectSprite sprite = body.GetComponent<ObjectSprite>();
            if (sprite != null && sprite.agent != null)
                return sprite.agent;

            AgentColliderBox box = body.GetComponent<AgentColliderBox>();
            if (box != null && box.objectSprite != null)
                return box.objectSprite.agent;

            if (body.transform.parent == null)
                return null;
            return body.transform.parent.GetComponent<Agent>();
        }

        static void Play(Agent agent, string clip)
        {
            if (agent == null || Time.time < nextSound)
                return;
            GameController gc = agent.gc;
            if (gc == null || gc.audioHandler == null)
                return;
            nextSound = Time.time + 0.12f;
            gc.audioHandler.Play(agent, clip);
        }

        static void PruneEchoes(GameController? gc)
        {
            if (echoed.Count == 0 || gc == null || gc.explosionList == null)
                return;

            HashSet<int> live = new HashSet<int>();
            for (int i = 0; i < gc.explosionList.Count; i++)
            {
                Explosion boom = gc.explosionList[i];
                if (boom != null)
                    live.Add(boom.GetInstanceID());
            }

            echoed.RemoveWhere(id => !live.Contains(id));
        }

        static void EnsureBubble(Agent owner)
        {
            if (bubble != null || owner == null)
                return;

            AgentHitbox hitbox = owner.agentHitboxScript;
            tk2dSprite? shadow = hitbox != null ? hitbox.shadow : null;
            if (shadow == null)
            {
                NoteBubble("Силовое поле черепахи не нашло тень персонажа.");
                return;
            }

            GameObject created = Object.Instantiate(shadow.gameObject);
            created.name = "TurtleShell";
            Animator[] animators = created.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
                Object.Destroy(animators[i]);

            created.transform.SetParent(null, false);
            tk2dSprite sprite = created.GetComponent<tk2dSprite>();
            MeshFilter filter = created.GetComponent<MeshFilter>();
            Renderer renderer = created.GetComponent<Renderer>();
            if (sprite == null || filter == null || renderer == null)
            {
                Object.Destroy(created);
                NoteBubble("Силовое поле черепахи не собралось.");
                return;
            }

            sprite.enabled = false;
            filter.mesh = Quad();
            Shader draw = Shader.Find("Sprites/Default");
            if (draw == null && renderer.sharedMaterial != null)
                draw = renderer.sharedMaterial.shader;
            if (draw != null)
            {
                Material material = new Material(draw);
                material.mainTexture = RingTexture();
                material.color = Color.white;
                renderer.material = material;
            }

            created.transform.localScale = new Vector3(Radius, Radius, 1f);
            bubble = created;
            field = sprite;
        }

        static void NoteBubble(string message)
        {
            if (bubbleWarned)
                return;

            bubbleWarned = true;
            Debug.LogWarning(message);
        }

        static void Follow(Agent owner)
        {
            if (bubble == null || field == null || owner.tr == null)
                return;

            Vector3 pos = owner.tr.position;
            ObjectSprite sprite = owner.objectSprite;
            if (sprite != null && sprite.sprTr != null)
                pos.z = sprite.sprTr.position.z;
            bubble.transform.SetParent(null, false);
            bubble.transform.position = pos;
            bubble.transform.rotation = Quaternion.identity;
            bubble.transform.localScale = new Vector3(Radius, Radius, 1f);

            Renderer mine = field.GetComponent<Renderer>();
            if (mine != null)
            {
                mine.enabled = true;
                Shader draw = Shader.Find("Sprites/Default");
                if (draw != null && (mine.sharedMaterial == null || mine.sharedMaterial.shader != draw))
                    mine.material.shader = draw;
                if (sprite != null && sprite.objectRenderer != null)
                {
                    mine.sortingLayerID = sprite.objectRenderer.sortingLayerID;
                    mine.sortingOrder = sprite.objectRenderer.sortingOrder + 8;
                    field.SortingOrder = sprite.objectRenderer.sortingOrder + 8;
                }
            }
        }

        static Texture2D RingTexture()
        {
            if (ring != null)
                return ring;

            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "TurtleShellField";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float outer = center - 1.5f;
            float inner = outer - 11f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > outer)
                        continue;
                    pixels[y * size + x] = dist >= inner
                        ? new Color32(150, 255, 170, 230)
                        : new Color32(70, 190, 100, 48);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            ring = texture;
            return ring;
        }

        static Mesh Quad()
        {
            Mesh mesh = new Mesh();
            mesh.name = "TurtleShellQuad";
            mesh.vertices = new[]
            {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f, 1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(1f, -1f, 0f),
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        [HarmonyPatch(typeof(Bullet), "travelForward")]
        static class Flight
        {
            static void Prefix(Bullet __instance)
            {
                BounceBullet(__instance);
            }
        }

        [HarmonyPatch(typeof(BulletHitbox), "HitObject")]
        static class Shot
        {
            static bool Prefix(BulletHitbox __instance, GameObject hitObject)
            {
                Bullet bullet = __instance.myBullet;
                Agent? victim = AgentOf(hitObject);
                if (bullet == null || !IsUp(victim))
                    return true;

                BounceBullet(bullet);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeleeHitbox), "HitObject")]
        static class Swing
        {
            static bool Prefix(MeleeHitbox __instance, GameObject hitObject)
            {
                Agent? victim = AgentOf(hitObject);
                if (!IsUp(victim))
                    return true;

                Melee melee = __instance.myMelee;
                Agent? attacker = melee != null ? melee.agent : null;
                if (attacker != null && attacker != victim && attacker.statusEffects != null && !attacker.dead)
                {
                    int damage = melee!.defaultDamage;
                    InvItem? weapon = attacker.inventory != null ? attacker.inventory.equippedWeapon : null;
                    if (weapon != null && weapon.itemType == "WeaponProjectile" && attacker.inventory!.fist != null)
                        weapon = attacker.inventory.fist;
                    if (weapon != null && weapon.meleeDamage > damage)
                        damage = weapon.meleeDamage;
                    Strike(attacker, victim!, Mathf.Max(1, damage));
                    if (attacker.rigidBody2D != null && attacker.tr != null && victim!.tr != null)
                    {
                        Vector2 away = (Vector2)attacker.tr.position - (Vector2)victim.tr.position;
                        if (away.sqrMagnitude < 0.01f)
                            away = Vector2.up;
                        attacker.rigidBody2D.velocity = away.normalized * 7f;
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Explosion), "OnTriggerEnter2D")]
        static class Blast
        {
            static bool Prefix(Explosion __instance, Collider2D other)
            {
                Agent? victim = other != null ? AgentOf(other.gameObject) : null;
                if (!IsUp(victim))
                    return true;

                Echo(__instance, victim!);
                return false;
            }
        }

        [HarmonyPatch(typeof(Explosion), "ExplosionHit")]
        static class BlastHit
        {
            static bool Prefix(Explosion __instance, GameObject hitObject)
            {
                Agent? victim = AgentOf(hitObject);
                if (!IsUp(victim))
                    return true;

                Echo(__instance, victim!);
                return false;
            }
        }

        [HarmonyPatch(typeof(Fire), "OnTriggerEnter2D")]
        static class Flame
        {
            static bool Prefix(Collider2D other)
            {
                Agent? victim = other != null ? AgentOf(other.gameObject) : null;
                return !IsUp(victim);
            }
        }

        [HarmonyPatch(typeof(Item), "OnCollisionEnter2D")]
        static class Thrown
        {
            static bool Prefix(Item __instance, Collision2D other)
            {
                if (__instance == null || !__instance.beingThrown || other == null)
                    return true;

                Agent? victim = AgentOf(other.collider != null ? other.collider.gameObject : other.gameObject);
                if (!IsUp(victim))
                    return true;

                DeflectItem(__instance, victim!, true);
                return false;
            }
        }

        [HarmonyPatch(typeof(StatusEffects), nameof(StatusEffects.ChangeHealth), typeof(float), typeof(PlayfieldObject), typeof(uint), typeof(float), typeof(string), typeof(byte))]
        static class Absorb
        {
            static bool Prefix(StatusEffects __instance, float healthNum, PlayfieldObject damagerObject)
            {
                if (healthNum >= 0f || __instance == null)
                    return true;

                Agent agent = __instance.agent;
                if (!IsUp(agent))
                    return true;
                if (sending)
                    return false;
                if (damagerObject is Agent attacker)
                    Strike(attacker, agent, -healthNum);
                return false;
            }
        }
    }
}
