using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;
using static Terraria.ModLoader.ModContent;

namespace NeoFantasyOnline.Content.Menus
{
	internal class MenuNFO : ModMenu
	{
        public override string DisplayName => "Neo Fantasy Online";
        public override int Music => NeoFantasyOnline.GetMusic("Opera of the wasteland 8bit-02");
        public override Asset<Texture2D> Logo => Request<Texture2D>("NeoFantasyOnline/Assets/Logo_NFO");
        public static ModMenu Instance { get; private set; }
        internal static int GetButtonStartY() => MenuLoader.CurrentMenu is MenuNFO ? _buttonStartY : 220;
        internal static int GetButtonSpacing() => MenuLoader.CurrentMenu is MenuNFO ? _buttonSpacing : 52;

        private Asset<Texture2D> _btnTexture;
        private Asset<Texture2D> _bgMenu;

        private static int _buttonStartY = 270;
        private static int _buttonSpacing = 85;
        private static float[] _buttonScales = [0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f];
        private int bgTimer = 0;
        private float titleTimer = 0;

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Main.time = 30000;
            Main.dayTime = true;

            DrawNFOMenu(spriteBatch);
            DrawMenuButtonBackgrounds(spriteBatch);

            logoRotation = 0f;
            logoDrawCenter += new Vector2(0, 30);
            drawColor = Color.White;

            titleTimer += 0.02f;
            if (titleTimer > MathF.PI * 2) titleTimer -= MathF.PI * 2;
            float t = (MathF.Sin(titleTimer) + 1f) / 2f;
            logoScale = MathHelper.Lerp(0.9f, 1f, t);

            return true;
        }

        private void DrawNFOMenu(SpriteBatch spriteBatch)
        {
            bgTimer++;
            _bgMenu ??= Request<Texture2D>("NeoFantasyOnline/Assets/UI/Menu/BgMenu");
            Texture2D texture = _bgMenu.Value;
            float speed = 0.75f;
            float dir = 1f;
            int offsetX = (int)(bgTimer * speed * dir) % texture.Width;
            int offsetY = (int)(bgTimer * speed * 0.6f * dir) % texture.Height;
            if (bgTimer > 100 && offsetX == 0 && offsetY == 0)
                bgTimer = 0;
            for (int x = -texture.Width - 10 + offsetX; x < Main.screenWidth; x += texture.Width)
                for (int y = -texture.Height - 10 + offsetY; y < Main.screenHeight; y += texture.Height)
                    spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
        }

        private void DrawMenuButtonBackgrounds(SpriteBatch spriteBatch)
        {
            if (Main.menuMode != 0) return;

            Texture2D tex = _btnTexture?.Value;
            if (tex == null) return;

            int centerX = Main.screenWidth / 2;

            string[] labels = new string[] {
                Lang.menu[12].Value,
                Lang.menu[13].Value,
                Lang.menu[131].Value,
                SocialAPI.Workshop != null ? Language.GetText("UI.Workshop").Value : Language.GetText("UI.ResourcePacks").Value,
                Lang.menu[14].Value,
                Language.GetText("UI.Credits").Value,
                Lang.menu[15].Value,
            };

            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null) continue;

                Vector2 textSize = FontAssets.DeathText.Value.MeasureString(labels[i]);
                float halfTextHeight = textSize.Y * 0.5f;

                int y = _buttonStartY + _buttonSpacing * i + (int)halfTextHeight - (int)textSize.Y / 4 - 4;

                Vector2 bgScale = new Vector2(0.9f, 0.8f);

                spriteBatch.Draw(tex, new Vector2(centerX, y), null, Color.White, 0f,
                    new Vector2(tex.Width / 2, tex.Height / 2), bgScale, SpriteEffects.None, 0f);
            }
        }

        public override void Update(bool isOnTitleScreen)
        {
            base.Update(isOnTitleScreen);
        }

        #region Hook：修改原版主界面按钮布局
        public override void Load()
        {
            Instance = this;
            _btnTexture = Request<Texture2D>("NeoFantasyOnline/Assets/UI/Menu/ButtonMenu");
            IL_Main.DrawMenu += ModifyButtonLayout;
        }
        public override void Unload()
        {
            IL_Main.DrawMenu -= ModifyButtonLayout;
            Instance = null;
            _btnTexture = null;
        }
        private static void ModifyButtonLayout(ILContext il)
        {
            var c = new ILCursor(il);

            // 匹配 menuMode == 0 独有的按钮初始化序列:
            // num2 = 220; num5 = 7; num4 = 52;
            // IL: ldc.i4 220; stloc *; ldc.i4 7; stloc *; ldc.i4 52; stloc *
            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdcI4(220),
                i => i.MatchStloc(out _),
                i => i.MatchLdcI4(7),
                i => i.MatchStloc(out _),
                i => i.MatchLdcI4(52),
                i => i.MatchStloc(out _)))
                return;

            var getY = typeof(MenuNFO).GetMethod("GetButtonStartY", BindingFlags.Static | BindingFlags.NonPublic)!;
            var getSpacing = typeof(MenuNFO).GetMethod("GetButtonSpacing", BindingFlags.Static | BindingFlags.NonPublic)!;
            var applyScales = typeof(MenuNFO).GetMethod("ApplyCustomButtonScales", BindingFlags.Static | BindingFlags.NonPublic)!;

            // 替换 ldc.i4 220 → call GetButtonStartY
            c.Remove();
            c.Emit(OpCodes.Call, getY);

            // 跳过 stloc(num2), ldc.i4 7, stloc(num5)
            c.Index += 3;

            // 替换 ldc.i4 52 → call GetButtonSpacing
            c.Remove();
            c.Emit(OpCodes.Call, getSpacing);

            // 跳过 stloc(num4)
            c.Index += 1;
            // 现在指向按钮标签枚举开始处 (int num12 = 0;)

            // 查找 array7 (float[]) 和 array6 (byte[]) 局部变量
            VariableDefinition array7Var = null;
            foreach (var v in il.Body.Variables)
            {
                if (!v.VariableType.IsArray) continue;
                var elem = v.VariableType.GetElementType();
                if (elem.FullName == "System.Single") array7Var = v;
            }

            if (array7Var == null) return;

            // 注入: ApplyCustomButtonScales(array7)
            c.Emit(OpCodes.Ldloc, array7Var);
            c.Emit(OpCodes.Call, applyScales);
        }

        internal static void ApplyCustomButtonScales(float[] array7)
        {
            if (!(MenuLoader.CurrentMenu is MenuNFO)) return;
            for (int i = 0; i < _buttonScales.Length; i++)
                array7[i] = _buttonScales[i];
        }
        #endregion
    }
}
