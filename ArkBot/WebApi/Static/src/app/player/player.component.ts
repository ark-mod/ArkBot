import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { Player } from '../player';
import { Creature } from '../creature';
import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-player',
  templateUrl: './player.component.html',
  styleUrls: ['./player.component.css']
})
export class PlayerComponent implements OnInit {
  player: Player;
  filteredCreatures: Creature[];
  creaturesFilter: string;
  filteredClusterCreatures: any[];
  creaturesClusterFilter: string;
  points: any[];
  keysGetter = Object.keys;
  steamId: string;
  loaded: boolean = false;
  showMap: boolean = false;
  serverKey: string;
  clusterKey: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpService: HttpService,
    private dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
      messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
    }

    getPlayer(): void {
      this.httpService
        .getPlayer(this.steamId)
        .then(player => {
          this.serverKey = Object.keys(player.Servers)[0];
          this.clusterKey = Object.keys(player.Clusters)[0];
          this.player = player;

          this.sort();
          this.filter();
          this.sortCluster();
          this.filterCluster();
          this.loaded = true;
        })
        .catch(error => {
          this.player = null;
          this.filteredCreatures = null;
          this.filteredClusterCreatures = null;
          this.loaded = true;
        });
  }
  
  ngOnInit(): void {
    this.steamId = this.route.snapshot.params['id'];

    this.getPlayer();
  }

  haveMatingCooldown(nextMating: string): boolean {
    return new Date(nextMating) > new Date();
  }

  toDate(datejson: string): string {
    //todo: fix locale
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
    let len = Object.keys(this.player.Servers).length;
    return 100.0/len;
  }

  activeCluster(clusterKey: string): boolean {
    return this.clusterKey == clusterKey;
  }

  activateCluster(clusterKey: string): void {
    this.clusterKey = clusterKey;
    this.sortCluster();
    this.filterCluster();
  }

  clusterWidth(): number {
    let len = Object.keys(this.player.Clusters).length;
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
      this.filteredCreatures = this.player.Servers[this.serverKey].Creatures.filter(creature => 
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

  sortCluster(): void {
    this.player.Clusters[this.clusterKey].Creatures.sort((c1, c2) => {
        if(c1.Level > c2.Level) {
          return -1;
        } else if(c1.Level < c2.Level){
            return 1;
        } else {
          return 0; 
        }
    });
  }

  filterCluster(): void {
    if (this.creaturesClusterFilter == null || this.creaturesClusterFilter.length == 0) this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures;
    else {
      let filter = this.creaturesClusterFilter.toLowerCase();
      this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures.filter(creature => 
        creature.Species.toLowerCase().indexOf(filter) >= 0 
        || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter) >= 0));
    }
  }

  run(): void {
    if(this.steamId == null || this.steamId == "") {
      this.player = null;
      this.filteredCreatures = null;
      return;
    }
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
    this.getPlayer();
  }
}
