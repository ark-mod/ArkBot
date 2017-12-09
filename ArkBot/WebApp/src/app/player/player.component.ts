import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { Player } from '../player';
import { Creature } from '../creature';
import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';

import { floatCompare, intCompare, stringLocaleCompare, nullCompare } from '../utils'

@Component({
  selector: 'app-player',
  templateUrl: './player.component.html',
  styleUrls: ['./player.component.css']
})
export class PlayerComponent implements OnInit, OnDestroy {
  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  serverUpdatedSubscription: any;
  player: Player;
  filteredCreatures: Creature[];
  imprintCreatures: Creature[];
  imprintNotifications: boolean = false;
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
  creaturesMode: string = "status";
  creatureStates: any = {};

  creaturesSortField: string = "food";
  creaturesAltSortFields: string = "name";
  creaturesSortFunctions: any = {
    "food": (o1, o2, asc) => floatCompare(o1.FoodStatus, o2.FoodStatus, asc, 2),
    "name": (o1, o2, asc) => stringLocaleCompare(o1.Name, o2.Name, asc),
    "species": (o1, o2, asc) => stringLocaleCompare(o1.Species, o2.Species, asc),
    "gender": (o1, o2, asc) => stringLocaleCompare(o1.Gender, o2.Gender, asc),
    "base_level": (o1, o2, asc) => intCompare(o1.BaseLevel, o2.BaseLevel, !asc),
    "level": (o1, o2, asc) => intCompare(o1.Level == o1.BaseLevel ? null : o1.Level, o2.Level == o2.BaseLevel ? null : o2.Level, !asc),
    "imprint": (o1, o2, asc) => floatCompare(o1.Imprint, o2.Imprint, !asc, 2),
    "latitude": (o1, o2, asc) => floatCompare(o1.Latitude, o2.Latitude, asc, 1),
    "longitude": (o1, o2, asc) => floatCompare(o1.Longitude, o2.Longitude, asc, 1),
    "owner": (o1, o2, asc) => stringLocaleCompare(o1.OwnerType, o2.OwnerType, asc),
    "stat_health": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Health : null, o2.BaseStats != undefined ? o2.BaseStats.Health : null, !asc),
    "stat_stamina": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Stamina : null, o2.BaseStats != undefined ? o2.BaseStats.Stamina : null, !asc),
    "stat_oxygen": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Oxygen : null, o2.BaseStats != undefined ? o2.BaseStats.Oxygen : null, !asc),
    "stat_food": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Food : null, o2.BaseStats != undefined ? o2.BaseStats.Food : null, !asc),
    "stat_weight": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Weight : null, o2.BaseStats != undefined ? o2.BaseStats.Weight : null, !asc),
    "stat_melee": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.Melee : null, o2.BaseStats != undefined ? o2.BaseStats.Melee : null, !asc),
    "stat_speed": (o1, o2, asc) => intCompare(o1.BaseStats != undefined ? o1.BaseStats.MovementSpeed : null, o2.BaseStats != undefined ? o2.BaseStats.MovementSpeed : null, !asc),
    "id1": (o1, o2, asc) => intCompare(o1.Id1, o2.Id1, asc),
    "id2": (o1, o2, asc) => intCompare(o1.Id1, o2.Id1, asc)
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpService: HttpService,
    public dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService,
    private ref: ChangeDetectorRef) {
    }

    getPlayer(): void {
      this.httpService
        .getPlayer(this.steamId)
        .then(player => {
          var serverKeys = Object.keys(player.Servers);
          if (!this.serverKey || serverKeys.find(k => k == this.serverKey) == undefined) this.serverKey = serverKeys.length > 0 ? serverKeys[0]: null;

          var clusterKeys = Object.keys(player.Clusters);
          if (!this.clusterKey || clusterKeys.find(k => k == this.clusterKey) == undefined) this.clusterKey = clusterKeys.length > 0 ? clusterKeys[0] : null;
          this.player = player;

          this.filterAndSort();

          this.sortCluster();
          this.filterCluster();
          this.loaded = true;

          this.ref.detectChanges(); //todo: evaluate
        })
        .catch(error => {
          this.player = null;
          this.filteredCreatures = null;
          this.imprintCreatures = null;
          this.filteredClusterCreatures = null;
          this.loaded = true;
        });
  }
  
  ngOnInit(): void {
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.menuOption = menuOption);
    this.steamId = this.route.snapshot.params['playerid'];

    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));

    this.getPlayer();
  }

  ngOnDestroy() {
    this.menuOptionSubscription.unsubscribe();
    this.serverUpdatedSubscription.unsubscribe();
  }

  haveMatingCooldown(creature: any): boolean {
    return creature.NextMating != null ? new Date(creature.NextMating) > new Date() : false;
  }

  active(serverKey: string): boolean {
    return this.serverKey == serverKey;
  }

  activate(serverKey: string): void {
    this.serverKey = serverKey;
    this.filterAndSort();
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

  sort() {
    let asc = this.creaturesSortField[0] != '-';
    let sortFunc = this.creaturesSortFunctions[this.creaturesSortField.replace(/^\-/, "")];
    let alts = this.creaturesAltSortFields.split(',').map(k => {
      let a = <any> {};
      a.asc = k[0] != '-';
      a.sortFunc = this.creaturesSortFunctions[k.replace(/^\-/, "")];
      return a;
    });

    this.filteredCreatures.sort((o1, o2) => {
      let r = sortFunc(o1, o2, asc);
      if(r == 0) {
        for (let alt of alts) {
          r = alt.sortFunc(o1, o2, alt.asc);

          if (r != 0) break;
        }
      }

      return r;
    });
  }

  filter(): void {
    if (this.creaturesFilter == null || this.creaturesFilter.length == 0) this.filteredCreatures = this.player.Servers[this.serverKey].Creatures;
    else {
      let filter = this.creaturesFilter.toLowerCase();
      this.filteredCreatures = this.player.Servers[this.serverKey].Creatures.filter(creature => 
        (creature.Species != null && creature.Species.toLowerCase().indexOf(filter) >= 0) 
        || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter) >= 0));
    }

    let imprintCreatures = this.player.Servers[this.serverKey].Creatures.filter(creature => creature.BabyAge != null);
    imprintCreatures.sort((c1, c2) => {
        if(new Date(c1.BabyNextCuddle) < new Date(c2.BabyNextCuddle)) {
          return -1;
        } else if(new Date(c1.BabyNextCuddle) > new Date(c2.BabyNextCuddle)) {
            return 1;
        } else {
          return 0; 
        }
    });
    this.imprintCreatures = imprintCreatures;

    let points = [];
    for(let creature of this.filteredCreatures) {
      let point = {} as any;
      point.x = creature.TopoMapX;
      point.y = creature.TopoMapY;
      points.push(point);
    }
    this.points = points;
  }

  filterAndSort(): void {
    this.filter();
    this.sort();
  }

  sortCluster(): void {
    if(this.clusterKey == null) return;

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
    if(this.clusterKey == null) {
      this.filteredClusterCreatures = null;
      return;
    }

    if (this.creaturesClusterFilter == null || this.creaturesClusterFilter.length == 0) this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures;
    else {
      let filter = this.creaturesClusterFilter.toLowerCase();
      this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures.filter(creature => 
        (creature.Species != null && creature.Species.toLowerCase().indexOf(filter) >= 0) 
        || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter) >= 0));
    }
  }

  run(): void {
    if(this.steamId == null || this.steamId == "") {
      this.player = null;
      this.filteredCreatures = null;
      this.imprintCreatures = null;
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
    this.showServerUpdateNotification(serverKey);
  }

  haveCluster(): boolean {
    return this.player != null && Object.keys(this.player.Clusters).length > 0;
  }

  sumKibbleAndEggs(): number {
    return this.player.Servers[this.serverKey].KibblesAndEggs != undefined ? this.player.Servers[this.serverKey].KibblesAndEggs.reduce((a, b) => a + b.KibbleCount + b.EggCount, 0) : 0;
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

  getStateForCreature(creature: any): any {
    if (!creature) return undefined;
    let s = this.creatureStates[creature.Id1 + "_" + creature.Id2];
    if (!s) {
      s = { imprintNotifications: true };
      this.creatureStates[creature.Id1 + "_" + creature.Id2] = s;
    }
    return s;
  }

  toggleImprintNotificationForCreature(creature: any): void {
    let s = this.getStateForCreature(creature);

    s.imprintNotifications = !s.imprintNotifications;
  }

  activeCreaturesMode(mode: string): boolean {
    return mode == this.creaturesMode;
  }

  activateCreaturesMode(mode: string): void {
    this.creaturesMode = mode;
  }

  setCreaturesSort(field: string) : void {
    let reverse = this.creaturesSortField == field;
    if (reverse) this.creaturesSortField = "-" + field;
    else this.creaturesSortField = field;

    if (field == "latitude") this.creaturesAltSortFields = !reverse ? "longitude,name" : "-longitude,name";
    else if (field == "longitude") this.creaturesAltSortFields = !reverse ? "latitude,name" : "-latitude,name";
    else this.creaturesAltSortFields = "name";

    this.sort();
  }

  copyCreature(creature: any): void {

  }

  getCurrentServer() {
    if (!(this.dataService && this.dataService.Servers && this.dataService.Servers.Servers)) return undefined;
    let server =  this.dataService.Servers.Servers.find(s => s.Key == this.serverKey);
    return server;
  }

  numCreatureTabs() : number {
    let num = 1;
    if (this.dataService.hasFeatureAccess('player', 'creatures-basestats', this.steamId)) num += 1;
    if (this.dataService.hasFeatureAccess('player', 'creatures-ids', this.steamId)) num += 1;

    return num;
  }
}
