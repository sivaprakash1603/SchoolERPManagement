import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Header } from './header';

describe('Header', () => {
  let component: Header;
  let fixture: ComponentFixture<Header>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Header);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle theme on button click', () => {
    const isDarkBefore = component.themeService.isDarkMode();
    component.themeService.toggleTheme();
    expect(component.themeService.isDarkMode()).toBe(!isDarkBefore);
  });

  it('should toggle layout sidebar', () => {
    const isSidebarOpenBefore = component.layoutService.isSidebarOpen();
    component.layoutService.toggleSidebar();
    expect(component.layoutService.isSidebarOpen()).toBe(!isSidebarOpenBefore);
  });

  it('should clear session storage and navigate to login on logout', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');
    sessionStorage.setItem('token', 'dummy-token');

    component.logout();

    expect(sessionStorage.getItem('token')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });
});
