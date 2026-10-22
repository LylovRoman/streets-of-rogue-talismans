using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class BeeEffect : CustomEffect
    {
        public const string EffectId = "BeeMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> beeFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
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

        public static BeeEffect? Find(Agent agent)
        {
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                BeeEffect? effect = list[i].GetHook<BeeEffect>();
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
            RaiseSpeed();
            EquipTrompo();
            GiveVenom();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, beeFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            MaskFace.Save(Owner, beeFace);
            MaskFace.Restore(Owner, rememberedRels);
            YoyoFlight.Vanish(Owner);
            RestoreVenom();
            RestoreWeapon();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, beeFace);
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

            Color32 yellow = new Color32(240, 196, 32, 255);
            Color32 black = new Color32(16, 16, 18, 255);
            hitbox.skinColor = yellow;
            hitbox.bodyColor = yellow;
            hitbox.legsColor = black;
            hitbox.footwearColor = black;
            hitbox.eyesColor = black;
            hitbox.hairColor = black;
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

        void GiveVenom()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(VenomAbility.Id);
            if (Owner.specialAbility == VenomAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = VenomAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = VenomAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreVenom()
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

        void EquipTrompo()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == TrompoItem.ItemId))
                previousWeapon = null;

            InvItem? trompo = inventory.FindItem(TrompoItem.ItemId);
            if (trompo == null)
                trompo = inventory.AddItem(TrompoItem.ItemId, 1);
            if (trompo == null)
                return;

            trompo.throwDamage = CatNoirPlugin.YoYoDamage.Value;
            trompo.throwDistance = LadybugYoyoItem.AimReach;
            trompo.invItemCount = 1;
            inventory.EquipWeapon(trompo, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            InvItem trompo = inventory.FindItem(TrompoItem.ItemId);
            if (trompo != null)
                inventory.DestroyItem(trompo, true);

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
