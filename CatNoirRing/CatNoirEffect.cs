using System;
using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class CatNoirEffect : CustomEffect
    {
        public const string EffectId = "CatNoir";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> catFace = new Dictionary<int, MaskFace.Memory>();
        static readonly string[] FormTraits = { "FastMelee" };

        readonly List<string> addedTraits = new List<string>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        int savedStrength;
        bool speedRaised;
        bool strengthRaised;

        InvItem? previousWeapon;
        InvItem? heldStaff;
        InvItem? previousAbility;
        string previousAbilityName = "";
        bool colorsSaved;
        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static CatNoirEffect? Find(Agent agent)
        {
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                CatNoirEffect? effect = list[i].GetHook<CatNoirEffect>();
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
            EquipStaff();
            GiveCataclysm();
            RememberHowPeopleSeeYou();
            WearCatFace();
        }

        public override void OnRemoved()
        {
            RememberCatFace();
            RestoreRememberedRels();
            RestoreCataclysm();
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
            RememberCatFace();
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

            Color32 suit = new Color32(12, 12, 16, 255);
            hitbox.skinColor = suit;
            hitbox.bodyColor = suit;
            hitbox.legsColor = suit;
            hitbox.footwearColor = suit;
            hitbox.eyesColor = new Color32(36, 230, 96, 255);
            hitbox.GetColorFromString("Blonde", "Hair");
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

        void RememberHowPeopleSeeYou()
        {
            MaskFace.Remember(Owner, rememberedRels);
        }

        void WearCatFace()
        {
            MaskFace.Wear(Owner, catFace, rememberedRels);
        }

        void RememberCatFace()
        {
            MaskFace.Save(Owner, catFace);
        }

        void RestoreRememberedRels()
        {
            MaskFace.Restore(Owner, rememberedRels);
        }

        void GiveCataclysm()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(CataclysmAbility.Id);
            if (Owner.specialAbility == CataclysmAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = CataclysmAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = CataclysmAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreCataclysm()
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

        void EquipStaff()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == SteelStaffItem.ItemId))
                previousWeapon = null;

            InvItem? staff = inventory.FindItem(SteelStaffItem.ItemId);
            if (staff == null)
                staff = inventory.AddItem(SteelStaffItem.ItemId, 100);

            if (staff == null)
                return;

            staff.meleeDamage = CatNoirPlugin.StaffDamage.Value;
            staff.invItemCount = Mathf.Max(staff.invItemCount, 100);
            string shownName = staff.invItemRealName;
            staff.invItemName = SteelStaffItem.SwingSprite;
            if (!string.IsNullOrEmpty(shownName))
                staff.invItemRealName = shownName;
            SteelStaffItem.Live.Add(staff);
            heldStaff = staff;
            inventory.EquipWeapon(staff, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            if (heldStaff != null)
            {
                SteelStaffItem.Live.Remove(heldStaff);
                inventory.DestroyItem(heldStaff, true);
                heldStaff = null;
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
