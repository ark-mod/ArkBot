import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-server',
  templateUrl: './server.component.html',
  styleUrls: ['./server.component.css']
})
export class ServerComponent implements OnInit, OnDestroy {
  server: any;
  filteredPlayers: any[];
  filteredTribes: any[];
  loaded: boolean = false;
  serverKey: string;
  serverUpdatedSubscription: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpService: HttpService,
    private dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
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

  ngOnInit() {
    this.serverKey = this.route.snapshot.params['id'];

    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => {
      if(this.serverKey == serverKey) {
        this.updateServer();
        this.showServerUpdateNotification(serverKey);
      }
    });
    
    this.getServer();
  }

  ngOnDestroy() {
    this.serverUpdatedSubscription.unsubscribe();
  }

  filter(): void {
    this.filteredPlayers = this.server.Players.filter(player => 
      moment(new Date(player.LastActiveTime)).isSameOrAfter(moment().subtract(90, 'day')));
      this.filteredTribes = this.server.Tribes.filter(tribe => 
      moment(new Date(tribe.LastActiveTime)).isSameOrAfter(moment().subtract(90, 'day')));
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
}
