import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/Rx";

import { HttpService } from './http.service';
import { MessageService } from './message.service';

import { Servers } from './servers';

import { environment } from '../environments/environment';

import * as moment from 'moment'

@Injectable()
export class DataService {
  _servers: BehaviorSubject<Servers> = new BehaviorSubject<Servers>(undefined);
  _onlinePlayers: BehaviorSubject<any> = new BehaviorSubject<any>(undefined);
  _playerLocations: BehaviorSubject<any> = new BehaviorSubject<any>(undefined);
  _chatMessages: BehaviorSubject<any[]> = new BehaviorSubject<any[]>(undefined);

  private _chatMessagesArr = [];

  public Servers: Servers;
  public UserSteamId: string;
  public ServersUpdated$: EventEmitter<Servers>;
  private menuOption: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);
  private theme: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);

  constructor(
    private httpService: HttpService,
    private messageService: MessageService) {
      this.ServersUpdated$ = new EventEmitter();
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
      messageService.onlinePlayers$.subscribe(onlinePlayers => this._onlinePlayers.next(onlinePlayers));
      messageService.playerLocations$.subscribe(playerLocations => this._playerLocations.next(playerLocations));
      messageService.chatMessages$.subscribe(chatMessages => {
        chatMessages.forEach(msg => {
          this._chatMessagesArr.push(msg);
        }); 
        this._chatMessages.next(this._chatMessagesArr);
      });
      //this.getServers();
    }

  get OnlinePlayers() : Observable<any> {
    return this._onlinePlayers.asObservable();
  }

  get PlayerLocations() : Observable<any> {
    return this._playerLocations.asObservable();
  }

  get ChatMessages() : Observable<any[]> {
    return this._chatMessages.asObservable();
  }

  get Theme() : Observable<string> {
    return this.theme.asObservable();
  }

  SetTheme(theme: string) {
    this.theme.next(theme);
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

          this._servers.next(servers);
          this.ServersUpdated$.emit(servers);
          return true;
        })
        .catch(error => {
          this.Servers = null;
          this.UserSteamId = undefined;
          this._servers.next(null);
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

  hasFeatureAccessObservable(featureGroup: string, featureName: string, forSteamId?: string): Observable<boolean> {
    return this._servers.asObservable().map(v => {
      let foo = this.hasFeatureAccess(featureGroup, featureName, forSteamId);
      return foo;
    });
  }

  getCurrentDate(): any {
    return !environment.demo ? moment() : moment(new Date(environment.demoDate));
  }

  toDate(datejson: string): string {
    //todo: fix locale
    return new Date(datejson).toLocaleString('sv-SE');
  }

  toRelativeDate(datejson: string): string {
    if(!datejson) return "";

    if (!environment.demo) return moment(new Date(datejson)).fromNow();
    else return moment(new Date(datejson)).from(new Date(environment.demoDate));
  }
}
