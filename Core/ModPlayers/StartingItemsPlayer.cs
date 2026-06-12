using NeoFantasyOnline.Content.Items.Props;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Core.ModPlayers
{
    public class StartingItemsPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            return [
                new Item(ModContent.ItemType<StarCoin>(), 2000)
                ];
        }
    }
}