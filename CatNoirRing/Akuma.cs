using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatNoirRing
{
    static class Akuma
    {
        static readonly string[] Races =
        {
            "Vampire",
            "Zombie",
            "Werewolf",
            "WerewolfB",
            "Gorilla",
            "Alien",
            "Robot",
            "ShapeShifter",
            "Cannibal",
        };

        static readonly Dictionary<int, Memory> minds = new Dictionary<int, Memory>();
        static bool releasing;

        public static bool Turn(Agent agent)
        {
            if (releasing || !CanTarget(agent))
                return false;

            Memory memory = Capture(agent);
            if (string.IsNullOrEmpty(memory.ClassName))
                return false;

            int id = agent.GetInstanceID();
            minds[id] = memory;
            if (agent.statusEffects != null)
                agent.statusEffects.AddStatusEffect(AkumaMarkEffect.EffectId);

            try
            {
                ChangeClass(agent, Pick(memory.ClassName), "Transforming");
            }
            catch (Exception)
            {
                TryRecall(agent);
                return false;
            }

            if (agent.dead)
            {
                Forget(agent);
                return false;
            }

            EnsureMark(agent);
            NoteExtras(agent, memory);
            KeepHealth(agent, memory.Health);
            ApplyBody(agent, memory, false);
            if (!memory.Statuses.Contains("Enraged"))
                memory.ClassStatuses.Add("Enraged");
            if (agent.statusEffects != null)
                agent.statusEffects.AddStatusEffect("Enraged", Mathf.Max(1, CatNoirPlugin.ButterflyDuration.Value));
            HateAll(agent);
            return true;
        }

        public static bool Recall(Agent agent)
        {
            if (agent == null)
                return false;

            int id = agent.GetInstanceID();
            if (!minds.TryGetValue(id, out Memory memory))
                return false;
            if (agent.dead)
            {
                Forget(agent);
                return false;
            }

            GameController gc = GameController.gameController;
            if (gc == null || gc.spawnerMain == null)
                return false;

            int wounded = agent.currentHealth;
            List<InvItem> pickups = Pickups(agent, memory);
            DestroyOwned(agent, memory.ClassItems);
            RemoveStatuses(agent, memory.ClassStatuses);
            HashSet<string> beforeBack = StatusNames(agent);

            agent.originalWerewolf = memory.OriginalWerewolf;
            try
            {
                ChangeClass(agent, memory.ClassName, "Possessing");
            }
            catch (Exception)
            {
                agent.originalWerewolf = memory.OriginalWerewolf;
                Forget(agent);
                return false;
            }

            agent.originalWerewolf = memory.OriginalWerewolf;
            agent.agentRealName = memory.RealName;
            agent.agentOriginalName = memory.OriginalName;
            RestoreTraits(agent, memory.Traits);
            RemoveFreshStatuses(agent, memory.Statuses, beforeBack);
            DestroyRebound(agent, memory, pickups);
            WearAgain(agent, memory);
            RestoreAbility(agent, memory);
            KeepHealth(agent, wounded);
            agent.SetSpeed(memory.Speed);
            agent.SetStrength(memory.Strength);
            ApplyBody(agent, memory, true);
            RestoreColors(agent, memory);
            agent.originalWerewolf = memory.OriginalWerewolf;
            minds.Remove(id);
            AkumaMarkEffect.Lift(agent);
            return true;
        }

        public static void Release(Agent? agent)
        {
            if (agent == null || releasing)
                return;

            releasing = true;
            try
            {
                if (agent.dead || !Recall(agent))
                    Forget(agent);
            }
            finally
            {
                releasing = false;
            }
        }

        static void TryRecall(Agent agent)
        {
            if (!Recall(agent))
                Forget(agent);
        }

        static void Forget(Agent agent)
        {
            if (agent == null)
                return;

            minds.Remove(agent.GetInstanceID());
            AkumaMarkEffect.Lift(agent);
        }

        internal static bool CanTarget(Agent agent)
        {
            if (agent == null || agent.dead || agent.statusEffects == null || agent.isPlayer != 0)
                return false;
            if (MouseEffect.IsClone(agent) || string.IsNullOrEmpty(agent.agentName))
                return false;

            GameController gc = GameController.gameController;
            if (gc == null || gc.spawnerMain == null || agent == gc.playerAgent)
                return false;
            if (minds.ContainsKey(agent.GetInstanceID()))
                return false;
            return AkumaMarkEffect.Find(agent) == null;
        }

        static string Pick(string current)
        {
            int start = UnityEngine.Random.Range(0, Races.Length);
            for (int i = 0; i < Races.Length; i++)
            {
                string race = Races[(start + i) % Races.Length];
                if (race != current)
                    return race;
            }

            return Races[start];
        }

        static void ChangeClass(Agent agent, string className, string kind)
        {
            GameController gc = GameController.gameController;
            if (gc == null || gc.spawnerMain == null || string.IsNullOrEmpty(className))
                return;

            gc.spawnerMain.TransformAgent(agent, className, kind);
        }

        static Memory Capture(Agent agent)
        {
            Memory memory = new Memory();
            memory.ClassName = agent.agentName ?? "";
            memory.OriginalWerewolf = agent.originalWerewolf;
            memory.RealName = agent.agentRealName ?? "";
            memory.OriginalName = agent.agentOriginalName ?? "";
            memory.Ability = agent.specialAbility ?? "";
            memory.Health = agent.currentHealth;
            memory.Speed = agent.speedStatMod;
            memory.Strength = agent.strengthStatMod;
            RememberBody(agent, memory);
            memory.Traits = TraitNames(agent);
            memory.Statuses = StatusNames(agent);

            InvDatabase inventory = agent.inventory;
            if (inventory != null)
            {
                memory.AbilityItem = inventory.equippedSpecialAbility;
                memory.Weapon = inventory.equippedWeapon;
                memory.Armor = inventory.equippedArmor;
                memory.Head = inventory.equippedArmorHead;
                memory.Items = ItemRefs(agent);
            }

            AgentHitbox hitbox = agent.agentHitboxScript;
            if (hitbox != null)
            {
                memory.ColorsSaved = true;
                memory.Skin = hitbox.skinColor;
                memory.Body = hitbox.bodyColor;
                memory.Legs = hitbox.legsColor;
                memory.Eyes = hitbox.eyesColor;
                memory.Footwear = hitbox.footwearColor;
                memory.Hair = hitbox.hairColor;
                memory.FacialHair = hitbox.facialHairColor;
                memory.SkinName = hitbox.skinColorName ?? "";
                memory.HairName = hitbox.hairColorName ?? "";
                memory.FacialHairName = hitbox.facialHairColorName ?? "";
            }

            return memory;
        }

        static void NoteExtras(Agent agent, Memory memory)
        {
            List<InvItem> items = ItemRefs(agent);
            for (int i = 0; i < items.Count; i++)
            {
                if (!Holds(memory.Items, items[i]))
                    memory.ClassItems.Add(items[i]);
            }

            HashSet<string> statuses = StatusNames(agent);
            foreach (string name in statuses)
            {
                if (name == AkumaMarkEffect.EffectId || memory.Statuses.Contains(name))
                    continue;
                memory.ClassStatuses.Add(name);
            }
        }

        static void EnsureMark(Agent agent)
        {
            if (agent.statusEffects == null || AkumaMarkEffect.Find(agent) != null)
                return;

            agent.statusEffects.AddStatusEffect(AkumaMarkEffect.EffectId);
        }

        static List<InvItem> Pickups(Agent agent, Memory memory)
        {
            List<InvItem> pickups = new List<InvItem>();
            List<InvItem> items = ItemRefs(agent);
            for (int i = 0; i < items.Count; i++)
            {
                InvItem item = items[i];
                if (!Holds(memory.Items, item) && !Holds(memory.ClassItems, item))
                    pickups.Add(item);
            }

            return pickups;
        }

        static void DestroyRebound(Agent agent, Memory memory, List<InvItem> pickups)
        {
            List<InvItem> extras = new List<InvItem>();
            List<InvItem> items = ItemRefs(agent);
            for (int i = 0; i < items.Count; i++)
            {
                InvItem item = items[i];
                if (!Holds(memory.Items, item) && !Holds(pickups, item))
                    extras.Add(item);
            }

            DestroyOwned(agent, extras);
        }

        static void DestroyOwned(Agent agent, List<InvItem> items)
        {
            InvDatabase inventory = agent.inventory;
            if (inventory == null || inventory.InvItemList == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                InvItem item = items[i];
                if (item == null || !inventory.InvItemList.Contains(item))
                    continue;

                item.cantDrop = false;
                if (inventory.equippedWeapon == item)
                    inventory.UnequipWeapon(false);
                if (inventory.equippedArmor == item)
                    inventory.UnequipArmor(false);
                if (inventory.equippedArmorHead == item)
                    inventory.UnequipArmorHead(false);
                inventory.DestroyItem(item, false, false);
            }
        }

        static void WearAgain(Agent agent, Memory memory)
        {
            InvDatabase inventory = agent.inventory;
            if (inventory == null || inventory.InvItemList == null)
                return;

            if (inventory.equippedWeapon != null && !inventory.InvItemList.Contains(inventory.equippedWeapon))
                inventory.UnequipWeapon(false);
            if (memory.Weapon != null && inventory.InvItemList.Contains(memory.Weapon))
                inventory.EquipWeapon(memory.Weapon, true);
            else if (memory.Weapon == null && inventory.equippedWeapon != null)
                inventory.UnequipWeapon(false);

            if (inventory.equippedArmor != null && !inventory.InvItemList.Contains(inventory.equippedArmor))
                inventory.UnequipArmor(false);
            if (memory.Armor != null && inventory.InvItemList.Contains(memory.Armor))
                inventory.EquipArmor(memory.Armor, false);
            else if (memory.Armor == null && inventory.equippedArmor != null)
                inventory.UnequipArmor(false);

            if (inventory.equippedArmorHead != null && !inventory.InvItemList.Contains(inventory.equippedArmorHead))
                inventory.UnequipArmorHead(false);
            if (memory.Head != null && inventory.InvItemList.Contains(memory.Head))
                inventory.EquipArmorHead(memory.Head, false);
            else if (memory.Head == null && inventory.equippedArmorHead != null)
                inventory.UnequipArmorHead(false);
        }

        static void RestoreAbility(Agent agent, Memory memory)
        {
            if (agent.statusEffects == null)
                return;

            if (agent.inventory != null && !string.IsNullOrEmpty(memory.Ability))
            {
                agent.statusEffects.GiveSpecialAbility(memory.Ability);
                if (agent.specialAbility != memory.Ability && memory.AbilityItem != null)
                    agent.inventory.equippedSpecialAbility = memory.AbilityItem;
                return;
            }

            agent.specialAbility = "";
            agent.specialAbilityAttack = false;
            if (agent.inventory != null)
                agent.inventory.equippedSpecialAbility = memory.AbilityItem;
        }

        static void RestoreTraits(Agent agent, HashSet<string> original)
        {
            StatusEffects effects = agent.statusEffects;
            if (effects == null)
                return;

            List<string> current = new List<string>(TraitNames(agent));
            for (int i = 0; i < current.Count; i++)
            {
                if (!original.Contains(current[i]))
                    effects.RemoveTrait(current[i]);
            }

            foreach (string name in original)
            {
                if (!effects.hasTrait(name))
                    effects.AddTrait(name);
            }
        }

        static void RemoveStatuses(Agent agent, HashSet<string> names)
        {
            if (agent.statusEffects == null)
                return;

            foreach (string name in names)
            {
                if (name == AkumaMarkEffect.EffectId || !HasStatus(agent, name))
                    continue;
                agent.statusEffects.RemoveStatusEffect(name);
            }
        }

        static void RemoveFreshStatuses(Agent agent, HashSet<string> original, HashSet<string> beforeBack)
        {
            if (agent.statusEffects == null)
                return;

            List<string> now = new List<string>(StatusNames(agent));
            for (int i = 0; i < now.Count; i++)
            {
                string name = now[i];
                if (name == AkumaMarkEffect.EffectId || original.Contains(name) || beforeBack.Contains(name))
                    continue;
                if (HasStatus(agent, name))
                    agent.statusEffects.RemoveStatusEffect(name);
            }
        }

        static void RestoreColors(Agent agent, Memory memory)
        {
            if (!memory.ColorsSaved)
                return;

            AgentHitbox hitbox = agent.agentHitboxScript;
            if (hitbox == null)
                return;

            hitbox.skinColor = memory.Skin;
            hitbox.bodyColor = memory.Body;
            hitbox.legsColor = memory.Legs;
            hitbox.eyesColor = memory.Eyes;
            hitbox.footwearColor = memory.Footwear;
            hitbox.hairColor = memory.Hair;
            hitbox.facialHairColor = memory.FacialHair;
            hitbox.skinColorName = memory.SkinName;
            hitbox.hairColorName = memory.HairName;
            hitbox.facialHairColorName = memory.FacialHairName;
            hitbox.MustRefresh();

            ObjectSprite sprite = agent.objectSprite;
            if (sprite == null)
                return;

            sprite.agentColorDirty = true;
            sprite.mustRefreshColor = true;
        }

        static void RememberBody(Agent agent, Memory memory)
        {
            AgentHitbox hitbox = agent.agentHitboxScript;
            if (hitbox == null)
                return;

            memory.BodySaved = true;
            memory.WholeBodyMode = hitbox.wholeBodyMode;
            memory.BodyStrings = CopyStrings(hitbox.agentBodyStrings);
            memory.HeadStrings = CopyStrings(hitbox.headStrings);
        }

        static List<string> CopyStrings(List<string>? source)
        {
            List<string> copy = new List<string>();
            if (source == null)
                return copy;

            for (int i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrEmpty(source[i]))
                    copy.Add(source[i]);
            }

            return copy;
        }

        static void ApplyBody(Agent agent, Memory memory, bool original)
        {
            AgentHitbox hitbox = agent.agentHitboxScript;
            if (hitbox == null)
                return;

            try
            {
                if (original && memory.BodySaved)
                    hitbox.wholeBodyMode = memory.WholeBodyMode;
                hitbox.SetupBodyStrings();
                if (original && memory.BodySaved && memory.ClassName == "Custom")
                {
                    Paste(hitbox.agentBodyStrings, memory.BodyStrings);
                    Paste(hitbox.headStrings, memory.HeadStrings);
                    hitbox.wholeBodyMode = memory.WholeBodyMode;
                }

                hitbox.SetWBSprites();
                hitbox.SetupFeatures();
                hitbox.MustRefresh();
                hitbox.UpdateAnim();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Акума не пересобрала тело.");
                Debug.LogWarning(exception);
            }
        }

        static void Paste(List<string>? target, List<string> source)
        {
            if (target == null)
                return;

            target.Clear();
            for (int i = 0; i < source.Count; i++)
                target.Add(source[i]);
        }

        static void HateAll(Agent victim)
        {
            if (victim == null || victim.relationships == null)
                return;

            GameController gc = victim.gc != null ? victim.gc : GameController.gameController;
            if (gc == null || gc.agentList == null)
                return;

            victim.opponent = null;
            for (int i = 0; i < gc.agentList.Count; i++)
            {
                Agent other = gc.agentList[i];
                if (other == null || other == victim || other.dead || other.ghost)
                    continue;
                victim.relationships.SetRel(other, "Hateful");
            }
        }

        static void KeepHealth(Agent agent, int health)
        {
            int hp = health < 0 ? 0 : health;
            if (agent.healthMax > 0f && hp > agent.healthMax)
                hp = (int)agent.healthMax;
            agent.currentHealth = hp;
        }

        static HashSet<string> TraitNames(Agent agent)
        {
            HashSet<string> names = new HashSet<string>();
            List<Trait>? list = agent.statusEffects != null ? agent.statusEffects.TraitList : null;
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

        static HashSet<string> StatusNames(Agent agent)
        {
            HashSet<string> names = new HashSet<string>();
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return names;

            for (int i = 0; i < list.Count; i++)
            {
                StatusEffect effect = list[i];
                if (effect != null && !string.IsNullOrEmpty(effect.statusEffectName))
                    names.Add(effect.statusEffectName);
            }

            return names;
        }

        static bool HasStatus(Agent agent, string name)
        {
            List<StatusEffect>? list = agent.statusEffects != null ? agent.statusEffects.StatusEffectList : null;
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                StatusEffect effect = list[i];
                if (effect != null && effect.statusEffectName == name)
                    return true;
            }

            return false;
        }

        static List<InvItem> ItemRefs(Agent agent)
        {
            List<InvItem> items = new List<InvItem>();
            InvDatabase inventory = agent.inventory;
            if (inventory == null || inventory.InvItemList == null)
                return items;

            for (int i = 0; i < inventory.InvItemList.Count; i++)
            {
                InvItem item = inventory.InvItemList[i];
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        static bool Holds(List<InvItem> items, InvItem item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (ReferenceEquals(items[i], item))
                    return true;
            }

            return false;
        }

        sealed class Memory
        {
            public string ClassName = "";
            public bool OriginalWerewolf;
            public string RealName = "";
            public string OriginalName = "";
            public string Ability = "";
            public InvItem? AbilityItem;
            public int Health;
            public int Speed;
            public int Strength;
            public bool ColorsSaved;
            public Color32 Skin;
            public Color32 Body;
            public Color32 Legs;
            public Color32 Eyes;
            public Color32 Footwear;
            public Color32 Hair;
            public Color32 FacialHair;
            public string SkinName = "";
            public string HairName = "";
            public string FacialHairName = "";
            public InvItem? Weapon;
            public InvItem? Armor;
            public InvItem? Head;
            public int WholeBodyMode;
            public bool BodySaved;
            public List<string> BodyStrings = new List<string>();
            public List<string> HeadStrings = new List<string>();
            public HashSet<string> Traits = new HashSet<string>();
            public HashSet<string> Statuses = new HashSet<string>();
            public List<InvItem> Items = new List<InvItem>();
            public List<InvItem> ClassItems = new List<InvItem>();
            public HashSet<string> ClassStatuses = new HashSet<string>();
        }
    }
}
