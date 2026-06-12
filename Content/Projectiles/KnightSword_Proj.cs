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
    public class KnightSword_Proj : SkillProjectile
    {
        public override int ItemType => ModContent.ItemType<KnightSword>();
        private int Index => (int)Projectile.ai[0];
        private int _direction => (int)Projectile.ai[1];
        private int TotalCount => Stats.Count;

        private int _initialTimeLeft;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetSkillDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 125;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.scale = Stats.Size;
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);

            float offset = 20 * (1f + (Projectile.scale - 1f) / 0.5f);
            Projectile.width += (int)offset;

            Projectile.timeLeft = 12;
            _initialTimeLeft = Projectile.timeLeft;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (_direction == 1)
            {
                Projectile.Center = new Vector2(
                    player.position.X + player.width + Projectile.width / 2,
                    player.Center.Y + (Index - (TotalCount - 1) * 0.5f) * 40f);
                Projectile.spriteDirection = 1;
            }
            else
            {
                Projectile.Center = new Vector2(
                    player.position.X - Projectile.width / 2,
                    player.Center.Y + (Index - (TotalCount - 1) * 0.5f) * 40f);
                Projectile.spriteDirection = -1;
            }

            float progress = 1f - (float)Projectile.timeLeft / _initialTimeLeft;
            Projectile.frame = (int)(progress * Main.projFrames[Type]);
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = Main.projFrames[Type] - 1;

            Projectile.rotation = 0f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X > player.MountedCenter.X ? 1 : -1;
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle source = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawPos = Projectile.Center - Main.screenPosition +
                new Vector2(Projectile.spriteDirection * 16f * Projectile.scale, 0);

            Main.spriteBatch.Draw(tex, drawPos,
                source, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            /*Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)(Projectile.position.X - Main.screenPosition.X),
                              (int)(Projectile.position.Y - Main.screenPosition.Y),
                              Projectile.width, Projectile.height),
                Color.Red * 0.3f);*/

            return false;
        }
    }
}
