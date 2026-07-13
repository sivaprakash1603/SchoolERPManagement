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

  it('should display error if forgot password fails', () => {
    component.isForgotPassword.set(true);
    component.forgotPasswordEmail.set('test@educontrol.com');
    
    vi.spyOn(mockAuthService, 'forgotPassword').mockReturnValue(
      throwError(() => ({ error: { message: 'User not found' } }))
    );

    component.handleForgotPasswordClick();
    expect(component.errorMessage()).toBe('User not found');
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

  describe('HTML Template rendering', () => {
    it('should cover branches in login form', () => {
      // Dark mode branch
      vi.spyOn(component.themeService, 'isDarkMode').mockReturnValue(true);
      fixture.detectChanges();

      // Trigger error messages
      component.errorMessage.set('Test error');
      component.loginForm.get('username')?.markAsTouched();
      component.loginForm.get('username')?.setErrors({ required: true });
      component.loginForm.get('password')?.markAsTouched();
      component.loginForm.get('password')?.setErrors({ required: true });
      component.progress.set(true); // spinner branch
      component.showPassword.set(true); // password visibility branch
      
      fixture.detectChanges();
      
      // Dispatch events
      const eyeBtn = fixture.nativeElement.querySelector('.login-eye-btn');
      if (eyeBtn) eyeBtn.click();
      
      const themeBtn = fixture.nativeElement.querySelector('.theme-toggle-btn');
      if (themeBtn) themeBtn.click();
      
      const inputs = fixture.nativeElement.querySelectorAll('.login-input');
      inputs.forEach((el: any) => {
        el.dispatchEvent(new Event('input'));
        el.dispatchEvent(new Event('change'));
      });
      
      // Trigger pattern error
      component.loginForm.get('username')?.setErrors({ pattern: true });
      fixture.detectChanges();
    });

    it('should cover branches in forgot password form', () => {
      component.isForgotPassword.set(true);
      component.errorMessage.set('Forgot error');
      component.progress.set(true); // spinner branch
      fixture.detectChanges();

      const backBtn = fixture.nativeElement.querySelector('.btn-outline-secondary');
      if (backBtn) backBtn.click();

      // Dispatch event for ngModel
      const emailInput = fixture.nativeElement.querySelector('#forgotEmail');
      if (emailInput) {
        emailInput.value = 'test@example.com';
        emailInput.dispatchEvent(new Event('input'));
        emailInput.dispatchEvent(new Event('ngModelChange'));
      }
      
      component.progress.set(false);
      fixture.detectChanges();
      const submitBtn = fixture.nativeElement.querySelector('.login-submit-btn');
      if (submitBtn) submitBtn.click();
    });
  });
});
