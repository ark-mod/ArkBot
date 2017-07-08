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
import { MessageService } from './message.service';
import { DataService } from './data.service';
import { SanitizeStylePipe } from './sanitize-style.pipe';
import { ClickOutsideDirective } from './clickOutside.directive';
import { ServerListMenuComponent } from './server-list-menu/server-list-menu.component';
import { MenuComponent } from './menu/menu.component';
import { ServerMenuComponent } from './server-menu/server-menu.component';
import { AdminServerMenuComponent } from './admin-server-menu/admin-server-menu.component';
import { ArkmapStructuresComponent } from './arkmap-structures/arkmap-structures.component';
import { ArkmapStructures2Component } from './arkmap-structures2/arkmap-structures2.component';

const appRoutes: Routes = [
  {
    path: 'player/:id',
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
    ArkmapStructures2Component
  ],
  imports: [
    RouterModule.forRoot(appRoutes),
    Ng2BreadcrumbModule.forRoot(),
    BrowserModule,
    FormsModule,
    HttpModule,
    BrowserAnimationsModule,
    SimpleNotificationsModule.forRoot()
  ],
  providers: [
    HttpService, 
    MessageService, 
    DataService,
    { provide: LOCALE_ID, useValue: "en-US" }],
  bootstrap: [AppComponent]
})
export class AppModule { }
