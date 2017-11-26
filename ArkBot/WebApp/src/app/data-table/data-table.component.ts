import { Component, OnInit, ContentChildren, ContentChild, QueryList, Input, ViewEncapsulation, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { DataTableColumnDirective } from './columns/column.directive';
import { DataTableModeDirective } from './modes/mode.directive';

import { Observable, Subscription, Subject, BehaviorSubject } from 'rxjs/Rx';
import 'rxjs/add/observable/combineLatest';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/switchMap';
import 'rxjs/add/operator/filter';

@Component({
  selector: 'ark-data-table',
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.css']
  , changeDetection: ChangeDetectionStrategy.OnPush
  , encapsulation: ViewEncapsulation.None
})
export class DataTableComponent implements OnInit {
  _modeEnabledSubscriptions: Subscription[] = [];
  _columnTemplates: any[];
  _rows: any[];
  _rows$: Observable<any[]> = Observable.of<any[]>([]);
  _orderByColumnKey: BehaviorSubject<string>;
  _sort: Subject<string> = new Subject<string>();
  _trackByProp: string;
  _modes: any[];
  _currentMode: string;
  _fromRow: number = 0;
  _numRows: number = 25;
  trackByRow: any;
  _enabledColumnsForMode: any = {};
  numEnabledModes: Observable<number>;
  _viewOptions: any[] = [
    { 'value': 25, 'text': '25' },
    { 'value': 50, 'text': '50' },
    { 'value': 100, 'text': '100' },
    { 'value': 250, 'text': '250' },
    { 'value': 500, 'text': '500' },
    { 'value': 1000, 'text': '1000' },
    { 'value': 1000000, 'text': 'All' }
  ];

  constructor(private ref: ChangeDetectorRef) { }

  ngOnInit() {
    this._orderByColumnKey = new BehaviorSubject<string>(undefined)
    
        this._rows$ = this._orderByColumnKey
        .switchMap(key => {
          return key ? 
          Observable.of<any[]>(this.sortData(this._rows, key)) 
          : Observable.of<any[]>(this._rows);
        })
        .catch(error => {
          console.log(`Error in component ... ${error}`);
          return Observable.of<any[]>(this._rows);
        });
  }

  @ContentChildren(DataTableModeDirective)
  set modeTemplates(val: QueryList<DataTableModeDirective>) {
    if (!val) return;

    const arr = val.toArray();
    if (!arr.length) return;

    const result: any[] = [];
    
    for(const temp of arr) {
      const mode: any = {};
  
      const props = Object.getOwnPropertyNames(temp);
      
      for(const prop of props) {
        mode[prop] = temp[prop];
      }

      result.push(mode);
    }

    this._modes = result;

    this.numEnabledModes = Observable.combineLatest(result.map<Observable<boolean>>(r => r.enabled))
      .map(r => r.map<number>(v => v ? 1 : 0).reduce((s, v) => s + v));

    if (this._modes.length > 0) this._currentMode = this._modes[0].key;
  }

  @ContentChildren(DataTableColumnDirective)
  set columnTemplates(val: QueryList<DataTableColumnDirective>) {
    if (!val) return;

    const arr = val.toArray();
    if (!arr.length) return;

    const result: any[] = [];
    
    for(const temp of arr) {
      const col: any = {};
  
      const props = Object.getOwnPropertyNames(temp);
      for(const prop of props) {
        col[prop] = temp[prop];
      }
  
      if(temp.headerTemplate) {
        col.headerTemplate = temp.headerTemplate;
      }
  
      if(temp.cellTemplate) {
        col.cellTemplate = temp.cellTemplate;
      }
  
      result.push(col);
    }

    this._columnTemplates = result;
    this.orderBy(this.orderByColumn);
  }

  @Input() set rows(val: any) {
    this._rows = val;
  }

  @Input() set trackByProp(val: string) {
    this._trackByProp = val;
    this.trackByRow = (index, row) => {
      return row[this._trackByProp];
    };
  }

  @Input() sortFunctions: any;
  @Input() orderByColumn: string;

  isCurrentMode(key: string): boolean {
    return key === this._currentMode;
  }

  setCurrentMode(key: string): void {
    this._currentMode = key;
  }

  showColumn(columnKey: string) {
    if ((this._enabledColumnsForMode[this._currentMode] || (this._enabledColumnsForMode[this._currentMode] = {}))[columnKey] === true) return true;

    if (!this._currentMode) return false;
    let currentMode = this._modes.find(x => x.key == this._currentMode);
    if (!currentMode) return false;

    let enabled = currentMode.ColumnKeys.find(x => x == columnKey) != undefined;
    this._enabledColumnsForMode[this._currentMode][columnKey] = enabled;

    return enabled;
  }

  trackByKey(index: number, data: any) : string {
    return data.key;
  }

  orderBy(columnKey: string, event: any = undefined) {
    let column = this._columnTemplates.find(x => x.key == columnKey && (event == undefined || x.orderBy == true));
    if (!column) return;

    this._orderByColumnKey.next((this._orderByColumnKey.getValue() == columnKey ? '-' : '') + columnKey);
  }

  sortData(rows: any[], columnKey: string): any[] {
    let asc = columnKey[0] != '-';
    let column = this._columnTemplates.find(x => x.key == columnKey.substr(asc ? 0 : 1));

    
    let sortFunc = this.sortFunctions[column.key.replace(/^\-/, "")];
    let alts = (column.thenBy || '').split(',').filter(k => this.sortFunctions.hasOwnProperty(k.replace(/^\-/, ""))).map(k => {
      let a = <any> {};
      a.asc = k[0] != '-';
      a.sortFunc = this.sortFunctions[k.replace(/^\-/, "")];
      return a;
    });

    return rows.slice().sort((o1, o2) => {
      let r = sortFunc(o1, o2, asc);
      if(r == 0) {
        for (let alt of alts) {
          r = alt.sortFunc(o1, o2, alt.asc);

          if (r != 0) break;
        }
      }

      return r;
    });
  }

  setViewOffset(offset: number) {
    let newOffset = offset;
    if (newOffset < 0) newOffset = 0;
    if (newOffset >= this._rows.length) newOffset = this._rows.length - 1;

    this._fromRow = parseInt("" + newOffset);

    this.ref.markForCheck();
  }

  setViewOffsetRelative(offset: number) {
    this.setViewOffset(this._fromRow + offset);
  }

  setFirstPage() {
    if (!this.isFirstPage()) this.setViewOffset(0);
  }

  setPrevPage() {
    if (!this.isFirstPage()) this.setViewOffsetRelative(-this._numRows);
  }

  setNextPage() {
    if (!this.isLastPage()) this.setViewOffsetRelative(this._numRows);
  }

  setLastPage() {
    if (!this.isLastPage()) this.setViewOffset(this._rows.length - this._numRows);
  }

  isFirstPage(): boolean {
    return this._fromRow <= 0;
  }

  isLastPage(): boolean {
    return this._fromRow >= this._rows.length - this._numRows;
  }

  setViewLimit(limit: number) {
    this._numRows = parseInt("" + (limit > 0 ? limit : 1000000));
    this.ref.markForCheck();
  }

  getLastRowOffset(): number {
    let last = this._fromRow + this._numRows;
    return last > this._rows.length ? this._rows.length : last;
  }
}