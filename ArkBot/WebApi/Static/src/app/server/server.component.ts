import { Component, OnInit } from '@angular/core';
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
export class ServerComponent implements OnInit {
  server: any;
  loaded: boolean = false;
  serverKey: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpService: HttpService,
    private dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
      //messageService.serverUpdated$.subscribe(serverKey => this.updateServer(serverKey));
    }

    getPlayer(): void {
      this.httpService
        .getServer(this.serverKey)
        .then(server => {
          this.server = server;
          this.loaded = true;
        })
        .catch(error => {
          this.server = null;
          this.loaded = true;
        });
  }

  ngOnInit() {
        this.serverKey = this.route.snapshot.params['id'];

    this.getPlayer();
  }

  toRelativeDate(datejson: string): string {
    return moment(new Date(datejson)).fromNow();
  }

  getTribeMember(steamId: string): string {
    return this.server.Players.find((p) => p.SteamId == steamId);
  }

  /*updateServer(): void {
    this.getServer();
  }*/
}
