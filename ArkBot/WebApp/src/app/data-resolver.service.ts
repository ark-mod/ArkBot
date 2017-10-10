import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Router, Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
 
import { DataService }  from './data.service';
 
@Injectable()
export class DataServiceResolver implements Resolve<any> {
  constructor(private dataService: DataService, private router: Router) {}
 
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<any> {
    return this.dataService.getServers()
    .then(servers => {
      return this.dataService;
    })
    .catch(error => {
      return this.dataService;
    });
  }
}