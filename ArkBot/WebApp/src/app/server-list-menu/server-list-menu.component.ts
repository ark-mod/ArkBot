import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';

@Component({
  selector: 'app-server-list-menu',
  templateUrl: './server-list-menu.component.html',
  styleUrls: ['./server-list-menu.component.css']
})
export class ServerListMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;
  ngOnInit() {
    this.menu.activate("overview");
  }
}