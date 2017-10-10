import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { ActivatedRoute } from '@angular/router';
import { DataService } from '../data.service';

@Component({
  selector: 'app-player-menu',
  host: {'[class]': 'menu.className'},
  templateUrl: './player-menu.component.html',
  styleUrls: ['./player-menu.component.css']
})
export class PlayerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  steamId: string;

  constructor(
    private route: ActivatedRoute,
    public dataService: DataService) {
    }

  ngOnInit() {
    this.steamId = this.route.snapshot.params['playerid'];

    if (this.dataService.hasFeatureAccess('player', 'profile', this.steamId)) this.menu.activate("profile");
    else if (this.dataService.hasFeatureAccess('player', 'creatures', this.steamId)) this.menu.activate("creatures");
    else if (this.dataService.hasFeatureAccess('player', 'creatures-cloud', this.steamId)) this.menu.activate("creatures_cloud");
    else if (this.dataService.hasFeatureAccess('player', 'breeding', this.steamId)) this.menu.activate("breeding");
    else if (this.dataService.hasFeatureAccess('player', 'crops', this.steamId)) this.menu.activate("crop_plots");
    else if (this.dataService.hasFeatureAccess('player', 'generators', this.steamId)) this.menu.activate("electrical_generators");
    else if (this.dataService.hasFeatureAccess('player', 'kibbles-eggs', this.steamId)) this.menu.activate("kibbles_and_eggs");
    else if (this.dataService.hasFeatureAccess('player', 'tribelog', this.steamId)) this.menu.activate("tribelog");
  }
}