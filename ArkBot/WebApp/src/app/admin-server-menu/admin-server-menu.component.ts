import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';

@Component({
  selector: 'app-admin-server-menu',
  templateUrl: './admin-server-menu.component.html',
  styleUrls: ['./admin-server-menu.component.css']
})
export class AdminServerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;
  ngOnInit() {
    this.menu.activate("players");
  }
}