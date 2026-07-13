import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';
import { vi } from 'vitest';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [ToastService] });
    service = TestBed.inject(ToastService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show success', () => {
    service.success('msg');
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('success');
    vi.advanceTimersByTime(4000);
    expect(service.toasts().length).toBe(0);
  });

  it('should show error', () => {
    service.error('msg');
    expect(service.toasts()[0].type).toBe('danger');
    vi.advanceTimersByTime(4000);
  });

  it('should show warning', () => {
    service.warning('msg');
    expect(service.toasts()[0].type).toBe('warning');
    vi.advanceTimersByTime(4000);
  });

  it('should show info', () => {
    service.info('msg');
    expect(service.toasts()[0].type).toBe('info');
    vi.advanceTimersByTime(4000);
  });

  it('should remove manually', () => {
    service.show('msg');
    const id = service.toasts()[0].id;
    service.remove(id);
    expect(service.toasts().length).toBe(0);
  });
});
