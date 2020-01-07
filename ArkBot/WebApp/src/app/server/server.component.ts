import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'
import { Observable } from "rxjs/Rx";

import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';

import { floatCompare, intCompare, stringLocaleCompare, nullCompare } from '../utils'

@Component({
  selector: 'app-server',
  templateUrl: './server.component.html',
  styleUrls: ['./server.component.css']
})
export class ServerComponent implements OnInit, OnDestroy {
  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  server: any;
  wild: any;
  filteredPlayers: any[];
  filteredTribes: any[];
  loaded: boolean = false;
  serverKey: string;
  serverUpdatedSubscription: any;

  filteredCreatures: any[];
  creaturesLoaded: boolean = false;
  creaturesFilter: string;
  selectedSpecies: any;
  species: string[];
  points: any[];
  keysGetter = Object.keys;
  showMap: boolean = false;
  creaturesMode: string = "status";

  creaturesSortField: string = "base_level";
  creaturesAltSortFields: string = "base_level,gender";
  creaturesSortFunctions: any = {
    "gender": (o1, o2, asc) => stringLocaleCompare(o1.Gender, o2.Gender, asc),
    "base_level": (o1, o2, asc) => intCompare(o1.BaseLevel, o2.BaseLevel, !asc),
    "tameable": (o1, o2, asc) => intCompare(o1.IsTameable, o2.IsTameable, !asc),
    "latitude": (o1, o2, asc) => floatCompare(o1.Latitude, o2.Latitude, asc, 1),
    "longitude": (o1, o2, asc) => floatCompare(o1.Longitude, o2.Longitude, asc, 1),
    "x": (o1, o2, asc) => floatCompare(o1.X, o2.X, asc, 0),
    "y": (o1, o2, asc) => floatCompare(o1.Y, o2.Y, asc, 0),
    "z": (o1, o2, asc) => floatCompare(o1.Z, o2.Z, asc, 0),
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

  playerSortFunctions: any = {
    "character_name": (o1, o2, asc) => stringLocaleCompare(o1.CharacterName, o2.CharacterName, asc),
    "tribe_name": (o1, o2, asc) => stringLocaleCompare(o1.TribeName, o2.TribeName, asc),
    "last_active": (o1, o2, asc) => intCompare(o1.LastActiveTime, o2.LastActiveTime, !asc)
  };

  tribeSortFunctions: any = {
    "tribe_name": (o1, o2, asc) => stringLocaleCompare(o1.Name, o2.Name, asc),
    "last_active": (o1, o2, asc) => intCompare(o1.LastActiveTime, o2.LastActiveTime, !asc)
  };

  wildStatisticsSortFunctions: any = {
    "species": (o1, o2, asc) => stringLocaleCompare(o1.Name, o2.Name, asc),
    "class_name": (o1, o2, asc) => stringLocaleCompare(o1.ClassName, o2.ClassName, asc),
    "count": (o1, o2, asc) => intCompare(o1.Count, o2.Count, !asc),
    "fraction": (o1, o2, asc) => floatCompare(o1.Fraction, o2.Fraction, !asc, 4),
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

    getServer(): void {
      this.httpService
        .getServer(this.serverKey)
        .then(server => {
          this.server = server;
          this.filter();
          this.loaded = true;
        })
        .catch(error => {
          this.server = null;
          this.filteredPlayers = null;
          this.filteredTribes = null;
          this.loaded = true;
        });
  }

  getWildCreatures(): void {
    this.httpService
      .getWildCreatures(this.serverKey)
      .then(wild => {
        this.wild = wild;
        this.species = Object.keys(this.wild.Species).sort((a, b) => stringLocaleCompare(this.wild.Species[a].Name || a, this.wild.Species[b].Name || b, true));
        if (!this.selectedSpecies || this.species.find(k => k == this.selectedSpecies) == undefined) this.selectedSpecies = this.species.length > 0 ? this.species[0]: null;
        this.filterAndSortWild();
        this.creaturesLoaded = true;
        this.ref.detectChanges(); //todo: evaluate
      })
      .catch(error => {
        this.wild = null;
        this.species = null;
        this.filteredCreatures = null;
        this.creaturesLoaded = true;
      });
  }

  public accessControl_pages_player: Observable<boolean>;

  ngOnInit() {
    this.accessControl_pages_player = this.dataService.hasFeatureAccessObservable('pages', 'player');

    this.serverKey = this.route.snapshot.params['id'];

    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => {
      this.menuOption = menuOption;
      //todo: should it be possible to reload this data?
      if (this.creaturesLoaded == false && (this.menuOption == "wildcreatures" || this.menuOption == "wildcreatures-statistics")) {
        this.getWildCreatures();
      }
    });
    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => {
      if(this.serverKey == serverKey) {
        this.updateServer();
        this.showServerUpdateNotification(serverKey);
      }
    });
    
    this.getServer();
  }

