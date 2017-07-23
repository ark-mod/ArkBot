import { Component, Output, Input, ViewChild, OnInit, OnDestroy, ElementRef, EventEmitter } from '@angular/core';

@Component({
  selector: 'confirm-button',
  templateUrl: './confirm-button.component.html',
  styleUrls: ['./confirm-button.component.css']
})
export class ConfirmButtonComponent implements OnInit {
  @Output() callback: EventEmitter <string> = new EventEmitter();
  @Input() width: number;
  @ViewChild('confirmButton') confirmButton: ElementRef;
  public confirming: boolean = false;
  private resetTimeout: any;

  constructor() { }

  ngOnInit() {
  }

  onClick(event: any): void {
    if(!this.confirming) {
      this.confirming = true;
      this.resetTimeout = window.setTimeout(() => {
        this.confirming = false;
      }, 5000);
    } else {
      if(event.detail >= 3) {
        window.clearTimeout(this.resetTimeout);
        this.confirming = false;
        this.callback.emit();
      }
    }
  }
}
