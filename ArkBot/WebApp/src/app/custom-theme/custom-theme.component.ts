import { Component, Inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationsService } from 'angular2-notifications';
import { DOCUMENT } from '@angular/common';
import { DataService } from '../data.service';
import { copyToClipboard } from '../utils'

type CustomTheme = {
  name: string;
  title: string;
  default: string;
  value: string;
}

type CustomThemeGroup = {
  name: string;
  key: string;
  theme: Partial<CustomTheme>[];
}

@Component({
  selector: 'custom-theme',
  templateUrl: './custom-theme.component.html',
  styleUrls: ['./custom-theme.component.css']
})
export class CustomThemeComponent {
  @ViewChild('colorPicker') colorPicker:any;

  private _show: boolean = false;
  private _hoverHide: boolean = false;
  private _isInitialized: boolean = false;

  public current: Partial<CustomTheme>;
  public customTheme: Partial<CustomTheme>[] = [];
  public customThemeGroups: CustomThemeGroup[] = [
    { key: 'dark', name: 'Dark Theme', theme: [] },
    { key: 'light', name: 'Light Theme', theme: [] }
  ];
  public currentCustomThemeGroup: CustomThemeGroup = this.customThemeGroups[0];

  constructor(@Inject(DOCUMENT) private doc: any,
    private notificationsService: NotificationsService,
    private dataService: DataService) {}

  private toHexColor(colorStr: string): string {
    let a = this.doc.createElement('div');
    a.style.display = "none";
    a.style.color = colorStr;
    let colors = window.getComputedStyle(this.doc.body.appendChild(a)).color.match(/\d+/g).map(function(a){ return parseInt(a, 10); });
    this.doc.body.removeChild(a);

    if (colors.length < 3) {
      console.log(`Could not convert color '${colorStr}' to hex`);
      return colorStr;
    }

    return '#' + (((1 << 24) + (colors[0] << 16) + (colors[1] << 8) + colors[2]).toString(16).substr(1));
  }

  show(): void {
    if (!this._isInitialized) this.init();

    this._show = true;
  }

  private init(): void {
    this._isInitialized = true;

    let result = [];
    for (let t of this.customThemeGroups) {
      t.theme = [
        { name: `--${t.key}-theme-bg`, title: "Background" },
        { name: `--${t.key}-theme`, title: "Foreground" },
        { name: `--${t.key}-theme-l1-bg`, title: "Low Contrast #1 (background)" },
        { name: `--${t.key}-theme-l1`, title: "Low Contrast #1 (text)" },
        { name: `--${t.key}-theme-l2-bg`, title: "Low Contrast #2 (background)" },
        { name: `--${t.key}-theme-l2`, title: "Low Contrast #2 (text)" },
        { name: `--${t.key}-theme-text-d1`, title: "High Contrast (text)" },
        { name: `--${t.key}-theme-text-c1`, title: "Colored (text)" },
        { name: `--${t.key}-theme-d1-bg`, title: "High Contrast (background)" },
        { name: `--${t.key}-theme-d1`, title: "High Contrast (text)" },
        { name: `--${t.key}-theme-c1-bg`, title: "Colored (background)" },
        { name: `--${t.key}-theme-c1`, title: "Colored (text)" },
        { name: `--${t.key}-theme-e1-bg`, title: "Error (background)" },
        { name: `--${t.key}-theme-e1`, title: "Error (text)" },
        { name: `--${t.key}-theme-l2-even-bg`, title: "Low Contrast #2 (background) [even-rows]" },
        { name: `--${t.key}-theme-l2-even`, title: "Low Contrast #2 (text) [even-rows]" },
        { name: `--${t.key}-theme-l1-hover-bg`, title: "Low Contrast #1 (background) [hover]" },
        { name: `--${t.key}-theme-l1-hover`, title: "Low Contrast #1 (text) [hover]" },
        { name: `--${t.key}-theme-l2-hover-bg`, title: "Low Contrast #2 (background) [hover]" },
        { name: `--${t.key}-theme-l2-hover`, title: "Low Contrast #2 (text) [hover]" },
        { name: `--${t.key}-theme-lighter-c1-bg`, title: "High Contrast (background) [lighter]" },
        { name: `--${t.key}-theme-text-l1-light`, title: "Low Contrast #1 (text) [inactive]" },
        { name: `--${t.key}-border-theme`, title: "Border" },
        { name: `--${t.key}-divider-theme`, title: "Divider" },
      ];

      result = result.concat(t.theme);
    }
    this.customTheme = result;

    let styles = getComputedStyle(this.doc.documentElement);
    for (let ct of this.customTheme) ct.default = ct.value = this.toHexColor(styles.getPropertyValue(ct.name).toString().trim());

    this.current = this.customTheme[0];
  }

  setCurrent(item: Partial<CustomTheme>): void {
    this.current = item;
    this.colorPicker.nativeElement.dispatchEvent(new MouseEvent('click'));
  }

  update(val: any): void {
    this.current.value = val;

    const root = this.doc.querySelector(':root')
    root.style.setProperty(this.current.name, this.current.value)
  }

  copyStyles(): void {
    let str = ":root {\r\n";
    for (let ct of this.customTheme) {
      if (ct.value !== ct.default) str += `    ${ct.name}: ${ct.value};\r\n`;
    }
    str += "}";

    copyToClipboard(this.doc, str);

    this.notificationsService.info(
      'Create Custom Theme',
      `Custom theme styles have been copied to the clipboard.`,
      {
        pauseOnHover: true,
        clickToClose: true
      }
    );
  }
}