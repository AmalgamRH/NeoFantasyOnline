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
    public class BraverySong : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<BraverySong_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   30, 1, 5,  1f, 0f, -1), // Lv1
            new(   30, 1, 10, 1f, 0f, -1), // Lv2
            new(   30, 1, 20, 1f, 0f, -1), // Lv3
            new(   30, 1, 35, 1f, 0f, -1), // Lv4
            new(   30, 1, 50, 1f, 0f, -1), // Lv5
            new(   30, 1, 70, 1f, 0f, -1), // Lv6
            new(   30, 1, 80, 1f, 0f, -1), // Lv7
            new(   30, 1, 100, 1f, 0f, -1), // Lv8
        };
        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 10;
            Item.useTime = CurrentStats.FireCD;
            Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 4f;
            Item.shootSpeed = 10f;

            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, 
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, 
                damage, knockback, player.whoAmI);
            return false;
        }
    }
}
