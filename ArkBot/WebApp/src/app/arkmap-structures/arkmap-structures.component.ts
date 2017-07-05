import { Component, Input, ViewChild, OnInit, OnDestroy, OnChanges, SimpleChanges, ElementRef, HostListener } from '@angular/core';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/Rx";

import { environment } from '../../environments/environment';
import * as d3 from "d3";

@Component({
  selector: 'arkmap-structures',
  templateUrl: './arkmap-structures.component.html',
  styleUrls: ['./arkmap-structures.component.css']
})
export class ArkmapStructuresComponent implements OnInit, OnDestroy, OnChanges {
    private _structures = new BehaviorSubject<any>(undefined);
    private _structuresSubscription: any;
    @Input()
    set structures(value) {
        this._structures.next(value);
    };

    get structures() {
        return this._structures.getValue();
    }

    @Input() mapName: string;
    @ViewChild('myCanvas') canvasRef: ElementRef;
    /*@HostListener('window:resize')
    onResize(): void {
      this.resize();
    }*/

    width: number;
    height: number;
    img: HTMLImageElement;
    zoom: any;
    points: any[];
    owners: any[];
    types: any[];
    max: number;
    selectedOwner: any;
    keysGetter = Object.keys;

    constructor() {
      this.width = 1024;
      this.height = 1024;
      this.zoom = d3.zoom().scaleExtent([1, 10]);
    }

    ngOnInit() {
      this._structuresSubscription = this._structures.subscribe(value => this.update(value));
    }

    ngOnDestroy() {
      this._structuresSubscription.unsubscribe();
    }

    updateSelection() {
      this.redraw();
    }

    private gridsize = 32;

    update(structures: any) {
      if(structures == undefined) return;

      
      var domain = d3.range(1024);
      var range = d3.range(this.gridsize);

      var scale = d3.scaleQuantile().domain(domain).range(range); 

      var points = d3.range(this.gridsize * this.gridsize).map(i => { 
        let point = {} as any;
        point.i = i;
        point.x = i % this.gridsize;
        point.y = Math.floor(i / this.gridsize)
        point.c = 0;
        //point.owners = {};
        return point;
       });
      var owners = structures.Owners;

      var max = 0;
      for (let point of structures.Structures2) {
        var x = scale(point[0]);
        var y = scale(point[1]);

        var pt = points[x + y * this.gridsize];
        pt.c += 1;
        //if(pt.owners[point[3]] == undefined) pt.owners[point[3]] = 1;
        //else pt.owners[point[3]]++;

        if(owners[point[3]].points == undefined) owners[point[3]].points = { };
        if(owners[point[3]].points[x + y * this.gridsize] == undefined) owners[point[3]].points[x + y * this.gridsize] = { count: 1, structures: {} };
        else owners[point[3]].points[x + y * this.gridsize].count += 1;

        if(owners[point[3]].points[x + y * this.gridsize].structures[point[2]] == undefined) owners[point[3]].points[x + y * this.gridsize].structures[point[2]] = 1;
        else owners[point[3]].points[x + y * this.gridsize].structures[point[2]] += 1;

        if (pt.c > max) max = pt.c;
      }

      for(let owner of owners) {
        owner.pointsNum = Object.keys(owner.points).length;
      }

      owners.sort((c1, c2) => {
        if(Object.keys(c1.points).length > Object.keys(c2.points).length) {
          return -1;
        } else if(Object.keys(c1.points).length < Object.keys(c2.points).length){
            return 1;
        } else {
          return 0; 
        }
      });

      this.points = points;
      this.owners = owners;
      this.types = structures.Types;
      this.max = max;
    }

    imageLoaded(img: HTMLImageElement): void {
      this.img = img;
      this.width = img.naturalWidth;
      this.height = img.naturalHeight;

      //d3.select(this.canvasRef.nativeElement).call(this.zoom.on("zoom", () => this.zoomed()));

      window.setTimeout(() => {this.resize(); this.redraw(); }, 100);
    }

    resize(): void {
      //this.zoom.translateExtent([[0, 0], [this.width, this.height]]);
    }

    zoomed(): void {
      var transform = d3.zoomTransform(this.canvasRef.nativeElement);
      let ctx: CanvasRenderingContext2D = this.canvasRef.nativeElement.getContext('2d');
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.clearRect(0, 0, this.width, this.height);
      ctx.translate(transform.x, transform.y);
      ctx.scale(transform.k, transform.k);
      this.redraw();
    }

    redraw(): void {
      let ctx: CanvasRenderingContext2D = this.canvasRef.nativeElement.getContext('2d');
      
      ctx.drawImage(this.img, 0, 0);

      if(this.structures == undefined) return;

      for (let point of this.points) {
        if(point.c == 0) continue;
        if(this.selectedOwner != undefined && this.selectedOwner.points[point.i] == undefined) continue;

        ctx.beginPath();
        //ctx.arc(point[0], point[1], 7, 0, Math.PI * 2);
        ctx.arc(point.x * this.gridsize + this.gridsize / 2, point.y * this.gridsize + this.gridsize / 2, Math.round(12 * (point.c / this.max)) + 4, 0, Math.PI * 2);
        ctx.fillStyle = 'black';
        ctx.fill();
        ctx.lineWidth = 1;
        ctx.strokeStyle = 'white';
        ctx.stroke();
      }
    }

    ngOnChanges(changes: SimpleChanges) {
      if(this.mapName == null) return;
      
      var img = new Image()
      img.onload = () => this.imageLoaded(img);
      img.src = `${this.getApiBaseUrl()}/map/${this.mapName}`
      if (img.complete) {
        img.onload = null
        this.imageLoaded(img);
      }
    }

    getApiBaseUrl(): string {
      return environment.apiBaseUrl.replace(/\<protocol\>/gi, window.location.protocol).replace(/\<hostname\>/gi, window.location.hostname);
    }
}