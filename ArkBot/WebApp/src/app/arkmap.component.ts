import { Component, Input, ViewChild, OnChanges, SimpleChanges, ElementRef, HostListener } from '@angular/core';

import { environment } from '../environments/environment';
import * as d3 from "d3";

declare var config: any;

@Component({
  selector: 'arkmap',
  template: `<canvas #myCanvas [width]="width" [height]="height" style="width: 100%;"></canvas>`
})
export class ArkMapComponent implements OnChanges {
    @Input() mapName: string;
    @Input() points: any[];
    @ViewChild('myCanvas') canvasRef: ElementRef;
    /*@HostListener('window:resize')
    onResize(): void {
      this.resize();
    }*/

    width: number;
    height: number;
    img: HTMLImageElement;
    zoom: any;

    constructor() {
      this.width = 1024;
      this.height = 1024;
      this.zoom = d3.zoom().scaleExtent([1, 10]);
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

      if(this.points == null) return;

      for (let point of this.points) {
        ctx.beginPath();
        ctx.arc(point.x, point.y, 7, 0, Math.PI * 2);
        ctx.fillStyle = 'black';
        ctx.fill();
        ctx.lineWidth = 2;
        ctx.strokeStyle = 'white';
        ctx.stroke();
      }
    }

    ngOnChanges(changes: SimpleChanges) {
      if(this.mapName == null) return;
      
      var img = new Image()
      img.onload = () => this.imageLoaded(img);
      img.src = !environment.demo ? `${this.getApiBaseUrl()}/map/${this.mapName}` : 'assets/demo/Ragnarok.jpg';
      if (img.complete) {
        img.onload = null
        this.imageLoaded(img);
      }
    }

    getApiBaseUrl(): string {
      return environment.apiBaseUrl
        .replace(/\<protocol\>/gi, window.location.protocol)
        .replace(/\<hostname\>/gi, window.location.hostname)
        .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    }
}