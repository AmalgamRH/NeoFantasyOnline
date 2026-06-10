using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Items.Props
{
    public class StarCoin : ModItem
    {
        public const string TextureBasePath = "NeoFantasyOnline/Assets/Items/Props/";
        public override string Texture => TextureBasePath + GetType().Name;
        public override void SetDefaults()
        {
            Item.width = Item.height = 16;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 1);
        }
    }
}
