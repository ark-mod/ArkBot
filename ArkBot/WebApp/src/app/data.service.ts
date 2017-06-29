import { Injectable } from '@angular/core';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/Rx";

import { HttpService } from './http.service';
import { MessageService } from './message.service';

import { Servers } from './servers';

@Injectable()
export class DataService {
  public Servers: Servers;
  private menuOption: BehaviorSubject<string> = new BehaviorSubject<string>(undefined);

  constructor(
    private httpService: HttpService,
    private messageService: MessageService) {
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
        .then(servers => this.Servers = servers)
        .catch(error => {
          this.Servers = null;
        });
  }

  updateServer(serverKey: string): void {
    this.getServers();
  }
}
