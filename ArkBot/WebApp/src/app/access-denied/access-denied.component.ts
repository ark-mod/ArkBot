import { Component, OnInit, OnDestroy } from '@angular/core';

import { DataService } from '../data.service';
import { NotificationsService } from 'angular2-notifications';
import { MessageService } from '../message.service';

import * as moment from 'moment'

@Component({
  selector: 'app-access-denied',
  templateUrl: './access-denied.component.html',
  styleUrls: ['./access-denied.component.css']
})
export class AccessDeniedComponent implements OnInit, OnDestroy {
  serverUpdatedSubscription: any;

  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  constructor(
    public dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
    }

  ngOnInit() {
    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => this.showServerUpdateNotification(serverKey));
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.menuOption = menuOption);
  }

  ngOnDestroy() {
    this.serverUpdatedSubscription.unsubscribe();
    this.menuOptionSubscription.unsubscribe();
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
}
