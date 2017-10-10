import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/Rx";

import { HttpService } from './http.service';
import { MessageService } from './message.service';

import { Servers } from './servers';

@Injectable()
export class DataService {
  public Servers: Servers;
  public UserSteamId: string;
  public ServersUpdated$: EventEmitter<Servers>;
  private menuOption: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);

  constructor(
    private httpService: HttpService,
    private messageService: MessageService) {
      this.ServersUpdated$ = new EventEmitter();
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
      //this.getServers();
    }

  get MenuOption() : Observable<string> {
    return this.menuOption.asObservable();
  }

  SetMenuOption(menuOption: string) {
    this.menuOption.next(menuOption);
  }

  getServers(): Promise<boolean> {
    return this.httpService
        .getServers()
        .then(servers => {
          this.Servers = servers;

          var user = servers ? servers.User : undefined;
          this.UserSteamId = user && user.SteamId ? user.SteamId : undefined;

          this.ServersUpdated$.emit(servers);
          return true;
        })
        .catch(error => {
          this.Servers = null;
          this.UserSteamId = undefined;
          this.ServersUpdated$.emit(null);
          return false;
        });
  }

  updateServer(serverKey: string): void {
    this.getServers();
  }

  hasFeatureAccess(featureGroup: string, featureName: string, forSteamId?: string): boolean {
    var accessControl = this.Servers ? this.Servers.AccessControl : undefined;
    if (!accessControl) return false;
    var fg = accessControl[featureGroup];
    if (!fg) return false;
    var rf = <string[]> fg[featureName];
    if (!rf) return false;

    var user = this.Servers ? this.Servers.User : undefined;
    let userRoles = <string[]> (user && user.Roles ? user.Roles.slice(0) : []);
    if (user && user.SteamId && user.SteamId == forSteamId) userRoles.push("self");

    for (let urole of userRoles) {
      if (rf.find((value) => urole.toLowerCase() === value.toLowerCase())) return true;
    }

    return false;
  }
}
