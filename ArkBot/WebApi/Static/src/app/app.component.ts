import { Component, OnInit } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { BreadcrumbService } from 'ng2-breadcrumb/ng2-breadcrumb';
import { MessageService } from './message.service';

@Component({
  selector: 'body',
  host: {'[class]': 'getTheme()'},
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public notificationOptions = {
      position: ["top", "right"],
      timeOut: 1000,
      lastOnBottom: false
  };

  constructor(
    private breadcrumbService: BreadcrumbService,
    private messageService: MessageService,
    private notificationsService: NotificationsService) { 
      messageService.serverUpdated$.subscribe(serverKey => this.showServerUpdateNotification(serverKey));

      //breadcrumbService.addFriendlyNameForRoute('/servers', 'Servers');
      breadcrumbService.hideRoute('/player');
      breadcrumbService.hideRoute('/servers');
      breadcrumbService.hideRoute('/server');
      breadcrumbService.addCallbackForRouteRegex('^/player/.+$', this.getNameForPlayer);
    }

  ngOnInit(): void {
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

  getNameForPlayer(id:string):string {
    return `Player`;
  }

  getTheme(): string {
    return localStorage.getItem('theme') || 'light';
  }

  setTheme(theme: string): boolean {
    localStorage.setItem('theme', theme);
    return false;
  }
}