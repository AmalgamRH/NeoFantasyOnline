using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class KnightSword : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<KnightSword_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   14, 1, 20, 1f, 1f, -1), // Lv1
            new(   14, 1, 30, 1f, 1.2f, -1), // Lv2
            new(   14, 1, 45, 2f, 1.3f, -1), // Lv3
            new(   14, 1, 60, 2f, 1.4f, -1), // Lv4
            new(   15, 1, 80, 3f, 1.5f, -1), // Lv5
            new(   15, 1, 100, 3f, 1.75f, -1), // Lv6
            new(   15, 1, 200, 3f, 1.8f, -1), // Lv7
            new(   15, 1, 300, 3f, 2f, -1), // Lv8
        };

        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 0;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = (int)(CurrentStats.FireCD / CurrentStats.Speed);
            Item.useAnimation = CurrentStats.FireCD;
            Item.reuseDelay = 15;
            Item.knockBack = 4 * MathHelper.Lerp(1f, 2f, Level / 8f);
            Item.shootSpeed = 0f;

            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < CurrentStats.Count; i++)
            {
                float yOffset = (i - (CurrentStats.Count - 1) * 0.5f) * 40f;
                Vector2 spawnPos = player.Center + new Vector2(0, yOffset);
                int p = Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback,
                    player.whoAmI, ai0: i, ai1: player.direction);
                if (p >= 0)
                {
                    Main.projectile[p].localAI[0] = CurrentStats.Count;
                }

                SoundEngine.PlaySound(SoundID.Item1, position);
            }
            return false;
        }
    }
}
