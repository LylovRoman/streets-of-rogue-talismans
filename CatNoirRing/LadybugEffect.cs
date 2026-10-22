using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class LadybugEffect : CustomEffect
    {
        public const string EffectId = "Ladybug";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> bugFace = new Dictionary<int, MaskFace.Memory>();
        static readonly string[] FormTraits = { };

        readonly List<string> addedTraits = new List<string>();
        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        int savedStrength;
        bool speedRaised;
        bool strengthRaised;
        bool colorsSaved;

        InvItem? previousWeapon;
        InvItem? previousAbility;
        string previousAbilityName = "";

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static LadybugEffect? Find(Agent agent)
        {
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                LadybugEffect? effect = list[i].GetHook<LadybugEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
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
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);

            ApplySuit();
            GiveTraits();
            RaiseSpeed();
            RaiseStrength();
            EquipYoyo();
            GiveCharm();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, bugFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            MaskFace.Save(Owner, bugFace);
            MaskFace.Restore(Owner, rememberedRels);
            YoyoFlight.Vanish(Owner);
            RestoreCharm();
            RestoreWeapon();
            RestoreTraits();
            RestoreSpeed();
            RestoreStrength();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            RaiseStrength();
            PaintSuit();
            MaskFace.Save(Owner, bugFace);
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

        public void PaintSuit()
        {
            if (!colorsSaved)
                return;

            AgentHitbox hitbox = Owner.agentHitboxScript;
            if (hitbox == null)
                return;

            Color32 suit = new Color32(214, 24, 40, 255);
            hitbox.skinColor = suit;
            hitbox.bodyColor = suit;
            hitbox.legsColor = suit;
            hitbox.footwearColor = new Color32(16, 16, 20, 255);
            hitbox.eyesColor = new Color32(70, 160, 255, 255);
            hitbox.hairColor = new Color32(12, 12, 16, 255);
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

        void RaiseStrength()
        {
            if (!strengthRaised)
                savedStrength = Owner.strengthStatMod;
            if (Owner.strengthStatMod >= 3)
                return;

            Owner.SetStrength(3);
            strengthRaised = true;
        }

        void RestoreStrength()
        {
            if (!strengthRaised)
                return;

            Owner.SetStrength(savedStrength);
            strengthRaised = false;
        }

        void GiveTraits()
        {
            StatusEffects effects = Owner.statusEffects;
            for (int i = 0; i < FormTraits.Length; i++)
            {
                string trait = FormTraits[i];
                if (effects.hasTrait(trait) || effects.hasTrait(trait + "2"))
                    continue;

                effects.AddTrait(trait);
                addedTraits.Add(trait);
            }
        }

        void RestoreTraits()
        {
            StatusEffects effects = Owner.statusEffects;
            for (int i = 0; i < addedTraits.Count; i++)
                effects.RemoveTrait(addedTraits[i]);
            addedTraits.Clear();
        }

        void GiveCharm()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(LuckyCharmAbility.Id);
            if (Owner.specialAbility == LuckyCharmAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = LuckyCharmAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = LuckyCharmAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreCharm()
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

        void EquipYoyo()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == LadybugYoyoItem.ItemId))
                previousWeapon = null;

            InvItem? yoyo = inventory.FindItem(LadybugYoyoItem.ItemId);
            if (yoyo == null)
                yoyo = inventory.AddItem(LadybugYoyoItem.ItemId, 1);
            if (yoyo == null)
                return;

            yoyo.throwDamage = CatNoirPlugin.YoYoDamage.Value;
            yoyo.throwDistance = LadybugYoyoItem.AimReach;
            yoyo.invItemCount = 1;
            inventory.EquipWeapon(yoyo, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            InvItem yoyo = inventory.FindItem(LadybugYoyoItem.ItemId);
            if (yoyo != null)
                inventory.DestroyItem(yoyo, true);

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
