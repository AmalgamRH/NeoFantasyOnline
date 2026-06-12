/* Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class BraverySong_Proj : SkillProjectile
    {
        public override int ItemType => ModContent.ItemType<BraverySong>();
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 183;
            Projectile.height = 41;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void OnSkillProjSpawn(IEntitySource source)
        {
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            Projectile.rotation = MathHelper.Pi + Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
        }

        public override void OnKill(int timeLeft)
        {
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X > player.MountedCenter.X ? 1 : -1;
        }
        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameWidth = tex.Width;
            int frameHeight = tex.Height / Main.projFrames[Type];

            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, frameWidth, frameHeight),
                Color.White,
                Projectile.rotation,
                new Vector2(frameWidth, frameHeight) / 2,
                Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);

            return false;
        }
    }
}*/
