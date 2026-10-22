namespace CatNoirRing
{
    static class TalismanForms
    {
        public static void RemoveOthers(Agent owner, string keep)
        {
            if (owner.statusEffects == null)
                return;

            Drop(owner, keep, CatNoirEffect.EffectId, CatNoirEffect.Find(owner) != null);
            Drop(owner, keep, LadybugEffect.EffectId, LadybugEffect.Find(owner) != null);
            Drop(owner, keep, MouseEffect.EffectId, MouseEffect.Find(owner) != null);
            Drop(owner, keep, BeeEffect.EffectId, BeeEffect.Find(owner) != null);
            Drop(owner, keep, SnakeEffect.EffectId, SnakeEffect.Find(owner) != null);
            Drop(owner, keep, HorseEffect.EffectId, HorseEffect.Find(owner) != null);
            Drop(owner, keep, TigerEffect.EffectId, TigerEffect.Find(owner) != null);
            Drop(owner, keep, DogEffect.EffectId, DogEffect.Find(owner) != null);
            Drop(owner, keep, GoatEffect.EffectId, GoatEffect.Find(owner) != null);
            Drop(owner, keep, RoosterEffect.EffectId, RoosterEffect.Find(owner) != null);
            Drop(owner, keep, MonkeyEffect.EffectId, MonkeyEffect.Find(owner) != null);
            Drop(owner, keep, DragonEffect.EffectId, DragonEffect.Find(owner) != null);
            Drop(owner, keep, OxEffect.EffectId, OxEffect.Find(owner) != null);
            Drop(owner, keep, PeacockEffect.EffectId, PeacockEffect.Find(owner) != null);
            Drop(owner, keep, ButterflyEffect.EffectId, ButterflyEffect.Find(owner) != null);
            Drop(owner, keep, TurtleEffect.EffectId, TurtleEffect.Find(owner) != null);
            Drop(owner, keep, PigEffect.EffectId, PigEffect.Find(owner) != null);
            Drop(owner, keep, FoxEffect.EffectId, FoxEffect.Find(owner) != null);
            Drop(owner, keep, RabbitEffect.EffectId, RabbitEffect.Find(owner) != null);
        }

        static void Drop(Agent owner, string keep, string effectId, bool present)
        {
            if (keep == effectId || !present)
                return;
            owner.statusEffects.RemoveStatusEffect(effectId);
        }
    }
}
