using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class MonkeyEffect : CustomEffect
    {
        public const string EffectId = "MonkeyMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> monkeyFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;

        InvItem? previousWeapon;
        InvItem? previousAbility;
        string previousAbilityName = "";
        Agent? marked;

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }
        public Agent? Marked => marked;

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static MonkeyEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                MonkeyEffect? effect = list[i].GetHook<MonkeyEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public static void Tag(Agent? thrower, Agent? target)
        {
            if (thrower == null || target == null)
                return;

            Find(thrower)?.Remember(target);
        }

        public void Remember(Agent target)
        {
            if (target == null || target == Owner || target.dead || target.ghost)
                return;
            if (marked == target)
                return;

            Agent? previous = marked;
            marked = target;
            if (previous != null)
                MonkeyMarkEffect.Lift(previous);
            MonkeyMarkEffect.Apply(target);

            GameController gc = Owner.gc;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(target, "SelectItem");
        }

        public void ClearMark()
        {
            Agent? who = marked;
            marked = null;
            if (who != null)
                MonkeyMarkEffect.Lift(who);
        }

        public override int GetEffectTime()
        {
            return Mathf.Max(1, CatNoirPlugin.FormDuration.Value);
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
            CurrentTime = Mathf.Max(1, CatNoirPlugin.FormDuration.Value);
        }

        public override void OnAdded()
        {
            Charge = true;
            CountdownStarted = false;
            marked = null;
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            EquipBanana();
            GiveConfusion();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, monkeyFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            ClearMark();
            MaskFace.Save(Owner, monkeyFace);
            MaskFace.Restore(Owner, rememberedRels);
            YoyoFlight.Vanish(Owner);
            RestoreConfusion();
            RestoreWeapon();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, monkeyFace);
            if (CountdownStarted && !e.IsFirstTick)
                CurrentTime--;
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

            Color32 fur = new Color32(120, 72, 36, 255);
            Color32 dark = new Color32(48, 28, 16, 255);
            hitbox.skinColor = new Color32(214, 156, 86, 255);
            hitbox.bodyColor = fur;
            hitbox.legsColor = dark;
            hitbox.footwearColor = new Color32(28, 16, 12, 255);
            hitbox.eyesColor = new Color32(255, 214, 64, 255);
            hitbox.hairColor = new Color32(72, 40, 22, 255);
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

        void GiveConfusion()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(ConfusionAbility.Id);
            if (Owner.specialAbility == ConfusionAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = ConfusionAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = ConfusionAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreConfusion()
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

        void EquipBanana()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == UproarItem.ItemId))
                previousWeapon = null;

            InvItem? banana = inventory.FindItem(UproarItem.ItemId);
            if (banana == null)
                banana = inventory.AddItem(UproarItem.ItemId, 1);
            if (banana == null)
                return;

            banana.meleeDamage = 0;
            banana.throwDamage = 0;
            banana.throwDistance = LadybugYoyoItem.AimReach;
            banana.invItemCount = 1;
            inventory.EquipWeapon(banana, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            InvItem banana = inventory.FindItem(UproarItem.ItemId);
            if (banana != null)
                inventory.DestroyItem(banana, true);

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
