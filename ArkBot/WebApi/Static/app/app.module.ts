import { NgModule, LOCALE_ID } from '@angular/core';
import { BrowserModule }  from '@angular/platform-browser';
import { HttpModule }    from '@angular/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { HttpService } from './http.service';
import { SanitizeStylePipe } from './sanitize-style.pipe';

@NgModule({
  imports: [
    BrowserModule,
    HttpModule,
    FormsModule
  ],
  declarations: [
    AppComponent,
    SanitizeStylePipe
  ],
  providers: [ HttpService, { provide: LOCALE_ID, useValue: "en-US" } ],
  bootstrap: [ AppComponent ]
})
export class AppModule { }