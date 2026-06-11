using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class Judgement : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<Judgement_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            // 借用 HitTimes 代表晕眩时间
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(  90, 1, 60, 1f, 1f, 30), // Lv1
            new(  90, 1, 80, 1f, 1.1f, 30), // Lv2
            new(  90, 1, 120, 1f, 1.2f, 45), // Lv3
            new(  90, 1, 180, 1f, 1.3f, 60), // Lv4
            new(  90, 1, 260, 1f, 1.4f, 75), // Lv5
            new(  90, 1, 320, 1f, 1.5f, 90), // Lv6
            new(  90, 1, 400, 1f, 1.6f, 100), // Lv7
            new(  90, 1, 480, 1f, 1.75f, 120), // Lv8
        };

        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 40;
            Item.useTime = CurrentStats.FireCD;
            Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 0f;
            Item.shootSpeed = 0f;
            Item.UseSound = SoundID.Item80;
            Item.autoReuse = true;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mouse = Main.MouseWorld;
            float targetY = mouse.Y;
            int tileX = (int)(mouse.X / 16);
            int tileY = (int)(mouse.Y / 16);

            float noPhase = 0f;

            if (WorldGen.SolidTile(tileX, tileY))
            {
                bool found = false;
                for (int dy = 1; dy <= 10; dy++)
                {
                    if (!WorldGen.SolidTile(tileX, tileY - dy))
                    {
                        targetY = (tileY - dy) * 16;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Point spawnTile = new Vector2(mouse.X, player.Center.Y - 600).ToTileCoordinates();
                    if (WorldGen.SolidTile(spawnTile.X, spawnTile.Y))
                        return false;
                    noPhase = 1f;
                }
            }

            float spawnY = player.Center.Y - 600;
            Vector2 spawnPos = new Vector2(mouse.X, spawnY);

            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback,
                player.whoAmI, ai1: targetY, ai2: noPhase);
            return false;
        }
    }
}