  ngOnDestroy() {
    this.menuOptionSubscription.unsubscribe();
    this.serverUpdatedSubscription.unsubscribe();
  }

  filter(): void {
    let currentDate = this.dataService.getCurrentDate();
    let pastDate = currentDate.subtract(90, 'day');

    this.filteredPlayers = this.server.Players.filter(player => 
      moment(new Date(player.LastActiveTime)).isSameOrAfter(pastDate));
      this.filteredTribes = this.server.Tribes.filter(tribe => 
      moment(new Date(tribe.LastActiveTime)).isSameOrAfter(pastDate));
  }

  sortWild() {
    let asc = this.creaturesSortField[0] != '-';
    let sortFunc = this.creaturesSortFunctions[this.creaturesSortField.replace(/^\-/, "")];
    let alts = this.creaturesAltSortFields.split(',').map(k => {
      let a = <any> {};
      a.asc = k[0] != '-';
      a.sortFunc = this.creaturesSortFunctions[k.replace(/^\-/, "")];
      return a;
    });

    if(this.filteredCreatures != undefined)
    {
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
  }

  filterWild(): void {
    if (!this.selectedSpecies)  this.filteredCreatures = undefined;
    else this.filteredCreatures = this.wild.Species[this.selectedSpecies].Creatures;

    /*if (this.creaturesFilter == null || this.creaturesFilter.length == 0) this.filteredCreatures = this.wild.Creatures;
    else {
      let filter = this.creaturesFilter.toLowerCase();
      this.filteredCreatures = this.wild.Creatures.filter(creature => 
        (creature.Species != null && creature.Species.toLowerCase().indexOf(filter) >= 0));
    }*/

    let points = [];
    if(this.filteredCreatures != undefined)
    {
      for(let creature of this.filteredCreatures) {
        let point = {} as any;
        point.x = creature.TopoMapX;
        point.y = creature.TopoMapY;
        points.push(point);
      }
    }
    this.points = points;
  }

  filterAndSortWild(): void {
    this.filterWild();
    this.sortWild();
  }

  openMap(event: any): void {
    this.showMap = true;
    event.stopPropagation();
  }

  closeMap(event: any): void {
    this.showMap = false;
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

    if (field == "latitude") this.creaturesAltSortFields = !reverse ? "longitude" : "-longitude";
    else if (field == "longitude") this.creaturesAltSortFields = !reverse ? "latitude" : "-latitude";
    else this.creaturesAltSortFields = "base_level,gender";

    this.sortWild();
  }

  numCreatureTabs() : number {
    let num = 1;
    if (this.dataService.hasFeatureAccess('server', 'wildcreatures-basestats')) num += 1;
    if (this.dataService.hasFeatureAccess('server', 'wildcreatures-ids')) num += 1;

    return num;
  }

  toRelativeDate(datejson: string): string {
    return moment(new Date(datejson)).fromNow();
  }

  getTribeMember(steamId: string): string {
    return this.server.Players.find((p) => p.SteamId == steamId);
  }

  updateServer(): void {
    this.getServer();
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
