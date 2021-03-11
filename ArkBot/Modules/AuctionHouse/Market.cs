using System;
using System.Collections.Generic;
using System.Text;

namespace ArkBot.Modules.AuctionHouse
{
    public class Market
    {
        public class Auction
        {
            public string Date { get; set; }
            public string Scope { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }

            public string SellingClass { get; set; }
            public string SellingClientName { get; set; }

            public int Quantity { get; set; }
            public string AskingClass { get; set; }
            public string AskingClientName { get; set; }
            public int AskingAmount { get; set; }
            public Seller Seller { get; set; }
            public Dino Dino { get; set; }
            public Item Item { get; set; }
        }

        public class Seller
        {
            public string SteamID { get; set; }
            public string Name { get; set; }
            public float ServerMultiplier { get; set; }
        }

        public class Dino
        {

            public class DinoStats
            {
                public float Health { get; set; }
                public float Stamina { get; set; }
                public float Torpidity { get; set; }
                public float Oxygen { get; set; }
                public float Food { get; set; }
                public int Water { get; set; }
                public float Weight { get; set; }
                public float Damage { get; set; }
                public float Speed { get; set; }
            }

            public class DinoFlags
            {
                public bool IsNeutered { get; set; }
                public bool IsWaterDino { get; set; }
                public bool IsFlyerDino { get; set; }
            }

            public string TamedName { get; set; }
            public int Level { get; set; }
            public int BaseLevel { get; set; }
            public int ExtraLevel { get; set; }
            public string Gender { get; set; }
            public int Experience { get; set; }
            public DinoStats Stats { get; set; }
            public DinoFlags Flags { get; set; }
        }

        public class Item
        {
            public class ItemFlags
            {
                public bool IsBlueprint { get; set; }
            }

            public class ItemStats
            {
                public float CraftedSkillBonus { get; set; }
                public float Armor { get; set; }
                public float MaxDurability { get; set; }
                public float Damage { get; set; }
                public float HypothermalInsulation { get; set; }
                public float HyperthermalInsulation { get; set; }
            }

            public ItemFlags Flags { get; set; }
            public int Quality { get; set; }
            public float Rating { get; set; }
            public ItemStats Stats { get; set; }
        }

        public string Name { get; set; }

        public string IP { get; set; }

        public bool Cached { get; set; }

        public Auction[] Auctions { get; set; }
    }
}
