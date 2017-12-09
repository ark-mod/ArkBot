import { Component, OnInit, OnDestroy } from '@angular/core';

import { DataService } from '../data.service';
import { NotificationsService } from 'angular2-notifications';
import { MessageService } from '../message.service';

import * as moment from 'moment'

@Component({
  selector: 'app-server-list',
  templateUrl: './server-list.component.html',
  styleUrls: ['./server-list.component.css']
})
export class ServerListComponent implements OnInit, OnDestroy {
  serverUpdatedSubscription: any;
  serverUpdateInterval: any;
  private serversUpdatedSubscription: any;

  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  public serverCount: number = 0;
  public onlinePlayerCount: number = 0;

  constructor(
    public dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
    }

  ngOnInit() {
    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => this.showServerUpdateNotification(serverKey));
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.menuOption = menuOption);

    this.serversUpdatedSubscription = this.dataService.ServersUpdated$.subscribe(servers => {
      this.updateData(servers);
    });

    this.serverUpdateInterval = window.setInterval(() => {
        this.dataService.updateServer(null);
      }, 60000);

    //init aggregated data
    this.updateData(this.dataService.Servers);
  }

  ngOnDestroy() {
    this.serverUpdatedSubscription.unsubscribe();
    this.menuOptionSubscription.unsubscribe();
    this.serversUpdatedSubscription.unsubscribe();
    window.clearInterval(this.serverUpdateInterval);
  }

  updateData(servers: any): void {
    let serverCount = 0;
    let onlinePlayerCount = 0;

    if (servers && servers.Servers) {
      serverCount = servers.Servers.length;

      for (let server of servers.Servers) {
        if (!server.OnlinePlayers) continue;
        onlinePlayerCount += server.OnlinePlayers.length;
      }
    }

    this.serverCount = serverCount;
    this.onlinePlayerCount = onlinePlayerCount;
  }

  showServerUpdateNotification(serverKey: string): void {
    this.notificationsService.success(
      'Server Update',
      `${serverKey} was updated; Reloading data...`,
      {
          showProgressBar: true,
          pauseOnHover: true,
          clickToClose: true
      }
    );
  }

  isMenuActive(menuOption: string): boolean {
    return this.menuOption == menuOption;
  }
}
