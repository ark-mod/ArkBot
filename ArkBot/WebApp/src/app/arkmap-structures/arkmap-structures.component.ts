import { Component, Input, ViewChild, OnInit, OnDestroy, OnChanges, SimpleChanges, ElementRef, HostListener, ViewEncapsulation, NgZone } from '@angular/core';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/Rx";

import { DataService } from '../data.service';
import { HttpService } from '../http.service';

import { environment } from '../../environments/environment';
import * as d3 from "d3";
import * as moment from 'moment'

declare var config: any;

@Component({
  selector: 'arkmap-structures',
  templateUrl: './arkmap-structures.component.html',
  styleUrls: ['./arkmap-structures.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class ArkmapStructuresComponent implements OnInit, OnDestroy, OnChanges {
    public currentArea: any;
    public currentOwner: any;
    public modalInfo: any;
    private _structures = new BehaviorSubject<any>(undefined);
    private _structuresSubscription: any;
    @Input()
    set structures(value) {
        this._structures.next(value);
    };

    get structures() {
        return this._structures.getValue();
    }

    @Input() serverKey: string;
    @Input() mapName: string;
    //@ViewChild('myCanvas') canvasRef: ElementRef;
    //@ViewChild('mySvg') svgRef: ElementRef;
    @ViewChild('map') mapContainer: ElementRef;
    @ViewChild('contextMenu') contextMenu: ElementRef;
    /*@HostListener('window:resize')
    onResize(): void {
      this.resize();
    }*/

    width: number;
    height: number;
    img: HTMLImageElement;
    zoom: any;
    ownersSorted: any[];
    max: number;
    selectedOwner: any;
    keysGetter = Object.keys;
    map: any;
    ownerSortField: string = "locations";
    ownerSortFunctions: any = {
      "locations": (o1, o2) => {
            if(o1.AreaCount > o2.AreaCount) {
              return -1;
            } else if(o1.AreaCount < o2.AreaCount){
                return 1;
            } else {
              if(o1.StructureCount > o2.StructureCount) {
                return -1;
              } else if(o1.StructureCount < o2.StructureCount){
                  return 1;
              } else {
                return 0; 
              }
            }
        },
      "structures": (o1, o2) => {
            if(o1.StructureCount > o2.StructureCount) {
              return -1;
            } else if(o1.StructureCount < o2.StructureCount){
                return 1;
            } else {
              return 0;
            }
        },
      "lastactive": (o1, o2) => {
            if(o1.LastActiveTime < o2.LastActiveTime || o1.LastActiveTime == undefined) {
              return -1;
            } else if(o1.LastActiveTime > o2.LastActiveTime  || o2.LastActiveTime == undefined){
                return 1;
            } else {
                return 0;
            }
        }
    };

    constructor(
      public dataService: DataService,
      private httpService: HttpService,
      private zone: NgZone) {
      this.width = 1024;
      this.height = 1024;
      this.zoom = d3.zoom().scaleExtent([1, 8]);
    }

    ngOnInit() {
      this._structuresSubscription = this._structures.subscribe(value => this.update(value));
      let element = this.mapContainer.nativeElement;
      this.map = {};
      this.map.canvas = d3.select(element)
        .append('canvas')
        .attr('width', 1024)
        .attr('height', 1024)
        .node().getContext('2d');
      this.map.svg = d3.select(element)
        .append('svg')
        //.attr('width', 1024)
        //.attr('height', 1024)
        .attr('viewBox', '0 0 1024 1024')
        .attr('preserveAspectRatio', 'xMidYMid')
        .append('g')
        .on("contextmenu", (d, e) => {
          d3.event.preventDefault();
        });

      /*this.map.tooltip = d3.select(document)
        .append("div")
        .style("position", "absolute")
        .style("z-index", "20")
        .style("visibility", "hidden")
        .text("");*/

      this.map.svg.append('rect')
        .attr('class', 'overlay')
        .attr('width', 1024)
        .attr('height', 1024);

        this.map.x =
        d3.scaleLinear()
          .domain([0, 1024])
          .range([0, 1024]);

      this.map.y =
        d3.scaleLinear()
          .domain([0, 1024])
          .range([0, 1024]);

      d3.select(element).call(this.zoom.on('zoom', () => {
        this.hideContextMenu();
        this.redraw();
      })).on('wheel.zoom', null);

      if(this.structures) this.updateMap();
    }

    ngOnDestroy() {
      this._structuresSubscription.unsubscribe();
    }

    zoomIn(): void {
      this.zoom.scaleBy(d3.select(this.mapContainer.nativeElement), 1.2);
    }

    zoomOut(): void {
      this.zoom.scaleBy(d3.select(this.mapContainer.nativeElement), 0.8);
    }

    updateSelection() {
      this.map.svg.circle.attr("display", (d) => {
        var owner = this.structures.Owners[d.OwnerId];
        return !d.Removed && !owner.Removed && (!this.selectedOwner || (this.selectedOwner && this.selectedOwner.Id == d.OwnerId)) ? 'block' : 'none'; 
      });

      this.redraw();
    }

    update(structures: any) {
      this.sortOwners(structures);

      if(this.map) this.updateMap();
    }

    sortOwners(structures: any) {
      let sortFunc = this.ownerSortFunctions[this.ownerSortField];

      if(structures) {
        var owners = structures.Owners.slice();
        owners.sort(sortFunc);
        this.ownersSorted = owners;
      } else this.ownersSorted = undefined;
    }

    updateMap() {
      this.map.svg.nodes = this.structures.Areas;

      this.map.svg.draw = () => {
          this.map.svg.circle = this.map.svg.selectAll('circle')
          .data(this.map.svg.nodes).enter()
          .append('circle')
            .attr('r', function(d) { return d.RadiusPx < 2 ? 2 : d.RadiusPx; })
            //.attr('cx', function(d) { return d.TopoMapX; })
            //.attr('cy', function(d) { return d.TopoMapY; })
            .attr('fill', 'transparent')
            .attr('stroke', (d) => {
              var owner = this.structures.Owners[d.OwnerId];
              var active = owner.LastActiveTime ? moment(new Date(owner.LastActiveTime)).isSameOrAfter(moment().subtract(28, 'day')) : false;
              return active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 'magenta' : 'red';
            })
            .attr('stroke-width', (d) => {
              var owner = this.structures.Owners[d.OwnerId];
              var active = owner.LastActiveTime ? moment(new Date(owner.LastActiveTime)).isSameOrAfter(moment().subtract(28, 'day')) : false;
              return active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 3 : 2;
            })
            .attr('transform', this.map.svg.transform);

            /*circle.on("mouseover", (d) => {
              var owner = this.structures.Owners[d.OwnerId];
              this.map.tooltip.text(owner.Name + ": " + d.StructureCount + " structures\n" 
                + d.Latitude + ", " + d.Longitude);
              return this.map.tooltip.style("visibility", "visible");
            })
              .on("mousemove", (d) => {
                let p = d3.mouse(d3.event.currentTarget); 
                console.log(p);
                return this.map.tooltip.style("top", x.invert(p[1])+"px").style("left",y.invert(p[0])+"px");})
              .on("mouseout", (d) => {return this.map.tooltip.style("visibility", "hidden");});*/

              this.map.svg.circle.on("click", (d) => {
                d3.event.preventDefault();
                let p = <any> { };
                p.x = d3.event.pageX;
                p.y = d3.event.pageY;
                
                this.showAreaModal(d, p);
              });

          this.map.svg.circle.append("svg:title")
            .text((d) => {
              var owner = this.structures.Owners[d.OwnerId];
              var lastActiveTime = owner.LastActiveTime ? moment(new Date(owner.LastActiveTime)).fromNow() : null;
              return owner.Name + ": " + d.StructureCount + " structures\n" 
                + "Coords: " + d.Latitude + ", " + d.Longitude + "\n"
                + (lastActiveTime ? "Last active: " + lastActiveTime + "\n" : "")
                + "---\n"
                + d.Structures.map((s) => {
                  let type = this.structures.Types[s.t];
                  return s.c + ": " + (type ? type.Name : s.t);
                }).join("\n"); 
            });
      };
      this.map.svg.draw();

      this.map.svg.transform = (d) => {
          return 'translate(' + this.map.x( d.TopoMapX ) + ',' + this.map.y( d.TopoMapY ) + ')';
        };

        this.map.svg.circle.attr('transform', this.map.svg.transform);
    }

    imageLoaded(img: HTMLImageElement): void {
      this.img = img;
      this.width = img ? img.naturalWidth : 1024;
      this.height = img ? img.naturalHeight : 1024;

      //d3.select(this.canvasRef.nativeElement).call(this.zoom.on("zoom", () => this.zoomed()));

      window.setTimeout(() => {this.resize(); this.redraw(); }, 100);
    }

    resize(): void {
      //this.zoom.translateExtent([[0, 0], [this.width, this.height]]);
    }

    /*zoomed(): void {
      var transform = d3.zoomTransform(this.canvasRef.nativeElement);
      let ctx: CanvasRenderingContext2D = this.canvasRef.nativeElement.getContext('2d');
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.clearRect(0, 0, this.width, this.height);
      ctx.translate(transform.x, transform.y);
      ctx.scale(transform.k, transform.k);
      this.redraw();
    }*/

    private prevTransformK: number;

    redraw(): void {
      var transform = d3.zoomTransform(this.mapContainer.nativeElement);

      this.map.svg.attr("transform", "translate(" + transform.x + "," + transform.y + ") scale(" + transform.k + ")");

      if(transform.k != this.prevTransformK) {
        this.map.svg.circle.attr("stroke-width", (d) => {
          var owner = this.structures.Owners[d.OwnerId];
          var active = owner.LastActiveTime ? moment(new Date(owner.LastActiveTime)).isSameOrAfter(moment().subtract(28, 'day')) : false;
          return (active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 3 : 2)/transform.k; 
        });
      }

      let ctx: CanvasRenderingContext2D = this.map.canvas;

      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.clearRect(0, 0, 1024, 1024);
      ctx.translate(transform.x, transform.y);
      ctx.scale(transform.k, transform.k);
      
      if (this.img) ctx.drawImage(this.img, 0, 0);

      this.prevTransformK = transform.k;
    }

    ngOnChanges(changes: SimpleChanges) {
      if(this.mapName == null) return;
      
      var img = new Image()
      img.onload = () => this.imageLoaded(img);
      img.onerror = () => this.imageLoaded(undefined);
      img.src = !environment.demo ? `${this.getApiBaseUrl()}/map/${this.mapName}` : 'assets/demo/Ragnarok.jpg';
      if (img.complete) {
        img.onload = null;
        img.onerror = null;
        this.imageLoaded(img);
      }
    }

    getApiBaseUrl(): string {
      return environment.apiBaseUrl
        .replace(/\<protocol\>/gi, window.location.protocol)
        .replace(/\<hostname\>/gi, window.location.hostname)
        .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    }

    reset() {
      this.selectedOwner = undefined;
      this.updateSelection();
      //this.zoom.scaleTo(d3.select(this.mapContainer.nativeElement), 1.0);
      //this.zoom.translateTo(d3.select(this.mapContainer.nativeElement), 0, 0); //not working
    }

    setSelectedOwner(owner: any) {
      this.selectedOwner = owner;
      this.updateSelection();
    }

    setOwnerSort(field: string) : void {
      this.ownerSortField = field;
      this.sortOwners(this.structures);
    }

    showAreaModal(area: any, point: any): void {
      this.currentArea = area;
      this.currentOwner = this.structures.Owners[area.OwnerId];

      let cm = d3.select(this.contextMenu.nativeElement);
      cm.style("display", "block");

      if(d3.event) d3.event.stopPropagation();
    }

    showOwnerModal(event: any, owner: any): void {
      this.currentOwner = owner;

      let cm = d3.select(this.contextMenu.nativeElement);
      cm.style("display", "block");

      event.stopPropagation();
    }

    showInfoModal(header: string, message: string): void {
      let modalInfo = <any> {};
      modalInfo.Header = header;
      modalInfo.Message = message;
      this.modalInfo = modalInfo;

      let cm = d3.select(this.contextMenu.nativeElement);
      cm.style("display", "block");

      if(d3.event) d3.event.stopPropagation();
    }

    hideContextMenu(): void {
      let cm = d3.select(this.contextMenu.nativeElement);
      cm.style("display", "none");

      this.currentArea = undefined;
      this.currentOwner = undefined;
      this.modalInfo = undefined;
    }

    destroyCurrentArea(event: string): void {
      this.httpService.adminDestroyStructuresForTeamIdAtPosition(this.serverKey, this.currentOwner.OwnerId, this.currentArea.X, this.currentArea.Y, +this.currentArea.RadiusUu + 1000 /* 10m */, 1)
      .then(response => {
          this.currentArea.Removed = true;
          this.currentOwner.AreaCount -= 1;
          //if(response.DestroyedStructureCount) this.currentOwner.StructureCount -= response.DestroyedStructureCount; //this does not work well because of server not saving which areas have been demolished inbetween updates
          this.currentOwner.StructureCount -= this.currentArea.StructureCount;

          this.hideContextMenu();
          this.showInfoModal("Action Successfull!", response.Message);

          this.updateSelection();
        })
        .catch(error => {
          this.hideContextMenu();
          let json = error && error._body ? JSON.parse(error._body) : undefined;
          this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    }

    destroyAllStructuresForTeam(event: string): void {
      this.httpService.adminDestroyAllStructuresForTeamId(this.serverKey, this.currentOwner.OwnerId)
      .then(response => {
          this.currentOwner.Removed = true;
          this.currentOwner.AreaCount = 0;
          this.currentOwner.StructureCount = 0;
          //if(response.DestroyedStructureCount) owner.StructureCount -= response.DestroyedStructureCount; //this does not work well because of server not saving which areas have been demolished inbetween updates

          this.hideContextMenu();
          this.showInfoModal("Action Successfull!", response.Message);

          this.updateSelection();
        })
        .catch(error => {
          this.hideContextMenu();
          let json = error && error._body ? JSON.parse(error._body) : undefined;
          this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    }

    destroyDinosForTeam(event: string): void {
      this.httpService.adminDestroyDinosForTeamId(this.serverKey, this.currentOwner.OwnerId)
      .then(response => {
          this.currentOwner.CreatureCount = 0;

          this.hideContextMenu();
          this.showInfoModal("Action Successfull!", response.Message);

          this.updateSelection();
        })
        .catch(error => {
          this.hideContextMenu();
          let json = error && error._body ? JSON.parse(error._body) : undefined;
          this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    }

    saveWorld(event: string): void {
      this.httpService.adminSaveWorld(this.serverKey)
      .then(response => {
          this.hideContextMenu();
          this.showInfoModal("Action Successfull!", response.Message);
        })
        .catch(error => {
          this.hideContextMenu();
          let json = error && error._body ? JSON.parse(error._body) : undefined;
          this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    }
}