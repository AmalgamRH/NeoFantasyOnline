using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class Fireball : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<Fireball_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   15, 1, 10,  1f, 0f, 1), // Lv1
            new(   15, 2, 20, 1f, 0f, 2), // Lv2
            new(   15, 2, 25, 2f, 0f, 2), // Lv3
            new(   15, 3, 30, 2f, 0f, 3), // Lv4
            new(   15, 3, 40, 2f, 0f, 3), // Lv5
            new(   15, 4, 50, 3f, 0f, 3), // Lv6
            new(   15, 4, 60, 3f, 0f, 4), // Lv7
            new(   15, 5, 70, 3f, 0f, 4), // Lv8
        };
        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 8;
            Item.useTime = CurrentStats.FireCD;
            Item.useAnimation = (int)(CurrentStats.FireCD * CurrentStats.Speed);
            Item.reuseDelay = 15;
            Item.knockBack = 4f;
            Item.shootSpeed = 10f;

            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, 
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int count = CurrentStats.Count;
            float baseSpeed = velocity.Length();
            Vector2 aimDir = Vector2.Normalize(Main.MouseWorld - player.Center);

            // 总是有一发朝向鼠标
            Projectile.NewProjectile(source, position, aimDir * baseSpeed, type, damage, knockback, player.whoAmI);

            if (count > 1)
            {
                // 收集屏幕内有直线视野的敌人
                var enemies = new List<NPC>();
                Rectangle screenRect = new Rectangle(
                    (int)Main.screenPosition.X - 100, (int)Main.screenPosition.Y - 100,
                    Main.screenWidth + 200, Main.screenHeight + 200);

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && npc.Hitbox.Intersects(screenRect)
                        && Collision.CanHitLine(player.Center, 1, 1, npc.Center, 1, 1))
                    {
                        enemies.Add(npc);
                    }
                }

                // 按距离玩家从近到远排序
                enemies.Sort((a, b) => Vector2.DistanceSquared(player.Center, a.Center)
                    .CompareTo(Vector2.DistanceSquared(player.Center, b.Center)));

                int targetCount = Math.Min(count - 1, enemies.Count);
                int scatterCount = count - 1 - targetCount;

                // 指向敌人发射
                for (int i = 0; i < targetCount; i++)
                {
                    Vector2 dir = Vector2.Normalize(enemies[i].Center - position);
                    Projectile.NewProjectile(source, position, dir * baseSpeed, type, damage, knockback, player.whoAmI);
                }

                // 多余弹幕向鼠标散射（带随机偏差）
                for (int i = 0; i < scatterCount; i++)
                {
                    Vector2 dir = velocity.RotatedByRandom(MathHelper.ToRadians(15));
                    dir *= 1f - Main.rand.NextFloat(0.2f);
                    Projectile.NewProjectile(source, position, dir, type, damage, knockback, player.whoAmI);
                }
            }

            return false;
        }
    }
}
