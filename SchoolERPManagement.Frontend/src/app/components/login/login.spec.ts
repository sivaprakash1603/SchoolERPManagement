import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { Auth } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let mockAuthService: any;
  let mockToastService: any;
  let router: Router;

  beforeEach(async () => {
    mockAuthService = {
      loginApiCall: () => of({ accessToken: 'header.eyJyb2xlIjoiQWRtaW4iLCJzdWIiOjEsInVuaXF1ZV9uYW1lIjoiYWRtaW4ifQ==.signature' }),
      forgotPassword: () => of({})
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Auth, useValue: mockAuthService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    let observerCallback: any;
    (window as any).IntersectionObserver = class IntersectionObserver {
      constructor(callback: any) {
        observerCallback = callback;
      }
      observe(el: any) {
        // Automatically trigger intersection
        observerCallback([{ isIntersecting: true, target: el }]);
      }
      unobserve() {}
      disconnect() {}
    };

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle password visibility signal', () => {
    expect(component.showPassword()).toBe(false);
    component.togglePasswordVisibility();
    expect(component.showPassword()).toBe(true);
  });

  it('should invalidate form and not call login if empty', () => {
    const loginSpy = vi.spyOn(mockAuthService, 'loginApiCall');
    component.handleLoginClick();
    expect(loginSpy).not.toHaveBeenCalled();
  });

  it('should perform login successfully and navigate', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');
    component.loginForm.patchValue({
      username: 'admin',
      password: 'password123'
    });

    component.handleLoginClick();

    expect(sessionStorage.getItem('token')).toBeTruthy();
    expect(sessionStorage.getItem('role')).toBe('Admin');
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should display error if login fails', () => {
    vi.spyOn(mockAuthService, 'loginApiCall').mockReturnValue(
      throwError(() => ({ error: { message: 'Invalid credentials' } }))
    );

    component.loginForm.patchValue({
      username: 'admin',
      password: 'password123'
    });

    component.handleLoginClick();
    expect(component.errorMessage()).toBe('Invalid credentials');
  });

  it('should toggle and run forgot password', () => {
    component.isForgotPassword.set(true);
    component.forgotPasswordEmail.set('');

    component.handleForgotPasswordClick();
    expect(component.errorMessage()).toBe('Please enter your email address.');

    component.forgotPasswordEmail.set('test@educontrol.com');
    component.handleForgotPasswordClick();
    expect(mockToastService.success).toHaveBeenCalledWith('Reset link sent to your email.');
  });

  it('should display generic error if login fails with 500 status', () => {
    vi.spyOn(mockAuthService, 'loginApiCall').mockReturnValue(
      throwError(() => ({ status: 500 }))
    );

    component.loginForm.patchValue({
      username: 'admin',
      password: 'password123'
    });

    component.handleLoginClick();
    expect(component.errorMessage()).toBe('An error occurred. Please try again later.');
  });

  it('should display invalid username error if login fails with 401 status and no message', () => {
    vi.spyOn(mockAuthService, 'loginApiCall').mockReturnValue(
      throwError(() => ({ status: 401 }))
    );

    component.loginForm.patchValue({
      username: 'admin',
      password: 'password123'
    });

    component.handleLoginClick();
    expect(component.errorMessage()).toBe('Invalid username or password.');
  });

  it('should advance carousel active quote index over time', () => {
    vi.useFakeTimers();
    component.ngOnInit();
    expect(component.activeQuoteIndex()).toBe(0);
    vi.advanceTimersByTime(5000);
    expect(component.activeQuoteIndex()).toBe(1);
    vi.useRealTimers();
  });
});
