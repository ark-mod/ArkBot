import { Component, OnInit, OnDestroy } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { BreadcrumbService } from 'ng2-breadcrumb/ng2-breadcrumb';
import { DataService } from './data.service';
import { HttpService } from './http.service';

@Component({
  selector: 'body',
  host: {'[class]': 'getTheme()'},
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  public notificationOptions = {
      position: ["top", "right"],
      timeOut: 1000,
      lastOnBottom: false
  };
  public showLogin: boolean = false;
  public currentUrl: string = "/";
  private serversUpdatedSubscription: any;
  private serversUpdatedBefore: boolean = false;

  constructor(
    public dataService: DataService,
    private httpService: HttpService,
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
    this.currentUrl = window.location.href || '/'
    this.serversUpdatedSubscription = this.dataService.ServersUpdated$.subscribe(servers => {
      if (!this.serversUpdatedBefore) {
        if (servers && !servers.User) {
          //prompt for login
          this.showLogin = true;
        }
      }
      this.serversUpdatedBefore = true;
    });
  }

  ngOnDestroy(): void {
    this.serversUpdatedSubscription.unsubscribe();
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

  openLogin(event: any): void {
    this.showLogin = true;
    event.stopPropagation();
    event.preventDefault();
  }

  closeLogin(event: any): void {
    this.showLogin = false;
  }

  getLoginUrl(): string {
    return this.httpService.getApiBaseUrl() + '/authentication/login';
  }

  getLogoutUrl(): string {
    return this.httpService.getApiBaseUrl() + '/authentication/logout?returnUrl=' + this.currentUrl;
  }
}