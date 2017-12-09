import { Component, ElementRef, OnInit, OnDestroy, Input } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { BehaviorSubject } from "rxjs/Rx";
import { environment } from '../../environments/environment';
import * as moment from 'moment'

@Component({
  selector: 'timer',
  template: `<span *ngIf="!_ready">{{_str}}</span><button *ngIf="_ready" style="padding: 4px 8px;" class="w3-button w3-small" [ngClass]="{'theme-d1': state._completed, 'theme-l2': !state._completed && _ready, 'theme-hover': !state._completed && _ready}" (click)="state._completed = !state._completed">{{(state._completed ? "Completed" : "Ready")}}</button>`,
  styleUrls: ['./timer.component.css']
})
export class TimerComponent implements OnInit, OnDestroy {
    public _ready: boolean = false;
    //private _completed: boolean = false;
    private _wasExpired: boolean = false;
    private _notificationSent: boolean = false;
    private _diff: any;
    private _counter: Observable<number>;
    private _counterSubscription: Subscription;
    private _str: string;
    private _loadedAt: any;

    @Input() state: any;

    private _time = new BehaviorSubject<string>(undefined);
    private _timeSubscription: any;
    @Input()
    set time(value) {
        this._time.next(value);
    };

    get time() {
        return this._time.getValue();
    }

    private _notification = new BehaviorSubject<boolean>(undefined);
    private _notificationSubscription: any;
    @Input()
    set notification(value) {
        this._notification.next(value);
    };

    get notification() {
        return this._notification.getValue();
    }

    constructor() {
        this._loadedAt = moment();
    }

    updateDiff(initialTime) {
      //console.log("initialTime: " + initialTime);
      if (initialTime) {
        if (!environment.demo) this._wasExpired = moment(new Date(initialTime)).diff(moment()) <= 0;
        else this._wasExpired = moment(new Date(initialTime)).diff(moment(new Date(environment.demoDate))) - moment().diff(this._loadedAt) <= 0;
        this._notificationSent = false;
        this._str = undefined;
        this._ready = this._wasExpired;
        if (!this._wasExpired && this.state._completed == true) this.state._completed = false;
      }

      if (!environment.demo) this._diff = initialTime || this.time ? moment.duration(moment(new Date(initialTime || this.time)).diff(moment())) : undefined;
      else this._diff = initialTime || this.time ? moment.duration(moment(new Date(initialTime || this.time)).diff(moment(new Date(environment.demoDate))) - moment().diff(this._loadedAt)) : undefined;
    }
    
    update() {
        //console.log("diff: " + this._diff + ", notificationSent: " + this._notificationSent + ", notification: " + this.notification + ", wasExpired: " + this._wasExpired);
        if (!this._diff) return "";
        if (this._diff.asMilliseconds() <= 0) {
          if (!this._notificationSent) {
            if (this.notification && this.state.imprintNotifications && !this._wasExpired) {
              var audio = new Audio('assets/Alarm01.mp3');
              audio.play();
            }
            this._ready = true;
            //this.state._completed = false;
          }
          this._notificationSent = true;

          this._str = undefined;
          return;
        }

        let seconds = this._diff.seconds();
        let minutes = this._diff.minutes();
        let hours = this._diff.hours();
        let days = Math.floor(this._diff.asDays());

        let components = [];
        if (days > 0) components.push(days + 'd');
        if (days > 0 || hours > 0) components.push(hours + 'h');
        if (days > 0 || hours > 0 || minutes > 0) components.push(minutes + 'm');
        components.push(seconds + 's');

        this._str = components.join(' ');
        this._ready = false;
        this.state._completed = false;
    }


    ngOnInit() {
      this._timeSubscription = this._time.subscribe(value => {
          this.updateDiff(value);
          this.update();
        });

      this._notificationSubscription = this._notification.subscribe(value => {
      });

        this._counter = Observable.interval(1000).map((x) => {
            this.updateDiff(undefined);
            return x;
        });

        this._counterSubscription = this._counter.subscribe((x) => this.update());
    }

    ngOnDestroy(): void {
        this._timeSubscription.unsubscribe();
        this._notificationSubscription.unsubscribe();
        this._counterSubscription.unsubscribe();
    }
}
