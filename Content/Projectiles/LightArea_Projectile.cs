using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class LightArea_Projectile : SkillProjectile
    {
        public override int ItemType => ModContent.ItemType<LightArea>();
        public override void SetSkillDefaults()
        {
            Projectile.width = 234;
            Projectile.height = 234;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 180;
            Projectile.drawLayer = ProjectileDrawLayerID.BehindNPCsAndTiles;
        }

        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.localNPCHitCooldown = Stats.HitTimes;
            Projectile.ownerHitCheck = Level < 5;
            Projectile.scale = Stats.Size;
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
        }

        public int timer;
        public override void AI()
        {
            timer++;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = 0f;

            int elapsed = timer;
            int introLen = 30;
            int outroLen = 30;

            if (Projectile.timeLeft <= outroLen)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / (float)outroLen));
                Projectile.scale = Stats.Size;
            }
            else if (elapsed <= introLen)
            {
                float t = elapsed / (float)introLen;
                Projectile.scale = Stats.Size * t;
                Projectile.alpha = (int)(255 * (1f - t));
            }
            else
            {
                Projectile.scale = Stats.Size;
                Projectile.alpha = 0;
            }
        }

        public override void OnKill(int timeLeft)
        {
            LightArea.ActiveLights.Remove(Projectile.whoAmI);
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle source = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source,
                Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale,
                SpriteEffects.None, 0f);
            return false;
        }
    }
}
