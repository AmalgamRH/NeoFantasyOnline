using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class LightArea : SkillItem
    {
        public static List<int> ActiveLights = new List<int>();

        public override int ProjectileType => ModContent.ProjectileType<LightArea_Projectile>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            // 这里用HitTimes指代攻击间隔
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new( 90, 3, 5, 0f, 1f, 20), // Lv1
            new( 90, 3, 8, 0f, 1.1f, 30), // Lv2
            new( 90, 4, 10, 0f, 1.2f, 40), // Lv3
            new( 90, 4, 15, 0f, 1.5f, 40), // Lv4
            new( 90, 5, 30, 0f, 1.6f, 30), // Lv5
            new( 90, 5, 45, 0f, 1.8f, 30), // Lv6
            new( 90, 6, 60, 0f, 2f, 20), // Lv7
            new( 90, 6, 70, 0f, 2f, 20), // Lv8
        };

        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 40;
            Item.useTime = CurrentStats.FireCD;
            Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 0f;
            Item.shootSpeed = 0f;
            Item.crit = -4;
            Item.autoReuse = false;
        }
        public override bool CanUseItem(Player player)
        {
            ActiveLights.RemoveAll(id => !Main.projectile[id].active || Main.projectile[id].owner != player.whoAmI);
            return ActiveLights.Count == 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            ActiveLights.RemoveAll(id => !Main.projectile[id].active || Main.projectile[id].owner != player.whoAmI);
            var usedPositions = new List<Vector2>();

            for (int i = 0; i < CurrentStats.Count; i++)
            {
                Vector2 spawnPos;
                int attempts = 0;
                do
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(80f, 160f * MathHelper.Lerp(1f, 3f, Level / 8f));
                    spawnPos = player.MountedCenter +
                        new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * radius;
                }
                while ((usedPositions.Exists(p => Vector2.Distance(p, spawnPos) < 100f * CurrentStats.Size)
                    || WorldGen.SolidOrSlopedTile((int)(spawnPos.X / 16), (int)(spawnPos.Y / 16))) && ++attempts < 20 * CurrentStats.Size);

                int p = Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback,
                    player.whoAmI);
                if (p >= 0)
                {
                    ActiveLights.Add(p);
                    usedPositions.Add(spawnPos);
                }
            }

            return false;
        }
    }
}
