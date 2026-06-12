using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class DarkSummon_Proj : SkillProjectile
    {
        public override int ItemType => ModContent.ItemType<DarkSummon>();

        private int CurrentTarget { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private int ChainsLeft { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private int BouncesLeft { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

        private float HomingSpeed => Stats.Speed;
        private bool _hasEverTracked;
        private bool _noTargetCountdown;
        private float _effectTimer;
        private List<int> _hitNPCs = new List<int>();
        private Dictionary<int, int> _bossTimer = new Dictionary<int, int>();
        private static SoundStyle HitSound;
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                HitSound = new SoundStyle("NeoFantasyOnline/Assets/Sounds/DarkSummon_Hit")
                {
                    MaxInstances = 10,
                    Volume = 0.3f,
                };
            }
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSkillProjSpawn(IEntitySource source)
        {
            ChainsLeft = (int)Projectile.localAI[0];
            BouncesLeft = Stats.HitTimes;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            if (!Main.dedServ && Main.rand.NextBool(180))
                SoundEngine.PlaySound(HitSound, Projectile.Center);

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            Projectile.rotation = MathHelper.Pi + Projectile.velocity.ToRotation();

            int target = CurrentTarget;
            if (target >= 0)
            {
                NPC npc = Main.npc[target];
                if (npc.active && npc.CanBeChasedBy(Projectile, false))
                {
                    Vector2 toTarget = npc.Center - Projectile.Center;
                    float dist = toTarget.Length();
                    if (dist > 0f)
                    {
                        Vector2 desired = toTarget / dist * HomingSpeed;
                        Projectile.velocity.X = (Projectile.velocity.X * 20f + desired.X) / 21f;
                        Projectile.velocity.Y = (Projectile.velocity.Y * 20f + desired.Y) / 21f;
                    }
                }
                else
                    CurrentTarget = -1;
            }

            if (CurrentTarget >= 0)
            {
                Projectile.timeLeft++;
                _hasEverTracked = true;
            }
            else
            {
                if (_hasEverTracked && !_noTargetCountdown)
                {
                    _noTargetCountdown = true;
                    Projectile.timeLeft = 120;
                    Projectile.tileCollide = true;
                }

                if (ChainsLeft > 0 && _hasEverTracked)
                {
                    NPC next = FindNextTarget();
                    if (next != null)
                    {
                        CurrentTarget = next.whoAmI;
                        Projectile.tileCollide = false;
                        _noTargetCountdown = false;
                    }
                }
            }

            if (_bossTimer.Count > 0)
            {
                var expired = new List<int>();
                foreach (var kv in _bossTimer)
                {
                    if (Main.GameUpdateCount - kv.Value >= 20)
                        expired.Add(kv.Key);
                }
                foreach (int who in expired)
                {
                    _hitNPCs.Remove(who);
                    _bossTimer.Remove(who);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            MakeDust();
            _hitNPCs.Add(target.whoAmI);
            if (target.boss)
                _bossTimer[target.whoAmI] = (int)Main.GameUpdateCount;
            ChainsLeft--;
            CurrentTarget = -1;
            Projectile.netUpdate = true;

            if (ChainsLeft <= 0)
            {
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 30);
                return;
            }

            NPC next = FindNextTarget();
            if (next != null)
            {
                CurrentTarget = next.whoAmI;
                Projectile.tileCollide = false;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            MakeDust();
            BouncesLeft--;
            Projectile.netUpdate = true;
            if (BouncesLeft < 0)
            {
                Projectile.Kill();
                return false;
            }

            if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
                Projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        private NPC FindNextTarget()
        {
            NPC result = null;
            float closestDist = -1f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy(Projectile, false))
                    continue;
                if (_hitNPCs.Contains(npc.whoAmI))
                    continue;
                if (Projectile.localNPCImmunity[npc.whoAmI] > 0)
                    continue;

                float dx = npc.Center.X - Projectile.Center.X;
                float dy = npc.Center.Y - Projectile.Center.Y;
                float dist = dx * dx + dy * dy;
                if (closestDist < 0f || dist < closestDist)
                {
                    closestDist = dist;
                    result = npc;
                }
            }
            return result;
        }

        public override void OnKill(int timeLeft)
        {
            MakeDust();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            writer.Write(ChainsLeft);
            writer.Write(BouncesLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            ChainsLeft = reader.ReadInt32();
            BouncesLeft = reader.ReadInt32();
        }

        public void MakeDust()
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

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            _effectTimer++;
            float amount = (float)Math.Cos(_effectTimer * MathHelper.TwoPi / 100f) * 0.5f + 0.5f;
            float bgScale = Projectile.scale * MathHelper.Lerp(0.9f, 1.1f, amount);

            DrawTreasureBagEffect(Main.spriteBatch, tex, ref _effectTimer,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight),
                Color.Purple * 0.8f,
                Projectile.rotation, new Vector2(tex.Width, frameHeight) / 2, bgScale,
                effects);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight),
                lightColor, Projectile.rotation, new Vector2(tex.Width, frameHeight) / 2, Projectile.scale,
                effects, 0f);
            return false;
        }

        public static void DrawTreasureBagEffect(SpriteBatch spriteBatch, Texture2D tex, ref float drawTimer, 
            Vector2 position, Rectangle? rect, Color color, float rot, Vector2 origin, float scale, 
            SpriteEffects effects = SpriteEffects.None)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float timer = drawTimer / 240f + time * 0.04f;
            time %= 4f;
            time /= 2f;
            if (time >= 1f)
            {
                time = 2f - time;
            }
            time = time * 0.5f + 0.5f;
            for (float i = 0f; i < 1f; i += 0.25f)
            {
                float radians = (i + timer) * 6.2831855f;
                spriteBatch.Draw(tex, position + new Vector2(0f, 8f).RotatedBy((double)radians, default(Vector2)) * time, 
                    rect, new Color((int)color.R, (int)color.G, (int)color.B, 50), rot, origin, scale, effects, 0f);
            }
            for (float j = 0f; j < 1f; j += 0.34f)
            {
                float radians2 = (j + timer) * 6.2831855f;
                spriteBatch.Draw(tex, position + new Vector2(0f, 4f).RotatedBy((double)radians2, default(Vector2)) * time, 
                    rect, new Color((int)color.R, (int)color.G, (int)color.B, 77), rot, origin, scale, effects, 0f);
            }
        }
    }
}
