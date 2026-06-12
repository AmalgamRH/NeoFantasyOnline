using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class Darkball_Proj : SkillProjectile
    {
        public float HomingSpeed => 12f * MathHelper.Lerp(1f, 1.5f, Level / 8f);
        public bool HasEverLocked { get => Projectile.localAI[0] != 0f; set => Projectile.localAI[0] = value ? 1f : 0f; }

        public override int ItemType => ModContent.ItemType<Darkball>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.penetrate = Stats.HitTimes;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            Projectile.rotation = MathHelper.Pi + Projectile.velocity.ToRotation();

            float targetX = Projectile.Center.X;
            float targetY = Projectile.Center.Y;
            float closestDist = -1f;
            bool found = false;

            if (!HasEverLocked && player.channel)
            {
            }
            else
            {
                Rectangle screenRect = new Rectangle(
                    (int)Main.screenPosition.X - 100, (int)Main.screenPosition.Y - 100,
                    Main.screenWidth + 200, Main.screenHeight + 200);

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile, false) && npc.Hitbox.Intersects(screenRect)
                        && (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1)|| Collision.CanHitLine(player.Center, 1, 1, npc.Center, 1, 1)))
                    {
                        float dx = Math.Abs(Projectile.Center.X - npc.Center.X);
                        float dy = Math.Abs(Projectile.Center.Y - npc.Center.Y);
                        float dist = dx + dy;
                        if (closestDist < 0 || dist < closestDist)
                        {
                            closestDist = dist;
                            targetX = npc.Center.X;
                            targetY = npc.Center.Y;
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                HasEverLocked = true;
                Projectile.tileCollide = false;
                Projectile.timeLeft++;
                Vector2 vector = Projectile.Center;
                float num8 = targetX - vector.X;
                float num9 = targetY - vector.Y;
                float num10 = (float)Math.Sqrt(num8 * num8 + num9 * num9);
                num10 = HomingSpeed / num10;
                num8 *= num10;
                num9 *= num10;
                Projectile.velocity.X = (Projectile.velocity.X * 20f + num8) / 21f;
                Projectile.velocity.Y = (Projectile.velocity.Y * 20f + num9) / 21f;
            }
            else if (!HasEverLocked && Projectile.timeLeft <= 260)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.tileCollide = true;
                    Vector2 mouseDir = Vector2.Normalize(Main.MouseWorld - Projectile.Center);
                    float dx = mouseDir.X * HomingSpeed;
                    float dy = mouseDir.Y * HomingSpeed;
                    Projectile.velocity.X = (Projectile.velocity.X * 20f + dx) / 21f;
                    Projectile.velocity.Y = (Projectile.velocity.Y * 20f + dy) / 21f;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.tileCollide = true;
            }

            if (!Main.dedServ && Main.rand.NextBool(8))
            {
                Vector2 vel = Projectile.velocity * 0.3f;
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, vel.X, vel.Y, 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, (int)(60 * MathHelper.Lerp(1f, 4f, Level / 8f)));
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X > player.MountedCenter.X ? 1 : -1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12 * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 0, default, Projectile.scale);
                d.noGravity = true;
            }

            for (int i = 0; i < 30; i++)
            {
                int type = Main.rand.NextBool(4) ? DustID.Granite : DustID.ShadowbeamStaff;
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    type, vel.X, vel.Y, 0, default, 1f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1f, 2f);
                if (type == DustID.ShadowbeamStaff)
                {
                    d.scale = Main.rand.NextFloat(1f, 2.5f);
                    d.velocity = Main.rand.NextVector2Circular(16f, 16f);
                }
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];

            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            int length = Math.Min(10, 6 + (int)Projectile.oldVelocity.Length());
            for (int i = length; i >= 0; i--)
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.oldVelocity * i * 0.5f;
                float trailOpacity = Projectile.Opacity - 0.05f - 0.95f / length * i;
                if (i != 0)
                {
                    trailOpacity /= 2f;
                }
                if (trailOpacity > 0f)
                {
                    float colMod = 0.4f + 0.6f * trailOpacity;
                    Main.spriteBatch.Draw(tex, drawPos.ToPoint().ToVector2(),
                        new Rectangle?(new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight)),
                        Color.Lerp(Color.Purple, Color.MediumPurple, i / (float)length) * trailOpacity,
                        Projectile.rotation,
                        new Vector2(tex.Width, frameHeight) / 2,
                        Projectile.scale * (1f - 0.03f * i), effects, 0f);
                }
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight),
                lightColor, Projectile.rotation, 
                new Vector2(tex.Width, frameHeight) / 2, 
                Projectile.scale,
                effects, 0f);
            return false;
        }
    }
}
