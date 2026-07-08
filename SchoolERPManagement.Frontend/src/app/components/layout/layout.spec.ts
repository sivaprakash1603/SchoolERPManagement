import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { vi } from 'vitest';
import { Layout } from './layout';

vi.mock('@microsoft/signalr', () => {
  const mockHub = {
    start: vi.fn().mockResolvedValue(undefined),
    on: vi.fn(),
    stop: vi.fn().mockResolvedValue(undefined)
  };
  
  function HubConnectionBuilder(this: any) {
    this.withUrl = () => this;
    this.withAutomaticReconnect = () => this;
    this.build = () => mockHub;
  }

  return {
    HubConnectionBuilder: HubConnectionBuilder as any
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

  it('should run ngOnDestroy cleanly', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
