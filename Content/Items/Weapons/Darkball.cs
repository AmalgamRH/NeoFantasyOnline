using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class Darkball : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<Darkball_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   60, 2, 15, 0f, 0f, 1), // Lv1
            new(   60, 2, 30, 0f, 0f, 1), // Lv2
            new(   50, 3, 45, 0f, 0f, 1), // Lv3
            new(   40, 3, 60, 0f, 0f, 1), // Lv4
            new(   40, 3, 75, 0f, 0f, 1), // Lv5
            new(   30, 3, 100, 0f, 0f, 1), // Lv6
            new(   30, 4, 105, 0f, 0f, 1), // Lv7
            new(   30, 4, 140, 0f, 0f, 1), // Lv8
        };
        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 15;
            Item.useTime = Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 3f;
            Item.shootSpeed = 6f;

            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType] <= 50;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float baseSpeed = velocity.Length();
            float angleStep = MathHelper.TwoPi / CurrentStats.Count;

            for (int i = 0; i < CurrentStats.Count; i++)
            {
                float angle = angleStep * i + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * baseSpeed;
                Projectile.NewProjectile(source, position, dir, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
    }
}
