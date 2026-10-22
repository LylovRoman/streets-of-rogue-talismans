using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class WorldRewind
    {
        static readonly List<Vector3> openWindows = new List<Vector3>();

        public static bool Busy { get; private set; }
        public static bool Holding { get; private set; }

        static bool allowKnockout;
        static readonly HashSet<int> keptSills = new HashSet<int>();
        static readonly List<ObjectReal> shelvedSills = new List<ObjectReal>();

        public class Mark
        {
            public List<Body> Bodies = new List<Body>();
            public List<Loot> Loot = new List<Loot>();
            public List<Wall> Walls = new List<Wall>();
            public List<Blaze> Flames = new List<Blaze>();
            public List<int> Debris = new List<int>();
            public HashSet<int> KeptLoot = new HashSet<int>();
            public List<int> Puddles = new List<int>();
            public List<Pond> Ponds = new List<Pond>();
            public List<Stack> Bag = new List<Stack>();
            public List<Prop> Props = new List<Prop>();
            public List<int> Holes = new List<int>();
            public List<Cloud> Clouds = new List<Cloud>();
            public List<int> Bombs = new List<int>();
            public List<Job> Jobs = new List<Job>();
            public List<string> Given = new List<string>();
            public bool HasSheet;
            public List<Quest?> Sheet = new List<Quest?>();
            public string Weapon = "";
            public string Armor = "";
            public string Head = "";
            public bool Lockdown;
            public bool PoliceLockdown;
            public bool FloorDone;
        }

        public class Body
        {
            public Agent? Who;
            public Vector3 At;
            public float Health;
            public bool Dead;
            public bool KnockedOut;
            public List<EffectSnap> Effects = new List<EffectSnap>();
            public List<Stack> Pockets = new List<Stack>();
            public List<RelSnap> Rels = new List<RelSnap>();
            public string Weapon = "";
            public string Armor = "";
            public string Head = "";
            public string Facing = "";
            public float Angle;
            public bool Frozen;
            public bool Invisible;
            public bool InCombat;
            public bool HasOpponent;
            public Agent? Opponent;
            public float AttackCooldown;
            public float WeaponCooldown;
            public bool Investigating;
            public Vector3 InvestigateAt;
            public int Prisoner;
            public Agent? Employer;
            public bool HasEmployer;
            public Agent? Following;
            public bool Sleeping;
            public float HealthBeforeKnockout;
            public int Deliveries;
            public bool FreedSlave;
            public bool QuestGiver;
            public bool QuestEnder;
            public Quest? GiverQuest;
            public Quest? EnderQuest;
            public Quest? KillQuest;
            public Quest? RescueQuest;
            public Quest? CurrentQuest;
            public Quest? LastTeleport;
            public bool KeepQuests;
            public List<Quest> Quests = new List<Quest>();
            public int QuestTrigger;
            public bool HadMarker;
            public string MarkerKind = "";
            public Quest? MarkerQuest;
        }

        public class RelSnap
        {
            public Agent? Other;
            public string Kind = "";
            public float Hate;
            public int Strikes;
            public bool DidHostile;
            public bool DidAnger;
            public relStatus CodeStatus;
            public bool Secret;
            public int Hurt;
            public float Annoyed;
            public Vector3 LastSaw;
            public int LastSawTimer;
        }

        public class EffectSnap
        {
            public string Name = "";
            public int Time;
            public bool Infinite;
        }

        public class Loot
        {
            public Item? Thing;
            public int Id;
            public Stack Data = new Stack();
            public Vector3 At;
            public Agent? Owner;
            public string Name = "";
            public int Count;
        }

        public class Wall
        {
            public int X;
            public int Y;
            public wallMaterialType Material;
            public bool Gone;
            public GameObject? Blocker;
            public wallSideType Side;
            public Vector2 Size;
            public Vector2 Offset;
            public BoxCollider2D? Face;
            public bool FaceLive;
            public Vector2 FaceSize;
            public Vector2 FaceOffset;
            public Vector3 FaceAt;
            public BoxCollider2D? Reach;
            public bool ReachLive;
            public Vector2 ReachSize;
            public Vector2 ReachOffset;
            public Vector3 ReachAt;
            public int Skin;
            public int SkinTop;
            public int Before;
            public int BeforeTop;
            public Vector3 At;
            public List<Stamp> Stamps = new List<Stamp>();
            public StreamingTileArray? Home;
            public tk2dTileMap? WallsMap;
            public int CellX;
            public int CellY;
            public int FaceLayer;
        }

        public class Stamp
        {
            public tk2dTileMap? Map;
            public int X;
            public int Y;
            public int Layer;
            public int Id = -1;
        }

        public class Blaze
        {
            public Fire? Thing;
            public int Id;
            public Vector3 At;
            public bool Oil;
            public bool Forever;
        }

        public class Stack
        {
            public string Name = "";
            public int Count;
            public List<string>? Contents;
            public List<int>? Chunks;
            public List<int>? Sectors;
            public int Owner;
            public int MaxAmmo;
            public bool Identified;
            public int Chunk;
            public int Sector;
            public bool Used;
            public int Value;
        }

        public class Pond
        {
            public int X;
            public int Y;
            public bool Water;
            public bool Oil;
            public bool Ooze;
            public bool Flame;
            public List<string> Poison = new List<string>();
        }

        public class Cloud
        {
            public Gas? Thing;
            public int Id;
            public Vector3 At;
            public float Time;
            public bool Stay;
        }

        public class Job
        {
            public Quest? Quest;
            public string Status = "";
            public questStatusType Code;
            public bool Accepted;
            public bool Discovered;
            public bool Took;
            public bool[] Killed = new bool[0];
            public Agent? Player;
            public List<Agent> People = new List<Agent>();
            public List<ObjectReal> Things = new List<ObjectReal>();
            public int Trigger;
            public int Trigger2;
            public int Trigger3;
            public bool ListsReady;
        }

        public class Prop
        {
            public ObjectReal? Thing;
            public bool Functional;
            public bool Interactable;
            public bool Destroyed;
            public bool Looked;
            public bool Treasure;
            public string Sprite = "";
            public List<Stack> Contents = new List<Stack>();
            public bool IsDoor;
            public bool Open;
            public bool Locked;
            public bool IsWindow;
            public bool WindowBroken;
            public bool HitOnce;
            public bool WindowMelee;
            public bool NoLight;
            public bool NoHighlight;
            public bool LightOn;
            public bool MeleePass;
            public bool BulletsPass;
            public bool ColliderOn;
            public bool HitboxOn;
            public bool HadAgent;
            public bool Exploded;
            public bool Burning;
            public bool Flashing;
            public bool IsLamp;
            public bool IsHydrant;
            public bool IsGenerator;
            public bool HasDetonator;
            public bool Used;
            public bool PowerOff;
            public bool CrateLocked;
            public bool IsCrate;
            public bool MineSet;
            public bool IsMine;
            public bool ManholeOpen;
            public bool IsManhole;
            public bool TrapOpen;
            public bool IsTrap;
            public bool IsPlate;
            public bool PlateOn;
            public bool AlarmOn;
            public bool AlarmHacked;
            public bool IsAlarm;
            public bool WallUp;
            public bool IsLockWall;
            public bool Emitting;
            public bool IsEmitter;
            public bool TripWaiting;
            public bool IsLaser;
            public int CamStage;
            public bool Spotted;
            public Agent? CamOpponent;
            public bool IsCam;
            public int TurretStage;
            public Agent? TurretOpponent;
            public List<Agent> TurretHate = new List<Agent>();
            public bool IsTurret;
            public bool Malware;
            public bool CanPoison;
            public bool ElectionHacked;
            public bool IsComputer;
            public int HydrantState;
            public float Damage;
            public int Threshold;
            public float Timer;
            public int Clock;
            public bool InvokeCountdown;
            public bool InvokeTime;
            public Agent? Operator;
            public bool IsSafe;
            public bool SafeLocked;
            public bool Hackable;
            public string Name1 = "";
            public string Name2 = "";
            public string Facing = "";
            public string CounterSprite = "";
            public List<Solid> Solids = new List<Solid>();
            public int QuestTrigger;
            public bool HadMarker;
            public string MarkerKind = "";
            public Quest? MarkerQuest;
            public bool MeshOn;
            public bool AnimOn;
            public int AnimHash;
            public float AnimTime;
            public float AnimSpeed;
            public bool SpriteFlash;
            public bool HasGraph;
            public int GraphTag;
        }

        public class Solid
        {
            public Collider2D? Shape;
            public bool On;
            public bool Trigger;
        }

        public static Mark? Capture(Agent hero)
        {
            GameController gc = hero.gc != null ? hero.gc : GameController.gameController;
            if (gc == null)
                return null;

            Mark mark = new Mark();
            CaptureBodies(gc, hero, mark);
            CaptureBag(hero, mark);
            CaptureProps(gc, mark);
            CaptureLoot(gc, mark);
            CaptureWalls(gc, mark);
            CaptureGround(gc, mark);
            CaptureFires(gc, mark);
            CaptureDebris(gc, mark);
            CapturePuddles(gc, mark);
            CaptureHoles(gc, mark);
            CaptureClouds(gc, mark);
            CaptureBombs(gc, mark);
            CaptureJobs(gc, mark);
            mark.Lockdown = gc.lockdown;
            mark.PoliceLockdown = gc.policeLockdown;
            return mark;
        }

        public static void Restore(Agent hero, Mark mark)
        {
            if (Busy)
                return;

            GameController gc = hero.gc != null ? hero.gc : GameController.gameController;
            if (gc == null)
                return;

            Busy = true;
            try
            {
                ClearShots(gc);
                ClearNoise(gc);
                ClearDangers(gc, mark);
                ClearFreshBombs(gc, mark);
                ClearDebris(gc, mark);
                ClearNewFires(gc, mark);
                PutFiresBack(gc, mark);
                ClearFreshClouds(gc, mark);
                ClearNewLoot(gc, mark);
                ClearPuddles(gc, mark);
                PutGroundBack(gc, mark);
                PutLootBack(gc, mark);
                ClearNewPeople(gc, hero, mark);
                ClearFreshHoles(gc, mark);
                TakeBack(hero, mark);
                PutPeopleBack(gc, hero, mark);
                ClearNewProps(gc, mark);
                PutPropsBack(gc, hero, mark);
                PutBagBack(hero, mark);
                PutWallsBack(gc, mark);
                ClearDebris(gc, mark);
                RepairGlass(gc, mark);
                SweepLoot(gc, mark);
                FinishSolids(gc, mark);
                PaintWindows(mark);
                FinishWindows(gc, mark);
                PutJobsBack(gc, mark);
                StickMarkers(gc, mark);
                SweepLoot(gc, mark);
                gc.lockdown = mark.Lockdown;
                gc.policeLockdown = mark.PoliceLockdown;
                gc.freezeFrame = 0f;
                gc.justEndedFreezeFrame = false;
                gc.SetTimeScale();
                KeepSills();
            }
            finally
            {
                Busy = false;
            }
        }

        static void CaptureBodies(GameController gc, Agent hero, Mark mark)
        {
            if (gc.agentList == null)
                return;

            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent agent = gc.agentList[i];
                if (agent == null || agent.tr == null)
                    continue;

                Body body = new Body
                {
                    Who = agent,
                    At = agent.tr.position,
                    Health = agent.health,
                    Dead = agent.dead,
                    KnockedOut = agent.knockedOut,
                    Facing = agent.playerDir ?? "",
                    Angle = agent.curAngle,
                    Frozen = agent.frozen,
                    Invisible = agent.invisible,
                    InCombat = agent.inCombat,
                    HasOpponent = agent.hasOpponent && agent.opponent != null,
                    Opponent = agent.opponent,
                    AttackCooldown = agent.attackCooldown,
                    WeaponCooldown = agent.weaponCooldown,
                    Investigating = agent.investigatingCrime,
                    InvestigateAt = agent.investigatePosition,
                    Prisoner = agent.prisoner,
                    Employer = agent.employer,
                    HasEmployer = agent.hasEmployer,
                    Following = agent.following,
                    Sleeping = agent.sleepKnockedOut,
                    HealthBeforeKnockout = agent.healthBeforeKnockout,
                    Deliveries = agent.deliveriesMade,
                    FreedSlave = agent.gaveFreeSlave,
                    QuestGiver = agent.hasQuestGiverQuest,
                    QuestEnder = agent.hasQuestEnderQuest,
                    GiverQuest = agent.questGiverQuest,
                    EnderQuest = agent.questEnderQuest,
                    KillQuest = agent.killForQuest,
                    RescueQuest = agent.rescueForQuest,
                    CurrentQuest = agent.curQuest,
                    LastTeleport = agent.lastTeleportedQuest,
                    QuestTrigger = agent.questTrigger,
                };
                if (agent.myQuests != null)
                {
                    body.KeepQuests = true;
                    for (int q = 0; q < agent.myQuests.Count; q++)
                    {
                        if (agent.myQuests[q] != null)
                            body.Quests.Add(agent.myQuests[q]);
                    }
                }

                RememberMarker(agent, body);
                CaptureEffects(agent, body);
                CaptureRel(agent, body);
                CapturePockets(agent, body);
                mark.Bodies.Add(body);
            }
        }

        static void CaptureEffects(Agent agent, Body body)
        {
            if (agent.statusEffects == null || agent.statusEffects.StatusEffectList == null)
                return;

            List<StatusEffect> list = agent.statusEffects.StatusEffectList;
            for (int i = 0; i < list.Count; i++)
            {
                StatusEffect effect = list[i];
                if (effect == null || string.IsNullOrEmpty(effect.statusEffectName) || effect.statusEffectName == SnakeEffect.EffectId)
                    continue;

                body.Effects.Add(new EffectSnap
                {
                    Name = effect.statusEffectName,
                    Time = effect.curTime,
                    Infinite = effect.infiniteTime,
                });
            }
        }

        static void CaptureRel(Agent agent, Body body)
        {
            if (agent.relationships == null || agent.relationships.RelList == null)
                return;

            List<Relationship> list = agent.relationships.RelList;
            for (int i = 0; i < list.Count; i++)
            {
                Relationship rel = list[i];
                if (rel == null || rel.agent == null)
                    continue;

                body.Rels.Add(new RelSnap
                {
                    Other = rel.agent,
                    Kind = rel.relType ?? "",
                    Hate = rel.relHate,
                    Strikes = rel.relStrikes,
                    DidHostile = rel.didHostile,
                    DidAnger = rel.didAnger,
                    CodeStatus = rel.relTypeCode,
                    Secret = rel.secretHate,
                    Hurt = rel.damageDone,
                    Annoyed = rel.annoyedCountdown,
                    LastSaw = rel.lastSawPosition,
                    LastSawTimer = rel.lastSawTimer,
                });
            }
        }

        static string HeldName(Agent agent, InvItem weapon)
        {
            if (weapon == null)
                return "";
            if (DragonArms.IsFormWeapon(weapon))
                return DragonEffect.RememberedName(agent);
            return weapon.invItemName ?? "";
        }

        static void ForgetFormWeapons(List<InvItem> current)
        {
            for (int i = current.Count - 1; i >= 0; i--)
            {
                if (DragonArms.IsFormWeapon(current[i]))
                    current.RemoveAt(i);
            }
        }

        static void CaptureBag(Agent hero, Mark mark)
        {
            InvDatabase inventory = hero.inventory;
            if (inventory == null || inventory.InvItemList == null)
                return;

            if (inventory.equippedWeapon != null)
                mark.Weapon = HeldName(hero, inventory.equippedWeapon);
            if (inventory.equippedArmor != null)
                mark.Armor = inventory.equippedArmor.invItemName ?? "";
            if (inventory.equippedArmorHead != null)
                mark.Head = inventory.equippedArmorHead.invItemName ?? "";

            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (item == null || string.IsNullOrEmpty(item.invItemName) || DragonArms.IsFormWeapon(item))
                    continue;

                mark.Bag.Add(ReadItem(item));
            }
        }

        static void CaptureLoot(GameController gc, Mark mark)
        {
            if (gc.itemList == null)
                return;

            for (int i = 0; i < gc.itemList.Count; i++)
            {
                Item item = gc.itemList[i];
                if (item == null || item.tr == null)
                    continue;
                if (item.invItem != null && DragonArms.IsFormWeapon(item.invItem))
                    continue;

                Stack data = item.invItem != null ? ReadItem(item.invItem) : new Stack { Name = item.name ?? "", Count = 1 };
                mark.Loot.Add(new Loot
                {
                    Thing = item,
                    Id = item.GetInstanceID(),
                    Data = data,
                    Name = data.Name,
                    Count = data.Count,
                    At = item.tr.position,
                    Owner = item.owner,
                });
            }
        }

        static void CaptureWalls(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null)
                return;

            HashSet<TileData> seen = new HashSet<TileData>();
            if (gc.streamingWorld && gc.streamingWorldController != null && gc.streamingWorldController.tileArrays != null)
            {
                List<StreamingTileArray> arrays = gc.streamingWorldController.tileArrays;
                for (int i = 0; i < arrays.Count; i++)
                {
                    StreamingTileArray stream = arrays[i];
                    if (stream == null || stream.tileArray == null || stream.streamingTileMaps == null)
                        continue;

                    ScanWalls(
                        gc,
                        mark,
                        stream.tileArray,
                        stream,
                        stream.streamingTileMaps.tilemapWalls,
                        stream.streamingTileMaps.tilemapWallTops,
                        stream.streamingTileMaps.tilemapShadows,
                        seen);
                }
            }

            if (gc.tileInfo.tileArray != null)
            {
                ScanWalls(
                    gc,
                    mark,
                    gc.tileInfo.tileArray,
                    null,
                    gc.tileInfo.tilemapWalls,
                    gc.tileInfo.tilemapWallTops,
                    gc.tileInfo.tilemapShadows,
                    seen);
            }
        }

        static void ScanWalls(GameController gc, Mark mark, TileData[,] tiles, StreamingTileArray? stream, tk2dTileMap walls, tk2dTileMap tops, tk2dTileMap shadows, HashSet<TileData> seen)
        {
            int x0 = tiles.GetLowerBound(0);
            int x1 = tiles.GetUpperBound(0);
            int y0 = tiles.GetLowerBound(1);
            int y1 = tiles.GetUpperBound(1);
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null || !seen.Add(tile))
                        continue;
                    if (tile.wallMaterial == wallMaterialType.None && !tile.destroyedWall)
                        continue;

                    int localX = gc.streamingWorld ? gc.tileInfo.CorrectTile(tile.posX) : tile.posX;
                    int localY = gc.streamingWorld ? gc.tileInfo.CorrectTile(tile.posY) : tile.posY;
                    Vector3 at = stream != null
                        ? new Vector3(stream.pos.x * 10.24f + localX * 0.64f, stream.pos.y * 10.24f + localY * 0.64f, 0f)
                        : new Vector3(tile.posX * 0.64f, tile.posY * 0.64f, 0f);
                    Wall wall = new Wall
                    {
                        X = tile.posX,
                        Y = tile.posY,
                        At = at,
                        Material = tile.wallMaterial,
                        Gone = tile.destroyedWall,
                        Side = tile.wallSide,
                        Size = tile.wallSize,
                        Offset = tile.wallOffset,
                        Blocker = gc.tileInfo.IsOverlapping(at, "Wall"),
                        Face = tile.wallCollider,
                        Reach = tile.wallColliderUnreachable,
                        Skin = tile.wallMaterialOffset,
                        SkinTop = tile.wallMaterialOffsetTop,
                        Before = tile.wallTileBeforeOffset,
                        BeforeTop = tile.wallTileBeforeOffsetTop,
                        Home = stream,
                    };
                    if (tile.wallMaterial != wallMaterialType.Door && tile.wallMaterial != wallMaterialType.DoorOpen)
                        NoteBlast(wall, stream, walls, tops, shadows, x, y, stream != null);
                    if (tile.wallMaterial == wallMaterialType.Window)
                        NoteSill(wall, tops, x, y, stream != null);
                    if (tile.wallCollider != null)
                    {
                        wall.FaceLive = tile.wallCollider.enabled && tile.wallCollider.gameObject.activeInHierarchy;
                        wall.FaceSize = tile.wallCollider.size;
                        wall.FaceOffset = tile.wallCollider.offset;
                        wall.FaceAt = tile.wallCollider.transform.position;
                        wall.FaceLayer = tile.wallCollider.gameObject.layer;
                    }

                    if (tile.wallColliderUnreachable != null)
                    {
                        wall.ReachLive = tile.wallColliderUnreachable.enabled && tile.wallColliderUnreachable.gameObject.activeInHierarchy;
                        wall.ReachSize = tile.wallColliderUnreachable.size;
                        wall.ReachOffset = tile.wallColliderUnreachable.offset;
                        wall.ReachAt = tile.wallColliderUnreachable.transform.position;
                    }

                    mark.Walls.Add(wall);
                }
            }
        }

        static bool Doorish(wallMaterialType material)
        {
            return material == wallMaterialType.Door || material == wallMaterialType.DoorOpen || material == wallMaterialType.Window;
        }

        static long WetKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

        static void CapturePuddles(GameController gc, Mark mark)
        {
            if (gc.watersList == null)
                return;

            for (int i = 0; i < gc.watersList.Count; i++)
            {
                Water water = gc.watersList[i];
                if (water != null)
                    mark.Puddles.Add(water.GetInstanceID());
            }
        }

        static void ClearPuddles(GameController gc, Mark mark)
        {
            HashSet<int> kept = new HashSet<int>(mark.Puddles);
            if (gc.watersList != null)
            {
                List<Water> waters = new List<Water>(gc.watersList);
                for (int i = 0; i < waters.Count; i++)
                {
                    Water water = waters[i];
                    if (water != null && !kept.Contains(water.GetInstanceID()))
                        water.DestroyMe();
                }
            }

            if (gc.tileInfo == null || gc.tileInfo.tileArray == null)
                return;
        }

        static void CaptureGround(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null || gc.tileInfo.tileArray == null)
                return;

            TileData[,] tiles = gc.tileInfo.tileArray;
            int x0 = tiles.GetLowerBound(0);
            int x1 = tiles.GetUpperBound(0);
            int y0 = tiles.GetLowerBound(1);
            int y1 = tiles.GetUpperBound(1);
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null)
                        continue;

                    bool water = tile.spillWater || tile.water || tile.waterReal != null;
                    bool poison = tile.poisonWaterType != null && tile.poisonWaterType.Count > 0;
                    if (!water && !tile.spillOil && !tile.spillOoze && !tile.groundFire && !poison)
                        continue;

                    Pond pond = new Pond
                    {
                        X = tile.posX,
                        Y = tile.posY,
                        Water = water,
                        Oil = tile.spillOil,
                        Ooze = tile.spillOoze,
                        Flame = tile.groundFire,
                    };
                    if (poison && tile.poisonWaterType != null)
                        pond.Poison.AddRange(tile.poisonWaterType);
                    mark.Ponds.Add(pond);
                }
            }
        }

        static void PutGroundBack(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null || gc.tileInfo.tileArray == null)
                return;

            Dictionary<long, Pond> wanted = new Dictionary<long, Pond>();
            for (int i = 0; i < mark.Ponds.Count; i++)
            {
                Pond pond = mark.Ponds[i];
                wanted[WetKey(pond.X, pond.Y)] = pond;
            }

            TileData[,] tiles = gc.tileInfo.tileArray;
            int x0 = tiles.GetLowerBound(0);
            int x1 = tiles.GetUpperBound(0);
            int y0 = tiles.GetLowerBound(1);
            int y1 = tiles.GetUpperBound(1);
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null)
                        continue;

                    wanted.TryGetValue(WetKey(tile.posX, tile.posY), out Pond? pond);
                    bool wantWater = pond != null && pond.Water;
                    bool wantOil = pond != null && pond.Oil;
                    bool wantOoze = pond != null && pond.Ooze;
                    Vector2 at = new Vector2(tile.posX * 0.64f, tile.posY * 0.64f);

                    if (!wantWater && (tile.spillWater || tile.water || tile.waterReal != null))
                    {
                        tile.spillWater = false;
                        tile.water = false;
                        tile.waterEdge = false;
                        if (tile.waterReal != null)
                        {
                            tile.waterReal.DestroyMe();
                            tile.waterReal = null;
                        }

                        gc.tileInfo.CleanUpLiquid(at, "Water");
                    }
                    else if (wantWater && !tile.spillWater && !tile.water && tile.waterReal == null)
                    {
                        gc.tileInfo.SpillLiquid(at, "Water", false);
                    }

                    if (!wantOil && tile.spillOil)
                    {
                        tile.spillOil = false;
                        gc.tileInfo.CleanUpLiquid(at, "Oil");
                    }
                    else if (wantOil && !tile.spillOil)
                    {
                        gc.tileInfo.SpillLiquid(at, "Oil", false);
                    }

                    if (!wantOoze && tile.spillOoze)
                    {
                        tile.spillOoze = false;
                        tile.dangerousToWalk = false;
                    }
                    else if (wantOoze && !tile.spillOoze)
                    {
                        gc.tileInfo.SpillOoze(at);
                    }

                    tile.groundFire = pond != null && pond.Flame;
                    if (tile.poisonWaterType == null)
                        tile.poisonWaterType = new List<string>();
                    else
                        tile.poisonWaterType.Clear();
                    if (pond != null)
                        tile.poisonWaterType.AddRange(pond.Poison);
                    if (tile.waterReal != null)
                    {
                        if (tile.waterReal.effectContents == null)
                            tile.waterReal.effectContents = new List<string>();
                        else
                            tile.waterReal.effectContents.Clear();
                        tile.waterReal.effectContents.AddRange(tile.poisonWaterType);
                    }

                    string kind = "None";
                    if (tile.spillOoze)
                        kind = "Ooze";
                    else if (tile.spillOil)
                        kind = "Oil";
                    else if (tile.spillWater || tile.water)
                        kind = "Water";
                    gc.tileInfo.UpdateLiquid(at, kind);
                }
            }
        }

        static void RepairGlass(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null)
                return;

            openWindows.Clear();
            for (int i = 0; i < mark.Props.Count; i++)
            {
                Prop prop = mark.Props[i];
                if (!prop.IsWindow || prop.WindowBroken || prop.Thing == null || prop.Thing.tr == null)
                    continue;

                Vector3 at = prop.Thing.tr.position;
                Window? window = prop.Thing as Window;
                bool brokenPane = window != null && window.broken;
                if (window != null)
                {
                    window.broken = false;
                    window.hitWindowOnce = prop.HitOnce;
                    window.windowMeleeCanPass = prop.WindowMelee;
                }

                prop.Thing.destroyed = false;
                prop.Thing.destroying = false;
                TileData tile = gc.tileInfo.GetTileData(at);
                bool brokenTile = tile != null && tile.destroyedWindow;
                bool stripped = tile != null && tile.wallMaterial != wallMaterialType.Window;
                bool repair = brokenPane || brokenTile || stripped;
                Wall? sill = FindSill(gc, mark, at);
                if (repair && tile != null && sill != null)
                {
                    RememberFrame(tile, sill);
                    PaintBlast(sill);
                }
                if (repair)
                {
                    SettleBuild(gc);
                    gc.tileInfo.SetupWindowAtPosition(at.x, at.y, false);
                    openWindows.Add(at);
                }
                if (!string.IsNullOrEmpty(prop.Facing) && prop.Thing.direction != prop.Facing)
                    prop.Thing.direction = prop.Facing;
                string sprite = GlassSprite(prop);
                if (!string.IsNullOrEmpty(sprite) && ShownSprite(prop.Thing) != sprite)
                    prop.Thing.SetObjectSprite(sprite);
                if (repair)
                    ClearBrokenPane(gc, at);
            }

            FlushMaps();
        }

        static void FinishWindows(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null || openWindows.Count == 0)
                return;

            SettleBuild(gc);
            List<Vector3> holes = new List<Vector3>(openWindows);
            for (int i = 0; i < holes.Count; i++)
            {
                Vector3 at = holes[i];
                TileData tile = gc.tileInfo.GetTileData(at);
                Wall? sill = FindSill(gc, mark, at);
                if (tile != null && sill != null)
                    RememberFrame(tile, sill);
                gc.tileInfo.SetupWindowAtPosition(at.x, at.y, false);
            }

            openWindows.Clear();
            PlaceSills(gc, mark, holes);
            ShowTops(gc);
            CatNoirPlugin.AfterFrame(() =>
            {
                if (gc == null || gc.tileInfo == null)
                    return;

                PlaceSills(gc, mark, holes);
                ShowTops(gc);
                KeepSills();
            });
        }

        static void PlaceSills(GameController gc, Mark mark, List<Vector3> holes)
        {
            TileInfo tiles = gc.tileInfo;
            if (tiles == null)
                return;

            System.Reflection.MethodInfo? raise = typeof(TileInfo).GetMethod(
                "setWallTopSameY",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (raise == null)
                return;

            for (int i = 0; i < holes.Count; i++)
            {
                Vector3 at = holes[i];
                Wall? sill = FindSill(gc, mark, at);
                TileData tile = tiles.GetTileData(at);
                if (sill == null || tile == null)
                    continue;

                RememberFrame(tile, sill);
                CellOf(gc, tiles, at, out int x, out int y);
                raise.Invoke(tiles, new object[] { x, y, SillCap(sill.Before) });
                tiles.justSetWallTop = false;
            }
        }

        static int SillCap(int before)
        {
            switch (before)
            {
                case 14:
                case 22:
                case 27:
                    return 10;
                case 15:
                case 23:
                case 26:
                    return 11;
                default:
                    return 8;
            }
        }

        static void CellOf(GameController gc, TileInfo tiles, Vector3 at, out int x, out int y)
        {
            x = (int)((at.x + 0.32f) / 0.64f);
            y = (int)((at.y + 0.32f) / 0.64f);
            if (gc.streamingWorld)
            {
                if (at.x < 0f)
                    x--;
                if (at.y < 0f)
                    y--;
                x = tiles.CorrectTile(x);
                y = tiles.CorrectTile(y);
            }
        }

        static void ShowTops(GameController gc)
        {
            TileInfo tiles = gc.tileInfo;
            if (tiles != null && tiles.tilemapWallTops != null)
                tiles.tilemapWallTops.Build();

            if (!gc.streamingWorld || gc.streamingWorldController == null || gc.streamingWorldController.tileArrays == null)
                return;

            List<StreamingTileArray> arrays = gc.streamingWorldController.tileArrays;
            for (int i = 0; i < arrays.Count; i++)
            {
                StreamingTileArray stream = arrays[i];
                if (stream != null && stream.streamingTileMaps != null && stream.streamingTileMaps.tilemapWallTops != null)
                    stream.streamingTileMaps.tilemapWallTops.Build();
            }
        }

        static void RememberFrame(TileData tile, Wall sill)
        {
            tile.destroyedWall = false;
            tile.wallSide = sill.Side;
            tile.wallMaterial = wallMaterialType.Window;
            tile.wallMaterialOffset = sill.Skin;
            tile.wallMaterialOffsetTop = sill.SkinTop;
            tile.wallTileBeforeOffset = sill.Before;
            tile.wallTileBeforeOffsetTop = sill.BeforeTop;
        }

        static void SettleBuild(GameController gc)
        {
            TileInfo tiles = gc.tileInfo;
            tiles.buildNow = false;
            tiles.buildNow2 = false;
            tiles.endOfFrameBuild = false;
            tiles.endOfFrameBuild2 = false;
        }

        static Wall? FindSill(GameController gc, Mark mark, Vector3 at)
        {
            TileData? live = gc.tileInfo != null ? gc.tileInfo.GetTileData(at) : null;
            Wall? nearest = null;
            float best = 0.5f * 0.5f;
            for (int i = 0; i < mark.Walls.Count; i++)
            {
                Wall wall = mark.Walls[i];
                if (wall.Material != wallMaterialType.Window)
                    continue;
                if (live != null && wall.X == live.posX && wall.Y == live.posY)
                    return wall;

                Vector2 center = (Vector2)wall.At + new Vector2(0.32f, 0.32f);
                float dist = (center - (Vector2)at).sqrMagnitude;
                if (dist < best)
                {
                    best = dist;
                    nearest = wall;
                }
            }

            return nearest;
        }

        static string GlassSprite(Prop prop)
        {
            string saved = prop.Sprite ?? "";
            if (!string.IsNullOrEmpty(saved) && saved.IndexOf("Broken", System.StringComparison.Ordinal) < 0)
                return saved;

            string facing = prop.Thing != null ? prop.Thing.direction ?? "" : "";
            if (facing == "N" || facing == "S")
                return "WindowNS";

            return "Window";
        }

        static void ClearBrokenPane(GameController gc, Vector3 at)
        {
            float tile = 0.64f;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    TileData data = gc.tileInfo.GetTileData(at + new Vector3(dx * tile, dy * tile, 0f));
                    if (data != null)
                        data.destroyedWindow = false;
                }
            }
        }

        static void CaptureProps(GameController gc, Mark mark)
        {
            List<ObjectReal>? list = gc.objectRealListWithDestroyed != null ? gc.objectRealListWithDestroyed : gc.objectRealList;
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                ObjectReal thing = list[i];
                if (thing == null)
                    continue;

                Prop prop = new Prop
                {
                    Thing = thing,
                    Functional = thing.functional,
                    Interactable = thing.interactable,
                    Destroyed = thing.destroyed,
                    Looked = thing.lookedInChest,
                    Treasure = thing.tookTreasure,
                    Sprite = ShownSprite(thing),
                    NoLight = thing.noLight,
                    NoHighlight = thing.noHighlighting,
                    LightOn = thing.lightReal != null && thing.lightReal.gameObject.activeSelf,
                    MeleePass = thing.meleeCanPass,
                    BulletsPass = thing.bulletsCanPass,
                    ColliderOn = thing.objectCollider != null && thing.objectCollider.enabled,
                    HitboxOn = thing.objectHitbox != null && thing.objectHitbox.enabled,
                    HadAgent = thing.hasObjectAgent && thing.objectAgent != null,
                    Exploded = thing.spawnedExplosion,
                    Burning = thing.attachedParticleEffect != null || thing.fire != null || thing.particleEffect != null,
                    Flashing = thing.startedFlashing,
                    Damage = thing.damageTotal,
                    Threshold = thing.damageThreshold,
                    Timer = thing.timer,
                    Clock = thing.timeCountdownClock,
                    InvokeCountdown = thing.IsInvoking("Countdown"),
                    InvokeTime = thing.IsInvoking("TimeCountdown"),
                    Operator = thing.operatingAgent,
                    Hackable = thing.hackable,
                    Used = thing.usedObject,
                    PowerOff = thing.powerBoxShutDown,
                    Name1 = thing.objectRealRealName ?? "",
                    Name2 = thing.objectRealRealName2 ?? "",
                };
                Collider2D[] shapes = thing.GetComponentsInChildren<Collider2D>(true);
                for (int s = 0; s < shapes.Length; s++)
                {
                    if (shapes[s] == null)
                        continue;
                    prop.Solids.Add(new Solid
                    {
                        Shape = shapes[s],
                        On = shapes[s].enabled,
                        Trigger = shapes[s].isTrigger,
                    });
                }
                Safe? safe = thing as Safe;
                if (safe != null)
                {
                    prop.IsSafe = true;
                    prop.SafeLocked = safe.locked;
                }

                FireHydrant? hydrant = thing as FireHydrant;
                if (hydrant != null)
                {
                    prop.IsHydrant = true;
                    prop.HydrantState = hydrant.state;
                }

                if (thing is Lamp)
                    prop.IsLamp = true;
                if (thing is Generator || thing is Generator2)
                    prop.IsGenerator = true;
                CaptureContents(thing.objectInvDatabase, prop.Contents);
                Door? door = thing as Door;
                if (door != null)
                {
                    prop.IsDoor = true;
                    prop.Open = door.open;
                    prop.Locked = door.locked;
                    prop.HasDetonator = door.hasDetonator;
                }

                Window? window = thing as Window;
                if (window != null)
                {
                    prop.IsWindow = true;
                    prop.WindowBroken = window.broken;
                    prop.HitOnce = window.hitWindowOnce;
                    prop.WindowMelee = window.windowMeleeCanPass;
                }

                Crate? crate = thing as Crate;
                if (crate != null)
                {
                    prop.IsCrate = true;
                    prop.CrateLocked = crate.locked;
                }

                Mine? mine = thing as Mine;
                if (mine != null)
                {
                    prop.IsMine = true;
                    prop.MineSet = mine.setOff;
                }

                Manhole? manhole = thing as Manhole;
                if (manhole != null)
                {
                    prop.IsManhole = true;
                    prop.ManholeOpen = manhole.opened;
                }

                TrapDoor? trap = thing as TrapDoor;
                if (trap != null)
                {
                    prop.IsTrap = true;
                    prop.TrapOpen = trap.opened;
                }

                SwitchFloor? plate = thing as SwitchFloor;
                if (plate != null)
                {
                    prop.IsPlate = true;
                    prop.PlateOn = plate.switchOn;
                }

                AlarmButton? alarm = thing as AlarmButton;
                if (alarm != null)
                {
                    prop.IsAlarm = true;
                    prop.AlarmOn = alarm.switchOn;
                    prop.AlarmHacked = alarm.hacked;
                }

                LockdownWall? lockdown = thing as LockdownWall;
                if (lockdown != null)
                {
                    prop.IsLockWall = true;
                    prop.WallUp = lockdown.wallUp;
                }

                LaserEmitter? emitter = thing as LaserEmitter;
                if (emitter != null)
                {
                    prop.IsEmitter = true;
                    prop.Emitting = emitter.emittingLaser;
                }

                Laser? laser = thing as Laser;
                if (laser != null)
                {
                    prop.IsLaser = true;
                    prop.TripWaiting = laser.tripLaserWaiting;
                }

                SecurityCam? cam = thing as SecurityCam;
                if (cam != null)
                {
                    prop.IsCam = true;
                    prop.CamStage = cam.stage;
                    prop.Spotted = cam.spottedSomeone;
                    prop.CamOpponent = cam.opponent;
                }

                Turret? turret = thing as Turret;
                if (turret != null)
                {
                    prop.IsTurret = true;
                    prop.TurretStage = turret.stage;
                    prop.TurretOpponent = turret.opponent;
                    if (turret.agentsWhoObjectHates != null)
                    {
                        for (int h = 0; h < turret.agentsWhoObjectHates.Count; h++)
                        {
                            Agent hated = turret.agentsWhoObjectHates[h];
                            if (hated != null)
                                prop.TurretHate.Add(hated);
                        }
                    }
                }

                Computer? computer = thing as Computer;
                if (computer != null)
                {
                    prop.IsComputer = true;
                    prop.Malware = computer.installedMalware;
                    prop.CanPoison = computer.canPoison;
                    prop.ElectionHacked = computer.electionHacked;
                }

                prop.Facing = thing.direction ?? "";
                if (thing is Counter counter)
                    prop.CounterSprite = counter.modifiedCounterSprite ?? "";
                prop.QuestTrigger = thing.questTrigger;
                prop.MeshOn = thing.objectMeshRenderer != null && thing.objectMeshRenderer.enabled;
                RememberMarker(thing, prop);
                RememberAnim(thing, prop);
                RememberGraph(thing, prop);

                mark.Props.Add(prop);
            }
        }

        static void CaptureContents(InvDatabase? inventory, List<Stack> into)
        {
            if (inventory == null || inventory.InvItemList == null)
                return;

            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (item == null || string.IsNullOrEmpty(item.invItemName) || DragonArms.IsFormWeapon(item))
                    continue;

                into.Add(ReadItem(item));
            }
        }

        static void PutPropsBack(GameController gc, Agent hero, Mark mark)
        {
            for (int i = 0; i < mark.Props.Count; i++)
            {
                Prop prop = mark.Props[i];
                ObjectReal? thing = prop.Thing;
                if (thing == null)
                    continue;

                if (prop.Destroyed)
                {
                    if (!thing.destroyed)
                        thing.DestroyMe();
                }
                else if (thing.destroyed || (thing.go != null && !thing.go.activeSelf))
                {
                    Revive(gc, thing, prop);
                    Door? blown = thing as Door;
                    if (blown != null && gc.tileInfo != null && thing.tr != null)
                    {
                        blown.open = !prop.Open;
                        if (prop.Open)
                            blown.OpenDoor(hero);
                        else
                            blown.CloseDoor(hero);
                    }
                }
                else if (prop.Functional && !thing.functional)
                {
                    thing.functional = true;
                    RepairSprite(thing, prop.Sprite);
                }

                if (!string.IsNullOrEmpty(prop.Facing) && thing.direction != prop.Facing)
                    thing.direction = prop.Facing;
                if (thing is Counter counter)
                {
                    counter.modifiedCounterSprite = prop.CounterSprite ?? "";
                    if (!string.IsNullOrEmpty(prop.Sprite) && ShownSprite(thing) != prop.Sprite)
                        RepairSprite(thing, prop.Sprite);
                }

                thing.lookedInChest = prop.Looked;
                thing.tookTreasure = prop.Treasure;
                thing.noLight = prop.NoLight;
                thing.noHighlighting = prop.NoHighlight;
                if (!string.IsNullOrEmpty(prop.Name1))
                    thing.objectRealRealName = prop.Name1;
                thing.objectRealRealName2 = prop.Name2;
                RestoreContents(thing.objectInvDatabase, prop.Contents);
                if (!(thing is Lamp) && thing.lightReal != null)
                    thing.lightReal.gameObject.SetActive(prop.LightOn && !prop.NoHighlight);

                Door? door = thing as Door;
                if (prop.IsDoor && door != null && !door.destroyed)
                {
                    if (door.open != prop.Open)
                    {
                        if (prop.Open)
                            door.OpenDoor(hero);
                        else
                            door.CloseDoor(hero);
                    }

                    if (prop.Locked && !prop.Open)
                        door.Lock();
                    else if (!prop.Locked)
                    {
                        door.locked = false;
                        if (!string.IsNullOrEmpty(prop.Sprite) && prop.Sprite.IndexOf("Locked", System.StringComparison.Ordinal) < 0)
                            door.SetObjectSprite(prop.Sprite);
                    }
                    if (prop.HasDetonator)
                    {
                        if (!door.hasDetonator)
                            door.PlaceDetonator();
                    }
                    else if (door.hasDetonator)
                    {
                        door.hasDetonator = false;
                        ClearDetonator(door);
                    }

                    BoxCollider2D plate = door.GetComponent<BoxCollider2D>();
                    if (plate != null)
                        plate.enabled = !prop.Open;
                    door.bulletsCanPass = prop.Open;
                    door.meleeCanPass = prop.Open;
                    WakeHelper(gc, door.doorHelper);
                    WakeHelper(gc, door.doorHelper2);
                    if (gc.tileInfo != null && door.tr != null)
                    {
                        gc.tileInfo.ToggleDoorAtPosition(door.tr.position.x, door.tr.position.y, false, prop.Open);
                        TileData doorTile = gc.tileInfo.GetTileData(door.tr.position);
                        if (doorTile != null && prop.Open)
                        {
                            if (doorTile.wallCollider != null)
                                doorTile.wallCollider.enabled = false;
                            if (doorTile.wallColliderUnreachable != null)
                                doorTile.wallColliderUnreachable.enabled = false;
                        }
                    }
                }

                Window? window = thing as Window;
                if (prop.IsWindow && window != null)
                {
                    window.reallyDestroyedWindow = false;
                    window.specialDestroying = false;
                    window.broken = prop.WindowBroken;
                    window.hitWindowOnce = prop.HitOnce;
                    window.windowMeleeCanPass = prop.WindowMelee;
                    thing.meleeCanPass = prop.MeleePass;
                    thing.bulletsCanPass = prop.BulletsPass;
                    thing.destroying = false;
                    if (!prop.WindowBroken)
                    {
                        RepairSprite(thing, GlassSprite(prop));
                        WakeHelper(gc, window.windowHelper);
                        if (prop.HadAgent && thing.objectAgent == null)
                            thing.SpawnObjectAgent();
                    }
                }

                Safe? safe = thing as Safe;
                if (prop.IsSafe && safe != null)
                    safe.locked = prop.SafeLocked;

                thing.hackable = prop.Hackable;
                thing.questTrigger = prop.QuestTrigger;
                thing.usedObject = prop.Used;
                thing.powerBoxShutDown = prop.PowerOff;
                if (!prop.Destroyed)
                {
                    thing.interactable = prop.Interactable;
                    Harden(thing, prop);
                }
                PutObjectBits(gc, thing, prop);
                WakeCamera(thing, prop);

                PutSolidsBack(prop);
                PutFireBack(thing, prop);
                SettleMachine(gc, thing, prop);
            }
        }

        static void SettleMachine(GameController gc, ObjectReal thing, Prop prop)
        {
            thing.damageTotal = prop.Damage;
            thing.damageThreshold = prop.Threshold;
            thing.startedFlashing = prop.Flashing;
            thing.spawnedExplosion = prop.Exploded;
            thing.CancelInvoke("Countdown");
            thing.CancelInvoke("TimeCountdown");
            bool counting = prop.Flashing || prop.Clock > 0;
            if (counting)
            {
                thing.timer = prop.Timer;
                thing.timeCountdownClock = prop.Clock;
                if (prop.InvokeCountdown || prop.InvokeTime)
                    thing.InvokeRepeating(prop.InvokeCountdown ? "Countdown" : "TimeCountdown", 0.01f, 1f);
            }
            else
            {
                thing.timer = prop.Timer;
                thing.timeCountdownClock = prop.Clock;
            }
            if (prop.Operator == null && thing.operatingAgent != null)
                thing.MustStopOperating();
            thing.operatingAgent = prop.Operator;

            if (prop.IsGenerator && (thing is Generator || thing is Generator2))
            {
                if (!counting)
                {
                    thing.CancelInvoke();
                    thing.StopAllCoroutines();
                    thing.timer = prop.Timer;
                    thing.timeCountdownClock = prop.Clock;
                    thing.startedFlashing = prop.Flashing;
                    thing.functional = prop.Functional;
                    thing.interactable = prop.Functional;
                    if (thing is Generator generator)
                        generator.countdownCauser = null;
                    if (gc.audioHandler != null)
                        gc.audioHandler.StopObjectSounds(thing);
                }
                else if (prop.Flashing)
                {
                    thing.PlayObjectSpriteEffect("FlashingRepeatedly");
                }

                PoseMachine(thing, prop);
            }

            if (prop.IsHydrant && thing is FireHydrant hydrant && hydrant.state != prop.HydrantState)
            {
                hydrant.StopAllCoroutines();
                hydrant.state = prop.HydrantState;
                if (prop.HydrantState == 0)
                {
                    hydrant.SwitchOff();
                    thing.functional = true;
                    RepairSprite(thing, prop.Sprite);
                }
            }

            if (!prop.IsLamp || thing.tr == null)
                return;

            Transform glow = thing.tr.Find("LightTemp(Clone)");
            Transform glow2 = thing.tr.Find("LightTemp2(Clone)");
            if (!prop.Functional || prop.Destroyed)
            {
                RecycleLight(glow);
                RecycleLight(glow2);
                if (thing.lightReal != null)
                    thing.lightReal.gameObject.SetActive(false);
                return;
            }

            if (glow == null && glow2 == null && gc.spawnerMain != null)
                gc.spawnerMain.SpawnLightTemp(thing.tr.position, thing, "Lamp");
            if (thing.lightReal != null)
                thing.lightReal.gameObject.SetActive(false);
        }

        static void PutSolidsBack(Prop prop)
        {
            for (int i = 0; i < prop.Solids.Count; i++)
            {
                Solid solid = prop.Solids[i];
                Collider2D? shape = solid.Shape;
                if (shape == null)
                    continue;

                if (solid.On)
                    WakeShape(prop.Thing, shape);
                shape.enabled = solid.On;
                shape.isTrigger = solid.Trigger;
            }
        }

        static void Harden(ObjectReal thing, Prop prop)
        {
            thing.meleeCanPass = prop.MeleePass;
            thing.bulletsCanPass = prop.BulletsPass;
            if (thing.go != null)
            {
                thing.go.layer = thing.originalLayer;
                if (!string.IsNullOrEmpty(thing.originalTag))
                    thing.go.tag = thing.originalTag;
            }

            if (prop.ColliderOn && thing.objectCollider != null)
            {
                thing.objectCollider.gameObject.layer = thing.originalLayer;
                if (thing.originalBoxColliderSize != Vector2.zero)
                {
                    thing.objectCollider.size = thing.originalBoxColliderSize;
                    thing.objectCollider.offset = thing.originalBoxColliderOffset;
                }
            }

            if (prop.ColliderOn && thing.extraCollider != null)
                thing.extraCollider.gameObject.layer = thing.originalLayer;
            if (thing.objectMeshRenderer != null)
                thing.objectMeshRenderer.enabled = prop.MeshOn;
            if (prop.ColliderOn && thing.objectCollider != null)
                WakeShape(thing, thing.objectCollider);
            if (prop.ColliderOn && thing.extraCollider != null)
                WakeShape(thing, thing.extraCollider);
            if (prop.HitboxOn && thing.objectHitbox != null)
                WakeShape(thing, thing.objectHitbox);
            if (prop.HasGraph)
            {
                Component? scene = GraphOf(thing);
                if (scene != null)
                    WriteGraph(scene, prop.GraphTag);
            }

            if (prop.ColliderOn && thing.extraCollider != null && thing.extraCollider.enabled && thing.extraCollider.gameObject.activeInHierarchy)
            {
                Component? extra = thing.extraCollider.GetComponent("GraphUpdateScene");
                if (extra != null)
                {
                    System.Reflection.MethodInfo apply = extra.GetType().GetMethod("Apply");
                    if (apply != null)
                        apply.Invoke(extra, null);
                }
            }

            if (!prop.ColliderOn || thing.tr == null)
                return;

            Transform moving = thing.tr.Find("MovingObjectItemCollider");
            if (moving != null && !moving.gameObject.activeSelf)
                moving.gameObject.SetActive(true);
        }

        static void WakeShape(ObjectReal? thing, Collider2D shape)
        {
            Transform? node = shape.transform;
            while (node != null)
            {
                if (!node.gameObject.activeSelf)
                    node.gameObject.SetActive(true);
                if (thing != null && thing.tr != null && node == thing.tr)
                    break;
                node = node.parent;
            }

            shape.enabled = true;
        }

        static void MarkSolid(GameController gc, ObjectReal thing, bool solid)
        {
            if (!solid)
                return;
            if (thing.curTileData != null)
                thing.curTileData.solidObject = true;
            if (gc.tileInfo == null || thing.tr == null)
                return;

            TileData under = gc.tileInfo.GetTileData(thing.tr.position);
            if (under != null)
                under.solidObject = true;
        }

        static void TakeBack(Agent hero, Mark mark)
        {
            InvDatabase bag = hero.inventory;
            if (bag == null || bag.InvItemList == null)
                return;

            List<InvItem> current = new List<InvItem>();
            for (int i = 0; i < bag.InvItemList.Count; i++)
            {
                if (bag.InvItemList[i] != null)
                    current.Add(bag.InvItemList[i]);
            }

            ForgetFormWeapons(current);

            List<InvItem> kept = new List<InvItem>();
            for (int i = 0; i < mark.Bag.Count; i++)
            {
                InvItem? found = Take(current, kept, mark.Bag[i]);
                if (found != null)
                    kept.Add(found);
            }

            List<InvItem> extras = new List<InvItem>();
            for (int i = 0; i < current.Count; i++)
            {
                InvItem item = current[i];
                if (kept.Contains(item) || item.invItemName == SnakeTalismanItem.ItemId || item.invItemName == SecondChanceAbility.Id)
                    continue;
                extras.Add(item);
            }

            List<InvItem> sent = new List<InvItem>();
            for (int b = 0; b < mark.Bodies.Count; b++)
            {
                Body body = mark.Bodies[b];
                Agent? owner = body.Who;
                if (owner == null || owner == hero || owner.inventory == null || owner.inventory.InvItemList == null)
                    continue;

                List<InvItem> pockets = new List<InvItem>();
                for (int i = 0; i < owner.inventory.InvItemList.Count; i++)
                {
                    if (owner.inventory.InvItemList[i] != null)
                        pockets.Add(owner.inventory.InvItemList[i]);
                }

                List<InvItem> pocketKept = new List<InvItem>();
                for (int i = 0; i < body.Pockets.Count; i++)
                {
                    Stack stack = body.Pockets[i];
                    InvItem? already = Take(pockets, pocketKept, stack);
                    if (already != null)
                    {
                        pocketKept.Add(already);
                        continue;
                    }

                    if (IsCarriedKey(stack.Name))
                        continue;

                    InvItem? carrying = Take(extras, sent, stack);
                    if (carrying == null)
                        continue;

                    carrying.cantDrop = false;
                    if (!HandOver(bag, owner.inventory, carrying))
                        Give(owner.inventory, stack);
                    sent.Add(carrying);
                }
            }
        }

        static void PutFireBack(ObjectReal thing, Prop prop)
        {
            if (!prop.Burning && thing.attachedParticleEffect == null && thing.fire == null)
                return;

            thing.StopFireParticles();
            if (thing.fire != null)
            {
                thing.fire.DestroyMe();
                thing.fire = null;
            }

            if (prop.Burning)
                thing.StartFireInObject();
        }

        static void WakeHelper(GameController gc, ObjectReal? helper)
        {
            if (helper == null)
                return;

            helper.destroyed = false;
            helper.destroying = false;
            helper.functional = true;
            if (helper.go != null && !helper.go.activeSelf)
                helper.go.SetActive(true);
            Rejoin(gc, helper);
        }

        static void Revive(GameController gc, ObjectReal thing, Prop prop)
        {
            if (thing.go != null)
                thing.go.SetActive(true);
            thing.destroying = false;
            thing.destroyed = false;
            thing.functional = prop.Functional;
            thing.spawnedExplosion = prop.Exploded;
            thing.meleeCanPass = prop.MeleePass;
            thing.bulletsCanPass = prop.BulletsPass;
            if (thing.objectCollider != null)
                thing.objectCollider.enabled = prop.ColliderOn;
            if (thing.objectHitbox != null)
                thing.objectHitbox.enabled = prop.HitboxOn;
            PutSolidsBack(prop);
            RepairSprite(thing, prop.Sprite);
            if (prop.HadAgent && thing.objectAgent == null)
                thing.SpawnObjectAgent();
            Rejoin(gc, thing);
        }

        static void Rejoin(GameController gc, ObjectReal thing)
        {
            Add(gc.objectRealList, thing);
            Add(gc.objectRealListActive, thing);
            Add(gc.objectRealListUpdate, thing);
            Add(gc.objectRealListLateUpdate, thing);
            Add(gc.objectRealListWithDestroyed, thing);
            if (gc.playfieldObjectDic != null && !gc.playfieldObjectDic.ContainsKey(thing.UID))
                gc.playfieldObjectDic[thing.UID] = thing;
        }

        static void Add(List<ObjectReal>? list, ObjectReal thing)
        {
            if (list != null && !list.Contains(thing))
                list.Add(thing);
        }

        static void CaptureDebris(GameController gc, Mark mark)
        {
            if (gc.wreckageList != null)
            {
                for (int i = 0; i < gc.wreckageList.Count; i++)
                {
                    if (gc.wreckageList[i] != null)
                        mark.Debris.Add(gc.wreckageList[i].GetInstanceID());
                }
            }

            Remember(gc.wreckageList2, mark);
            Remember(gc.floorDecalsList, mark);
        }

        static void Remember(List<GameObject>? list, Mark mark)
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                    mark.Debris.Add(list[i].GetInstanceID());
            }
        }

        static void ClearDebris(GameController gc, Mark mark)
        {
            HashSet<int> kept = new HashSet<int>(mark.Debris);
            if (gc.wreckageList != null)
            {
                List<Item> piles = new List<Item>(gc.wreckageList);
                for (int i = 0; i < piles.Count; i++)
                {
                    Item pile = piles[i];
                    if (pile != null && !kept.Contains(pile.GetInstanceID()))
                        pile.DestroyMe();
                }
            }

            HideFresh(gc.wreckageList2, kept);
            HideFresh(gc.floorDecalsList, kept);
        }

        static void HideFresh(List<GameObject>? list, HashSet<int> kept)
        {
            if (list == null)
                return;

            List<GameObject> copy = new List<GameObject>(list);
            for (int i = 0; i < copy.Count; i++)
            {
                GameObject piece = copy[i];
                if (piece != null && !kept.Contains(piece.GetInstanceID()) && !HoldsSolid(piece, kept))
                {
                    piece.SetActive(false);
                    UnityEngine.Object.Destroy(piece);
                }
            }
        }

        static bool HoldsSolid(GameObject piece, HashSet<int> kept)
        {
            if (piece.CompareTag("Wall"))
                return true;

            ObjectReal host = piece.GetComponentInParent<ObjectReal>();
            return host != null;
        }

        static void FinishSolids(GameController gc, Mark mark)
        {
            if (gc.tileInfo != null)
            {
                for (int i = 0; i < mark.Walls.Count; i++)
                {
                    Wall wall = mark.Walls[i];
                    if (wall.Gone || Doorish(wall.Material))
                        continue;

                    Vector3 at = wall.At;
                    TileData live = gc.tileInfo.GetTileData(at);
                    if (live == null)
                        continue;
                    if (live.wallMaterial == wallMaterialType.None && wall.Material != wallMaterialType.None)
                        live.wallMaterial = wall.Material;
                    if (live.wallMaterial == wallMaterialType.None)
                        continue;

                    Solidify(gc, wall, at);
                }

                FlushMaps();
                FreeOpenings(gc, mark);
            }

            Physics2D.SyncTransforms();

            for (int i = 0; i < mark.Props.Count; i++)
            {
                Prop prop = mark.Props[i];
                ObjectReal? thing = prop.Thing;
                if (prop.Destroyed || thing == null || SideBeam(thing))
                    continue;
                if (thing.go != null && !thing.go.activeSelf)
                    thing.go.SetActive(true);
                thing.destroyed = false;
                thing.destroying = false;
                Rejoin(gc, thing);
                Harden(thing, prop);
                PutSolidsBack(prop);
                MarkSolid(gc, thing, prop.ColliderOn || !prop.MeleePass);
            }
        }

        static bool SideBeam(ObjectReal thing)
        {
            Laser? beam = thing as Laser;
            if (beam == null)
                return false;

            LaserEmitter? host = beam.laserEmitter != null ? beam.laserEmitter : beam.laserEmitter2;
            if (host == null)
                return false;
            if (host.emittingLaser)
                return false;
            return host.otherLaserEmitter == null || !host.otherLaserEmitter.emittingLaser;
        }

        static void RepairSprite(ObjectReal thing, string sprite)
        {
            if (string.IsNullOrEmpty(sprite) || thing.destroyed)
                return;

            thing.SetObjectSprite(sprite);
        }

        static void RestoreContents(InvDatabase? inventory, List<Stack> saved)
        {
            if (inventory == null || inventory.InvItemList == null || saved.Count == 0 && inventory.InvItemList.Count == 0)
                return;

            List<InvItem> current = new List<InvItem>();
            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                if (inventory.InvItemList[i] != null)
                    current.Add(inventory.InvItemList[i]);
            }

            ForgetFormWeapons(current);

            List<InvItem> used = new List<InvItem>();
            for (int i = 0; i < saved.Count; i++)
            {
                Stack stack = saved[i];
                InvItem? found = Take(current, used, stack);
                if (found != null)
                {
                    Apply(found, stack);
                    used.Add(found);
                    continue;
                }

                InvItem? added = Give(inventory, stack);
                if (added != null)
                    used.Add(added);
            }

            for (int i = 0; i < current.Count; i++)
            {
                InvItem item = current[i];
                if (used.Contains(item) || DragonArms.IsFormWeapon(item))
                    continue;

                item.cantDrop = false;
                inventory.DestroyItem(item, false, false);
            }
        }

        static void CaptureFires(GameController gc, Mark mark)
        {
            if (gc.firesList == null)
                return;

            for (int i = 0; i < gc.firesList.Count; i++)
            {
                Fire fire = gc.firesList[i];
                if (fire == null || fire.tr == null)
                    continue;

                mark.Flames.Add(new Blaze
                {
                    Thing = fire,
                    Id = fire.GetInstanceID(),
                    At = fire.tr.position,
                    Oil = fire.hasSpreadOil || fire.oilFireTime > 0,
                    Forever = fire.neverGoOut,
                });
            }
        }

        static void ClearShots(GameController gc)
        {
            if (gc.bulletList != null)
            {
                List<Bullet> shots = new List<Bullet>(gc.bulletList);
                for (int i = 0; i < shots.Count; i++)
                {
                    if (shots[i] != null)
                        shots[i].DestroyMe();
                }
            }

            if (gc.explosionList == null)
                return;

            List<Explosion> blasts = new List<Explosion>(gc.explosionList);
            for (int i = 0; i < blasts.Count; i++)
            {
                if (blasts[i] != null)
                    blasts[i].DestroyMe();
            }
        }

        static void ClearNewFires(GameController gc, Mark mark)
        {
            if (gc.firesList == null)
                return;

            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Flames.Count; i++)
                kept.Add(mark.Flames[i].Id);

            List<Fire> fires = new List<Fire>(gc.firesList);
            for (int i = 0; i < fires.Count; i++)
            {
                Fire fire = fires[i];
                if (fire != null && !kept.Contains(fire.GetInstanceID()))
                    fire.DestroyMe();
            }

            for (int i = 0; i < mark.Flames.Count; i++)
            {
                Fire? fire = mark.Flames[i].Thing;
                if (fire != null && fire.tr != null)
                    fire.tr.position = mark.Flames[i].At;
            }
        }

        static void PutFiresBack(GameController gc, Mark mark)
        {
            if (gc.spawnerMain == null)
                return;

            for (int i = 0; i < mark.Flames.Count; i++)
            {
                Blaze blaze = mark.Flames[i];
                Fire? fire = blaze.Thing;
                if (fire != null && fire.tr != null)
                    continue;

                gc.spawnerMain.SpawnFire(null, blaze.At, blaze.Oil, blaze.Forever);
            }
        }

        static void ClearNewLoot(GameController gc, Mark mark)
        {
            if (gc.itemList == null)
                return;

            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Loot.Count; i++)
                kept.Add(mark.Loot[i].Id);

            List<Item> items = new List<Item>(gc.itemList);
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (item != null && !kept.Contains(item.GetInstanceID()))
                    EraseWorldItem(item);
            }
        }

        static void PutLootBack(GameController gc, Mark mark)
        {
            if (gc.spawnerMain == null)
                return;

            for (int i = 0; i < mark.Loot.Count; i++)
            {
                Loot loot = mark.Loot[i];
                Item? item = loot.Thing;
                if (item == null || item.tr == null)
                {
                    if (!string.IsNullOrEmpty(loot.Name))
                        item = gc.spawnerMain.SpawnItem(loot.At, loot.Name);
                }
                else
                {
                    item.tr.position = loot.At;
                }

                if (item == null)
                    continue;

                loot.Thing = item;
                loot.Id = item.GetInstanceID();
                item.beingThrown = false;
                if (item.rb != null)
                    item.rb.velocity = Vector2.zero;
                if (loot.Owner != null)
                    item.owner = loot.Owner;
                if (item.invItem != null)
                    Apply(item.invItem, loot.Data);
            }

            RememberLoot(gc, mark);
        }

        static void RememberLoot(GameController gc, Mark mark)
        {
            if (gc.itemList == null)
                return;

            for (int i = 0; i < mark.Loot.Count; i++)
            {
                mark.KeptLoot.Add(mark.Loot[i].Id);
                Item? thing = mark.Loot[i].Thing;
                if (thing != null)
                    mark.KeptLoot.Add(thing.GetInstanceID());
            }
        }

        static void SweepLoot(GameController gc, Mark mark)
        {
            if (gc.itemList == null)
                return;

            List<Item> items = new List<Item>(gc.itemList);
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (item != null && !mark.KeptLoot.Contains(item.GetInstanceID()))
                    EraseWorldItem(item);
            }
        }

        static void EraseWorldItem(Item item)
        {
            if (item.invItem != null && IsCarriedKey(item.invItem.invItemName))
            {
                item.pickingUp = true;
                item.destroyedForReal = true;
            }

            item.DestroyMe();
        }

        static void ClearNewPeople(GameController gc, Agent hero, Mark mark)
        {
            if (gc.agentList == null)
                return;

            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Bodies.Count; i++)
            {
                Agent? who = mark.Bodies[i].Who;
                if (who != null)
                    kept.Add(who.GetInstanceID());
            }

            List<Agent> people = new List<Agent>(gc.agentList);
            for (int i = 0; i < people.Count; i++)
            {
                Agent agent = people[i];
                if (agent == null || agent == hero || agent.isPlayer != 0 || kept.Contains(agent.GetInstanceID()))
                    continue;

                agent.DestroyMe();
            }
        }

        static void PutPeopleBack(GameController gc, Agent hero, Mark mark)
        {
            for (int i = 0; i < mark.Bodies.Count; i++)
            {
                Body body = mark.Bodies[i];
                Agent? agent = body.Who;
                if (agent == null || agent.tr == null || agent.objectAgent)
                    continue;

                if (body.Dead)
                {
                    agent.dead = true;
                    ShowHealth(agent, 0f);
                    agent.Teleport(body.At);
                    if (agent.rigidBody2D != null)
                        agent.rigidBody2D.velocity = Vector2.zero;
                    continue;
                }

                if (agent.dead && agent.statusEffects != null)
                    agent.statusEffects.Resurrect(false);
                agent.dead = false;
                agent.knockedOut = false;
                agent.health = body.Health;
                agent.currentHealth = Mathf.Max(0, Mathf.RoundToInt(body.Health));
                agent.Teleport(body.At);
                if (agent.rigidBody2D != null)
                    agent.rigidBody2D.velocity = Vector2.zero;
                agent.playerDir = body.Facing;
                agent.curAngle = body.Angle;
                agent.frozen = body.Frozen;
                agent.invisible = body.Invisible;
                agent.attackCooldown = body.AttackCooldown;
                agent.weaponCooldown = body.WeaponCooldown;
                agent.investigatingCrime = body.Investigating;
                agent.investigatePosition = body.InvestigateAt;
                agent.prisoner = body.Prisoner;
                agent.employer = body.Employer;
                agent.hasEmployer = body.HasEmployer;
                agent.following = body.Following;
                agent.deliveriesMade = body.Deliveries;
                agent.gaveFreeSlave = body.FreedSlave;
                agent.hasQuestGiverQuest = body.QuestGiver;
                agent.hasQuestEnderQuest = body.QuestEnder;
                agent.questGiverQuest = body.GiverQuest;
                agent.questEnderQuest = body.EnderQuest;
                agent.killForQuest = body.KillQuest;
                agent.rescueForQuest = body.RescueQuest;
                agent.curQuest = body.CurrentQuest;
                agent.lastTeleportedQuest = body.LastTeleport;
                agent.questTrigger = body.QuestTrigger;
                if (body.KeepQuests && agent.myQuests != null)
                {
                    agent.myQuests.Clear();
                    for (int q = 0; q < body.Quests.Count; q++)
                        agent.myQuests.Add(body.Quests[q]);
                }
                if (agent.melee != null)
                    agent.melee.HideMelee();
                PutEffectsBack(agent, body);
                PutRelBack(agent, body);
                ForgetFight(hero, agent);
                if (agent != hero)
                    PutPocketsBack(agent, body);
                allowKnockout = true;
                try
                {
                    agent.knockedOut = body.KnockedOut;
                    agent.sleepKnockedOut = body.Sleeping;
                    agent.healthBeforeKnockout = body.HealthBeforeKnockout;
                }
                finally
                {
                    allowKnockout = false;
                }

                agent.health = body.Health;
                ShowHealth(agent, body.Health);
            }
        }

        static void ShowHealth(Agent agent, float health)
        {
            int shown = Mathf.Max(0, Mathf.RoundToInt(health));
            agent.health = health;
            agent.currentHealth = shown;
            HealthBar bar = agent.healthBar;
            if (bar == null)
                return;

            bar.healthBarChanging = false;
            bar.healthBar2Changing = false;
            bar.SetHealthBar();
            bar.UpdateHealthText();
        }

        static void CapturePockets(Agent agent, Body body)
        {
            InvDatabase inventory = agent.inventory;
            if (inventory == null)
                return;

            if (inventory.equippedWeapon != null)
                body.Weapon = HeldName(agent, inventory.equippedWeapon);
            if (inventory.equippedArmor != null)
                body.Armor = inventory.equippedArmor.invItemName ?? "";
            if (inventory.equippedArmorHead != null)
                body.Head = inventory.equippedArmorHead.invItemName ?? "";

            CaptureContents(inventory, body.Pockets);
        }

        static void PutPocketsBack(Agent agent, Body body)
        {
            if (agent.inventory == null)
                return;

            RestoreContents(agent.inventory, body.Pockets);
            EquipWorn(agent.inventory, body.Armor, body.Head);
            if (!string.IsNullOrEmpty(body.Weapon))
            {
                InvItem? weapon = DragonArms.RealItem(agent.inventory, body.Weapon);
                if (weapon != null)
                    agent.inventory.EquipWeapon(weapon, true);
            }

            agent.inventory.MoneyCheck();
        }

        static void PutEffectsBack(Agent agent, Body body)
        {
            if (agent.statusEffects == null || agent.statusEffects.StatusEffectList == null)
                return;

            HashSet<string> wanted = new HashSet<string>();
            for (int i = 0; i < body.Effects.Count; i++)
                wanted.Add(body.Effects[i].Name);

            List<string> extra = new List<string>();
            List<StatusEffect> list = agent.statusEffects.StatusEffectList;
            for (int i = 0; i < list.Count; i++)
            {
                StatusEffect effect = list[i];
                if (effect == null || string.IsNullOrEmpty(effect.statusEffectName) || effect.statusEffectName == SnakeEffect.EffectId)
                    continue;
                if (!wanted.Contains(effect.statusEffectName))
                    extra.Add(effect.statusEffectName);
            }

            for (int i = 0; i < extra.Count; i++)
                agent.statusEffects.RemoveStatusEffect(extra[i], false);

            for (int i = 0; i < body.Effects.Count; i++)
            {
                EffectSnap snap = body.Effects[i];
                StatusEffect? live = FindEffect(agent, snap.Name);
                if (live != null)
                {
                    live.infiniteTime = snap.Infinite;
                    if (!snap.Infinite)
                        live.curTime = snap.Time;
                    continue;
                }

                if (snap.Infinite)
                    agent.statusEffects.AddStatusEffect(snap.Name);
                else
                    agent.statusEffects.AddStatusEffect(snap.Name, Mathf.Max(1, snap.Time));
            }
        }

        static void PutRelBack(Agent agent, Body body)
        {
            if (agent.relationships == null || agent.relationships.RelList == null)
                return;

            HashSet<int> saved = new HashSet<int>();
            for (int i = 0; i < body.Rels.Count; i++)
            {
                RelSnap snap = body.Rels[i];
                if (snap.Other == null)
                    continue;

                saved.Add(snap.Other.GetInstanceID());
                Relationship rel = agent.relationships.GetRelationship(snap.Other);
                if (rel == null)
                    continue;

                WriteRel(rel, snap.Kind, snap.CodeStatus, snap.Hate, snap.Strikes, snap.DidHostile, snap.DidAnger);
                rel.secretHate = snap.Secret;
                rel.damageDone = snap.Hurt;
                rel.annoyedCountdown = snap.Annoyed;
                rel.lastSawPosition = snap.LastSaw;
                rel.lastSawTimer = snap.LastSawTimer;
            }

            List<Relationship> list = agent.relationships.RelList;
            for (int i = 0; i < list.Count; i++)
            {
                Relationship rel = list[i];
                if (rel == null || rel.agent == null || saved.Contains(rel.agent.GetInstanceID()))
                    continue;

                string kind = string.IsNullOrEmpty(rel.initialRelType) ? "Neutral" : rel.initialRelType;
                WriteRel(rel, kind, rel.initialRelTypeCode, 0f, 0, false, false);
                rel.annoyedCountdown = 0f;
            }

            RefreshRelLists(agent.relationships);
        }

        static void ForgetFight(Agent hero, Agent agent)
        {
            if (agent == hero || agent.relationships == null)
                return;

            Relationship rel = agent.relationships.GetRelationship(hero);
            if (rel != null && rel.relTypeCode == relStatus.Hostile)
                return;

            if (agent.opponent == hero)
            {
                agent.opponent = null;
                agent.hasOpponent = false;
            }

            if (agent.lastHitByAgent == hero)
                agent.lastHitByAgent = null;
            agent.inCombat = false;
            if (agent.brain == null || agent.brain.Goals == null)
                return;

            List<Goal> goals = agent.brain.Goals;
            for (int i = goals.Count - 1; i >= 0; i--)
            {
                Goal goal = goals[i];
                if (goal == null)
                    continue;
                if (goal.goalCode == goalType.Battle || goal.goalCode == goalType.Flee || goal.goalCode == goalType.FleeDanger || goal.goalCode == goalType.NoiseReact)
                    goals.RemoveAt(i);
            }
        }

        static void WriteRel(Relationship rel, string kind, relStatus code, float hate, int strikes, bool hostile, bool anger)
        {
            rel.relType = string.IsNullOrEmpty(kind) ? RelName(code) : kind;
            rel.relTypeCode = code;
            rel.relHate = hate;
            rel.relStrikes = strikes;
            rel.didHostile = hostile;
            rel.didAnger = anger;
        }

        static string RelName(relStatus code)
        {
            switch (code)
            {
                case relStatus.Aligned: return "Aligned";
                case relStatus.Loyal: return "Loyal";
                case relStatus.Friendly: return "Friendly";
                case relStatus.Annoyed: return "Annoyed";
                case relStatus.Hostile: return "Hateful";
                case relStatus.Submissive: return "Submissive";
                default: return "Neutral";
            }
        }

        static void PutBagBack(Agent hero, Mark mark)
        {
            InvDatabase inventory = hero.inventory;
            if (inventory == null || inventory.InvItemList == null)
                return;

            List<InvItem> current = new List<InvItem>();
            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                if (inventory.InvItemList[i] != null)
                    current.Add(inventory.InvItemList[i]);
            }

            ForgetFormWeapons(current);

            List<InvItem> used = new List<InvItem>();
            for (int i = 0; i < mark.Bag.Count; i++)
            {
                Stack stack = mark.Bag[i];
                InvItem? found = Take(current, used, stack);
                if (found != null)
                {
                    Apply(found, stack);
                    used.Add(found);
                    continue;
                }

                InvItem? added = Give(inventory, stack);
                if (added != null)
                    used.Add(added);
            }

            for (int i = 0; i < current.Count; i++)
            {
                InvItem item = current[i];
                if (used.Contains(item) || item.invItemName == SnakeTalismanItem.ItemId || item.invItemName == SecondChanceAbility.Id || DragonArms.IsFormWeapon(item))
                    continue;

                item.cantDrop = false;
                inventory.DestroyItem(item, false, false);
            }

            if (!string.IsNullOrEmpty(mark.Weapon))
            {
                InvItem? weapon = DragonArms.RealItem(inventory, mark.Weapon);
                if (weapon != null)
                    inventory.EquipWeapon(weapon, true);
            }

            DragonArms.KeepInHand(hero);
            EquipWorn(inventory, mark.Armor, mark.Head);
            inventory.MoneyCheck();
        }

        static void PutWallsBack(GameController gc, Mark mark)
        {
            if (gc.tileInfo == null)
                return;

            for (int i = 0; i < mark.Walls.Count; i++)
            {
                Wall wall = mark.Walls[i];
                Vector3 at = wall.At;
                bool doorish = Doorish(wall.Material);
                TileData live = gc.tileInfo.GetTileData(at);
                if (live == null)
                    continue;

                if (wall.Gone)
                {
                    if (!live.destroyedWall && live.wallMaterial != wallMaterialType.None && !doorish)
                        gc.tileInfo.DestroyWallTileAtPosition(at.x, at.y, false, null);
                    continue;
                }

                if (doorish)
                    continue;

                Solidify(gc, wall, at);
            }

            FlushMaps();
        }

        static void WakeWall(GameController gc, Wall wall, Vector3 at)
        {
            GameObject? blocker = wall.Blocker;
            if (blocker == null && gc.tileInfo != null)
                blocker = gc.tileInfo.IsOverlapping(new Vector2(at.x, at.y), "Wall");
            if (blocker == null)
                return;

            if (!blocker.activeSelf)
                blocker.SetActive(true);

            Collider2D[] shapes = blocker.GetComponents<Collider2D>();
            for (int i = 0; i < shapes.Length; i++)
            {
                if (shapes[i] == null)
                    continue;
                shapes[i].enabled = true;
                shapes[i].isTrigger = false;
            }

            if (gc.collidersList == null)
                return;

            BoxCollider2D box = blocker.GetComponent<BoxCollider2D>();
            if (box == null)
                return;

            for (int i = 0; i < gc.collidersList.Count; i++)
            {
                ColliderInfo info = gc.collidersList[i];
                if (info != null && info.go == blocker)
                    return;
            }

            gc.collidersList.Add(new ColliderInfo
            {
                go = blocker,
                collider = box,
                pos = blocker.transform.position,
            });
        }

        static void ClearDetonator(Door door)
        {
            if (door.tr == null)
                return;

            for (int i = door.tr.childCount - 1; i >= 0; i--)
            {
                Transform child = door.tr.GetChild(i);
                if (child == null)
                    continue;
                string name = child.name ?? "";
                if (name.StartsWith("DoorDetonator"))
                    UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        static void Solidify(GameController gc, Wall wall, Vector3 at)
        {
            if (gc.tileInfo == null)
                return;

            TileData live = gc.tileInfo.GetTileData(at);
            if (live == null)
                return;
            if (wall.Material == wallMaterialType.Door || wall.Material == wallMaterialType.DoorOpen || wall.Material == wallMaterialType.Window)
                return;

            live.destroyedWall = false;
            live.wallSide = wall.Side;
            live.wallMaterial = wall.Material;
            live.wallMaterialOffset = wall.Skin;
            live.wallMaterialOffsetTop = wall.SkinTop;
            if (wall.Size.x > 0f)
                live.wallSize = wall.Size;
            live.wallOffset = wall.Offset;
            PaintBlast(wall);

            if (wall.FaceLive && PutBody(wall.Face, wall.FaceAt, wall.FaceSize, wall.FaceOffset, wall.FaceLayer))
                live.wallCollider = wall.Face;
            if (wall.ReachLive && PutBody(wall.Reach, wall.ReachAt, wall.ReachSize, wall.ReachOffset, wall.FaceLayer))
                live.wallColliderUnreachable = wall.Reach;

            if ((wall.FaceLive || wall.Size.x > 0f || wall.Blocker != null) && !HasWall(gc, BodyPlace(wall, at)))
                GrowBody(gc, live, wall, at);

            WakeWall(gc, wall, BodyPlace(wall, at));
        }

        static bool HasWall(GameController gc, Vector3 at)
        {
            return gc.tileInfo != null && gc.tileInfo.IsOverlapping(at, "Wall", 0.2f) != null;
        }

        static Vector3 BodyPlace(Wall wall, Vector3 fallback)
        {
            if (wall.WallsMap == null)
                return fallback;

            Vector3 tile = wall.WallsMap.GetTilePosition(wall.CellX, wall.CellY);
            return new Vector3(tile.x + 0.32f, tile.y + 0.32f, 0f);
        }

        static void GrowBody(GameController gc, TileData live, Wall wall, Vector3 fallback)
        {
            if (live.wallSize.x <= 0f)
                live.wallSize = wall.Size.x > 0f ? wall.Size : new Vector2(0.64f, 0.64f);
            live.destroyedWall = false;
            if (live.wallCollider != null && !live.wallCollider.gameObject.activeInHierarchy)
                live.wallCollider = null;

            StreamingTileArray? stream = wall.Home;
            if (gc.streamingWorld && stream == null)
                return;

            GameObject? made = gc.tileInfo.BuildWallObject(live, stream, false);
            if (made == null && live.wallCollider != null)
                made = live.wallCollider.gameObject;
            if (made == null)
                return;

            Vector3 place = BodyPlace(wall, fallback);
            made.transform.position = new Vector3(place.x, place.y, made.transform.position.z);
            made.tag = "Wall";
            if (!made.activeSelf)
                made.SetActive(true);
            BoxCollider2D box = made.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.enabled = true;
                box.isTrigger = false;
                box.size = CellSize(wall);
                box.offset = Vector2.zero;

                live.wallCollider = box;
            }

            wall.Blocker = made;
        }

        static Vector2 CellSize(Wall wall)
        {
            float x = wall.Size.x > 0f ? wall.Size.x : 0.64f;
            float y = wall.Size.y > 0f ? wall.Size.y : 0.64f;
            if (x > 0.64f)
                x = 0.64f;
            if (y > 0.64f)
                y = 0.64f;
            return new Vector2(x, y);
        }

        static void FreeOpenings(GameController gc, Mark mark)
        {
            for (int i = 0; i < mark.Props.Count; i++)
            {
                Prop prop = mark.Props[i];
                if (prop.Destroyed || prop.Thing == null || prop.Thing.tr == null)
                    continue;
                if (!prop.IsDoor && !prop.IsWindow)
                    continue;

                Collider2D[] hits = Physics2D.OverlapPointAll(prop.Thing.tr.position);
                for (int h = 0; h < hits.Length; h++)
                {
                    Collider2D hit = hits[h];
                    if (hit == null || !hit.CompareTag("Wall"))
                        continue;
                    if (hit.transform == prop.Thing.tr || hit.transform.IsChildOf(prop.Thing.tr))
                        continue;

                    hit.enabled = false;
                }
            }
        }

        static void NoteSill(Wall wall, tk2dTileMap tops, int x, int y, bool streaming)
        {
            if (streaming)
                Note(wall, tops, x + 1, y - 1, 0);
            else
                Note(wall, tops, x, y, 0);
        }

        static void PaintWindows(Mark mark)
        {
            for (int i = 0; i < mark.Walls.Count; i++)
            {
                Wall wall = mark.Walls[i];
                if (wall.Material != wallMaterialType.Window || wall.Gone)
                    continue;
                PaintBlast(wall);
            }

            FlushMaps();
        }

        static void NoteBlast(Wall wall, StreamingTileArray? stream, tk2dTileMap walls, tk2dTileMap tops, tk2dTileMap shadows, int x, int y, bool streaming)
        {
            wall.WallsMap = walls;
            wall.CellX = x;
            wall.CellY = y;
            Note(wall, walls, x, y, 0);
            int num = x;
            int num2 = y;
            int num3 = x;
            int num4 = y;
            if (streaming)
            {
                num++;
                num2--;
                num3++;
                num4++;
            }

            for (int layer = 0; layer < 10; layer++)
                Note(wall, tops, num, num2 + 1, layer);

            if (!streaming || stream == null)
            {
                Note(wall, shadows, num3 + 1, num4, 0);
                Note(wall, shadows, num3, num4 + 1, 0);
                Note(wall, shadows, num3 + 1, num4 + 1, 0);
                return;
            }

            bool plain = true;
            if (num3 + 1 == 17 && num4 + 1 == 17 && stream.hasTileArrayNE)
            {
                if (stream.hasTileArrayE && stream.tileArrayE != null && stream.tileArrayE.streamingTileMaps != null)
                    Note(wall, stream.tileArrayE.streamingTileMaps.tilemapShadows, 1, num4, 0);
                if (stream.hasTileArrayN && stream.tileArrayN != null && stream.tileArrayN.streamingTileMaps != null)
                    Note(wall, stream.tileArrayN.streamingTileMaps.tilemapShadows, num3, 1, 0);
                if (stream.tileArrayNE != null && stream.tileArrayNE.streamingTileMaps != null)
                    Note(wall, stream.tileArrayNE.streamingTileMaps.tilemapShadows, 1, 1, 0);
                plain = false;
            }
            else if (num3 + 1 == 17 && stream.hasTileArrayE && stream.tileArrayE != null && stream.tileArrayE.streamingTileMaps != null)
            {
                Note(wall, stream.tileArrayE.streamingTileMaps.tilemapShadows, 1, num4, 0);
                Note(wall, shadows, num3, num4 + 1, 0);
                Note(wall, stream.tileArrayE.streamingTileMaps.tilemapShadows, 1, num4 + 1, 0);
                plain = false;
            }
            else if (num4 + 1 == 17 && stream.hasTileArrayN && stream.tileArrayN != null && stream.tileArrayN.streamingTileMaps != null)
            {
                Note(wall, shadows, num3 + 1, num4, 0);
                Note(wall, stream.tileArrayN.streamingTileMaps.tilemapShadows, num3, 1, 0);
                Note(wall, stream.tileArrayN.streamingTileMaps.tilemapShadows, num3 + 1, 1, 0);
                plain = false;
            }

            if (plain)
            {
                Note(wall, shadows, num3 + 1, num4, 0);
                Note(wall, shadows, num3, num4 + 1, 0);
                Note(wall, shadows, num3 + 1, num4 + 1, 0);
            }
        }

        static void Note(Wall wall, tk2dTileMap map, int x, int y, int layer)
        {
            if (map == null)
                return;

            int id = ReadTile(map, x, y, layer);
            if (id < 0)
                return;

            wall.Stamps.Add(new Stamp
            {
                Map = map,
                X = x,
                Y = y,
                Layer = layer,
                Id = id,
            });
        }

        static int ReadTile(tk2dTileMap map, int x, int y, int layer)
        {
            try
            {
                return map.GetTile(x, y, layer);
            }
            catch
            {
                return -1;
            }
        }

        static void PaintBlast(Wall wall)
        {
            for (int i = 0; i < wall.Stamps.Count; i++)
            {
                Stamp stamp = wall.Stamps[i];
                if (stamp.Map == null || stamp.Id < 0)
                    continue;

                try
                {
                    stamp.Map.SetTile(stamp.X, stamp.Y, stamp.Layer, stamp.Id);
                    dirtyMaps.Add(stamp.Map);
                }
                catch
                {
                }
            }
        }

        static readonly HashSet<tk2dTileMap> dirtyMaps = new HashSet<tk2dTileMap>();

        static void FlushMaps()
        {
            foreach (tk2dTileMap map in dirtyMaps)
            {
                if (map != null)
                    map.Build();
            }

            dirtyMaps.Clear();
        }

        static bool IsCarriedKey(string name)
        {
            return name == "Key" || name == "KeyCard" || name == "SafeCombination";
        }

        static bool PutBody(BoxCollider2D? collider, Vector3 place, Vector2 size, Vector2 offset, int layer)
        {
            if (collider == null)
                return false;

            GameObject body = collider.gameObject;
            if (body.activeInHierarchy && Vector2.Distance(body.transform.position, place) > 0.4f)
                return false;

            if (!body.activeSelf)
                body.SetActive(true);
            body.transform.position = place;
            body.tag = "Wall";
            if (layer != 0)
                body.layer = layer;
            collider.enabled = true;
            collider.isTrigger = false;
            if (size.x > 0f || size.y > 0f)
            {
                collider.size = size;
                collider.offset = offset;
            }

            return true;
        }

        static void SealTile(GameController gc, Vector3 at)
        {
            if (gc.tileInfo == null)
                return;

            TileData live = gc.tileInfo.GetTileData(at);
            if (live == null)
                return;

            WakeCollider(live.wallCollider);
            WakeCollider(live.wallColliderUnreachable);
        }

        static void PoseMachine(ObjectReal thing, Prop prop)
        {
            if (thing.animator != null)
            {
                if (prop.AnimOn && prop.AnimHash != 0)
                {
                    thing.animator.enabled = true;
                    thing.animator.speed = prop.AnimSpeed > 0f ? prop.AnimSpeed : 1f;
                    thing.animator.Play(prop.AnimHash, 0, prop.AnimTime);
                    thing.animator.Update(0f);
                }
                else
                {
                    thing.animator.speed = 1f;
                    thing.animator.enabled = false;
                }
            }

            if (!prop.AnimOn)
                RepairSprite(thing, prop.Sprite);
            if (thing.objectSprite == null)
                return;

            bool flash = prop.SpriteFlash || prop.Flashing;
            thing.objectSprite.flashingRepeatedly = flash;
            if (!flash)
                thing.objectSprite.flashRepeatedlyOn = false;
        }

        static void RememberMarker(PlayfieldObject host, Body body)
        {
            Component? marker = MarkerOf(host);
            if (marker == null)
                return;

            body.HadMarker = true;
            body.MarkerKind = MarkerKind(marker);
            body.MarkerQuest = MarkerQuest(marker);
        }

        static void RememberMarker(PlayfieldObject host, Prop prop)
        {
            Component? marker = MarkerOf(host);
            if (marker == null)
                return;

            prop.HadMarker = true;
            prop.MarkerKind = MarkerKind(marker);
            prop.MarkerQuest = MarkerQuest(marker);
        }

        static Component? MarkerOf(PlayfieldObject host)
        {
            System.Reflection.FieldInfo field = host.GetType().GetField("questObjectMarker");
            if (field == null)
                return null;

            return field.GetValue(host) as Component;
        }

        static Quest? MarkerQuest(Component marker)
        {
            System.Reflection.FieldInfo field = marker.GetType().GetField("quest");
            if (field == null)
                return null;

            return field.GetValue(marker) as Quest;
        }

        static bool MarkerFlag(Component marker, string name)
        {
            System.Reflection.FieldInfo field = marker.GetType().GetField(name);
            return field != null && field.GetValue(marker) is bool value && value;
        }

        static string MarkerKind(Component marker)
        {
            bool giver = MarkerFlag(marker, "giver");
            bool ender = MarkerFlag(marker, "ender");
            if (giver && ender)
                return "QuestGiverAndEnder";
            if (giver)
                return "QuestGiver";
            if (ender)
                return "QuestEnder";
            if (MarkerFlag(marker, "target"))
                return "QuestTarget";
            return "";
        }

        static void RememberAnim(ObjectReal thing, Prop prop)
        {
            if (thing.objectSprite != null)
                prop.SpriteFlash = thing.objectSprite.flashingRepeatedly;
            if (thing.animator == null || !thing.animator.enabled || thing.animator.runtimeAnimatorController == null)
                return;

            AnimatorStateInfo info = thing.animator.GetCurrentAnimatorStateInfo(0);
            prop.AnimOn = true;
            prop.AnimHash = info.fullPathHash;
            prop.AnimTime = info.normalizedTime;
            prop.AnimSpeed = thing.animator.speed;
        }

        static void RememberGraph(ObjectReal thing, Prop prop)
        {
            Component? scene = GraphOf(thing);
            if (scene == null)
                return;

            System.Reflection.FieldInfo tag = scene.GetType().GetField("setTag");
            if (tag == null)
                return;

            prop.HasGraph = true;
            object value = tag.GetValue(scene);
            if (value is int number)
                prop.GraphTag = number;
        }

        static Component? GraphOf(ObjectReal thing)
        {
            System.Reflection.FieldInfo field = thing.GetType().GetField("graphUpdateScene");
            if (field == null)
                return null;

            return field.GetValue(thing) as Component;
        }

        static void WriteGraph(Component scene, int tag)
        {
            System.Reflection.FieldInfo field = scene.GetType().GetField("setTag");
            if (field != null)
                field.SetValue(scene, tag);

            System.Reflection.MethodInfo apply = scene.GetType().GetMethod("Apply");
            if (apply != null)
                apply.Invoke(scene, null);
        }

        static void FillAgents(Quest quest, List<Agent> saved)
        {
            if (saved.Count == 0 && quest.questAgentList == null)
                return;

            if (quest.questAgentList == null)
                quest.questAgentList = new List<Agent>();
            quest.questAgentList.Clear();
            for (int i = 0; i < saved.Count; i++)
                quest.questAgentList.Add(saved[i]);
        }

        static void FillThings(Quest quest, List<ObjectReal> saved)
        {
            if (saved.Count == 0 && quest.questObjectList == null)
                return;

            if (quest.questObjectList == null)
                quest.questObjectList = new List<ObjectReal>();
            quest.questObjectList.Clear();
            for (int i = 0; i < saved.Count; i++)
                quest.questObjectList.Add(saved[i]);
        }

        static void PutSheetBack(GameController gc, Mark mark)
        {
            if (!mark.HasSheet || gc.playerAgent == null || gc.playerAgent.mainGUI == null)
                return;

            QuestSheet sheet = gc.playerAgent.mainGUI.questSheetScript;
            if (sheet == null || sheet.questSlot == null)
                return;

            int count = sheet.questSlot.Length;
            if (mark.Sheet.Count < count)
                count = mark.Sheet.Count;
            for (int i = 0; i < count; i++)
            {
                QuestSlot slot = sheet.questSlot[i];
                if (slot != null)
                    slot.myQuest = mark.Sheet[i];
            }

            sheet.UpdateQuest();
        }

        static void StickMarkers(GameController gc, Mark mark)
        {
            if (gc.quests == null)
                return;

            for (int i = 0; i < mark.Bodies.Count; i++)
            {
                Body body = mark.Bodies[i];
                if (body.Dead || body.Who == null)
                    continue;
                Stick(gc, body.Who, body.HadMarker, body.MarkerKind, body.MarkerQuest);
            }

            for (int i = 0; i < mark.Props.Count; i++)
            {
                Prop prop = mark.Props[i];
                if (prop.Destroyed || prop.Thing == null)
                    continue;
                Stick(gc, prop.Thing, prop.HadMarker, prop.MarkerKind, prop.MarkerQuest);
            }
        }

        static void Stick(GameController gc, PlayfieldObject host, bool had, string kind, Quest? quest)
        {
            if (!had || string.IsNullOrEmpty(kind) || MarkerOf(host) != null)
                return;

            gc.quests.CreateQuestMarker(host, quest!, kind);
        }

        static void WakeCollider(BoxCollider2D collider)
        {
            if (collider == null)
                return;

            GameObject body = collider.gameObject;
            if (body != null && !body.activeSelf)
                body.SetActive(true);
            collider.enabled = true;
            Component[] parts = body != null ? body.GetComponents<Component>() : new Component[0];
            for (int i = 0; i < parts.Length; i++)
            {
                Component part = parts[i];
                if (part == null || part.GetType().Name != "GraphUpdateScene")
                    continue;

                System.Reflection.MethodInfo apply = part.GetType().GetMethod("Apply");
                if (apply != null)
                    apply.Invoke(part, null);
            }
        }

        static Stack ReadItem(InvItem item)
        {
            return new Stack
            {
                Name = item.invItemName ?? "",
                Count = item.invItemCount,
                Contents = CopyWords(item.contents),
                Chunks = CopyNums(item.chunks),
                Sectors = CopyNums(item.sectors),
                Owner = item.ownerID,
                MaxAmmo = item.maxAmmo,
                Identified = item.identifiedContents,
                Chunk = item.specificChunk,
                Sector = item.specificSector,
                Used = item.used,
                Value = item.itemValue,
            };
        }

        static List<string>? CopyWords(List<string>? from)
        {
            return from == null ? null : new List<string>(from);
        }

        static List<int>? CopyNums(List<int>? from)
        {
            return from == null ? null : new List<int>(from);
        }

        static void Apply(InvItem item, Stack stack)
        {
            item.invItemCount = stack.Count;
            item.contents = CopyWords(stack.Contents);
            item.chunks = CopyNums(stack.Chunks);
            item.sectors = CopyNums(stack.Sectors);
            item.ownerID = stack.Owner;
            item.identifiedContents = stack.Identified;
            item.specificChunk = stack.Chunk;
            item.specificSector = stack.Sector;
            item.used = stack.Used;
            if (stack.MaxAmmo > 0)
                item.maxAmmo = stack.MaxAmmo;
            if (stack.Value > 0)
                item.itemValue = stack.Value;
        }

        static InvItem? Take(List<InvItem> current, List<InvItem> used, Stack stack)
        {
            InvItem? loose = null;
            for (int i = 0; i < current.Count; i++)
            {
                InvItem item = current[i];
                if (used.Contains(item) || item.invItemName != stack.Name)
                    continue;
                if (Same(item, stack))
                    return item;
                if (loose == null)
                    loose = item;
            }

            return loose;
        }

        static bool Same(InvItem item, Stack stack)
        {
            if (item.ownerID != stack.Owner || item.specificChunk != stack.Chunk || item.specificSector != stack.Sector)
                return false;

            return SameWords(item.contents, stack.Contents) && SameNums(item.chunks, stack.Chunks);
        }

        static bool SameWords(List<string>? live, List<string>? saved)
        {
            int liveCount = live == null ? 0 : live.Count;
            int savedCount = saved == null ? 0 : saved.Count;
            if (liveCount != savedCount)
                return false;
            for (int i = 0; i < liveCount; i++)
            {
                if (live == null || saved == null || live[i] != saved[i])
                    return false;
            }

            return true;
        }

        static bool SameNums(List<int>? live, List<int>? saved)
        {
            int liveCount = live == null ? 0 : live.Count;
            int savedCount = saved == null ? 0 : saved.Count;
            if (liveCount != savedCount)
                return false;
            for (int i = 0; i < liveCount; i++)
            {
                if (live == null || saved == null || live[i] != saved[i])
                    return false;
            }

            return true;
        }

        static bool HandOver(InvDatabase from, InvDatabase to, InvItem item)
        {
            if (from.InvItemList == null || !from.InvItemList.Remove(item))
                return false;

            if (from.failsafeItemList != null)
                from.failsafeItemList.Remove(item);
            if (from.equippedWeapon == item)
                from.equippedWeapon = null;

            return to.AddItem(item) != null;
        }

        static InvItem? Give(InvDatabase inventory, Stack stack)
        {
            int count = stack.Count > 0 ? stack.Count : 1;
            List<string> contents = stack.Contents ?? new List<string>();
            List<int> chunks = stack.Chunks ?? new List<int>();
            List<int> sectors = stack.Sectors ?? new List<int>();
            int net = InvItem.totalInvItems;
            InvItem.totalInvItems = net + 1;
            InvItem added = inventory.AddItem(
                stack.Name,
                count,
                contents,
                chunks,
                sectors,
                -1,
                false,
                false,
                0,
                net,
                false,
                "",
                false,
                0,
                stack.Identified,
                stack.Chunk,
                stack.Sector,
                -1,
                stack.MaxAmmo,
                stack.Value,
                false);
            if (added != null)
                Apply(added, stack);
            return added;
        }

        static void EquipWorn(InvDatabase inventory, string armor, string head)
        {
            if (string.IsNullOrEmpty(armor))
            {
                if (inventory.equippedArmor != null)
                    inventory.UnequipArmor(false);
            }
            else
            {
                InvItem piece = inventory.FindItem(armor);
                if (piece != null)
                    inventory.EquipArmor(piece, false);
            }

            if (string.IsNullOrEmpty(head))
            {
                if (inventory.equippedArmorHead != null)
                    inventory.UnequipArmorHead(false);
            }
            else
            {
                InvItem piece = inventory.FindItem(head);
                if (piece != null)
                    inventory.EquipArmorHead(piece, false);
            }
        }

        static StatusEffect? FindEffect(Agent agent, string name)
        {
            if (agent.statusEffects == null || agent.statusEffects.StatusEffectList == null)
                return null;

            List<StatusEffect> list = agent.statusEffects.StatusEffectList;
            for (int i = 0; i < list.Count; i++)
            {
                StatusEffect effect = list[i];
                if (effect != null && effect.statusEffectName == name)
                    return effect;
            }

            return null;
        }

        static void RefreshRelLists(Relationships rels)
        {
            if (rels.RelList == null)
                return;

            for (int i = 0; i < rels.RelList.Count; i++)
            {
                Relationship rel = rels.RelList[i];
                if (rel == null || rel.agent == null)
                    continue;

                Slot(rels.hateList, rel.agent, rel.relTypeCode == relStatus.Hostile);
                Slot(rels.loyalList, rel.agent, rel.relTypeCode == relStatus.Loyal);
                Slot(rels.alignedList, rel.agent, rel.relTypeCode == relStatus.Aligned || rel.relTypeCode == relStatus.Loyal);
            }
        }

        static void Slot(List<Agent>? list, Agent agent, bool inside)
        {
            if (list == null)
                return;
            if (inside && !list.Contains(agent))
                list.Add(agent);
            if (!inside && list.Contains(agent))
                list.Remove(agent);
        }

        static void WakeCamera(ObjectReal thing, Prop prop)
        {
            if (!prop.IsCam || prop.Destroyed || !prop.Functional)
                return;

            SecurityCam? cam = thing as SecurityCam;
            if (cam == null)
                return;

            cam.functional = true;
            cam.cameraTurningState = 0;
            if (cam.particles == null || cam.particles.Count == 0)
                cam.SwitchOn();
        }

        static void RecycleLight(Transform? glow)
        {
            if (glow == null)
                return;

            glow.gameObject.SetActive(false);
        }

        static string ShownSprite(ObjectReal thing)
        {
            if (thing.spr != null && thing.spr.CurrentSprite != null && !string.IsNullOrEmpty(thing.spr.CurrentSprite.name))
                return thing.spr.CurrentSprite.name;

            return thing.startingObjectSprite ?? "";
        }

        static void PutObjectBits(GameController gc, ObjectReal thing, Prop prop)
        {
            if (prop.IsCrate && thing is Crate crate)
                crate.locked = prop.CrateLocked;
            if (prop.IsMine && thing is Mine mine)
                mine.setOff = prop.MineSet;
            if (prop.IsManhole && thing is Manhole manhole)
                manhole.opened = prop.ManholeOpen;
            if (prop.IsTrap && thing is TrapDoor trap)
            {
                if (prop.TrapOpen && !trap.opened)
                    trap.OpenTrapDoor(false, false, false);
                else if (!prop.TrapOpen && trap.opened)
                    trap.CloseTrapDoor();
            }

            if (prop.IsPlate && thing is SwitchFloor plate && plate.switchOn != prop.PlateOn)
            {
                if (prop.PlateOn)
                    plate.SwitchOn(null);
                else
                {
                    plate.switchOn = false;
                    RepairSprite(plate, string.IsNullOrEmpty(prop.Sprite) ? "SwitchFloor" : prop.Sprite);
                }
            }
            if (prop.IsAlarm && thing is AlarmButton alarm)
            {
                alarm.switchOn = prop.AlarmOn;
                alarm.hacked = prop.AlarmHacked;
            }

            if (prop.IsLockWall && thing is LockdownWall wall)
                wall.wallUp = prop.WallUp;
            if (prop.IsEmitter && thing is LaserEmitter emitter)
            {
                if (prop.Emitting)
                {
                    if (emitter.laser == null && emitter.otherLaserEmitter != null)
                        emitter.laser = emitter.otherLaserEmitter.laser;
                    if (emitter.laser == null)
                        emitter.LaserOn();
                    else
                    {
                        emitter.emittingLaser = true;
                        RefreshBeam(gc, emitter);
                    }
                }
                else if (emitter.emittingLaser || emitter.laser != null)
                {
                    emitter.LaserOff();
                }
            }
            if (prop.IsLaser && thing is Laser laser)
                laser.tripLaserWaiting = prop.TripWaiting;
            if (prop.IsCam && thing is SecurityCam cam)
            {
                cam.spottedSomeone = prop.Spotted;
                cam.opponent = prop.CamOpponent;
                if (!prop.Spotted && cam.agentsInView != null)
                    cam.agentsInView.Clear();
            }

            if (prop.IsTurret && thing is Turret turret)
            {
                turret.stage = prop.TurretStage;
                turret.opponent = prop.TurretOpponent;
                if (turret.agentsWhoObjectHates != null)
                {
                    turret.agentsWhoObjectHates.Clear();
                    for (int i = 0; i < prop.TurretHate.Count; i++)
                        turret.agentsWhoObjectHates.Add(prop.TurretHate[i]);
                }
            }

            if (prop.IsComputer && thing is Computer computer)
            {
                computer.installedMalware = prop.Malware;
                computer.canPoison = prop.CanPoison;
                computer.electionHacked = prop.ElectionHacked;
            }
        }

        static void CaptureHoles(GameController gc, Mark mark)
        {
            if (gc.holeList == null)
                return;
            for (int i = 0; i < gc.holeList.Count; i++)
            {
                Hole hole = gc.holeList[i];
                if (hole != null)
                    mark.Holes.Add(hole.GetInstanceID());
            }
        }

        static void ClearFreshHoles(GameController gc, Mark mark)
        {
            if (gc.holeList == null)
                return;
            HashSet<int> kept = new HashSet<int>(mark.Holes);
            List<Hole> holes = new List<Hole>(gc.holeList);
            for (int i = 0; i < holes.Count; i++)
            {
                Hole hole = holes[i];
                if (hole != null && !kept.Contains(hole.GetInstanceID()))
                    hole.DestroyMe();
            }
        }

        static void CaptureClouds(GameController gc, Mark mark)
        {
            if (gc.gasesList == null)
                return;
            for (int i = 0; i < gc.gasesList.Count; i++)
            {
                Gas gas = gc.gasesList[i];
                if (gas == null || gas.tr == null)
                    continue;
                mark.Clouds.Add(new Cloud
                {
                    Thing = gas,
                    Id = gas.GetInstanceID(),
                    At = gas.tr.position,
                    Time = gas.timeLeft,
                    Stay = gas.neverGoAway,
                });
            }
        }

        static void ClearFreshClouds(GameController gc, Mark mark)
        {
            if (gc.gasesList == null)
                return;
            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Clouds.Count; i++)
                kept.Add(mark.Clouds[i].Id);

            List<Gas> gases = new List<Gas>(gc.gasesList);
            for (int i = 0; i < gases.Count; i++)
            {
                Gas gas = gases[i];
                if (gas != null && !kept.Contains(gas.GetInstanceID()))
                    gas.StopGas();
            }

            for (int i = 0; i < mark.Clouds.Count; i++)
            {
                Cloud cloud = mark.Clouds[i];
                if (cloud.Thing == null || cloud.Thing.tr == null)
                    continue;
                cloud.Thing.tr.position = cloud.At;
                cloud.Thing.timeLeft = cloud.Time;
                cloud.Thing.neverGoAway = cloud.Stay;
            }
        }

        static void CaptureBombs(GameController gc, Mark mark)
        {
            if (gc.bombsFallingList == null)
                return;
            for (int i = 0; i < gc.bombsFallingList.Count; i++)
            {
                BombFalling bomb = gc.bombsFallingList[i];
                if (bomb != null)
                    mark.Bombs.Add(bomb.GetInstanceID());
            }
        }

        static void ClearFreshBombs(GameController gc, Mark mark)
        {
            if (gc.bombsFallingList == null)
                return;
            HashSet<int> kept = new HashSet<int>(mark.Bombs);
            List<BombFalling> bombs = new List<BombFalling>(gc.bombsFallingList);
            for (int i = 0; i < bombs.Count; i++)
            {
                BombFalling bomb = bombs[i];
                if (bomb != null && !kept.Contains(bomb.GetInstanceID()))
                    bomb.DestroyMe();
            }
        }

        static void ClearNoise(GameController gc)
        {
            if (gc.noiseList == null)
                return;
            List<Noise> noises = new List<Noise>(gc.noiseList);
            for (int i = 0; i < noises.Count; i++)
            {
                if (noises[i] != null)
                    noises[i].DestroyMe();
            }
        }

        static void RefreshBeam(GameController gc, LaserEmitter emitter)
        {
            Laser? beam = emitter.laser;
            if (beam == null)
                return;

            string kind = emitter.securityType ?? "";
            if ((kind == "Noise" || kind == "Explosive" || kind == "Trap") && (beam.danger == null || beam.danger.dangerCollider == null) && gc.spawnerMain != null)
                gc.spawnerMain.SpawnDanger(beam, "Major", "Varied");
            beam.SetColor();
        }

        static void ClearDangers(GameController gc, Mark mark)
        {
            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Props.Count; i++)
            {
                ObjectReal? thing = mark.Props[i].Thing;
                if (thing != null)
                    kept.Add(thing.GetInstanceID());
            }

            DropDangers(gc.dangerList, kept);
            DropDangers(gc.dangerListNoCollider, kept);
        }

        static void DropDangers(List<Danger>? list, HashSet<int> kept)
        {
            if (list == null)
                return;
            List<Danger> copy = new List<Danger>(list);
            for (int i = 0; i < copy.Count; i++)
            {
                Danger danger = copy[i];
                if (danger == null)
                    continue;
                ObjectReal? host = danger.objectReal;
                if (host != null && kept.Contains(host.GetInstanceID()))
                    continue;
                if (danger.go != null)
                    UnityEngine.Object.Destroy(danger.go);
            }
        }

        static void ClearNewProps(GameController gc, Mark mark)
        {
            List<ObjectReal>? list = gc.objectRealListWithDestroyed != null ? gc.objectRealListWithDestroyed : gc.objectRealList;
            if (list == null)
                return;

            HashSet<int> kept = new HashSet<int>();
            for (int i = 0; i < mark.Props.Count; i++)
            {
                ObjectReal? thing = mark.Props[i].Thing;
                if (thing != null)
                    kept.Add(thing.GetInstanceID());
            }

            List<ObjectReal> copy = new List<ObjectReal>(list);
            for (int i = 0; i < copy.Count; i++)
            {
                ObjectReal thing = copy[i];
                if (thing != null && !kept.Contains(thing.GetInstanceID()))
                    thing.DestroyMe();
            }
        }

        static void CaptureJobs(GameController gc, Mark mark)
        {
            if (gc.quests == null)
                return;

            mark.FloorDone = gc.quests.floorComplete;
            if (gc.quests.questItemsGiven != null)
            {
                for (int i = 0; i < gc.quests.questItemsGiven.Count; i++)
                {
                    string name = gc.quests.questItemsGiven[i];
                    if (!string.IsNullOrEmpty(name))
                        mark.Given.Add(name);
                }
            }

            if (gc.quests.mainQuestList == null)
                return;

            for (int i = 0; i < gc.quests.mainQuestList.Count; i++)
            {
                Quest quest = gc.quests.mainQuestList[i];
                if (quest == null)
                    continue;

                bool[] killed = new bool[0];
                if (quest.killedOnQuest != null)
                {
                    killed = new bool[quest.killedOnQuest.Length];
                    for (int k = 0; k < killed.Length; k++)
                        killed[k] = quest.killedOnQuest[k];
                }

                Job job = new Job
                {
                    Quest = quest,
                    Status = quest.questStatus ?? "",
                    Code = quest.questStatusCode,
                    Accepted = quest.wasAccepted,
                    Discovered = quest.discovered,
                    Took = quest.tookRetrieveTarget,
                    Killed = killed,
                    Player = quest.player,
                    Trigger = quest.triggerPoint,
                    Trigger2 = quest.triggerPoint2,
                    Trigger3 = quest.triggerPoint3,
                    ListsReady = quest.createdLists,
                };
                if (quest.questAgentList != null)
                {
                    for (int a = 0; a < quest.questAgentList.Count; a++)
                    {
                        if (quest.questAgentList[a] != null)
                            job.People.Add(quest.questAgentList[a]);
                    }
                }

                if (quest.questObjectList != null)
                {
                    for (int o = 0; o < quest.questObjectList.Count; o++)
                    {
                        if (quest.questObjectList[o] != null)
                            job.Things.Add(quest.questObjectList[o]);
                    }
                }

                mark.Jobs.Add(job);
            }

            Agent? player = gc.playerAgent;
            QuestSheet? sheet = player != null && player.mainGUI != null ? player.mainGUI.questSheetScript : null;
            if (sheet != null && sheet.questSlot != null)
            {
                mark.HasSheet = true;
                for (int s = 0; s < sheet.questSlot.Length; s++)
                {
                    QuestSlot slot = sheet.questSlot[s];
                    mark.Sheet.Add(slot != null ? slot.myQuest : null);
                }
            }
        }

        static void PutJobsBack(GameController gc, Mark mark)
        {
            if (gc.quests == null)
                return;

            gc.quests.floorComplete = mark.FloorDone;
            if (gc.quests.questItemsGiven != null)
            {
                gc.quests.questItemsGiven.Clear();
                for (int i = 0; i < mark.Given.Count; i++)
                    gc.quests.questItemsGiven.Add(mark.Given[i]);
            }

            for (int i = 0; i < mark.Jobs.Count; i++)
            {
                Job job = mark.Jobs[i];
                Quest? quest = job.Quest;
                if (quest == null)
                    continue;

                quest.questStatus = job.Status;
                quest.questStatusCode = job.Code;
                quest.wasAccepted = job.Accepted;
                quest.discovered = job.Discovered;
                quest.tookRetrieveTarget = job.Took;
                quest.player = job.Player;
                quest.triggerPoint = job.Trigger;
                quest.triggerPoint2 = job.Trigger2;
                quest.triggerPoint3 = job.Trigger3;
                quest.createdLists = job.ListsReady;
                FillAgents(quest, job.People);
                FillThings(quest, job.Things);
                if (job.Killed == null)
                    continue;
                if (quest.killedOnQuest == null || quest.killedOnQuest.Length != job.Killed.Length)
                    quest.killedOnQuest = new bool[job.Killed.Length];
                for (int k = 0; k < job.Killed.Length; k++)
                    quest.killedOnQuest[k] = job.Killed[k];
            }

            if (gc.quests.mainQuestList != null)
            {
                gc.quests.mainQuestList.Clear();
                for (int i = 0; i < mark.Jobs.Count; i++)
                {
                    Quest? quest = mark.Jobs[i].Quest;
                    if (quest != null && !gc.quests.mainQuestList.Contains(quest))
                        gc.quests.mainQuestList.Add(quest);
                }
            }

            PutSheetBack(gc, mark);
        }

        [HarmonyPatch(typeof(StatusEffects), nameof(StatusEffects.SetupDeath), typeof(PlayfieldObject), typeof(bool))]
        static class StopDeath
        {
            static bool Prefix(StatusEffects __instance)
            {
                return Spare(__instance);
            }
        }

        [HarmonyPatch(typeof(StatusEffects), nameof(StatusEffects.SetupDeath), typeof(PlayfieldObject), typeof(bool), typeof(bool))]
        static class StopDeathQuiet
        {
            static bool Prefix(StatusEffects __instance)
            {
                return Spare(__instance);
            }
        }

        [HarmonyPatch(typeof(Agent), "set_knockedOut")]
        static class StopKnockout
        {
            static bool Prefix(Agent __instance, bool value)
            {
                if (!value)
                    return true;
                if (allowKnockout)
                    return true;

                SnakeEffect? form = SnakeEffect.Find(__instance);
                if (form == null || !form.HasMark)
                    return true;
                if (Busy)
                    return false;

                form.Rewind();
                return false;
            }
        }

        public static void Hold()
        {
            Holding = true;
            keptSills.Clear();
            shelvedSills.Clear();
            GameController gc = GameController.gameController;
            List<ObjectReal>? list = gc != null
                ? (gc.objectRealListWithDestroyed != null ? gc.objectRealListWithDestroyed : gc.objectRealList)
                : null;
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                ObjectReal thing = list[i];
                if (thing is Window || thing is WindowHelper || thing is Door)
                    keptSills.Add(thing.GetInstanceID());
            }
        }

        public static void Release()
        {
            Holding = false;
            keptSills.Clear();
            List<ObjectReal> copy = new List<ObjectReal>(shelvedSills);
            shelvedSills.Clear();
            for (int i = 0; i < copy.Count; i++)
            {
                ObjectReal thing = copy[i];
                if (thing != null)
                    thing.DestroyMe();
            }
        }

        static void KeepSills()
        {
            for (int i = 0; i < shelvedSills.Count; i++)
            {
                ObjectReal thing = shelvedSills[i];
                if (thing == null)
                    continue;
                thing.destroyed = false;
                thing.destroying = false;
                if (thing.go != null && !thing.go.activeSelf)
                    thing.go.SetActive(true);
            }

            shelvedSills.Clear();
        }

        static void Shelve(ObjectReal thing)
        {
            if (shelvedSills.Contains(thing))
                return;

            thing.destroyed = true;
            thing.destroying = false;
            if (thing.go != null)
                thing.go.SetActive(false);
            shelvedSills.Add(thing);
        }

        [HarmonyPatch(typeof(LaserEmitter), nameof(LaserEmitter.DestroyMe), new System.Type[0])]
        static class BreakBeam
        {
            static void Prefix(LaserEmitter __instance)
            {
                Cut(__instance);
                Cut(__instance.otherLaserEmitter);
                Cut(__instance.originalLaserEmitter);
            }

            static void Cut(LaserEmitter? emitter)
            {
                if (emitter == null)
                    return;

                Hide(emitter.laser);
                emitter.emittingLaser = false;
            }

            static void Hide(Laser? beam)
            {
                if (beam == null)
                    return;

                if (beam.danger != null && beam.danger.go != null)
                    beam.danger.go.SetActive(false);
                beam.destroyed = true;
                if (beam.go != null)
                    beam.go.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(ObjectReal), nameof(ObjectReal.DestroyMe), new System.Type[] { typeof(PlayfieldObject) })]
        static class KeepWindowSill
        {
            static bool Prefix(ObjectReal __instance)
            {
                if (!Holding || !keptSills.Contains(__instance.GetInstanceID()))
                    return true;

                Shelve(__instance);
                return false;
            }
        }

        static bool Spare(StatusEffects effects)
        {
            if (effects == null)
                return true;

            Agent agent = effects.agent;
            SnakeEffect? form = SnakeEffect.Find(agent);
            if (form == null || !form.HasMark)
                return true;
            if (Busy)
                return false;

            form.Rewind();
            return false;
        }
    }
}
