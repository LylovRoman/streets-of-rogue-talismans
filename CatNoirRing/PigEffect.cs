using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class PigEffect : CustomEffect
    {
        public const string EffectId = "PigMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> pigFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly Color32[] savedColors = new Color32[6];
        int savedSpeed;
        bool speedRaised;
        bool colorsSaved;

        InvItem? previousAbility;
        string previousAbilityName = "";

        public bool Charge { get; set; } = true;
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static PigEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                PigEffect? effect = list[i].GetHook<PigEffect>();
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

        public void Accept(Agent person)
        {
            MaskFace.Pardon(person, rememberedRels);
        }

        public override void OnAdded()
        {
            Charge = true;
            CountdownStarted = false;
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            GiveGift();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, pigFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            MaskFace.Save(Owner, pigFace);
            MaskFace.Restore(Owner, rememberedRels);
            RestoreGift();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, pigFace);
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

            Color32 pink = new Color32(255, 150, 196, 255);
            Color32 hoof = new Color32(196, 72, 132, 255);
            hitbox.skinColor = pink;
            hitbox.bodyColor = pink;
            hitbox.legsColor = hoof;
            hitbox.footwearColor = new Color32(120, 36, 78, 255);
            hitbox.eyesColor = new Color32(255, 236, 246, 255);
            hitbox.hairColor = new Color32(232, 96, 156, 255);
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

        void GiveGift()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(GiftAbility.Id);
            if (Owner.specialAbility == GiftAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = GiftAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = GiftAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreGift()
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
