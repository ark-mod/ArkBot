import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-admin-server',
  templateUrl: './admin-server.component.html',
  styleUrls: ['./admin-server.component.css']
})
export class AdminServerComponent implements OnInit, OnDestroy {
  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  serverUpdatedSubscription: any;
  server: any;
  loaded: boolean = false;
  loadedStructures: boolean = false;
  serverKey: string;
  structures: any;

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

  ngOnInit() {
    this.serverKey = this.route.snapshot.params['id'];

    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => {
      this.menuOption = menuOption;
      if (this.menuOption == "structures") {
        this.getStructures();
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
