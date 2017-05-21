import { PlayerServer } from './playerserver';

export class Player {
  constructor(
    public Servers: Map<string, PlayerServer>,
    public Clusters: Map<string, any>,
    public MapNames: Map<string, string>) { }
}