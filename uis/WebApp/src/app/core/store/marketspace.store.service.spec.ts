import { TestBed } from '@angular/core/testing';

import { MarketspaceStoreService } from './marketspace.store.service';

describe('MarketspaceStoreService', () => {
  let service: MarketspaceStoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MarketspaceStoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
