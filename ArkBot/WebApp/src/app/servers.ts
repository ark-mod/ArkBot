import { Server } from './server';
import { Cluster } from './cluster';

export class Servers {
  constructor(
    public Servers: Server[],
    public Clusters: Cluster[],
    public User: any,
    public AccessControl: any) { }
}