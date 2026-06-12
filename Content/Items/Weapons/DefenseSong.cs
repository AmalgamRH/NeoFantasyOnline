using Microsoft.Xna.Framework;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Weapons
{
    public class DefenseSong : SkillItem
    {
        public override int ProjectileType => ModContent.ProjectileType<DefenseSong_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   150, 3, 15, 0f, 1f, 0), // Lv1
            new(   150, 4, 30, 0f, 1f, 0), // Lv2
            new(   150, 4, 45, 0f, 1.2f, 0), // Lv3
            new(   150, 5, 60, 0f, 1.2f, 0), // Lv4
            new(   150, 5, 75, 0f, 1.5f, 0), // Lv5
            new(   150, 6, 90, 0f, 1.5f, 0), // Lv6
            new(   150, 6, 105, 0f, 1.75f, 0), // Lv7
            new(   150, 7, 120, 0f, 1.75f, 0), // Lv8
        };
        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 20;
            Item.useTime = Item.useAnimation = 30;
            Item.knockBack = 3 * MathHelper.Lerp(1f, 2f, Level / 8f);
            Item.shootSpeed = 0f;

            Item.UseSound = SoundID.Item29;
            Item.autoReuse = false;
        }

        public override bool CanUseItem(Player player)
        {
            DefenseSong_Proj.ActiveDefenseSongs.RemoveAll(id => !Main.projectile[id].active || Main.projectile[id].owner != player.whoAmI);
            return DefenseSong_Proj.ActiveDefenseSongs.Count == 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            DefenseSong_Proj.ActiveDefenseSongs.RemoveAll(id => !Main.projectile[id].active || Main.projectile[id].owner != player.whoAmI);

            for (int i = 0; i < CurrentStats.Count; i++)
            {
                int proj = Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                    type, damage, knockback, player.whoAmI, ai0: i);
                Main.projectile[proj].localAI[0] = CurrentStats.Count;
                DefenseSong_Proj.ActiveDefenseSongs.Add(proj);
            }

            return false;
        }

    }
}
