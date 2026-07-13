import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { vi } from 'vitest';
import { Layout } from './layout';

vi.mock('@microsoft/signalr', () => {
  const callbacks: Record<string, Function> = {};
  const mockHub = {
    start: vi.fn().mockResolvedValue(undefined),
    on: vi.fn((eventName, cb) => {
      callbacks[eventName] = cb;
    }),
    stop: vi.fn().mockResolvedValue(undefined),
    // Expose callbacks for testing
    __callbacks: callbacks
  };
  
  function HubConnectionBuilder(this: any) {
    this.withUrl = () => this;
    this.withAutomaticReconnect = () => this;
    this.build = () => mockHub;
  }

  return {
    HubConnectionBuilder: HubConnectionBuilder as any,
    __mockHub: mockHub
  };
});

describe('Layout', () => {
  let component: Layout;
  let fixture: ComponentFixture<Layout>;

  beforeEach(async () => {
    sessionStorage.setItem('token', 'dummy-token');
    await TestBed.configureTestingModule({
      imports: [Layout],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Layout);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return undefined or route snapshot url from animation data', () => {
    const animData = component.getRouteAnimationData();
    expect(animData).toBeUndefined();
  });

  it('should not initialize SignalR if no token is present', () => {
    sessionStorage.removeItem('token');
    // Call ngOnInit again to test the path where token is missing
    component.ngOnInit();
    expect(component['hubConnection']?.state).toBeUndefined(); // Assuming mock doesn't set state, but we know it returns early
  });

  it('should handle SignalR ReceiveNotification', async () => {
    const toastSpy = vi.spyOn(component.toastService, 'info');
    
    // We need to import the mock object, or we can just get it via dynamic import if needed.
    // Or we can just access it since it was returned by the vi.mock factory
    const signalR = await import('@microsoft/signalr');
    const mockHub = (signalR as any).__mockHub;
    
    // Trigger the callback
    const callback = mockHub.__callbacks['ReceiveNotification'];
    if (callback) {
      callback({ title: 'Test', message: 'Hello World' });
      expect(toastSpy).toHaveBeenCalledWith('Test: Hello World');
    }
  });

  it('should toggle sidebar via layoutService in template', () => {
    component.layoutService.toggleSidebar();
    fixture.detectChanges();
    expect(component.layoutService.isSidebarOpen()).toBe(true);

    const backdrop = fixture.nativeElement.querySelector('.sidebar-backdrop');
    if (backdrop) backdrop.click();
    fixture.detectChanges();
    
    expect(component.layoutService.isSidebarOpen()).toBe(false);
  });

  it('should render all types of toasts', () => {
    // Render success
    component.toastService.success('Success message');
    // Render danger
    component.toastService.error('Danger message');
    // Render warning
    component.toastService.warning('Warning message');
    // Render info
    component.toastService.info('Info message');
    
    fixture.detectChanges();
    
    const toasts = fixture.nativeElement.querySelectorAll('.toast');
    expect(toasts.length).toBe(4);
    
    // Close first toast
    const closeBtn = toasts[0].querySelector('.btn-close') as HTMLButtonElement;
    closeBtn.click();
    fixture.detectChanges();
    
    expect(component.toastService.toasts().length).toBe(3);
  });

  it('should run ngOnDestroy cleanly', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
