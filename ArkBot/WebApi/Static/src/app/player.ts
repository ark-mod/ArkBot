import { PlayerServer } from './playerserver';

export class Player {
  constructor(
    public Servers: Map<string, PlayerServer>,
    public MapNames: Map<string, string>) { }
}