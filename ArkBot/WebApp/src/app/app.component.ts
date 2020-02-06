import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { Router, Event as RouterEvent, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { NotificationsService } from 'angular2-notifications';
import { BreadcrumbService } from 'ng2-breadcrumb/ng2-breadcrumb';
import { MessageService } from './message.service';
import { DataService } from './data.service';
import { HttpService } from './http.service';
import { HotkeysService } from './hotkeys.service';
import { environment } from '../environments/environment';
import { DOCUMENT } from '@angular/common';

declare var config: any;

@Component({
  selector: 'body',
  host: {'[class]': 'getBodyClasses()'},
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
  public showAdmin: boolean = false;
  public previewOverrideMenuOption: boolean = false;
  public previewMenuName: string;
  public currentUrl: string = "/";
  private serversUpdatedSubscription: any;
  private serversUpdatedBefore: boolean = false;
  private routerEventsSubscription: any;
  private adminOptionsHotkeySubscription: any;
  private loading: boolean = true;

  constructor(
    @Inject(DOCUMENT) private doc: any,
    public messageService: MessageService,
    public dataService: DataService,
    private httpService: HttpService,
    private hotkeysService: HotkeysService,
    private breadcrumbService: BreadcrumbService,
    private notificationsService: NotificationsService,
    private router: Router) { 
      const script_configjs = this.doc.getElementById('configjs');
      let contents: string = null;
      if (environment.configJsOverride != null) contents = environment.configJsOverride;
      else if (script_configjs.text == "/*[[config]]*/") contents = environment.configJsDefault;

      if (contents != null) {
        const s = this.doc.createElement('script');
        s.type = 'text/javascript';
        s.id = "configjs";
        s.text = contents;
        script_configjs.parentNode.replaceChild(s, script_configjs);
      }

      if (typeof config !== 'undefined' && config.webapp !== 'undefined' && config.webapp.useCustomCssFile === true) {
        const l = this.doc.createElement('link');
        l.rel = 'stylesheet';
        l.href = '/custom.css';
        this.doc.getElementsByTagName("head")[0].appendChild(l);
      }

      if (typeof config !== 'undefined' && config.webapp !== 'undefined' && config.webapp.topMenu === true) this.previewMenuName = "Sidebar Menu";
      else this.previewMenuName = "Top Menu";

      breadcrumbService.addFriendlyNameForRoute('/accessdenied', 'Access Denied');
      breadcrumbService.addFriendlyNameForRoute('/connectionerror', 'Connection error');
      breadcrumbService.hideRoute('/player');
      breadcrumbService.hideRoute('/servers');
      breadcrumbService.hideRoute('/server');
      breadcrumbService.hideRoute('/admin');
      breadcrumbService.addCallbackForRouteRegex('^/player/.+$', this.getNameForPlayer);
      if (!environment.demo) messageService.connect();
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

    this.adminOptionsHotkeySubscription = this.hotkeysService.add({ keys: 'control.shift.a' }).subscribe(() => {
      if (this.dataService.hasFeatureAccess('pages', 'admin-server')) this.showAdmin = true;
    });
  }

  ngOnDestroy(): void {
    this.routerEventsSubscription.unsubscribe();
    this.serversUpdatedSubscription.unsubscribe();
    this.adminOptionsHotkeySubscription.unsubscribe();
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

  getBodyClasses(): string {
    let classes = this.getTheme();
    let topmenu = typeof config !== 'undefined' && config.webapp !== 'undefined' && config.webapp.topMenu === true;
    if (topmenu !== this.previewOverrideMenuOption)
      classes += " topmenu";
    return classes;
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

  openAdminOptions(event: any) {
    this.showAdmin = true;
    event.stopPropagation();
    event.preventDefault();
  }

  openCustomTheme(event: any, customTheme: any): void {
    event.stopPropagation();
    event.preventDefault();

    this.showAdmin = false;
    customTheme.show();
  }

  getLoginUrl(): string {
    return !environment.demo ? this.httpService.getApiBaseUrl() + '/authentication/login' : '';
  }

  getLogoutUrl(): string {
    return !environment.demo ? this.httpService.getApiBaseUrl() + '/authentication/logout?returnUrl=' + this.currentUrl : '';
  }
}