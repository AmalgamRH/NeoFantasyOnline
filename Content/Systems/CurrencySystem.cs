using NeoFantasyOnline.Content.Currencies;
using NeoFantasyOnline.Content.Items.Props;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Systems
{
    public sealed class CurrencySystem : ModSystem
    {
        public static int StarCoinCurrencyId { get; private set; }

        public override void PostSetupContent()
        {
            StarCoinCurrencyId = CustomCurrencyManager.RegisterCurrency(new StarCoinCurrency(
                coinItemId: ModContent.ItemType<StarCoin>(),
                currencyCap: 99999
            ));
        }
    }
}
