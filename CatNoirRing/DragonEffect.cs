using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class DragonEffect : CustomEffect
    {
        public const string EffectId = "DragonMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> dragonFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;
        bool handRemembered;
        bool hadWaterTrait;

        InvItem? previousWeapon;
        InvItem? previousAbility;
        string previousAbilityName = "";

        internal DragonArms.Kind Arm { get; private set; }
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static string RememberedName(Agent agent)
        {
            DragonEffect? form = Find(agent);
            if (form == null || form.previousWeapon == null)
                return "";

            string name = form.previousWeapon.invItemName;
            if (string.IsNullOrEmpty(name) || name == "Fist" || name == DragonArmItem.ItemId)
                return "";
            if (DragonArms.IsFormWeapon(form.previousWeapon))
                return "";

            return name;
        }

        public static DragonEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                DragonEffect? effect = list[i].GetHook<DragonEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public override int GetEffectTime()
        {
            return Mathf.Max(1, CatNoirPlugin.DragonDuration.Value);
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
            CurrentTime = Mathf.Max(1, CatNoirPlugin.DragonDuration.Value);
        }

        public bool CycleWeapon()
        {
            if (Owner == null || Owner.inventory == null)
                return false;

            RememberHand();
            DragonArms.Kind next = DragonArms.Next(Arm);
            if (!DragonArms.Give(Owner, next))
            {
                if (Arm != DragonArms.Kind.None)
                    DragonArms.Give(Owner, Arm);
                return false;
            }

            Arm = next;
            DragonArms.SyncWater(Owner, Arm == DragonArms.Kind.Water, hadWaterTrait);
            return true;
        }

        public override void OnAdded()
        {
            Arm = DragonArms.Kind.None;
            CountdownStarted = false;
            handRemembered = false;
            previousWeapon = null;
            Effect.infiniteTime = true;
            hadWaterTrait = Owner.statusEffects != null && Owner.statusEffects.hasTrait(DragonArms.WaterTrait);
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            GiveDragon();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, dragonFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            MaskFace.Save(Owner, dragonFace);
            MaskFace.Restore(Owner, rememberedRels);
            RestoreDragon();
            if (Owner.inventory != null)
                DragonArms.Clear(Owner.inventory);
            DragonArms.SyncWater(Owner, false, hadWaterTrait);
            RestoreHand();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, dragonFace);
            if (Owner.inventory != null)
                DragonArms.Feed(Owner.inventory, Arm);
            DragonArms.SyncWater(Owner, Arm == DragonArms.Kind.Water, hadWaterTrait);
            if (CountdownStarted && !e.IsFirstTick)
                CurrentTime--;
        }

        void RememberHand()
        {
            if (handRemembered || Owner.inventory == null)
                return;

            handRemembered = true;
            InvItem equipped = Owner.inventory.equippedWeapon;
            if (equipped == null || equipped.invItemName == "Fist" || DragonArms.IsFormWeapon(equipped))
                return;

            previousWeapon = equipped;
        }

        void RestoreHand()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null || previousWeapon == null)
                return;

            if (inventory.InvItemList != null && inventory.InvItemList.Contains(previousWeapon) && !DragonArms.IsFormWeapon(previousWeapon))
            {
                inventory.EquipWeapon(previousWeapon, true);
                return;
            }

            InvItem? still = DragonArms.RealItem(inventory, previousWeapon.invItemName);
            if (still != null)
                inventory.EquipWeapon(still, true);
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

            Color32 red = new Color32(168, 24, 32, 255);
            Color32 gold = new Color32(232, 184, 56, 255);
            hitbox.skinColor = gold;
            hitbox.bodyColor = red;
            hitbox.legsColor = red;
            hitbox.footwearColor = gold;
            hitbox.eyesColor = new Color32(255, 214, 80, 255);
            hitbox.hairColor = gold;
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

        void GiveDragon()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(DragonAbility.Id);
            if (Owner.specialAbility == DragonAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = DragonAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = DragonAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreDragon()
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
