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
import { ServerComponent } from './server/server.component';
import { ServerListComponent } from './server-list/server-list.component';

import { ArkMapComponent } from './arkmap.component';
import { HttpService } from './http.service';
import { MessageService } from './message.service';
import { DataService } from './data.service';
import { SanitizeStylePipe } from './sanitize-style.pipe';
import { ClickOutsideDirective } from './clickOutside.directive';

const appRoutes: Routes = [
  {
    path: 'player/:id', 
    component: PlayerComponent,
    data: { title: 'Player' }
  },
  {
    path: 'server/:id', 
    component: ServerComponent,
    data: { title: 'Server' }
  },
  {
    path: 'servers',
    component: ServerListComponent,
    data: { title: 'Server List' }
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
    ServerComponent
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
