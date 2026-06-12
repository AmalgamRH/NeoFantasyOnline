using NeoFantasyOnline.Content.DamageClasses;
using NeoFantasyOnline.Content.Items.Props;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NeoFantasyOnline.Content.Bases
{
    public abstract class SkillItem : ModItem
    {
        /// <summary>
        /// 1级:无限制  2级:击败克苏鲁之眼
        /// 3级:击败邪恶Boss  4级:击败骷髅王
        /// 5级:击败血肉之墙  6级:击败机械Boss其二
        /// 7级:击败世纪之花  8级:击败邪教徒
        /// </summary>
        public int Level { get; set; }
        private bool _needsRefresh = true;

        public const string TextureBasePath = "NeoFantasyOnline/Assets/Items/Weapons/";

        protected virtual string TextureName => GetType().Name;

        public override string Texture => TextureBasePath + TextureName;

        public abstract int ProjectileType { get; }

        public virtual int MaxLevel { get; } = 8;
        public abstract SkillLevelData[] StatsByLevel { get; }

        public SkillLevelData CurrentStats => StatsByLevel[Math.Clamp(Level, 1, MaxLevel) - 1];

        public void RefreshStats()
        {
            _needsRefresh = true;
        }

        public sealed override void SetDefaults()
        {
            _needsRefresh = false;
            Level = Math.Clamp(Level, 1, 8);
            Item.width = Item.height = 20;
            Item.DamageType = ModContent.GetInstance<SkillDamage>();
            Item.shoot = ProjectileType;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true; 
            Item.noMelee = true;
            Item.rare = GetRarity(Level);
            SetSkillDefaults();
        }

        public static int GetRarity(int level)
        {
            int rarity = level switch
            {
                1 => ItemRarityID.Green,
                2 => ItemRarityID.Orange,
                3 => ItemRarityID.LightRed,
                4 => ItemRarityID.Pink,
                5 => ItemRarityID.Lime,
                6 => ItemRarityID.Yellow,
                7 => ItemRarityID.Cyan,
                8 => ItemRarityID.Red,
                _ => ItemRarityID.White 
            };
            return rarity;
        }

        public virtual void SetSkillDefaults() { }

        public override void SaveData(TagCompound tag)
        {
            tag["Level"] = Level;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("Level", out int savedLevel))
            {
                Level = savedLevel;
                _needsRefresh = true;
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(_needsRefresh);
            writer.Write(Level);
        }

        public override void NetReceive(BinaryReader reader)
        {
            _needsRefresh = reader.ReadBoolean();
            Level = reader.ReadInt32();
        }

        /// <summary>升级所需的星石数量</summary>
        public static int[] StarCoinCount =
            [
                250,
                500,
                800,
                1000,
                1500,
                2000,
                2200,
                2500
            ];

        /// <summary>根据世界Boss进度返回当前可解锁的最高等级</summary>
        public static int GetLevelCap()
        {
            if (NPC.downedAncientCultist) return 8;
            if (NPC.downedPlantBoss) return 7;
            int mechCount = (NPC.downedMechBoss1 ? 1 : 0)
                          + (NPC.downedMechBoss2 ? 1 : 0)
                          + (NPC.downedMechBoss3 ? 1 : 0);
            if (mechCount >= 2) return 6;
            if (Main.hardMode) return 5;
            if (NPC.downedBoss3) return 4;
            if (NPC.downedBoss2) return 3;
            if (NPC.downedBoss1) return 2;
            return 1;
        }

        public override void UpdateInventory(Player player)
        {
            int cap = GetLevelCap();
            if (Level < cap)
            {
                Level = cap;
                _needsRefresh = true;
            }
            if (_needsRefresh)
            {
                SetDefaults();
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine nameLine = tooltips.Find(t => t.Name == "ItemName");
            if (nameLine != null)
            {
                nameLine.Text += $" (Level {Level})";
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient<StarCoin>(2500)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
