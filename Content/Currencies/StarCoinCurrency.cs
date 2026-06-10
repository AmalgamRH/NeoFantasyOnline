using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Content.Currencies
{
    public class StarCoinCurrency : CustomCurrencySingleCoin
    {
        public StarCoinCurrency(int coinItemId, long currencyCap)
            : base(coinItemId, currencyCap)
        {
            CurrencyTextKey = "Mods.NeoFantasyOnline.Currencies.StarCoinCurrency";
            CurrencyTextColor = Color.Cyan;
        }
    }
}
