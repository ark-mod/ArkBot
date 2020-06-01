import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'mapValues'})
export class MapValuesPipe implements PipeTransform {
    transform(map: any): any[] {
        return Object.values(map);
    }
}