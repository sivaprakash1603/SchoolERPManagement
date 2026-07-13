import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ResetPassword } from './reset-password';

describe('ResetPassword', () => {
  let component: ResetPassword;
  let fixture: ComponentFixture<ResetPassword>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResetPassword],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    // Mock IntersectionObserver for jsdom test environment
    (window as any).IntersectionObserver = class IntersectionObserver {
      constructor() {}
      observe() {}
      unobserve() {}
      disconnect() {}
    };

    fixture = TestBed.createComponent(ResetPassword);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit & Validation', () => {
    it('should show error if email or token is missing', () => {
      // Re-trigger ngOnInit without token
      component.email = '';
      component.token = '';
      component.ngOnInit();
      expect(component.errorMessage()).toBe('Invalid password reset link. Please request a new one.');
    });

    it('should initialize form with empty values', () => {
      expect(component.resetForm).toBeDefined();
      expect(component.resetForm.get('newPassword')?.value).toBe('');
    });

    it('should validate password match', () => {
      component.resetForm.get('newPassword')?.setValue('ValidPass1!');
      component.resetForm.get('confirmPassword')?.setValue('InvalidPass2!');
      component.resetForm.updateValueAndValidity();

      expect(component.resetForm.hasError('passwordMismatch')).toBe(true);
      expect(component.resetForm.get('confirmPassword')?.hasError('passwordMismatch')).toBe(true);
    });

    it('should clear mismatch error when passwords match', () => {
      component.resetForm.get('newPassword')?.setValue('ValidPass1!');
      component.resetForm.get('confirmPassword')?.setValue('ValidPass1!');
      component.resetForm.updateValueAndValidity();

      expect(component.resetForm.hasError('passwordMismatch')).toBe(false);
    });
  });

  describe('UI interactions', () => {
    it('should toggle new password visibility', () => {
      expect(component.showNewPassword()).toBe(false);
      component.toggleNewPasswordVisibility();
      expect(component.showNewPassword()).toBe(true);
    });

    it('should toggle confirm password visibility', () => {
      expect(component.showConfirmPassword()).toBe(false);
      component.toggleConfirmPasswordVisibility();
      expect(component.showConfirmPassword()).toBe(true);
    });

    it('should not submit if form is invalid', () => {
      const authSpy = vi.spyOn((component as any).authService, 'resetPassword');
      component.resetForm.get('newPassword')?.setValue('');
      component.handleResetClick();
      expect(authSpy).not.toHaveBeenCalled();
    });

    it('should mark all fields as touched if form is invalid on submit', () => {
      component.resetForm.get('newPassword')?.setValue('');
      component.handleResetClick();
      expect(component.resetForm.touched).toBe(true);
    });
  });

  describe('handleResetClick', () => {
    it('should submit successfully', () => {
      const authService = (component as any).authService;
      const router = (component as any).router;
      const toastService = (component as any).toastService;

      // Ensure form is valid
      component.email = 'test@example.com';
      component.token = 'abc';
      component.resetForm.get('newPassword')?.setValue('ValidPass123!');
      component.resetForm.get('confirmPassword')?.setValue('ValidPass123!');
      component.resetForm.updateValueAndValidity();

      const authSpy = vi.spyOn(authService, 'resetPassword').mockReturnValue({ subscribe: (obs: any) => obs.next() } as any);
      const routerSpy = vi.spyOn(router, 'navigate');
      const toastSpy = vi.spyOn(toastService, 'success');

      component.handleResetClick();

      expect(authSpy).toHaveBeenCalledWith({ email: 'test@example.com', token: 'abc', newPassword: 'ValidPass123!' });
      expect(toastSpy).toHaveBeenCalledWith('Password has been successfully reset. Please log in.');
      expect(routerSpy).toHaveBeenCalledWith(['/login']);
      expect(component.progress()).toBe(false);
    });

    it('should handle submission error', () => {
      const authService = (component as any).authService;
      
      component.email = 'test@example.com';
      component.token = 'abc';
      component.resetForm.get('newPassword')?.setValue('ValidPass123!');
      component.resetForm.get('confirmPassword')?.setValue('ValidPass123!');
      component.resetForm.updateValueAndValidity();

      vi.spyOn(authService, 'resetPassword').mockReturnValue({ 
        subscribe: (obs: any) => obs.error({ error: { message: 'Reset failed' } }) 
      } as any);

      component.handleResetClick();

      expect(component.progress()).toBe(false);
      expect(component.errorMessage()).toBe('Reset failed');
    });

    it('should handle submission error with fallback message', () => {
      const authService = (component as any).authService;
      
      component.email = 'test@example.com';
      component.token = 'abc';
      component.resetForm.get('newPassword')?.setValue('ValidPass123!');
      component.resetForm.get('confirmPassword')?.setValue('ValidPass123!');
      component.resetForm.updateValueAndValidity();

      vi.spyOn(authService, 'resetPassword').mockReturnValue({ 
        subscribe: (obs: any) => obs.error({}) 
      } as any);

      component.handleResetClick();

      expect(component.errorMessage()).toBe('Failed to reset password. The link may have expired.');
    });
  });

  describe('HTML Template rendering', () => {
    it('should dispatch events on inputs and buttons', () => {
      component.errorMessage.set('Test error');
      component.email = 'test@example.com';
      component.token = 'abc';
      // Force it to be valid and touched to trigger error states
      component.resetForm.get('newPassword')?.setValue('');
      component.resetForm.get('newPassword')?.markAsTouched();
      component.resetForm.get('confirmPassword')?.setValue('');
      component.resetForm.get('confirmPassword')?.markAsTouched();
      component.resetForm.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      component.progress.set(true); // to test progress branch
      
      fixture.detectChanges();
      
      // Toggle toggles
      const toggles = fixture.nativeElement.querySelectorAll('.input-group-text.login-eye-btn');
      toggles.forEach((t: HTMLElement) => {
        t.click();
      });

      // Change values to trigger correct state
      component.resetForm.get('newPassword')?.setValue('Aaa111!!!');
      component.resetForm.get('confirmPassword')?.setValue('Aaa111!!!');
      component.progress.set(false);
      fixture.detectChanges();
      
      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');
      if (submitBtn && !submitBtn.disabled) submitBtn.click();
    });
  });
});
