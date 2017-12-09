import { Injectable }    from '@angular/core';
import { Headers, Http, RequestOptions  } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { environment } from '../environments/environment';
import { Servers } from './servers';
import { Player } from './player';

import { HttpService } from './http.service';

@Injectable()
export class DemoHttpService extends HttpService {

  constructor(http: Http) { super(http); }

  getServers(): Promise<Servers> {
    return this.http.get('assets/demo/servers.json')
    .toPromise()
    .then(response => response.json() as Servers)
    .catch(this.handleError);
  }

  getServer(serverKey: string): Promise<any> {
    return this.http.get('assets/demo/server.json')
    .toPromise()
    .then(response => response.json() as any)
    .catch(this.handleError);
  }

  getWildCreatures(serverKey: string): Promise<any> {
    return this.http.get('assets/demo/wildcreatures.json')
    .toPromise()
    .then(response => response.json() as any)
    .catch(this.handleError);
  }

  getStructures(serverKey: string): Promise<any> {
    return this.http.get('assets/demo/structures.json')
    .toPromise()
    .then(response => response.json() as any)
    .catch(this.handleError);
  }

  getPlayer(steamId: string): Promise<Player> {
    return this.http.get('assets/demo/player.json')
    .toPromise()
    .then(response => response.json() as Player)
    .catch(this.handleError);
  }

  getAdminServer(serverKey: string): Promise<any> {
    return this.http.get('assets/demo/adminserver.json')
    .toPromise()
    .then(response => response.json() as any)
    .catch(this.handleError);
  }

  adminDestroyAllStructuresForTeamId(serverKey: string, teamId: string): Promise<any> {
    return Promise.resolve(null);
  }

  adminDestroyStructuresForTeamIdAtPosition(serverKey: string, teamId: string, x: number, y: number, radius: number, rafts: number): Promise<any> {
    return Promise.resolve(null);
  }

  adminDestroyDinosForTeamId(serverKey: string, teamId: string): Promise<any> {
    return Promise.resolve(null);
  }

  adminSaveWorld(serverKey: string): Promise<any> {
    return Promise.resolve(null);
  }
}