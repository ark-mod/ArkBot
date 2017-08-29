import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';

@Component({
  selector: 'app-server-menu',
  templateUrl: './server-menu.component.html',
  styleUrls: ['./server-menu.component.css']
})
export class ServerMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;
  ngOnInit() {
    this.menu.activate("players");
  }
}