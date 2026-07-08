import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Landing } from './landing';

describe('Landing', () => {
  let component: Landing;
  let fixture: ComponentFixture<Landing>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Landing],
      providers: [
        provideRouter([])
      ]
    }).compileComponents();

    let observerCallback: any;
    (window as any).IntersectionObserver = class IntersectionObserver {
      constructor(callback: any) {
        observerCallback = callback;
      }
      observe(el: any) {
        observerCallback([{ isIntersecting: true, intersectionRatio: 0.15, target: el }]);
      }
      unobserve() {}
      disconnect() {}
    };

    fixture = TestBed.createComponent(Landing);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should compute linear progress correctly', () => {
    component.scrollY.set(100);
    expect(component.progress(0, 200)).toBe(0.5);
    expect(component.progress(200, 300)).toBe(0);
    expect(component.progress(0, 50)).toBe(1);
  });

  it('should compute eased and lerp values correctly', () => {
    const eased = component.eased(0, 100);
    expect(eased).toBeGreaterThanOrEqual(0);
    expect(component.lerp(10, 20, 0.5)).toBe(15);
  });

  it('should trigger onWindowScroll and update scrollY', () => {
    vi.useFakeTimers();
    vi.spyOn(window, 'requestAnimationFrame').mockImplementation((cb: any) => {
      setTimeout(cb, 16);
      return 0;
    });
    // Stub global scrollY and dispatch event
    Object.defineProperty(window, 'scrollY', { value: 250, writable: true });
    component.onWindowScroll();
    vi.advanceTimersByTime(16);
    expect(component.scrollY()).toBe(250);
    vi.useRealTimers();
  });

  it('should trigger onWindowResize and update viewportH', () => {
    Object.defineProperty(window, 'innerHeight', { value: 850, writable: true });
    window.dispatchEvent(new Event('resize'));
    expect(component.viewportH()).toBe(850);
  });

  it('should evaluate individual cards and orbs correctly', () => {
    expect(component.featureCard1()).toBeDefined();
    expect(component.featureCard2()).toBeDefined();
    expect(component.featureCard3()).toBeDefined();
    expect(component.metricCard(2)).toBeDefined();
    expect(component.orb(0.2, 1)).toBeDefined();
  });

  it('should disconnect observer on destroy', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
