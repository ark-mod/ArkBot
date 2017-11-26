import { Directive, Input } from '@angular/core';
import { Observable, /*BehaviorSubject*/ } from "rxjs/Rx";

@Directive({ selector: 'ark-dt-mode' })
export class DataTableModeDirective {
    //_enabled = new BehaviorSubject<boolean>(undefined);
    //_enabledSubscription: any;
    _columnKeys: string;
    ColumnKeys: string[];

    @Input() key: string;
    @Input() name: string;

    @Input() enabled: Observable<boolean> = Observable.of(true);

    @Input()
    set columnKeys(val: string) {
        this._columnKeys = val;
        this.ColumnKeys = this._columnKeys.split(',')
    };

    /*@Input()
    set enabled(val) {
        this._enabled.next(val);
    };

    get enabled() {
        return this._enabled.getValue();
    }*/
}