using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class MouseEffect : CustomEffect
    {
        public const string EffectId = "MouseMiraculous";
        const string ShrinkTrait = "Diminutive";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> mouseFace = new Dictionary<int, MaskFace.Memory>();
        static readonly HashSet<int> cloneIds = new HashSet<int>();

        readonly List<Agent> clones = new List<Agent>();
        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;
        bool addedShrink;
        bool addedTeam;
        bool multiplied;
        bool countdownStarted;
        float multiplyEndsAt;

        InvItem? previousWeapon;
        InvItem? heldRope;
        InvItem? previousAbility;
        string previousAbilityName = "";

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static bool IsClone(Agent agent)
        {
            return agent != null && cloneIds.Contains(agent.GetInstanceID());
        }

        public static MouseEffect? Find(Agent agent)
        {
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                MouseEffect? effect = list[i].GetHook<MouseEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public static void Maintain(Agent agent)
        {
            Find(agent)?.TickMultiply();
        }

        public override int GetEffectTime()
        {
            return Mathf.Max(1, CatNoirPlugin.MultiplyDuration.Value);
        }

        public override int GetEffectHate()
        {
            return 0;
        }

        public override void OnAdded()
        {
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            EquipRope();
            GiveMultiply();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, mouseFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            EndMultiply();
            MaskFace.Save(Owner, mouseFace);
            MaskFace.Restore(Owner, rememberedRels);
            RestoreMultiply();
            RestoreWeapon();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            MaskFace.Save(Owner, mouseFace);
            if (countdownStarted && !e.IsFirstTick)
                CurrentTime--;
            TickMultiply();
        }

        public void BeginCountdown()
        {
            if (countdownStarted)
                return;

            countdownStarted = true;
            Effect.infiniteTime = false;
            CurrentTime = Mathf.Max(1, CatNoirPlugin.MultiplyDuration.Value);
        }

        public bool BeginMultiply()
        {
            if (multiplied || Owner.tr == null)
                return false;

            Shrink(Owner, true);
            AllowTeam();
            SpawnClones();
            multiplied = true;
            multiplyEndsAt = Time.time + Mathf.Max(1, CatNoirPlugin.MultiplyDuration.Value);
            BeginCountdown();
            return true;
        }

        void TickMultiply()
        {
            if (!multiplied || Time.time < multiplyEndsAt || Owner.statusEffects == null)
                return;

            Owner.statusEffects.RemoveStatusEffect(EffectId, true);
            if (multiplied)
                EndMultiply();
        }

        void EndMultiply()
        {
            for (int i = 0; i < clones.Count; i++)
            {
                Agent clone = clones[i];
                if (clone == null)
                    continue;
                cloneIds.Remove(clone.GetInstanceID());
                clone.DestroyMe();
            }

            clones.Clear();
            if (addedShrink)
                Grow(Owner);
            if (addedTeam && Owner.statusEffects != null && Owner.statusEffects.hasTrait("MoreFollowers"))
                Owner.statusEffects.RemoveTrait("MoreFollowers");
            multiplied = false;
            addedShrink = false;
            addedTeam = false;
        }

        void SpawnClones()
        {
            GameController gc = Owner.gc;
            if (gc == null || gc.spawnerMain == null || Owner.tr == null)
                return;

            float tile = Consts.tileSize > 0.01f ? Consts.tileSize : 0.64f;
            Vector3 origin = Owner.tr.position;
            Vector3[] spots =
            {
                origin + new Vector3(tile, 0f, 0f),
                origin + new Vector3(-tile, 0f, 0f),
                origin + new Vector3(0f, -tile, 0f),
            };
            string kind = Owner.agentName;
            if (string.IsNullOrEmpty(kind))
                kind = "Custom";

            for (int i = 0; i < spots.Length; i++)
            {
                Agent clone = gc.spawnerMain.SpawnAgent(spots[i], Owner, kind, "CloneMachine", Owner);
                if (clone == null || clone == Owner || clone.isPlayer != 0)
                    continue;

                cloneIds.Add(clone.GetInstanceID());
                clones.Add(clone);
                PaintMouse(Owner, clone);
                Shrink(clone, false);
                StripLoadout(clone);
                clone.SetSpeed(4);
            }
        }

        void AllowTeam()
        {
            if (Owner.statusEffects == null || Owner.statusEffects.hasTrait("MoreFollowers"))
                return;

            Owner.statusEffects.AddTrait("MoreFollowers");
            addedTeam = true;
        }

        static void PaintMouse(Agent source, Agent clone)
        {
            AgentHitbox from = source.agentHitboxScript;
            AgentHitbox to = clone.agentHitboxScript;
            if (from == null || to == null)
                return;

            to.skinColor = from.skinColor;
            to.hairColor = from.hairColor;
            to.facialHairColor = from.facialHairColor;
            to.bodyColor = from.bodyColor;
            to.legsColor = from.legsColor;
            to.footwearColor = from.footwearColor;
            to.eyesColor = from.eyesColor;
            to.hairColorName = from.hairColorName;
            to.facialHairColorName = from.facialHairColorName;
            to.skinColorName = from.skinColorName;
            ObjectSprite sprite = clone.objectSprite;
            if (sprite == null)
                return;

            sprite.agentColorDirty = true;
            sprite.mustRefreshColor = true;
            sprite.SetAgentOff(0);
        }

        static void StripLoadout(Agent clone)
        {
            InvDatabase inventory = clone.inventory;
            if (inventory == null)
                return;

            string[] ours =
            {
                CatNoirRingItem.ItemId,
                LadybugEarringsItem.ItemId,
                MouseTalismanItem.ItemId,
                BeeTalismanItem.ItemId,
                SteelStaffItem.ItemId,
                JumpRopeItem.ItemId,
                LadybugYoyoItem.ItemId,
                TrompoItem.ItemId,
                TigerTalismanItem.ItemId,
                DogTalismanItem.ItemId,
                DogBallItem.ItemId,
                GoatTalismanItem.ItemId,
                MonkeyTalismanItem.ItemId,
                UproarItem.ItemId,
                DragonTalismanItem.ItemId,
                DragonArmItem.ItemId,
                OxTalismanItem.ItemId,
                PeacockTalismanItem.ItemId,
                TurtleTalismanItem.ItemId,
                FoxTalismanItem.ItemId,
                PigTalismanItem.ItemId,
                ButterflyTalismanItem.ItemId,
                RabbitTalismanItem.ItemId,
                CataclysmAbility.Id,
                LuckyCharmAbility.Id,
                MultiplyAbility.Id,
                VenomAbility.Id,
                CloutAbility.Id,
                FetchAbility.Id,
                GenesisAbility.Id,
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

            if (clone.specialAbility == CataclysmAbility.Id
                || clone.specialAbility == LuckyCharmAbility.Id
                || clone.specialAbility == MultiplyAbility.Id
                || clone.specialAbility == VenomAbility.Id
                || clone.specialAbility == CloutAbility.Id
                || clone.specialAbility == FetchAbility.Id
                || clone.specialAbility == GenesisAbility.Id
                || clone.specialAbility == DragonAbility.Id
                || clone.specialAbility == ResistanceAbility.Id
                || clone.specialAbility == ConfusionAbility.Id
                || clone.specialAbility == AmokAbility.Id
                || clone.specialAbility == MirageAbility.Id
                || clone.specialAbility == GiftAbility.Id
                || clone.specialAbility == ShellAbility.Id
                || clone.specialAbility == ButterflyAbility.Id
                || clone.specialAbility == TimeStopAbility.Id)
            {
                clone.specialAbility = "";
                clone.specialAbilityAttack = false;
                inventory.equippedSpecialAbility = null;
            }

            DragonArms.Strip(inventory);
        }

        void Shrink(Agent agent, bool remember)
        {
            bool already = agent.statusEffects != null && agent.statusEffects.hasTrait(ShrinkTrait);
            if (agent.statusEffects != null && !already)
                agent.statusEffects.AddTrait(ShrinkTrait);
            agent.diminutive = true;
            if (remember)
                addedShrink = !already;
        }

        void Grow(Agent agent)
        {
            if (agent.statusEffects != null && agent.statusEffects.hasTrait(ShrinkTrait))
                agent.statusEffects.RemoveTrait(ShrinkTrait);
            agent.diminutive = false;
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

            Color32 suit = new Color32(168, 168, 176, 255);
            hitbox.skinColor = suit;
            hitbox.bodyColor = suit;
            hitbox.legsColor = new Color32(92, 92, 102, 255);
            hitbox.footwearColor = new Color32(230, 120, 168, 255);
            hitbox.eyesColor = new Color32(240, 90, 150, 255);
            hitbox.hairColor = new Color32(240, 150, 186, 255);
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

        void GiveMultiply()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(MultiplyAbility.Id);
            if (Owner.specialAbility == MultiplyAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = MultiplyAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = MultiplyAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreMultiply()
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

        void EquipRope()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == JumpRopeItem.ItemId))
                previousWeapon = null;

            InvItem? rope = inventory.FindItem(JumpRopeItem.ItemId);
            if (rope == null)
                rope = inventory.AddItem(JumpRopeItem.ItemId, 100);
            if (rope == null)
                return;

            rope.meleeDamage = CatNoirPlugin.StaffDamage.Value;
            rope.invItemCount = Mathf.Max(rope.invItemCount, 100);
            string shownName = rope.invItemRealName;
            rope.invItemName = SteelStaffItem.SwingSprite;
            if (!string.IsNullOrEmpty(shownName))
                rope.invItemRealName = shownName;
            JumpRopeItem.Live.Add(rope);
            heldRope = rope;
            inventory.EquipWeapon(rope, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            if (heldRope != null)
            {
                JumpRopeItem.Live.Remove(heldRope);
                inventory.DestroyItem(heldRope, true);
                heldRope = null;
            }

            if (previousWeapon != null)
            {
                InvItem? stillThere = inventory.FindItem(previousWeapon.invItemName);
                if (stillThere != null)
                    inventory.EquipWeapon(stillThere, true);
            }

            previousWeapon = null;
        }
    }
}
