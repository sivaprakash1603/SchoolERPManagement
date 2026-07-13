import { TestBed } from '@angular/core/testing';
import { FilterStateService } from './filter-state.service';
import { vi } from 'vitest';

describe('FilterStateService', () => {
  let service: FilterStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [FilterStateService] });
    service = TestBed.inject(FilterStateService);
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should save and get state', () => {
    service.saveState('test', { a: 1 });
    const val = service.getState('test');
    expect(val).toEqual({ a: 1 });
  });

  it('should return null if no state', () => {
    const val = service.getState('test');
    expect(val).toBeNull();
  });

  it('should handle JSON parse error gracefully', () => {
    sessionStorage.setItem('filter_state_test', '{ invalid');
    const val = service.getState('test');
    expect(val).toBeNull();
  });

  it('should handle setItem error gracefully', () => {
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new Error('Quota'); });
    expect(() => service.saveState('test', { a: 1 })).not.toThrow();
  });
});
