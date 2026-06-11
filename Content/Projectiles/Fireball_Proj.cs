using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class Fireball_Proj : SkillProjectile
    {
        public override int ItemType => ModContent.ItemType<Fireball>();
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 10000;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.scale = 0.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.penetrate = Stats.HitTimes;
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

            if (Main.rand.NextBool(10))
            {
                Vector2 velocity = Projectile.velocity * 0.5f;
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch,
                    velocity.X, velocity.Y, 100, default, 1.5f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.2f, 1.7f) * Projectile.scale;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, (int)(60 * MathHelper.Lerp(1f, 8f, Level / 8f)));
            if (Projectile.penetrate > 0)
                MakeDust();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.OnFire, (int)(60 * MathHelper.Lerp(1f, 8f, Level / 8f)));
            if (Projectile.penetrate > 0)
                MakeDust();
        }

        public override void OnKill(int timeLeft)
        {
            MakeDust();
        }

        private void MakeDust()
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16 * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, 1.5f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 2.5f) * Projectile.scale;
            }

            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.Torch, vel, 0, default, 1.5f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 2.5f) * Projectile.scale;
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameWidth = tex.Width;
            int frameHeight = tex.Height / Main.projFrames[Type];

            Main.spriteBatch.Draw(tex,
                Projectile.Center - Main.screenPosition + new Vector2(12 * Projectile.scale, 0).RotatedBy(Projectile.rotation),
                new Rectangle(0, frameHeight * Projectile.frame, frameWidth, frameHeight),
                Color.White,
                Projectile.rotation,
                new Vector2(frameWidth, frameHeight) / 2,
                Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);

            /*Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(
                    (int)(Projectile.getRect().X - Main.screenPosition.X),
                    (int)(Projectile.getRect().Y - Main.screenPosition.Y),
                    Projectile.getRect().Width, Projectile.getRect().Height),
                Color.Blue * 0.5f);*/
            return false;
        }
    }
}
