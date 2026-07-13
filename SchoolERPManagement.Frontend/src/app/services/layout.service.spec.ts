import { TestBed } from '@angular/core/testing';
import { LayoutService } from './layout.service';

describe('LayoutService', () => {
  let service: LayoutService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [LayoutService] });
    service = TestBed.inject(LayoutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should toggle sidebar', () => {
    expect(service.isSidebarOpen()).toBe(false);
    service.toggleSidebar();
    expect(service.isSidebarOpen()).toBe(true);
    service.toggleSidebar();
    expect(service.isSidebarOpen()).toBe(false);
  });

  it('should close sidebar', () => {
    service.openSidebar();
    expect(service.isSidebarOpen()).toBe(true);
    service.closeSidebar();
    expect(service.isSidebarOpen()).toBe(false);
  });

  it('should open sidebar', () => {
    service.closeSidebar();
    expect(service.isSidebarOpen()).toBe(false);
    service.openSidebar();
    expect(service.isSidebarOpen()).toBe(true);
  });
});
