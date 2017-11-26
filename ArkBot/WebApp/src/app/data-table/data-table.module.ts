import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DataTableComponent } from './data-table.component';
import { DataTableColumnDirective, DataTableColumnHeaderDirective, DataTableColumnCellDirective } from './columns/column.directive'
import { DataTableModeDirective } from './modes/mode.directive'

@NgModule({
  imports: [
    CommonModule,
    FormsModule
  ],
  providers: [
  ],
  declarations: [
    DataTableComponent,
    DataTableColumnDirective,
    DataTableColumnHeaderDirective,
    DataTableColumnCellDirective,
    DataTableModeDirective
  ],
  exports: [
    DataTableComponent,
    DataTableColumnDirective,
    DataTableColumnHeaderDirective,
    DataTableColumnCellDirective,
    DataTableModeDirective
  ]
})
export class DataTableModule { }