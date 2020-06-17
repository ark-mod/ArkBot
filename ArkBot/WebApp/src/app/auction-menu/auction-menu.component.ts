import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuComponent } from '../menu/menu.component';
import { ActivatedRoute } from '@angular/router';
import { DataService } from '../data.service';

@Component({
  selector: 'app-auction-menu',
  host: {'[class]': 'menu.className'},
  templateUrl: './auction-menu.component.html',
  styleUrls: ['./auction-menu.component.css']
})
export class AuctionMenuComponent implements OnInit {
  @ViewChild('menu') menu:MenuComponent;

  steamId: string;

  constructor(
    private route: ActivatedRoute,
    public dataService: DataService) {
    }

  ngOnInit() {
    this.steamId = this.route.snapshot.params['playerid'];
  }
}