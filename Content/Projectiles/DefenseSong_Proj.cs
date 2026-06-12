using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class DefenseSong_Proj : SkillProjectile
    {
        public static List<int> ActiveDefenseSongs = new List<int>();

        public override int ItemType => ModContent.ItemType<DefenseSong>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = (int)(300 + OutroTime);
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.scale = 1.2f;
        }
        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.scale = Stats.Size;
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
        }
        public int Index { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        public float OrbitTimer { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }
        public int TotalCount => Stats.Count;

        public float Radius => 80 * (1f + (Projectile.scale - 1f) / 2f);

        public static float OutroTime = 30f;

        public override void AI()
        {
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            float orbitSpeed = 0.025f;

            OrbitTimer += orbitSpeed;

            float angle = OrbitTimer + Index * MathHelper.TwoPi / TotalCount;
            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Radius;

            Projectile.Center = owner.MountedCenter + offset;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = 0;

            if (Projectile.timeLeft <= OutroTime)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / OutroTime));
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X > player.MountedCenter.X ? 1 : -1;
        }

        public override void OnKill(int timeLeft)
        {
            ActiveDefenseSongs.Remove(Projectile.whoAmI);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight),
                Color.White * Projectile.Opacity, Projectile.rotation, new Vector2(tex.Width, frameHeight) / 2,
                Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
