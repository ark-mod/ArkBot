import { Injectable } from '@angular/core';
import {
  CanActivate, Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot/*,
  CanActivateChild,
  NavigationExtras*/
} from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { DataService } from './data.service';

@Injectable()
export class AccessControlRouteGuardService implements CanActivate /*, CanActivateChild*/ {
  constructor(private dataService: DataService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | boolean {
    return Observable.fromPromise<string>(this.dataService.getServers().then(e => {
        if (e) {
          let pid = route.params['playerid'];
          return this.dataService.hasFeatureAccess("pages", route.data.name, pid) ? "access" : "noaccess";
        }

        return "connectionerror";
      }).catch(() => {
          return "connectionerror";
      })).map(e => {
        if (e == "noaccess") this.router.navigateByUrl('/accessdenied', { skipLocationChange: true });
        else if (e == "connectionerror") this.router.navigateByUrl('/connectionerror', { skipLocationChange: true });
        return e == "access";
      });
  }
}