import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, Event as RouterEvent, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import { BreadcrumbService } from 'ng2-breadcrumb/ng2-breadcrumb';
import { DataService } from './data.service';
import { HttpService } from './http.service';
import { environment } from '../environments/environment';

declare var config: any;

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
  private routerEventsSubscription: any;
  private loading: boolean = true;

  constructor(
    public dataService: DataService,
    private httpService: HttpService,
    private breadcrumbService: BreadcrumbService,
    private notificationsService: NotificationsService,
    private router: Router) { 
      breadcrumbService.addFriendlyNameForRoute('/accessdenied', 'Access Denied');
      breadcrumbService.addFriendlyNameForRoute('/connectionerror', 'Connection error');
      breadcrumbService.hideRoute('/player');
      breadcrumbService.hideRoute('/servers');
      breadcrumbService.hideRoute('/server');
      breadcrumbService.hideRoute('/admin');
      breadcrumbService.addCallbackForRouteRegex('^/player/.+$', this.getNameForPlayer);
    }

  ngOnInit(): void {
    this.dataService.SetTheme(this.getTheme());
    this.routerEventsSubscription = this.router.events.subscribe((event: RouterEvent) => {
      this.navigationInterceptor(event);
    });

    this.currentUrl = window.location.href || '/'
    this.serversUpdatedSubscription = this.dataService.ServersUpdated$.subscribe(servers => {
      if (!this.serversUpdatedBefore) {
        if (servers && (!servers.User || !servers.User.SteamId)) {
          //prompt for login
          this.showLogin = true;
        }
      }
      this.serversUpdatedBefore = true;
    });
  }

  ngOnDestroy(): void {
    this.routerEventsSubscription.unsubscribe();
    this.serversUpdatedSubscription.unsubscribe();
  }

  navigationInterceptor(event: RouterEvent): void {
    if (event instanceof NavigationStart) this.loading = true;
    else if (event instanceof NavigationEnd) this.loading = false;
    else if (event instanceof NavigationCancel) this.loading = false;
    else if (event instanceof NavigationError) this.loading = false;
  }

  getNameForPlayer(id:string):string {
    return `Player`;
  }

  getDefaultTheme(): string {
    var value = (typeof config !== 'undefined' && config.webapp !== 'undefined' && typeof config.webapp.defaultTheme === 'string' ? config.webapp.defaultTheme.toLowerCase() : undefined);
    return value != 'light' && value != 'dark' ? 'dark' : value;
  }

  getTheme(): string {
    return localStorage.getItem('theme') || this.getDefaultTheme();
  }
  
  setTheme(theme: string): boolean {
    this.dataService.SetTheme(theme);
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
    return !environment.demo ? this.httpService.getApiBaseUrl() + '/authentication/logout?returnUrl=' + this.currentUrl : '';
  }
}