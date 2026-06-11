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
    public class Hurricane : SkillItem
    {
        public bool TileCollide = true;

        private Vector2? _spawnPos;
        public override int ProjectileType => ModContent.ProjectileType<Hurricane_Proj>();
        public override SkillLevelData[] StatsByLevel => new SkillLevelData[]
        {
            // 这里借用速度来算最大距离
            //  FireCD, Cnt, ATK, SPD, Size, HitTimes
            new(   60, 1, 20,  16f,  1f,    -1), // Lv1
            new(   60, 1, 30,  24f,  1f,    -1), // Lv2
            new(   60, 2, 40,  48f,  1.2f,  -1), // Lv3
            new(   60, 2, 60,  64f,  1.2f,  -1), // Lv4
            new(   60, 2, 80,  100f, 1.5f,  -1), // Lv5
            new(   60, 2, 120, 100f, 1.5f,  -1), // Lv6
            new(   60, 2, 160, 100f, 1.75f, -1), // Lv7
            new(   60, 2, 220, 100f, 1.75f, -1), // Lv8
        };
        public override void SetSkillDefaults()
        {
            Item.damage = CurrentStats.Damage;
            Item.mana = 25;
            Item.useTime = Item.useAnimation = CurrentStats.FireCD;
            Item.knockBack = 1 * MathHelper.Lerp(1f, 2f, Level / 8f);
            Item.shootSpeed = 0f;

            Item.UseSound = SoundID.Item84;
            Item.autoReuse = true;

            TileCollide = Level <= 4;
        }

        public override bool CanUseItem(Player player)
        {
            Vector2 mouse = Main.MouseWorld;
            Vector2 checkPos = TileCollide ? RaycastBlocked(player.Center, mouse) : mouse;
            _spawnPos = FindEmptySpace(checkPos);
            return _spawnPos.HasValue;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var list = Hurricane_Proj.ActiveHurricanes;
            list.RemoveAll(id => !Main.projectile[id].active);

            if (list.Count >= CurrentStats.Count)
            {
                int oldestId = list[0];
                list.RemoveAt(0);
                if (Main.projectile[oldestId].active)
                    Main.projectile[oldestId].timeLeft = (int)Hurricane_Proj.OutroTime;
            }

            int proj = Projectile.NewProjectile(source, _spawnPos.Value, Vector2.Zero, 
                type, damage, knockback, player.whoAmI);
            list.Add(proj);
            return false;
        }

        #region 检测方式1：射线检测，中间不能有阻挡，并且有最大距离
        private Vector2 RaycastBlocked(Vector2 origin, Vector2 target)
        {
            // 这里借用速度来算最大距离
            float MaxDistance = CurrentStats.Speed * 16f;
            Vector2 toTarget = target - origin;

            if (toTarget.Length() > MaxDistance)
            {
                toTarget = toTarget.SafeNormalize(Vector2.UnitX) * MaxDistance;
                target = origin + toTarget;
            }

            Vector2 dir = toTarget.SafeNormalize(Vector2.UnitX);
            float length = toTarget.Length();

            for (float i = 0f; i < length; i += 4f)
            {
                Point tile = (origin + dir * i).ToTileCoordinates();
                if (WorldGen.SolidTile(tile.X, tile.Y))
                {
                    return origin + dir * Math.Max(0f, i - 4f);
                }
            }

            return target;
        }
        #endregion

        #region 检测方式2：检测鼠标周围是否有空地

        private static Vector2? FindEmptySpace(Vector2 worldPos)
        {
            int tileX = (int)(worldPos.X / 16);
            int tileY = (int)(worldPos.Y / 16);

            if (!IsSolid(tileX, tileY))
                return worldPos;

            for (int dist = 1; dist <= 10; dist++)
            {
                int[] offsets = { -dist, dist };
                foreach (int dy in offsets)
                {
                    int nx = tileX;
                    int ny = tileY + dy;
                    if (nx >= 0 && ny >= 0 && nx < Main.maxTilesX && ny < Main.maxTilesY && !IsSolid(nx, ny))
                        return new Vector2(nx * 16 + 8, ny * 16 + 8);
                }
                foreach (int dx in offsets)
                {
                    int nx = tileX + dx;
                    int ny = tileY;
                    if (nx >= 0 && ny >= 0 && nx < Main.maxTilesX && ny < Main.maxTilesY && !IsSolid(nx, ny))
                        return new Vector2(nx * 16 + 8, ny * 16 + 8);
                }
            }

            return null;
        }

        private static bool IsSolid(int x, int y)
        {
            Tile t = Framing.GetTileSafely(x, y);
            return t.HasTile && Main.tileSolid[t.TileType];
        }

        #endregion

    }
}
