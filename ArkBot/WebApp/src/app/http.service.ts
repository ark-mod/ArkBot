import { Injectable }    from '@angular/core';
import { Headers, Http, RequestOptions  } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { environment } from '../environments/environment';
import { Servers } from './servers';
import { Player } from './player';

declare var config: any;

@Injectable()
export class HttpService {

  private headers = new Headers({'Content-Type': 'application/json'});
  private serversUrl = '/servers';
  private serverUrl = '/server';
  private wildCreaturesUrl = '/wildcreatures';
  private structuresUrl = '/structures';
  private adminServerUrl = '/adminserver';
  private administerUrl = '/administer';
  private playerUrl = '/player';

  constructor(protected http: Http) { }

  private getOptions(): RequestOptions {
    let demoMode = localStorage.getItem('demoMode') == 'true';
    let options = new RequestOptions({ withCredentials: true });
    if (demoMode)
    {
      if (!options.headers) options.headers = new Headers();
      options.headers.append("demoMode", "true");
    }

    return options;
  }

  getServers(): Promise<Servers> {
    return this.http.get(`${this.getApiBaseUrl()}${this.serversUrl}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as Servers)
               .catch(this.handleError);
  }

  getServer(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.serverUrl}/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  getWildCreatures(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.wildCreaturesUrl}/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  getStructures(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.structuresUrl}/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  getPlayer(steamId: string): Promise<Player> {
    return this.http.get(`${this.getApiBaseUrl()}${this.playerUrl}/${steamId}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as Player)
               .catch(this.handleError);
  }

  getAdminServer(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.adminServerUrl}/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminDestroyAllStructuresForTeamId(serverKey: string, teamId: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DestroyAllStructuresForTeamId/${serverKey}?teamId=${teamId}&t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminDestroyStructuresForTeamIdAtPosition(serverKey: string, teamId: string, x: number, y: number, radius: number, rafts: number): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DestroyStructuresForTeamIdAtPosition/${serverKey}?teamId=${teamId}&x=${x}&y=${y}&radius=${radius}&rafts=${rafts}&t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminDestroyDinosForTeamId(serverKey: string, teamId: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DestroyDinosForTeamId/${serverKey}?teamId=${teamId}&t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminSaveWorld(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/SaveWorld/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminListFertilizedEggs(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DroppedEggsList/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminDestroyAllEggs(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DestroyAllEggs/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  adminDestroySpoiledEggs(serverKey: string): Promise<any> {
    return this.http.get(`${this.getApiBaseUrl()}${this.administerUrl}/DestroySpoiledEggs/${serverKey}?t=${+new Date()}`, this.getOptions())
               .toPromise()
               .then(response => response.json() as any)
               .catch(this.handleError);
  }

  getApiBaseUrl(): string {
    return environment.apiBaseUrl
      .replace(/\<protocol\>/gi, window.location.protocol)
      .replace(/\<hostname\>/gi, window.location.hostname)
      .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
  }

  protected handleError(error: any): Promise<any> {
    return Promise.reject(error.message || error);
  }
}