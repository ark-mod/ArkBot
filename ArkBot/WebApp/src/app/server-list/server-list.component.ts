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

  private onlinePlayers: any = undefined; 
  private onlinePlayersSubscription: any;

  private playerLocations: any = undefined; 
  private playerLocationsSubscription: any;

  private chatMessages: any[] = []; 
  private chatMessagesSubscription: any;

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
    this.onlinePlayersSubscription = this.dataService.OnlinePlayers.subscribe(onlinePlayers => this.updateOnlinePlayers(onlinePlayers));
    this.playerLocationsSubscription = this.dataService.PlayerLocations.subscribe(playerLocations => this.updatePlayerLocations(playerLocations));
    this.chatMessagesSubscription = this.dataService.ChatMessages.subscribe(chatMessages => this.updateChatMessages(chatMessages));

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
    this.onlinePlayersSubscription.unsubscribe();
    this.playerLocationsSubscription.unsubscribe();
    this.onlinePlayersSubscription.unsubscribe();
    this.chatMessagesSubscription.unsubscribe();
    window.clearInterval(this.serverUpdateInterval);
  }

  updateData(servers: any): void {
    let serverCount = 0;
    

    if (servers && servers.Servers) {
      serverCount = servers.Servers.length;
    }

    this.serverCount = serverCount;
    
  }

  updateOnlinePlayers(onlinePlayers: any): void {
    let onlinePlayerCount = 0;

    if (onlinePlayers) onlinePlayerCount = (<any[]> Object.values(onlinePlayers)).reduce((n, p) => n + p.length, 0)
    
    this.onlinePlayerCount = onlinePlayerCount;
    this.onlinePlayers = onlinePlayers;
  }

  getOnlinePlayerName(serverKey: string, steamId: number): string {
    return (<any[]> this.onlinePlayers[serverKey]).find(x => x.steamId == steamId)?.steamName || steamId;
  }

  updatePlayerLocations(playerLocations: any): void {
    if (!playerLocations) return;

    let result = (<any[]> Object.entries(playerLocations)).reduce((map, kv) => { map[kv[0]] = kv[1].map((pl) => ({ x: pl.topoMapX, y: pl.topoMapY, label: this.getOnlinePlayerName(kv[0], pl.steamId) })); return map; }, {});
    this.playerLocations = result;
  }

  updateChatMessages(chatMessages: any[]): void {
    this.chatMessages.splice(0, this.chatMessages.length);
    this.chatMessages.push(...chatMessages);
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
