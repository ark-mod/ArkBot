import { Component, OnInit, OnDestroy } from '@angular/core';

import { DataService } from '../data.service';
import { NotificationsService } from 'angular2-notifications';
import { MessageService } from '../message.service';

import * as moment from 'moment'

@Component({
  selector: 'app-developer',
  templateUrl: './developer.component.html',
  styleUrls: ['./developer.component.css']
})
export class DeveloperComponent implements OnInit, OnDestroy {
  serverUpdatedSubscription: any;

  private menuOption: string = undefined; 
  private menuOptionSubscription: any;

  public demoMode: boolean = false;

  constructor(
    public dataService: DataService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) {
    }

  ngOnInit() {
    this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(serverKey => this.showServerUpdateNotification(serverKey));
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.menuOption = menuOption);

    this.demoMode = localStorage.getItem('demoMode') == 'true';
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

  toggleDemoMode(): void {
    let demoMode = localStorage.getItem('demoMode') != 'true';
    this.demoMode = demoMode;
    localStorage.setItem('demoMode', demoMode + '');
  }
}
