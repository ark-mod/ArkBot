import { Component, OnInit, OnDestroy } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit, OnDestroy {
private menuOption: string = undefined; 
  private menuOptionSubscription: any;
  public menuVisible: boolean = false;
  private className: string = "menucontainer";

  constructor(
    public dataService: DataService) { }

  ngOnInit() {
    //this.activate("overview");
    this.menuOptionSubscription = this.dataService.MenuOption.subscribe(menuOption => this.menuOption = menuOption);
  }

  ngOnDestroy() {
    this.menuOptionSubscription.unsubscribe();
  }

  public activate(menuOption: string): void {
    this.dataService.SetMenuOption(menuOption);
  }

  public active(menuOption: string): boolean {
    return this.menuOption == menuOption;
  }

  public toggleMenu(): void {
    this.menuVisible = !this.menuVisible;
  }
}