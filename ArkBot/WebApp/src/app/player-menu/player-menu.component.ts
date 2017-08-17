import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { ActivatedRoute } from '@angular/router';
import { DataService } from '../data.service';

@Component({
  selector: 'app-player-menu',
  templateUrl: './player-menu.component.html',
  styleUrls: ['./player-menu.component.css']
})
export class PlayerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  steamId: string;

  constructor(
    private route: ActivatedRoute,
    private dataService: DataService) {
    }

  ngOnInit() {
    this.steamId = this.route.snapshot.params['id'];
    this.menu.activate("profile");
  }

   isSelf(): boolean {
    var user = this.dataService.Servers ? this.dataService.Servers.User : undefined;
    return user && user.SteamId ? user.SteamId == this.steamId : false;
  }
}