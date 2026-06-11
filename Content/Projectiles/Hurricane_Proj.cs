using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class Hurricane_Proj : SkillProjectile
    {
        public static List<int> ActiveHurricanes = new List<int>();
        public override int ItemType => ModContent.ItemType<Hurricane>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public static float OutroTime = 30f;
        public override void SetSkillDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = (int)(300 + OutroTime);
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void OnSkillProjSpawn(IEntitySource source)
        {
            Projectile.scale = Stats.Size;
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
        }
        public override bool? CanDamage()
        {
            return Projectile.timeLeft > OutroTime;
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Projectile.velocity = Vector2.Zero;

            if (Projectile.timeLeft <= OutroTime)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / OutroTime));
        }

        public override void OnKill(int timeLeft)
        {
            ActiveHurricanes.Remove(Projectile.whoAmI);
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

            Main.spriteBatch.Draw(tex, 
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight),
                lightColor * Projectile.Opacity, 0f, new Vector2(tex.Width, frameHeight) / 2, Projectile.scale,
                SpriteEffects.None, 0f);

            /*Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(
                    (int)(Projectile.getRect().X - Main.screenPosition.X),
                    (int)(Projectile.getRect().Y - Main.screenPosition.Y),
                    Projectile.getRect().Width, Projectile.getRect().Height),
                Color.Red * 0.5f);*/
            return false;
        }
    }
}
