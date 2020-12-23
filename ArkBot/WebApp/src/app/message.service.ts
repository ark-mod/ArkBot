import { Injectable, EventEmitter, NgZone }    from '@angular/core';
import { environment } from '../environments/environment';
import * as signalR from "@microsoft/signalr";
//import * as $ from 'jquery'; //problem: no $.hubConnection because this is not JQueryStatic
declare var $: any;
declare var config: any;

@Injectable()
export class MessageService {
    private connection: signalR.HubConnection;

    public serverUpdated$: EventEmitter<string> = new EventEmitter();
    public onlinePlayers$: EventEmitter<any> = new EventEmitter();
    public chatMessages$: EventEmitter<any> = new EventEmitter();
    public playerLocations$: EventEmitter<any> = new EventEmitter();
    public marketsUpdated$: EventEmitter<any> = new EventEmitter();

    constructor(private zone:NgZone) {  }

    connect(): void {
        this.connection = new signalR.HubConnectionBuilder().withUrl(`${this.getSignalRBaseUrl()}/hub`).build();
        this.connection.on("ServerUpdate", (serverKey: string) => {
            this.zone.run(() => { 
                this.serverUpdated$.emit(serverKey);
            });
        });
        this.connection.on("OnlinePlayers", (onlinePlayers: any) => {
            this.zone.run(() => { 
                this.onlinePlayers$.emit(onlinePlayers);
            });
        });
        this.connection.on("ChatMessages", (messages: any) => {
            this.zone.run(() => { 
                this.chatMessages$.emit(messages);
            });
        });
        this.connection.on("PlayerLocations", (playerLocations: any) => {
            this.zone.run(() => { 
                this.playerLocations$.emit(playerLocations);
            });
        });
        this.connection.on("MarketUpdate", () => {
            this.zone.run(() => { 
                this.marketsUpdated$.emit();
            });
        });
        
        this.connection.start()
            .then(() => console.log('[signalR] Now connected, connection ID=' + this.connection.connectionId))
            .catch((err) => console.log(`[signalR] Could not connect: ${err}`));
    }

    getSignalRBaseUrl(): string {
        return environment.signalrBaseUrl
            .replace(/\<protocol\>/gi, window.location.protocol)
            .replace(/\<hostname\>/gi, window.location.hostname)
            .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    }
}