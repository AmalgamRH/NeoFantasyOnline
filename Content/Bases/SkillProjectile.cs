using NeoFantasyOnline.Content.DamageClasses;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Bases
{
    public abstract class SkillProjectile : ModProjectile
    {
        public int Level { get; set; }

        public const string TextureBasePath = "NeoFantasyOnline/Assets/Projectiles/";

        protected virtual string TextureName => GetType().Name;

        public override string Texture => TextureBasePath + TextureName;

        public abstract int ItemType { get; }

        public SkillLevelData Stats { get; set; }

        public sealed override void SetDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<SkillDamage>();
            SetSkillDefaults();
        }

        public virtual void SetSkillDefaults() { }

        public sealed override void OnSpawn(IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem { Item: Item item } && item.ModItem is SkillItem skillItem)
            {
                Level = skillItem.Level;
                Stats = skillItem.CurrentStats;
            }
            OnSkillProjSpawn(source);
        }
        public virtual void OnSkillProjSpawn(IEntitySource source) { }
    }
}
