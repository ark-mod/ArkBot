import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';
import * as d3 from "d3";

@Component({
  selector: 'app-admin-server',
  templateUrl: './admin-server.component.html',
  styleUrls: ['./admin-server.component.css']
})
export class AdminServerComponent implements OnInit, OnDestroy {
  private menuOption: string = undefined;
  private menuOptionSubscription: any;

  public modalInfo: any;

  serverUpdatedSubscription: any;
  server: any;
  loaded: boolean = false;
  loadedStructures: boolean = false;
  loadedFertilizedEggs: boolean = false;
  serverKey: string;
  structures: any;
  fertilizedEggsList: any[];
  spoiledEggsList: any[];
  fertilizedEggsCount: number;
  spoiledEggsCount: number;
  totalEggCount: number;

  @ViewChild('contextMenu') contextMenu: ElementRef;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpService: HttpService,
    public dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
  }

  getServer(): void {
    this.httpService
      .getAdminServer(this.serverKey)
      .then(server => {
        this.server = server;
        this.loaded = true;
      })
      .catch(error => {
        this.server = null;
        this.loaded = true;
      });
  }

  getStructures(): void {
    this.httpService
      .getStructures(this.serverKey)
      .then(structures => {
        this.structures = structures;
        this.loadedStructures = true;
      })
      .catch(error => {
        this.structures = undefined;
        this.loadedStructures = true;
      });
  }

  getListFertilizedEggs(): void {
    this.httpService
      .adminListFertilizedEggs(this.serverKey)
      .then(fertilizedEggs => {
        this.spoiledEggsList = fertilizedEggs.SpoiledEggList;
        this.fertilizedEggsList = fertilizedEggs.FertilizedEggList;
        this.fertilizedEggsCount = fertilizedEggs.FertilizedEggsCount === undefined ? 0 : fertilizedEggs.FertilizedEggsCount;
        this.spoiledEggsCount = fertilizedEggs.SpoiledEggsCount === undefined ? 0 : fertilizedEggs.SpoiledEggsCount;
        this.totalEggCount = this.spoiledEggsCount + this.fertilizedEggsCount;
        this.loadedFertilizedEggs = true;
      })
      .catch(error => {
        this.fertilizedEggsList = undefined;
        this.loadedFertilizedEggs = true;
      });
  }

  ngOnInit() {
    this.serverKey = this.route.snapshot.params['id'];

    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => {
      this.menuOption = menuOption;
      if (this.menuOption == "structures") {
        this.getStructures();
      }
      else if (this.menuOption == "fertilized-eggs") {
        this.getListFertilizedEggs();
      }
    });
    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => {
      if (this.serverKey == serverKey) {
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

  showInfoModal(header: string, message: string): void {
    let modalInfo = <any>{};
    modalInfo.Header = header;
    modalInfo.Message = message;
    this.modalInfo = modalInfo;

    let cm = d3.select(this.contextMenu.nativeElement);
    cm.style("display", "block");

    if (d3.event) d3.event.stopPropagation();
  }

  hideContextMenu(): void {
    let cm = d3.select(this.contextMenu.nativeElement);
    cm.style("display", "none");

    this.modalInfo = undefined;
  }

  saveWorld(event: string): void {
    this.httpService.adminSaveWorld(this.serverKey)
      .then(response => {
        this.hideContextMenu();
        this.getListFertilizedEggs();
        this.showInfoModal("Action Successfull!", response.Message);
      })
      .catch(error => {
        this.hideContextMenu();
        let json = error && error._body ? JSON.parse(error._body) : undefined;
        this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
      });
  }

  destroyAllEggs(event: string): void {
    this.httpService.adminDestroyAllEggs(this.serverKey)
      .then(response => {
        this.hideContextMenu();
        this.getListFertilizedEggs();
        this.showInfoModal("Action Successfull!", response.Message);
      })
      .catch(error => {
        this.hideContextMenu();
        let json = error && error._body ? JSON.parse(error._body) : undefined;
        this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
      });
  }

  destroySpoiledEggs(event: string): void {
    this.httpService.adminDestroySpoiledEggs(this.serverKey)
      .then(response => {
        this.hideContextMenu();
        this.getListFertilizedEggs();
        this.showInfoModal("Action Successfull!", response.Message);
      })
      .catch(error => {
        this.hideContextMenu();
        let json = error && error._body ? JSON.parse(error._body) : undefined;
        this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
      });
  }
}
