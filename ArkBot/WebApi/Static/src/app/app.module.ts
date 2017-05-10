import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER , LOCALE_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { SimpleNotificationsModule } from 'angular2-notifications';

import { AppComponent } from './app.component';

import { ArkMapComponent } from './arkmap.component';
import { HttpService } from './http.service';
import { MessageService } from './message.service';
import { SanitizeStylePipe } from './sanitize-style.pipe';
import { ClickOutsideDirective } from './clickOutside.directive';

@NgModule({
  declarations: [
    AppComponent,
    ArkMapComponent,
    SanitizeStylePipe,
    ClickOutsideDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    BrowserAnimationsModule,
    SimpleNotificationsModule.forRoot()
  ],
  providers: [
    HttpService, 
    MessageService, 
    { provide: LOCALE_ID, useValue: "en-US" }],
  bootstrap: [AppComponent]
})
export class AppModule { }
