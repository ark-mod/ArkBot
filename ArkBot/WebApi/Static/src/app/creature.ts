export class Creature {
  constructor(
    public Name: string,
    public ClassName: string,
    public Species: string,
    public Aliases: string[],
    public Gender: string,
    public BaseLevel: number,
    public Level: number,
    public Imprint: number,
    public FoodStatus: number,
    public Latitude: number,
    public Longitude: number,
    public NextMating: string,
    public OwnerType: string,
    public TopoMapX: number,
    public TopoMapY: number) {
    }
}