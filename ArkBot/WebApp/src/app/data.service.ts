import { Injectable } from '@angular/core';

import { HttpService } from './http.service';
import { MessageService } from './message.service';

import { Servers } from './servers';

@Injectable()
export class DataService {
  public Servers: Servers;

  constructor(
    private httpService: HttpService,
    private messageService: MessageService) {
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
      this.getServers();
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
