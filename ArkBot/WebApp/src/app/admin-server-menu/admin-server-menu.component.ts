import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { DataService } from '../data.service';

@Component({
  selector: 'app-admin-server-menu',
  host: {'[class]': 'menu.className'},
  templateUrl: './admin-server-menu.component.html',
  styleUrls: ['./admin-server-menu.component.css']
})
export class AdminServerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  constructor(
    public dataService: DataService) {
    }

  ngOnInit() {
    if (this.dataService.hasFeatureAccess('admin-server', 'structures')) this.menu.activate("structures");
    else if (this.dataService.hasFeatureAccess('admin-server', 'players')) this.menu.activate("players");
    else if (this.dataService.hasFeatureAccess('admin-server', 'tribes')) this.menu.activate("tribes");
    else if (this.dataService.hasFeatureAccess('admin-server', 'eggs')) this.menu.activate("eggs");
  }
}