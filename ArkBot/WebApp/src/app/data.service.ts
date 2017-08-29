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
  public UserIsAdmin: boolean = false;
  public ServersUpdated$: EventEmitter<Servers>;
  private menuOption: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);

  constructor(
    private httpService: HttpService,
    private messageService: MessageService) {
      this.ServersUpdated$ = new EventEmitter();
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
      this.getServers();
    }

  get MenuOption() : Observable<string> {
    return this.menuOption.asObservable();
  }

  SetMenuOption(menuOption: string) {
    this.menuOption.next(menuOption);
  }

  getServers(): void {
    this.httpService
        .getServers()
        .then(servers => {
          this.Servers = servers;

          var user = servers ? servers.User : undefined;
          this.UserSteamId = user && user.SteamId ? user.SteamId : undefined;
          this.UserIsAdmin = user && user.IsAdmin == true;

          this.ServersUpdated$.emit(servers);
        })
        .catch(error => {
          this.Servers = null;
          this.UserSteamId = undefined;
          this.UserIsAdmin = false;
          this.ServersUpdated$.emit(null);
        });
  }

  updateServer(serverKey: string): void {
    this.getServers();
  }
}
