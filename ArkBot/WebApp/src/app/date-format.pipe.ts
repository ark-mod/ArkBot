import {Pipe, PipeTransform} from '@angular/core';

import * as moment from 'moment'

@Pipe({name: 'dateFormat'})
export class DateFormatPipe implements PipeTransform {
    transform(date: any, format: string): string {
        return (moment(typeof(date) == 'number' ? new Date(date) : date).format(format));
    }
}