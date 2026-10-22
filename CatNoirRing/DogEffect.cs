using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class DogEffect : CustomEffect
    {
        public const string EffectId = "DogMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> dogFace = new Dictionary<int, MaskFace.Memory>();

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

        public static DogEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                DogEffect? effect = list[i].GetHook<DogEffect>();
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
                DogMarkEffect.Lift(previous);
            DogMarkEffect.Apply(target);

            GameController gc = Owner.gc;
            if (gc != null && gc.audioHandler != null)
                gc.audioHandler.Play(target, "SelectItem");
        }

        public void ClearMark()
        {
            Agent? who = marked;
            marked = null;
            if (who != null)
                DogMarkEffect.Lift(who);
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
            EquipBall();
            GiveFetch();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, dogFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            ClearMark();
            MaskFace.Save(Owner, dogFace);
            MaskFace.Restore(Owner, rememberedRels);
            YoyoFlight.Vanish(Owner);
            RestoreFetch();
            RestoreWeapon();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, dogFace);
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

            Color32 orange = new Color32(232, 122, 36, 255);
            Color32 brown = new Color32(72, 36, 18, 255);
            hitbox.skinColor = orange;
            hitbox.bodyColor = orange;
            hitbox.legsColor = brown;
            hitbox.footwearColor = new Color32(28, 16, 12, 255);
            hitbox.eyesColor = new Color32(24, 14, 10, 255);
            hitbox.hairColor = new Color32(120, 64, 28, 255);
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

        void GiveFetch()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(FetchAbility.Id);
            if (Owner.specialAbility == FetchAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = FetchAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = FetchAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreFetch()
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

        void EquipBall()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            previousWeapon = inventory.equippedWeapon;
            if (previousWeapon != null && (previousWeapon.invItemName == "Fist" || previousWeapon.invItemName == DogBallItem.ItemId))
                previousWeapon = null;

            InvItem? ball = inventory.FindItem(DogBallItem.ItemId);
            if (ball == null)
                ball = inventory.AddItem(DogBallItem.ItemId, 1);
            if (ball == null)
                return;

            ball.throwDamage = 0;
            ball.throwDistance = LadybugYoyoItem.AimReach;
            ball.invItemCount = 1;
            inventory.EquipWeapon(ball, true);
        }

        void RestoreWeapon()
        {
            InvDatabase inventory = Owner.inventory;
            if (inventory == null)
                return;

            InvItem ball = inventory.FindItem(DogBallItem.ItemId);
            if (ball != null)
                inventory.DestroyItem(ball, true);

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
