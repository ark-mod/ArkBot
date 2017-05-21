import { Injectable, EventEmitter, NgZone }    from '@angular/core';
import { environment } from '../environments/environment';
//import * as $ from 'jquery'; //problem: no $.hubConnection because this is not JQueryStatic
//import 'ms-signalr-client'; //problem: does not appear to work with typescript
//import {SignalR} from 'signalr'; //problem: 'signalr' is not a module
declare var $: any;

@Injectable()
export class MessageService {
    //private connection: SignalR.Hub.Connection;
    //private proxy: SignalR.Hub.Proxy;
    private connection: any;
    private proxy: any;

    public serverUpdated$: EventEmitter<string>;

    constructor(private zone:NgZone) {
        this.serverUpdated$ = new EventEmitter();

        this.connection = $.hubConnection(environment.signalrBaseUrl);
        this.proxy = this.connection.createHubProxy('ServerUpdateHub');
        
        this.proxy.on('serverUpdateNotification', (serverKey: string) => { 
            this.zone.run(() => { 
                this.serverUpdated$.emit(serverKey);
            });
         });
        
        this.connection.start()
        .done(() => console.log('Now connected, connection ID=' + this.connection.id))
        .fail(() => console.log('Could not connect'));
    }
}