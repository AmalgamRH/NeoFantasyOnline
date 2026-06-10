using Terraria;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.DamageClasses
{
    public class SkillDamage : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            if (damageClass == DamageClass.Generic)
                return StatInheritanceData.Full;

            return StatInheritanceData.None;
        }

        public override bool GetEffectInheritance(DamageClass damageClass) => false;

        public override void SetDefaultStats(Player player)
        {
            player.GetCritChance<SkillDamage>() += 4;
        }

        public override bool UseStandardCritCalcs => true;
    }
}
