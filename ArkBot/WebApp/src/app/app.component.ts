import { Component, OnInit } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { BreadcrumbService } from 'ng2-breadcrumb/ng2-breadcrumb';

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
    private notificationsService: NotificationsService) { 
      //breadcrumbService.addFriendlyNameForRoute('/servers', 'Servers');
      breadcrumbService.hideRoute('/player');
      breadcrumbService.hideRoute('/servers');
      breadcrumbService.hideRoute('/server');
      breadcrumbService.hideRoute('/admin');
      breadcrumbService.addCallbackForRouteRegex('^/player/.+$', this.getNameForPlayer);
    }

  ngOnInit(): void {
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