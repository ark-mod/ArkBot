import { Component, OnInit } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { Servers } from './servers';
import { Player } from './player';
import { Creature } from './creature';
import { HttpService } from './http.service';
import { MessageService } from './message.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  servers: Servers;
  player: Player;
  filteredCreatures: Creature[];
  creaturesFilter: string;
  points: any[];
  keysGetter = Object.keys;
  steamId: string;
  loaded: boolean = false;
  showMap: boolean = false;
  serverKey: string;

  public notificationOptions = {
      position: ["top", "right"],
      timeOut: 1000,
      lastOnBottom: false,
  };

  constructor(
    private httpService: HttpService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
    }
  
  getServers(): void {
    this.httpService
        .getServers()
        .then(servers => this.servers = servers)
        .catch(error => {
          this.servers = null;
        });
  }

  getPlayer(): void {
    this.httpService
        .getPlayer(this.steamId)
        .then(player => {
          this.serverKey = Object.keys(player.Servers)[0];
          this.player = player;

          this.sort();
          this.filter();
          this.loaded = true;
        })
        .catch(error => {
          this.player = null;
          this.filteredCreatures = null;
          this.loaded = true;
        });
  }
  
  ngOnInit(): void {
    //set steamId to querystring param if there is one
    var urlParams = new URLSearchParams(window.location.search)
    if(urlParams.has('steamId')) this.steamId = urlParams.get('steamId');

    this.getServers();
    this.getPlayer();
  }

  haveMatingCooldown(nextMating: string): boolean {
    return new Date(nextMating) > new Date();
  }

  toDate(datejson: string): string {
    return new Date(datejson).toLocaleString('sv-SE');
  }

  toRelativeDate(datejson: string): string {
    return moment(new Date(datejson)).fromNow();
  }

  active(serverKey: string): boolean {
    return this.serverKey == serverKey;
  }

  activate(serverKey: string): void {
    this.serverKey = serverKey;
    this.sort();
    this.filter();
  }

  serverWidth(): number {
    //if (this.player == null || this.player.Servers == null) return 0;
    let len = Object.keys(this.player.Servers).length;
    return 100.0/len;
  }

  sort(): void {
    this.player.Servers[this.serverKey].Creatures.sort((c1, c2) => {
        if(c1.FoodStatus < c2.FoodStatus) {
          return -1;
        } else if(c1.FoodStatus > c2.FoodStatus){
            return 1;
        } else {
          return 0; 
        }
    });
  }

  filter(): void {
    if (this.creaturesFilter == null || this.creaturesFilter.length == 0) this.filteredCreatures = this.player.Servers[this.serverKey].Creatures;
    else {
      let filter = this.creaturesFilter.toLowerCase();
      this.filteredCreatures = this.player.Servers.get(this.serverKey).Creatures.filter(creature => 
        creature.Species.toLowerCase().indexOf(filter) >= 0 
        || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter) >= 0));
    }

    let points = [];
    for(let creature of this.filteredCreatures) {
      let point = {} as any;
      point.x = creature.TopoMapX;
      point.y = creature.TopoMapY;
      points.push(point);
    }
    this.points = points;
  }

  run(): void {
    if(this.steamId == null || this.steamId == "") {
      this.player = null;
      this.filteredCreatures = null;
      return;
    }
    this.getServers();
    this.getPlayer();
  }

  openMap(event: any): void {
    this.showMap = true;
    event.stopPropagation();
  }

  closeMap(event: any): void {
    this.showMap = false;
  }

  updateServer(serverKey: string): void {
    this.showServerUpdateNotification(serverKey);
    this.getServers();
    this.getPlayer();
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
}