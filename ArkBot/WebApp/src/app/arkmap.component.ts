import { Component, Input, ViewChild, OnInit, OnChanges, SimpleChanges, ElementRef, HostListener } from '@angular/core';

import { environment } from '../environments/environment';
import * as d3 from "d3";

declare var config: any;

@Component({
  selector: 'arkmap',
  template: `<canvas #myCanvas [width]="width" [height]="height" style="width: 100%;"></canvas><div #mapTooltip class="map-tooltip theme-d1 border-theme">{{tooltipLabel}}</div>`,
  styles: ['.map-tooltip { visibility: hidden; position: fixed; left: -9999px; top: -9999px; font-size: 15px; pointer-events: none; padding: 5px 10px; border-width: 2px; border-style: solid; opacity: 0; transition: opacity 0.5s ease-in-out; }']
})
export class ArkMapComponent implements OnInit, OnChanges {
    @Input() mapName: string;
    @Input() points: any[];
    @ViewChild('myCanvas') canvasRef: ElementRef;
    @ViewChild('mapTooltip') mapTooltip: ElementRef;
    /*@HostListener('window:resize')
    onResize(): void {
      this.resize();
    }*/

    width: number;
    height: number;
    img: HTMLImageElement;
    zoom: any;
    tooltipLabel: string;

    constructor() {
      this.width = 1024;
      this.height = 1024;
      this.zoom = d3.zoom().scaleExtent([1, 10]);
    }
    ngOnInit() {
      this.canvasRef.nativeElement.addEventListener('mousemove', e => {
        if (this.points == null) return;

        const rect = this.canvasRef.nativeElement.getBoundingClientRect();
        let x = (e.clientX - rect.left) / (rect.right - rect.left) * this.width;
        let y = (e.clientY - rect.top) / (rect.bottom - rect.top) * this.height;

        let hideTooltip = true;
        for (let point of this.points) {
          if ((point.label || undefined) == undefined) continue;

          if (x >= point.x - 6 && x <= point.x + 6 && y >= point.y - 6 && y <= point.y + 6) {
            this.tooltipLabel = point.label;
            if ((this.mapTooltip.nativeElement.style.visibility || undefined) == undefined) {
              this.mapTooltip.nativeElement.style.visibility = 'visible';
              this.mapTooltip.nativeElement.style.opacity = 1;
            }
            this.mapTooltip.nativeElement.style.left = (Math.round(e.clientX) + 5) + 'px';
            this.mapTooltip.nativeElement.style.top = (Math.round(e.clientY) - this.mapTooltip.nativeElement.clientHeight - 5) + 'px';
            hideTooltip = false;
            break;
          }
        }

        if (hideTooltip && (this.mapTooltip.nativeElement.style.visibility || undefined) != undefined) {
          this.mapTooltip.nativeElement.style.visibility = null;
          this.mapTooltip.nativeElement.style.opacity = null;
          this.mapTooltip.nativeElement.style.left = null;
          this.mapTooltip.nativeElement.style.top = null;
        }
      });
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