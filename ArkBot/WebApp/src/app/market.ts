import {Auction} from './auction';

export class Market {
  constructor(
    public Key: string,
    public Name: string,    
    public Auctions: Auction[]) { }
}