import { Component, Input, ViewChild, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'arkmap',
  template: `<canvas #myCanvas [width]="width" [height]="height" style="width: 100%;"></canvas>`
})
export class ArkMapComponent implements OnChanges {
    @Input() mapName: string;
    @Input() points: any[];
    @ViewChild('myCanvas') canvasRef: ElementRef;

    width: number;
    height: number;
    img: Image;

    imageLoaded(img: Image): void {
      this.img = img;
      this.width = img.naturalWidth;
      this.height = img.naturalHeight;
      window.setTimeout(() => this.redraw(), 100);
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
      img.src = "/api/map/" + this.mapName
      if (img.complete) {
        img.onload = null
        this.imageLoaded(img);
      }
    }
}