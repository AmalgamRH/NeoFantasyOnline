using System.IO;
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

        private bool _spawnSynced;

        public sealed override void SetDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<SkillDamage>();
            _spawnSynced = false;
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

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Level);
            writer.Write(Stats.FireCD);
            writer.Write(Stats.Count);
            writer.Write(Stats.Damage);
            writer.Write(Stats.Speed);
            writer.Write(Stats.Size);
            writer.Write(Stats.HitTimes);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Level = reader.ReadInt32();
            Stats = new SkillLevelData(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadInt32()
            );
            if (!_spawnSynced)
            {
                OnSkillProjSpawn(null);
                _spawnSynced = true;
            }
        }
    }
}
