using NeoFantasyOnline.Content.Items.Props;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace NeoFantasyOnline.Core.GlobalNPCs
{
    public class StarCoinDropGlobalNPC : GlobalNPC
    {
        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            globalLoot.Add(
                ItemDropRule.Common(ModContent.ItemType<StarCoin>(),
                chanceDenominator: 2, 
                minimumDropped: 1, maximumDropped: 9));
        }
    }
}
