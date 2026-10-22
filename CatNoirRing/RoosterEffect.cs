using System.Collections.Generic;
using RogueLibsCore;
using UnityEngine;

namespace CatNoirRing
{
    [EffectName(EffectId)]
    [EffectParameters(EffectLimitations.RemoveOnDeath | EffectLimitations.RemoveOnKnockOut | EffectLimitations.RemoveOnNextLevel)]
    public class RoosterEffect : CustomEffect
    {
        public const string EffectId = "RoosterMiraculous";

        static readonly Dictionary<int, float> readyAt = new Dictionary<int, float>();
        static readonly Dictionary<int, MaskFace.Memory> roosterFace = new Dictionary<int, MaskFace.Memory>();

        readonly Dictionary<int, MaskFace.Memory> rememberedRels = new Dictionary<int, MaskFace.Memory>();
        readonly List<string> displaced = new List<string>();
        readonly Color32[] savedColors = new Color32[6];
        string wished = "";
        int savedSpeed;
        bool speedSaved;
        bool colorsSaved;

        InvItem? previousAbility;
        string previousAbilityName = "";

        public bool Charge { get; set; } = true;
        public bool Granted { get; private set; }
        public bool CountdownStarted { get; private set; }

        public static float ReadyAt(Agent agent)
        {
            return readyAt.TryGetValue(agent.GetInstanceID(), out float time) ? time : 0f;
        }

        public static RoosterEffect? Find(Agent agent)
        {
            if (agent == null || agent.statusEffects == null)
                return null;

            List<StatusEffect>? list = agent.statusEffects.StatusEffectList;
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                RoosterEffect? effect = list[i].GetHook<RoosterEffect>();
                if (effect != null)
                    return effect;
            }

            return null;
        }

        public override int GetEffectTime()
        {
            return Mathf.Max(1, CatNoirPlugin.SublimationDuration.Value);
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
            CurrentTime = Mathf.Max(1, CatNoirPlugin.SublimationDuration.Value);
        }

        public bool TryGrant(string name)
        {
            StatusEffects? effects = Owner.statusEffects;
            if (effects == null || string.IsNullOrEmpty(name) || effects.hasTrait(name))
                return false;

            List<string> before = TraitNames();
            effects.AddTrait(name);
            if (!effects.hasTrait(name))
                return false;

            wished = name;
            displaced.Clear();
            for (int i = 0; i < before.Count; i++)
            {
                string had = before[i];
                if (had != name && !effects.hasTrait(had))
                    displaced.Add(had);
            }

            Granted = true;
            return true;
        }

        public override void OnAdded()
        {
            Charge = true;
            Granted = false;
            CountdownStarted = false;
            wished = "";
            displaced.Clear();
            Effect.infiniteTime = true;
            TalismanForms.RemoveOthers(Owner, EffectId);
            ApplySuit();
            RaiseSpeed();
            GiveSublimation();
            MaskFace.Remember(Owner, rememberedRels);
            MaskFace.Wear(Owner, roosterFace, rememberedRels);
        }

        public override void OnRemoved()
        {
            RoosterWish.Dismiss(Owner);
            MaskFace.Save(Owner, roosterFace);
            MaskFace.Restore(Owner, rememberedRels);
            ReleaseTrait();
            RestoreSublimation();
            RestoreSpeed();
            RestoreSuit();
            readyAt[Owner.GetInstanceID()] = Time.time + Mathf.Max(0, CatNoirPlugin.FormCooldown.Value);
        }

        public override void OnUpdated(EffectUpdatedArgs e)
        {
            if (!Granted)
                RaiseSpeed();
            PaintSuit();
            MaskFace.Save(Owner, roosterFace);
            if (CountdownStarted && !e.IsFirstTick)
                CurrentTime--;
        }

        void ReleaseTrait()
        {
            StatusEffects? effects = Owner.statusEffects;
            string name = wished;
            List<string> back = new List<string>(displaced);
            wished = "";
            displaced.Clear();
            Granted = false;
            if (effects == null || string.IsNullOrEmpty(name))
                return;

            if (effects.hasTrait(name))
                effects.RemoveTrait(name);

            for (int i = 0; i < back.Count; i++)
            {
                if (!effects.hasTrait(back[i]))
                    effects.AddTrait(back[i]);
            }
        }

        List<string> TraitNames()
        {
            List<string> names = new List<string>();
            List<Trait>? list = Owner.statusEffects != null ? Owner.statusEffects.TraitList : null;
            if (list == null)
                return names;

            for (int i = 0; i < list.Count; i++)
            {
                Trait trait = list[i];
                if (trait != null && !string.IsNullOrEmpty(trait.traitName))
                    names.Add(trait.traitName);
            }

            return names;
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

            Color32 orange = new Color32(232, 112, 24, 255);
            Color32 dark = new Color32(92, 24, 16, 255);
            hitbox.skinColor = orange;
            hitbox.bodyColor = orange;
            hitbox.legsColor = dark;
            hitbox.footwearColor = new Color32(255, 196, 48, 255);
            hitbox.eyesColor = new Color32(255, 220, 80, 255);
            hitbox.hairColor = new Color32(196, 32, 28, 255);
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
            if (!speedSaved)
            {
                savedSpeed = Owner.speedStatMod;
                speedSaved = true;
            }

            if (Owner.speedStatMod >= 4)
                return;

            Owner.SetSpeed(4);
        }

        void RestoreSpeed()
        {
            if (!speedSaved)
                return;

            Owner.SetSpeed(savedSpeed);
            speedSaved = false;
        }

        void GiveSublimation()
        {
            previousAbilityName = Owner.specialAbility ?? "";
            previousAbility = Owner.inventory != null ? Owner.inventory.equippedSpecialAbility : null;
            Owner.statusEffects.GiveSpecialAbility(SublimationAbility.Id);
            if (Owner.specialAbility == SublimationAbility.Id)
                return;

            InvItem ability = new InvItem();
            ability.invItemName = SublimationAbility.Id;
            ability.SetupDetails(false);
            ability.invItemCount = 0;
            ability.agent = Owner;
            if (Owner.mainGUI != null)
                ability.invInterface = Owner.mainGUI.invInterface;
            if (Owner.inventory != null)
                Owner.inventory.equippedSpecialAbility = ability;
            Owner.specialAbility = SublimationAbility.Id;
            Owner.specialAbilityAttack = true;
        }

        void RestoreSublimation()
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
