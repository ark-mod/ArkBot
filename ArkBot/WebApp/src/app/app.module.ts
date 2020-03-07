import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER , LOCALE_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HttpModule } from '@angular/http';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { Ng2BreadcrumbModule } from 'ng2-breadcrumb/ng2-breadcrumb';

import { AppComponent } from './app.component';
import { PlayerComponent } from './player/player.component';
import { PlayerMenuComponent } from './player-menu/player-menu.component';
import { ServerComponent } from './server/server.component';
import { ServerListComponent } from './server-list/server-list.component';
import { AdminServerComponent } from './admin-server/admin-server.component';

import { ArkMapComponent } from './arkmap.component';
import { HttpService } from './http.service';
import { DemoHttpService } from './demo.http.service';
import { MessageService } from './message.service';
import { DataService } from './data.service';
import { DataServiceResolver } from './data-resolver.service';
import { HotkeysService } from './hotkeys.service';
import { AccessControlRouteGuardService } from './access-control-route-guard.service';
import { SanitizeStylePipe } from './sanitize-style.pipe';
import { SanitizeHtmlPipe } from './sanitize-html.pipe';
import { ClickOutsideDirective } from './clickOutside.directive';
import { ServerListMenuComponent } from './server-list-menu/server-list-menu.component';
import { MenuComponent } from './menu/menu.component';
import { ServerMenuComponent } from './server-menu/server-menu.component';
import { AdminServerMenuComponent } from './admin-server-menu/admin-server-menu.component';
import { ArkmapStructuresComponent } from './arkmap-structures/arkmap-structures.component';
import { TimerComponent } from './timer/timer.component';
import { RelativeTimeComponent } from './relative-time/relative-time.component';
import { ConfirmButtonComponent } from './confirm-button/confirm-button.component';
import { AccessDeniedComponent } from './access-denied/access-denied.component';
import { ConnectionErrorComponent } from './connection-error/connection-error.component';
import { DeveloperComponent } from './developer/developer.component';
import { CustomThemeComponent } from './custom-theme/custom-theme.component';

import { DataTableModule } from './data-table/data-table.module';
import { environment } from '../environments/environment';

const appRoutes: Routes = [
  {
    path: 'player/:playerid',
    canActivate: [AccessControlRouteGuardService],
    //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
    data: { name: 'player' },
    children: [
      {
        path: '',
        component: PlayerComponent
      },
      {
        path: '',
        component: PlayerMenuComponent,
        outlet: 'menu'
      }
    ]
  },
  {
    path: 'server/:id',
    canActivate: [AccessControlRouteGuardService],
    //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
    data: { name: 'server' },
    children: [
      {
        path: '',
        component: ServerComponent
      },
      {
        path: '',
        component: ServerMenuComponent,
        outlet: 'menu'
      }
    ]
  },
  {
    path: 'admin/:id',
    canActivate: [AccessControlRouteGuardService],
    //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
    data: { name: 'admin-server' },
    children: [
      {
        path: '',
        component: AdminServerComponent
      },
      {
        path: '',
        component: AdminServerMenuComponent,
        outlet: 'menu'
      }
    ]
  },
  {
    path: 'servers',
    canActivate: [AccessControlRouteGuardService],
    //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
    data: { name: 'home' },
    children: [
      {
        path: '',
        component: ServerListComponent
      },
      {
        path: '',
        component: ServerListMenuComponent,
        outlet: 'menu'
      }
    ]
  },
  {
    path: 'developer',
    component: DeveloperComponent
  },
  {
    path: 'accessdenied',
    component: AccessDeniedComponent
  },
  {
    path: 'connectionerror',
    component: ConnectionErrorComponent
  },
  { path: '',
    redirectTo: '/servers',
    pathMatch: 'full'
  }
];

@NgModule({
  declarations: [
    AppComponent,
    ServerListComponent,
    ArkMapComponent,
    SanitizeStylePipe,
    SanitizeHtmlPipe,
    ClickOutsideDirective,
    PlayerComponent,
    PlayerMenuComponent,
    ServerComponent,
    AdminServerComponent,
    ServerListMenuComponent,
    MenuComponent,
    ServerMenuComponent,
    AdminServerMenuComponent,
    ArkmapStructuresComponent,
    TimerComponent,
    RelativeTimeComponent,
    ConfirmButtonComponent,
    AccessDeniedComponent,
    ConnectionErrorComponent,
    DeveloperComponent,
    CustomThemeComponent
  ],
  imports: [
    RouterModule.forRoot(appRoutes),
    Ng2BreadcrumbModule.forRoot(),
    BrowserModule,
    FormsModule,
    HttpModule,
    BrowserAnimationsModule,
    SimpleNotificationsModule.forRoot(), 
    DataTableModule
  ],
  providers: [
    [{ provide: HttpService, useClass: !environment.demo ? HttpService : DemoHttpService }], 
    MessageService, 
    DataService,
    DataServiceResolver,
    HotkeysService,
    AccessControlRouteGuardService,
    { provide: LOCALE_ID, useValue: "en-US" }],
  bootstrap: [AppComponent]
})
export class AppModule { }
