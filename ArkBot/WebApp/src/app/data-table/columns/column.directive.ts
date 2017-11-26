import { Directive, TemplateRef, ContentChild, Input } from '@angular/core';

@Directive({ selector: '[ark-dt-cell]' })
export class DataTableColumnCellDirective {
    constructor(public template: TemplateRef<any>) { }
}

@Directive({ selector: '[ark-dt-header]' })
export class DataTableColumnHeaderDirective {
    constructor(public template: TemplateRef<any>) { }
}

@Directive({ selector: 'ark-dt-column' })
export class DataTableColumnDirective {
    @Input()
    @ContentChild(DataTableColumnCellDirective, { read: TemplateRef }) 
    cellTemplate: TemplateRef<any>;

    @Input()
    @ContentChild(DataTableColumnHeaderDirective, { read: TemplateRef }) 
    headerTemplate: TemplateRef<any>;

    @Input() mode: string;
    @Input() key: string;
    @Input() thenSort: string;
    @Input() title: string;
    @Input() orderBy: boolean;
}