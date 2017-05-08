import { Creature } from './creature';

export class PlayerServer {
  constructor(
    public SteamId: string,
    public CharacterName: string,
    public Gender: string,
    public TribeName: string,
    public TribeId: number,
    public Level: number,
    public EngramPoints: number,
    public Latitude: number,
    public Longitude: number,
    public SavedAt: string,
    public Creatures: Creature[]) { }
}