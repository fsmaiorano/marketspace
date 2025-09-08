import { TestBed } from '@angular/core/testing';
import {MarketSpaceService} from './marketspace.service';


describe(MarketSpaceService, () => {
  let service: MarketSpaceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MarketSpaceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
