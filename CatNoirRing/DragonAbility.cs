using RogueLibsCore;

namespace CatNoirRing
{
    [ItemName(Id)]
    public class DragonAbility : CustomAbility
    {
        public const string Id = "DragonPower";

        public override void OnAdded()
        {
            if (Owner != null)
                Owner.specialAbilityAttack = true;
        }

        public override void OnPressed()
        {
            Agent? agent = Owner;
            if (agent == null)
                return;

            DragonEffect? form = DragonEffect.Find(agent);
            if (form == null || !form.CycleWeapon())
            {
                gc.audioHandler.Play(agent, "CantDo");
                return;
            }

            form.BeginCountdown();
            gc.audioHandler.Play(agent, "SelectItem");
        }
    }
}
