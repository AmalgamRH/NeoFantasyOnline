using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeoFantasyOnline.Content.Bases;
using NeoFantasyOnline.Content.Items.Weapons;
using NeoFantasyOnline.Core.GlobalNPCs;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NeoFantasyOnline.Content.Projectiles
{
    public class Blizzard_Proj : SkillProjectile
    {
        public static List<int> ActiveBlizzards = new List<int>();
        public override int ItemType => ModContent.ItemType<Blizzard>();

        public static float OutroTime = 30f;
        public override void SetSkillDefaults()
        {
            Projectile.width = 330;
            Projectile.height = 330;
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
            Projectile.rotation += 0.05f;
            Projectile.velocity = Vector2.Zero;

            if (Projectile.timeLeft <= OutroTime)
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / OutroTime));
        }

        public override void OnKill(int timeLeft)
        {
            ActiveBlizzards.Remove(Projectile.whoAmI);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.Center.X > player.MountedCenter.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Stats.HitTimes > 0)
                Core.Networking.NfoNetHelper.SendNpcSlowed(target.whoAmI, Stats.HitTimes);
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D tex2 = ModContent.Request<Texture2D>(Texture + "2").Value;

            Main.spriteBatch.Draw(tex, 
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * Projectile.Opacity, Projectile.rotation, 
                new Vector2(tex.Width, tex.Height) / 2, Projectile.scale,
                SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(tex2,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * Projectile.Opacity, Projectile.rotation, 
                new Vector2(tex2.Width, tex2.Height) / 2, Projectile.scale,
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
