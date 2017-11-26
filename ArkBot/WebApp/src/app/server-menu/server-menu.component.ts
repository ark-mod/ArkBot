import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { DataService } from '../data.service';

@Component({
  selector: 'app-server-menu',
  host: {'[class]': 'menu.className'},
  templateUrl: './server-menu.component.html',
  styleUrls: ['./server-menu.component.css']
})
export class ServerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  constructor(
    public dataService: DataService) {
    }

  ngOnInit() {
    if (this.dataService.hasFeatureAccess('server', 'players')) this.menu.activate("players");
    else if (this.dataService.hasFeatureAccess('server', 'tribes')) this.menu.activate("tribes");
    else if (this.dataService.hasFeatureAccess('server', 'wildcreatures-statistics')) this.menu.activate("wildcreatures-statistics");
    else if (this.dataService.hasFeatureAccess('server', 'wildcreatures')) this.menu.activate("wildcreatures");
  }
}