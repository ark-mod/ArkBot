import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import * as moment from 'moment'

import { DataService } from '../data.service';
import { MessageService } from '../message.service';
import { HttpService } from '../http.service';
import { Auction } from '../auction';
import { Market } from '../market';

import { floatCompare, intCompare, stringLocaleCompare, nullCompare, fromHsvToRgb } from '../utils'
import { Observable } from 'rxjs';

@Component({
  selector: 'app-auction',
  templateUrl: './auction.component.html',
  styleUrls: ['./auction.component.css']
})
export class AuctionComponent implements OnInit, OnDestroy {
  private marketsUpdatedSubscription: any;
  public filteredAuctions: Auction[];
  public filteredMarket: Market;

  private menuOption: string = undefined;
  private menuOptionSubscription: any;

  private theme: string = undefined;
  private theme$: Observable<string>;
  private themeSubscription: any;

  keysGetter = Object.keys;
  steamId: string;
  loaded: boolean = false;
  showMap: boolean = false;
  serverKey: string;
  sub: string = "dino";
  clusterKey: string;
  auctionsFilter: string;
  isBreedableFilter: boolean;
  isBlueprintFilter: boolean;

  constructor(
    private route: ActivatedRoute,
    public dataService: DataService,
    private notificationsService: NotificationsService) {
    this.loaded = true;
  }

  ngOnInit(): void {
    //this.marketsUpdatedSubscription = this.messageService.marketsUpdated$.subscribe();
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.setMenuOption(menuOption));
    this.theme$ = this.dataService.Theme;
    this.themeSubscription = this.theme$.subscribe(theme => { this.theme = theme; });
    this.steamId = this.route.snapshot.params['playerid'];
  }

  setMenuOption(menuOption: string): void {
    this.menuOption = menuOption;
    this.refresh();
  }

  ngOnDestroy() {
    this.menuOptionSubscription.unsubscribe();
    this.themeSubscription.unsubscribe();
    //this.marketsUpdatedSubscription.unsubscribe();
  }

  activeSub(menuOption: string, sub: string): boolean {
    return this.menuOption == menuOption && this.sub == sub;
  }

  activateSub(sub: string): void {
    this.sub = sub;
    this.refresh();
  }

  filterAndSort(): void {
    this.refresh();
  }

  refresh(): void {
    if (this.auctionsFilter == null) {
      this.auctionsFilter = "";
    }

    if (this.dataService.Markets?.Markets == null) {
      return;
    }

    if (this.sub == "dino") {
      this.filterDino();
    } else {
      this.filterItem();
    }
  }

  filterDino(): void {
    console.info("filterDino");
    let filter = this.auctionsFilter.toLowerCase();
    this.filteredMarket = this.dataService.Markets.Markets.filter(market => market.Name == this.menuOption)[0];
    if (this.filteredMarket?.Auctions == null) {
      console.info("no auctions");
      this.filteredAuctions = null;
      return;
    }

    this.filteredAuctions = this.filteredMarket.Auctions.filter(auction =>
      auction.Type == "Dino" &&
      (filter == "" ||
        (auction.SellingClientName != null && auction.SellingClientName.toLowerCase().indexOf(filter) >= 0)
        || (auction.Seller.Name != null && auction.Seller.Name.toLowerCase().indexOf(filter) >= 0)
      ) &&
      (this.isBreedableFilter == null || this.isBreedableFilter == false || !auction.Dino.Flags.IsNeutered));
  }

  filterItem(): void {
    console.info("filterItem");
    let filter = this.auctionsFilter.toLowerCase();
    this.filteredMarket = this.dataService.Markets.Markets.filter(market => market.Name == this.menuOption)[0];
    if (this.filteredMarket?.Auctions == null) {
      console.info("no auctions");
      this.filteredAuctions = null;
      return;
    }
    
    this.filteredAuctions = this.filteredMarket.Auctions.filter(auction =>
      auction.Type == "Item" &&
      ( filter == "" ||
        (auction.SellingClientName != null && auction.SellingClientName.toLowerCase().indexOf(filter) >= 0)
        || (auction.Seller.Name != null && auction.Seller.Name.toLowerCase().indexOf(filter) >= 0)
      ) &&
      (this.isBlueprintFilter == null || this.isBlueprintFilter == false || auction.Item.Flags.IsBlueprint));
  }

  run(): void {
    if (this.steamId == null || this.steamId == "") {
      return;
    }
    //this.getPlayer();
  }

  updateServer(serverKey: string): void {
    //this.getPlayer();
    this.showServerUpdateNotification(serverKey);
  }

  showServerUpdateNotification(serverKey: string): void {
    this.notificationsService.success(
      'Server Update',
      `${serverKey} was updated; Reloading data...`,
      {
        showProgressBar: true,
        pauseOnHover: true,
        clickToClose: true
      }
    );
  }

  isMenuActive(menuOption: string): boolean {
    return this.menuOption == menuOption;
  }

  isTheme(theme: string): boolean {
    return this.theme == theme;
  }

  toggleCollapsible(event, triggerElement, contentElement) {
    triggerElement.classList.toggle("active");
    contentElement.style.maxHeight = contentElement.style.maxHeight ? null : contentElement.scrollHeight + "px";
  }
}
