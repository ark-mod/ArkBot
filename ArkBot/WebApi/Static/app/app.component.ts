import { Component, OnInit } from '@angular/core';

import { Servers } from './servers';
import { Player } from './player';
import { HttpService } from './http.service';

@Component({
  selector: 'my-app',
  template: `
  <div class="w3-container w3-green">
    <div class="w3-cell"><h1>/api/player/</h1></div>
    <div class="w3-cell"><input [ngModel]="steamId" (ngModelChange)="steamId = $event; run();" class="w3-input w3-green w3-border-0 w3-xlarge" placeholder="<Steam ID>" /></div>
  </div>
  <section *ngIf="loaded == false" class="w3-container">
    <div class="w3-panel w3-light-grey">
      <h3 class="w3-text-grey">Loading...</h3>
    </div> 
  </section>
  <section *ngIf="loaded == true && player == null" class="w3-container">
    <div class="w3-panel w3-red">
      <h3>Error!</h3>
      <p>No data could be loaded for the given steam id.</p>
    </div> 
  </section>
  <section *ngIf="player" class="w3-container">
    <h2 class="w3-text-blue">Servers</h2>
    <div *ngIf="player?.Servers" class="w3-bar w3-light-grey w3-card-4">
      <a *ngFor="let server of keysGetter(player?.Servers)" href="#" class="w3-bar-item w3-button w3-mobile" [ngClass]="{'w3-blue': active(server), 'w3-text-white': active(server)}" [style.width.%]="serverWidth()" (click)="activate(server)">{{server}}</a>
    </div>
  </section>
  <section *ngIf="player" class="w3-container">
    <h2 class="w3-text-blue">Player</h2>
    <div class="w3-responsive">
      <table class="w3-table-all w3-card-4">
        <thead>
          <tr class="w3-blue">
            <th>Character Name</th>
            <th>Gender</th>
            <th>Tribe Name</th>
            <th>Steam Id</th>
            <th>Tribe Id</th>
            <th>Level</th>
            <th>Engram Points</th>
            <th>Latitude</th>
            <th>Longitude</th>
            <th>Saved At</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>{{player?.Servers[serverKey]?.CharacterName}}</td>
            <td>{{player?.Servers[serverKey]?.Gender}}</td>
            <td>{{player?.Servers[serverKey]?.TribeName}}</td>
            <td>{{player?.Servers[serverKey]?.SteamId}}</td>
            <td>{{player?.Servers[serverKey]?.TribeId}}</td>
            <td>{{player?.Servers[serverKey]?.Level}}</td>
            <td>{{player?.Servers[serverKey]?.EngramPoints | number}}</td>
            <td>{{player?.Servers[serverKey]?.Latitude | number:'1.1-1'}}</td>
            <td>{{player?.Servers[serverKey]?.Longitude | number:'1.1-1'}}</td>
            <td>{{toDate(player?.Servers[serverKey]?.SavedAt)}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
  <section *ngIf="player" class="w3-container">
    <div class="w3-cell-row">
      <div class="w3-cell"><h2 class="w3-text-blue w3-left">Creatures <span class="w3-tag w3-large w3-blue">{{filteredCreatures.length}}</span></h2></div>
      <div class="w3-cell w3-cell-middle"><button class="w3-button w3-blue w3-right" (click)="openMap($event)">Show Map</button></div>
    </div>
    <div class="inner-addon right-addon">
      <i *ngIf="creaturesFilter != null && creaturesFilter != ''" class="material-icons" style="cursor: pointer;" (click)="creaturesFilter = ''; filter();">close</i>
      <input [ngModel]="creaturesFilter" (ngModelChange)="creaturesFilter = $event; filter();" class="w3-input w3-border w3-round-xlarge w3-large w3-margin-bottom" placeholder="Filter" />
    </div>
    <div class="w3-responsive">
      <table class="w3-table-all w3-card-4">
        <thead>
          <tr class="w3-blue">
            <th>Name</th>
            <!--<th>ClassName</th>-->
            <th>Species</th>
            <!--<th>Aliases</th>-->
            <th>Gender</th>
            <th>Base Level</th>
            <th>Level</th>
            <th>Imprint</th>
            <th>Food</th>
            <th>Latitude</th>
            <th>Longitude</th>
            <th>Next Mating</th>
            <th>Owner</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let creature of filteredCreatures">
            <td>{{creature.Name}}</td>
            <!--<td>{{creature.ClassName}}</td>-->
            <td>{{creature.Species}}</td>
            <!--<td>{{creature.Aliases}}</td>-->
            <td>{{creature.Gender}}</td>
            <td>{{creature.BaseLevel}}</td>
            <td><span *ngIf="creature.BaseLevel != creature.Level">{{creature.Level}}</span></td>
            <td>{{creature.Imprint | percent:'1.0-0'}}</td>
            <td>
              <div class="w3-light-grey">
                <div class="w3-container w3-green w3-center" [style.width.%]="creature.FoodStatus * 100">{{creature.FoodStatus | percent:'1.0-0'}}</div>
              </div>
            </td>
            <td>{{creature.Latitude | number:'1.1-1'}}</td>
            <td>{{creature.Longitude | number:'1.1-1'}}</td>
            <td><span *ngIf="haveMatingCooldown(creature.NextMating)">{{toRelativeDate(creature.NextMating)}}</span></td>
            <td>{{creature.OwnerType}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
  <div id="modal_map" class="w3-modal" [style.display]="showMap ? 'block' : 'none'">
   <div class="w3-modal-content w3-card-4 w3-animate-zoom" (clickOutside)="closeMap($event)" style="font-size: 0;">
    <header class="w3-container w3-white"> 
     <span (click)="showMap = false" class="w3-button w3-white w3-xlarge w3-display-topright">&times;</span>
     <h2>Map</h2>
    </header>
    <arkmap [mapName]="player?.MapNames[serverKey]" [points]="points"></arkmap>
   </div>
  </div>
`
})
export class AppComponent implements OnInit {
  servers: Servers;
  player: Player;
  filteredCreatures: any[];
  creaturesFilter: string;
  points: any[];

  constructor(
    private httpService: HttpService) { }
  
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

  haveMatingCooldown(nextMating: string): bool {
    return new Date(nextMating) > new Date();
  }

  toDate(datejson: string): string {
    return new Date(datejson).toLocaleString('sv-SE');
  }

  toRelativeDate(datejson: string): string {
    return moment(new Date(datejson)).fromNow();
  }

  active(serverKey: string): string {
    return this.serverKey == serverKey;
  }

  activate(serverKey: string): string {
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

  keysGetter = Object.keys;
  steamId: string;
  loaded: bool = false;
  showMap: bool = false;
}