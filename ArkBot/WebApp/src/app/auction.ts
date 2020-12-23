export class Seller {
  SteamID: string;
  Name: string;
}

export class ItemStats {
  CraftedSkillBonus: number;
  Armor: number;
  MaxDurability: number;
  Damage: number;
}

export class ItemFlags {
  IsBlueprint: boolean;
}

export class Item {
  Stats: ItemStats;
  Flags: ItemFlags;
  Quality: number;
}

export class DinoStats {
  Health: number;
  Stamina: number;
  Oxygen: number;
  Food: number;
  Damage: number;
  Speed: number;
}

export class DinoFlags {
  IsNeutered: boolean;
  IsWaterDino: boolean;
  IsFlyerDino: boolean;
}

export class Dino {
  Stats: DinoStats;
  Flags: DinoFlags;
  Level: number;
}

export class Auction {
  Date: string;
  Seller: Seller;
  Scope: string;
  Type: string;
  Name: string;
  SellingClass: string;
  SellingClientName: string;
  Quantity: number;
  AskingClass: string;
  AskingAmount: number;
  AskingClientName: string;
  Item: Item;
  Dino: Dino;
}