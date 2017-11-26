import { Component, OnInit, OnDestroy, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Observable, Subscription, BehaviorSubject } from 'rxjs/Rx';
import * as moment from 'moment'

@Component({
  selector: 'relative-time',
  template: `<span>{{_str}}</span>`,
  styleUrls: ['./relative-time.component.css']
  , changeDetection: ChangeDetectionStrategy.OnPush
})
export class RelativeTimeComponent implements OnInit, OnDestroy {
    private _counter: Observable<number>;
    private _time = new BehaviorSubject<string>(undefined);
    private _timeSubscription: Subscription;
    private _counterSubscription: Subscription;
    _str: string;

    @Input()
    set time(value) {
        this._time.next(value);
    };

    get time() {
        return this._time.getValue();
    }

    constructor(private ref: ChangeDetectorRef) { }

    ngOnInit() {
        this._timeSubscription = this._time.subscribe(value => {
            this.update();
          });
  
          this._counter = Observable.interval(1000).map(x => x);
          this._counterSubscription = this._counter.subscribe(x => this.update());
      }
  
      ngOnDestroy(): void {
          this._timeSubscription.unsubscribe();
          this._counterSubscription.unsubscribe();
      }

    update() {
        let newValue = this.toRelativeDate(this.time);
        let oldValue = this._str;
        if (newValue != oldValue) 
        {
            this._str = newValue;
            this.ref.markForCheck();
        }
    }

    toRelativeDate(datejson: string): string {
        return moment(new Date(datejson)).fromNow();
    }
}
