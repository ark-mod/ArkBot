import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'mapEntries'})
export class MapEntriesPipe implements PipeTransform {
    transform(map: any): any[] {
        return (<any[]> Object.entries(map)).map((kv) => ({ 'key': kv[0], 'value': kv[1] }));
    }
}