import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { DataService } from '../data.service';

@Component({
  selector: 'app-server-list-menu',
  host: {'[class]': 'menu.className'},
  templateUrl: './server-list-menu.component.html',
  styleUrls: ['./server-list-menu.component.css']
})
export class ServerListMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  constructor(
    public dataService: DataService) {
    }

  ngOnInit() {
    this.menu.activate("overview");
  }
}