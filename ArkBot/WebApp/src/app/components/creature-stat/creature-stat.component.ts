import { Component, Input, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { Observable } from 'rxjs';
import { DataService } from '../../data.service';
import { fromHsvToRgb } from '../../utils'

@Component({
  selector: 'creature-stat',
  templateUrl: './creature-stat.component.html',
  styleUrls: ['./creature-stat.component.css']
})
export class CreatureStatComponent implements OnInit, OnDestroy {
    @Input() value: number;
    @Input() top: number;
    @Input() inheritedBest: number;

    public color: string;
    
    private theme: string = undefined; 
    private theme$: Observable<string>;
    private themeSubscription: any;

    constructor(private dataService: DataService) { }

    ngOnInit() {
        this.theme$ = this.dataService.Theme;
        this.themeSubscription = this.theme$.subscribe(theme => { this.theme = theme; this.colorCalculation(); });
    }

    ngOnDestroy() {
        this.themeSubscription.unsubscribe();
    }

    ngOnChanges(changes: SimpleChanges) {
        this.colorCalculation();
    }

    colorCalculation() {
        let max = this.top;
        let current = this.value;
    
        let value = current;
        var f = max > 0 ? (value / max) : 0.0;
        var h = Math.pow(f, 2) * (1 / 3.0);
        var s = 0.5 + (value == max ? 0.4 : 0);
        if (s > 1) s = 1;
    
        let rgb = this.theme != 'light' ? fromHsvToRgb(h, s + 0.1, (value == max ? 0.7 : 0.4)) : fromHsvToRgb(h, s, 1.0);
    
        this.color = '#' + (((1 << 24) + (rgb.r << 16) + (rgb.g << 8) + rgb.b).toString(16).substr(1));
      }
}
