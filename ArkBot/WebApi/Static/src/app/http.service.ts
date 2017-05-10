import { Injectable }    from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { environment } from '../environments/environment';
import { Servers } from './servers';
import { Player } from './player';

@Injectable()
export class HttpService {

  private headers = new Headers({'Content-Type': 'application/json'});
  private serversUrl = '/servers';
  private playerUrl = '/player';

  constructor(private http: Http) { }

  getServers(): Promise<Servers> {
    return this.http.get(`${environment.apiBaseUrl}${this.serversUrl}`)
               .toPromise()
               .then(response => response.json() as Servers)
               .catch(this.handleError);
  }

  getPlayer(steamId: string): Promise<Player> {
    return this.http.get(`${environment.apiBaseUrl}${this.playerUrl}/${steamId}`)
               .toPromise()
               .then(response => response.json() as Player)
               .catch(this.handleError);
  }

  private handleError(error: any): Promise<any> {
    return Promise.reject(error.message || error);
  }
}