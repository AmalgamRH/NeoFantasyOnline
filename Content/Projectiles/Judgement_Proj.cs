using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class Judgement_Proj : SkillProjectile
    {
        private enum Phase
        {
            FallingThrough,
            FallingCheckTile,
            Landing,
            Staying,
            FadingOut
        }
        public override int ItemType => ModContent.ItemType<Judgement>();

        private Phase State { get => (Phase)(int)Projectile.ai[0]; set => Projectile.ai[0] = (int)value; }
        private float TargetY { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }

        private int _animTimer;
        private int _stayTimer;
        private int _fadeTimer;
        private int _hitTileX = -1;
        private int _hitTileY = -1;
        private readonly HashSet<int> _hitNPCs = new HashSet<int>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 11;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 250;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.drawLayer = ProjectileDrawLayerID.BehindNPCsAndTiles;
        }

        public override void OnSkillProjSpawn(IEntitySource source)
        {
            TargetY = Projectile.ai[1];
            _animTimer = 0;

            Projectile.scale = Stats.Size;
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);

            State = Projectile.ai[2] == 1f ? Phase.FallingCheckTile : Phase.FallingThrough;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            Lighting.AddLight(Projectile.Center, TorchID.Yellow);
            switch (State)
            {
                case Phase.FallingThrough:
                    {
                        _fadeTimer++;
                        Projectile.alpha = (int)MathHelper.Lerp(255, 0, _fadeTimer / 10f);

                        Projectile.velocity.Y = 8f;
                        if (++_animTimer >= 8)
                        {
                            _animTimer = 0;

                            Projectile.frame++;
                            if (Projectile.frame > 2)
                            {
                                Projectile.frame = 0;
                            }
                        }

                        if (Projectile.Bottom.Y >= TargetY)
                        {
                            State = Phase.FallingCheckTile;
                        }
                        break;
                    }
                case Phase.FallingCheckTile:
                    {
                        _fadeTimer++;
                        Projectile.alpha = (int)MathHelper.Lerp(255, 0, _fadeTimer / 30f);

                        Projectile.velocity.Y = 8f;
                        if (++_animTimer >= 8)
                        {
                            _animTimer = 0;

                            Projectile.frame++;
                            if (Projectile.frame > 2)
                            {
                                Projectile.frame = 0;
                            }
                        }

                        int checkLeft = (int)(Projectile.Center.X - 24) / 16;
                        int checkTop = (int)(Projectile.Bottom.Y - 16) / 16;
                        int checkRight = (int)(Projectile.Center.X + 24) / 16;
                        int checkBottom = (int)(Projectile.Bottom.Y) / 16;

                        for (int tx = checkLeft; tx <= checkRight; tx++)
                        {
                            for (int ty = checkTop; ty <= checkBottom; ty++)
                            {
                                Tile t = Framing.GetTileSafely(tx, ty);
                                if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                                {
                                    _hitTileX = tx;
                                    _hitTileY = ty;

                                    PunchCameraModifier modifier = new PunchCameraModifier(
                                        Projectile.Center, 
                                        (Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(), 
                                        12f, 6f, 20, 800f, 
                                        Projectile.identity.ToString());

                                    Main.instance.CameraModifiers.Add(modifier);
                                    State = Phase.Landing;
                                    _animTimer = 0;
                                    Projectile.velocity = Vector2.Zero;
                                    Projectile.frame = 3;
                                    goto landed;
                                }
                            }
                        }
                        landed:;
                        break;
                    }
                case Phase.Landing:
                    {
                        Projectile.alpha = 0;
                        if (++_animTimer >= 4)
                        {
                            _animTimer = 0;
                            if (Projectile.frame < 8)
                                Projectile.frame++;
                            else
                            {
                                State = Phase.Staying;
                                _stayTimer = 0;
                            }
                        }
                        Projectile.velocity *= Vector2.Zero;
                        break;
                    }
                case Phase.Staying:
                    {
                        Projectile.velocity *= Vector2.Zero;
                        _stayTimer++;
                        if (_stayTimer >= 60)
                        {
                            State = Phase.FadingOut;
                            _fadeTimer = 0;
                        }
                        break;
                    }
                case Phase.FadingOut:
                    {
                        Projectile.velocity *= Vector2.Zero;
                        _fadeTimer++;
                        Projectile.alpha = (int)MathHelper.Lerp(0, 255, _fadeTimer / 30f);
                        if (_fadeTimer >= 30)
                            Projectile.Kill();
                        break;
                    }
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (_hitNPCs.Contains(target.whoAmI))
                return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.GetGlobalNPC<JudgementGlobalNPC>().Dazed < Stats.HitTimes)
            {
                target.GetGlobalNPC<JudgementGlobalNPC>().Dazed = Stats.HitTimes;
            }

            if (State >= Phase.Landing)
            {
                _hitNPCs.Add(target.whoAmI);
            }
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle source = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(tex, drawPos,
                source, lightColor * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            // DrawDebugRects();
            return false;
        }

#if DEBUG
        private void DrawDebugRects()
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            int cl = (int)(Projectile.Center.X - 24 * Projectile.scale) / 16;
            int ct = (int)(Projectile.Bottom.Y - 48 * Projectile.scale) / 16;
            int cr = (int)(Projectile.Center.X + 24 * Projectile.scale) / 16;
            int cb = (int)(Projectile.Bottom.Y) / 16;

            Rectangle checkZone = new Rectangle(
                cl * 16 - (int)Main.screenPosition.X,
                ct * 16 - (int)Main.screenPosition.Y,
                (cr - cl + 1) * 16,
                (cb - ct + 1) * 16);
            DrawRect(pixel, checkZone, Color.Red * 0.5f);

            Rectangle hitbox = new Rectangle(
                (int)(Projectile.position.X - Main.screenPosition.X),
                (int)(Projectile.position.Y - Main.screenPosition.Y),
                Projectile.width, Projectile.height);
            DrawRect(pixel, hitbox, Color.Lime * 0.4f);

            Rectangle tLine = new Rectangle(
                cl * 16 - (int)Main.screenPosition.X,
                (int)(TargetY - Main.screenPosition.Y),
                (cr - cl + 1) * 16, 2);
            Main.spriteBatch.Draw(pixel, tLine, Color.Cyan * 0.6f);

            if (_hitTileX >= 0)
            {
                Rectangle hitTile = new Rectangle(
                    _hitTileX * 16 - (int)Main.screenPosition.X,
                    _hitTileY * 16 - (int)Main.screenPosition.Y,
                    16, 16);
                Main.spriteBatch.Draw(pixel, hitTile, Color.Yellow * 0.5f);
                DrawRect(pixel, hitTile, Color.Yellow * 0.8f);
            }
        }
        private static void DrawRect(Texture2D pixel, Rectangle rect, Color color)
        {
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color);
        }
#endif
    }

    public class JudgementGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int Dazed = 0;
        public override void AI(NPC npc)
        {
            if (Dazed > 0)
            {
                Dazed--;
                npc.velocity.Y = npc.velocity.Y * 0;
                npc.velocity.X = npc.velocity.X * 0;
            }
        }
    }
}
