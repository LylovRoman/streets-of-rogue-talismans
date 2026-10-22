using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class RabbitEffect : CustomEffect
    {
        public const string EffectId = "RabbitMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> rabbitFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;
        float second;

        InvItem? previousAbility;
        string previousAbilityName = "";

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static RabbitEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                RabbitEffect? effect = list[i].GetHook<RabbitEffect>();
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
            second = 0f;
            Effect.infiniteTime = false;
            CurrentTime = Mathf.Max(1, CatNoirPlugin.FormDuration.Value);
        }

        public void Maintain()
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, rabbitFace);
            if (!CountdownStarted || !TimeStop.Active)
                return;

            second += Time.unscaledDeltaTime;
            while (second >= 1f)
            {
                second -= 1f;
                if (CurrentTime > 0)
                    CurrentTime--;
                RefreshClock();
                if (CurrentTime > 0)
                    continue;

                Owner.statusEffects.RemoveStatusEffect(EffectId);
                return;
            }
        }

        public override void OnAdded()
        {
            Charge = true;
            CountdownStarted = false;
            second = 0f;
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            GiveTimeStop();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, rabbitFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            if (TimeStop.Subject == Owner)
                TimeStop.End();
            MaskFace.Save(Owner, rabbitFace);
            MaskFace.Restore(Owner, rememberedRels);
            RestoreTimeStop();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, rabbitFace);
            if (CountdownStarted && !e.IsFirstTick && !TimeStop.Active)
                CurrentTime--;
        }

        void RefreshClock()
        {
            StatusEffectDisplay? display = Owner.statusEffects != null ? Owner.statusEffects.myStatusEffectDisplay : null;
            if (display != null)
                display.RefreshStatusEffectText();
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

            Color32 cloth = new Color32(245, 245, 250, 255);
            Color32 pink = new Color32(255, 146, 186, 255);
            hitbox.skinColor = cloth;
            hitbox.bodyColor = cloth;
            hitbox.legsColor = pink;
            hitbox.footwearColor = new Color32(48, 56, 92, 255);
            hitbox.eyesColor = new Color32(255, 96, 150, 255);
            hitbox.hairColor = new Color32(255, 210, 226, 255);
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

        void GiveTimeStop()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(TimeStopAbility.Id);
            if (Owner.specialAbility == TimeStopAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = TimeStopAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = TimeStopAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreTimeStop()
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
