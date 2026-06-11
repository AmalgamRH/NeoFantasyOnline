using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class DarkSummon : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<DarkSummon_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //       FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(  30, 1, 9, 10f, 1f, 3), // Lv1  ← 在这里填数据
            new(  30, 1, 14, 10f, 1f, 4), // Lv2
            new(  30, 1, 20, 10f, 1f, 5), // Lv3
            new(  30, 1, 30, 10f, 1f, 6), // Lv4
            new(  30, 1, 40, 12f, 1.2f, 7), // Lv5
            new(  30, 1, 60, 12f, 1.2f, 8), // Lv6
            new(  30, 1, 80, 12f, 1.2f, 9), // Lv7
            new(  30, 1, 99, 12f, 1.2f, 10), // Lv8
        };

        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 10;
            Item.useTime = CurrentStats.FireCD;
            Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 2f;
            Item.shootSpeed = CurrentStats.Speed;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType] <= 5 * CurrentStats.Count;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < CurrentStats.Count; i++)
            {
                Vector2 dir = velocity.RotatedByRandom(MathHelper.ToRadians(15 * (i - (CurrentStats.Count - 1) * 0.5f)));
                int p = Projectile.NewProjectile(source, position, dir, type, damage, knockback,
                    player.whoAmI, ai1: -1);
                if (p >= 0)
                    Main.projectile[p].localAI[0] = CurrentStats.HitTimes;
            }
            return false;
        }
    }
}
