import { Injectable, Inject } from '@angular/core';
import { EventManager } from '@angular/platform-browser';
import { Observable } from 'rxjs/Observable';
import { DOCUMENT } from "@angular/common";

type Options = {
  element: any;
  description: string | undefined;
  keys: string;
}

@Injectable()
export class HotkeysService {
  private hotkeys = new Map();
  private defaults: Partial<Options> = { element: this.doc }

  constructor(
    @Inject(DOCUMENT) private doc: Document,
    private eventManager: EventManager) {}

  add(options: Partial<Options>): Observable<any> {
    const merged = { ...this.defaults, ...options };
    const event = `keydown.${merged.keys}`;

    merged.description && this.hotkeys.set(merged.keys, merged.description);

    return new Observable(observer => {
      const handler = (e) => {
        e.preventDefault()
        observer.next(e);
      };

      const dispose = this.eventManager.addEventListener(merged.element, event, handler);

      return () => {
        dispose();
        this.hotkeys.delete(merged.keys);
      };
    })
  }
}