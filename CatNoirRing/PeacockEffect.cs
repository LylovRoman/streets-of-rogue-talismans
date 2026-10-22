using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class PeacockEffect : CustomEffect
    {
        public const string EffectId = "PeacockMiraculous";
        const string TeamTrait = "MoreFollowers";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> peacockFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;
        bool addedTeam;
        bool dismissing;

        Agent? ally;
        InvItem? previousAbility;
        string previousAbilityName = "";

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }
        public bool HasAlly => ally != null;

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static PeacockEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                PeacockEffect? effect = list[i].GetHook<PeacockEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public override int GetEffectTime()
        {
            return Mathf.Max(1, CatNoirPlugin.PeacockDuration.Value);
        }

        public override int GetEffectHate()
        {
            return 0;
        }

        public void BeginCountdown()
        {
            if (CountdownStarted)
                return;

            CountdownStarted = true;
            Effect.infiniteTime = false;
            CurrentTime = Mathf.Max(1, CatNoirPlugin.PeacockDuration.Value);
        }

        public bool TrySummon(string className)
        {
            if (!Charge || HasAlly || string.IsNullOrEmpty(className) || Owner.tr == null)
                return false;
            if (className == "Custom" || className == "Hologram" || className == "Playerr" || className == "ObjectAgent")
                return false;

            GameController gc = Owner.gc;
            if (gc == null || gc.spawnerMain == null)
                return false;

            float tile = Consts.tileSize > 0.01f ? Consts.tileSize : 0.64f;
            Vector3 spot = Owner.tr.position + new Vector3(tile, 0f, 0f);
            Agent spawned = gc.spawnerMain.SpawnAgent(spot, Owner, className, "CloneMachine", Owner);
            if (spawned == null || spawned == Owner || spawned.isPlayer != 0)
                return false;

            AllowTeam();
            if (Owner.agentInteractions != null)
                Owner.agentInteractions.HireUnofficially(spawned, Owner);
            StripOurs(spawned);
            ally = spawned;
            return true;
        }

        public void DismissAlly()
        {
            if (dismissing)
                return;

            dismissing = true;
            try
            {
                Agent? gone = ally;
                ally = null;
                if (gone != null && gone != Owner)
                    gone.DestroyMe();
                if (addedTeam && Owner != null && Owner.statusEffects != null && Owner.statusEffects.hasTrait(TeamTrait))
                    Owner.statusEffects.RemoveTrait(TeamTrait);
                addedTeam = false;
            }
            finally
            {
                dismissing = false;
            }
        }

        public override void OnAdded()
        {
            Charge = true;
            CountdownStarted = false;
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            GiveAmok();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, peacockFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            PeacockWish.Dismiss(Owner);
            DismissAlly();
            MaskFace.Save(Owner, peacockFace);
            MaskFace.Restore(Owner, rememberedRels);
            RestoreAmok();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, peacockFace);
            if (CountdownStarted && !e.IsFirstTick)
                CurrentTime--;
        }

        void AllowTeam()
        {
            if (Owner.statusEffects == null || Owner.statusEffects.hasTrait(TeamTrait))
                return;
            if (Owner.CanHaveMoreFollowers(false))
                return;

            Owner.statusEffects.AddTrait(TeamTrait);
            addedTeam = true;
        }

        static void StripOurs(Agent person)
        {
            InvDatabase inventory = person.inventory;
            if (inventory == null)
                return;

            string[] ours =
            {
                CatNoirRingItem.ItemId,
                LadybugEarringsItem.ItemId,
                MouseTalismanItem.ItemId,
                BeeTalismanItem.ItemId,
                SnakeTalismanItem.ItemId,
                HorseTalismanItem.ItemId,
                TigerTalismanItem.ItemId,
                DogTalismanItem.ItemId,
                DogBallItem.ItemId,
                GoatTalismanItem.ItemId,
                RoosterTalismanItem.ItemId,
                MonkeyTalismanItem.ItemId,
                UproarItem.ItemId,
                DragonTalismanItem.ItemId,
                DragonArmItem.ItemId,
                OxTalismanItem.ItemId,
                PeacockTalismanItem.ItemId,
                ButterflyTalismanItem.ItemId,
                TurtleTalismanItem.ItemId,
                FoxTalismanItem.ItemId,
                PigTalismanItem.ItemId,
                RabbitTalismanItem.ItemId,
                SteelStaffItem.ItemId,
                JumpRopeItem.ItemId,
                LadybugYoyoItem.ItemId,
                TrompoItem.ItemId,
                CataclysmAbility.Id,
                LuckyCharmAbility.Id,
                MultiplyAbility.Id,
                VenomAbility.Id,
                SecondChanceAbility.Id,
                VoyageAbility.Id,
                CloutAbility.Id,
                FetchAbility.Id,
                GenesisAbility.Id,
                SublimationAbility.Id,
                DragonAbility.Id,
                ResistanceAbility.Id,
                ConfusionAbility.Id,
                AmokAbility.Id,
                ShellAbility.Id,
                MirageAbility.Id,
                GiftAbility.Id,
                ButterflyAbility.Id,
                TimeStopAbility.Id,
            };
            for (int i = 0; i < ours.Length; i++)
            {
                InvItem found = inventory.FindItem(ours[i]);
                if (found == null)
                    continue;
                found.cantDrop = false;
                inventory.DestroyItem(found, true);
            }

            if (person.specialAbility == CataclysmAbility.Id
                || person.specialAbility == LuckyCharmAbility.Id
                || person.specialAbility == MultiplyAbility.Id
                || person.specialAbility == VenomAbility.Id
                || person.specialAbility == SecondChanceAbility.Id
                || person.specialAbility == VoyageAbility.Id
                || person.specialAbility == CloutAbility.Id
                || person.specialAbility == FetchAbility.Id
                || person.specialAbility == GenesisAbility.Id
                || person.specialAbility == SublimationAbility.Id
                || person.specialAbility == DragonAbility.Id
                || person.specialAbility == ResistanceAbility.Id
                || person.specialAbility == ConfusionAbility.Id
                || person.specialAbility == AmokAbility.Id
                || person.specialAbility == MirageAbility.Id
                || person.specialAbility == GiftAbility.Id
                || person.specialAbility == ShellAbility.Id
                || person.specialAbility == ButterflyAbility.Id
                || person.specialAbility == TimeStopAbility.Id)
            {
                person.specialAbility = "";
                person.specialAbilityAttack = false;
                inventory.equippedSpecialAbility = null;
            }

            DragonArms.Strip(inventory);
        }

        void ApplySuit()
        {
            AgentHitbox hitbox = Owner.agentHitboxScript;
            if (hitbox == null)
                return;

            savedColors[0] = hitbox.skinColor;
            savedColors[1] = hitbox.bodyColor;
            savedColors[2] = hitbox.legsColor;
            savedColors[3] = hitbox.eyesColor;
            savedColors[4] = hitbox.footwearColor;
            savedColors[5] = hitbox.hairColor;
            colorsSaved = true;
            PaintSuit();
        }

        void PaintSuit()
        {
            if (!colorsSaved)
                return;

            AgentHitbox hitbox = Owner.agentHitboxScript;
            if (hitbox == null)
                return;

            Color32 feather = new Color32(18, 120, 138, 255);
            Color32 deep = new Color32(10, 48, 72, 255);
            hitbox.skinColor = feather;
            hitbox.bodyColor = feather;
            hitbox.legsColor = deep;
            hitbox.footwearColor = new Color32(212, 168, 48, 255);
            hitbox.eyesColor = new Color32(232, 196, 48, 255);
            hitbox.hairColor = new Color32(36, 170, 110, 255);
            RefreshColors();
        }

        void RestoreSuit()
        {
            AgentHitbox hitbox = Owner.agentHitboxScript;
            if (hitbox == null || !colorsSaved)
                return;

            hitbox.skinColor = savedColors[0];
            hitbox.bodyColor = savedColors[1];
            hitbox.legsColor = savedColors[2];
            hitbox.eyesColor = savedColors[3];
            hitbox.footwearColor = savedColors[4];
            hitbox.hairColor = savedColors[5];
            hitbox.MustRefresh();
            RefreshColors();
            colorsSaved = false;
        }

        void RefreshColors()
        {
            ObjectSprite sprite = Owner.objectSprite;
            if (sprite == null)
                return;

            sprite.agentColorDirty = true;
            sprite.mustRefreshColor = true;
            int slot = Owner.isPlayer > 0 ? Owner.isPlayer - 1 : 0;
            sprite.SetAgentOff(slot);
        }

        void RaiseSpeed()
        {
            if (!speedRaised)
                savedSpeed = Owner.speedStatMod;
            if (Owner.speedStatMod >= 4)
                return;

            Owner.SetSpeed(4);
            speedRaised = true;
        }

        void RestoreSpeed()
        {
            if (!speedRaised)
                return;

            Owner.SetSpeed(savedSpeed);
            speedRaised = false;
        }

        void GiveAmok()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(AmokAbility.Id);
            if (Owner.specialAbility == AmokAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = AmokAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = AmokAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreAmok()
        {
            if (Owner.inventory != null && !string.IsNullOrEmpty(previousAbilityName))
            {
                Owner.statusEffects.GiveSpecialAbility(previousAbilityName);
                if (Owner.specialAbility != previousAbilityName && previousAbility != null)
                    Owner.inventory.equippedSpecialAbility = previousAbility;
                return;
            }

            Owner.specialAbility = "";
            Owner.specialAbilityAttack = false;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = previousAbility;
        }
    }
}
