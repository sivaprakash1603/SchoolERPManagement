import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { PaymentResultComponent } from './payment-result';
import { FeeService } from '../../services/fee.service';
import { HttpClient } from '@angular/common/http';
import { of, throwError, BehaviorSubject } from 'rxjs';

describe('PaymentResultComponent', () => {
  let component: PaymentResultComponent;
  let fixture: ComponentFixture<PaymentResultComponent>;
  let mockRouter: any;
  let mockFeeService: any;
  let mockHttpClient: any;
  let queryParamsSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    mockRouter = { navigate: vi.fn() };
    mockFeeService = {
      getCheckoutSessionDetails: vi.fn().mockReturnValue(of({
        transactionId: 'txn_123',
        date: new Date('2025-01-01'),
        studentName: 'John Doe',
        feeName: 'Tuition Fee',
        amountPaid: 1000
      }))
    };
    mockHttpClient = {
      get: vi.fn().mockReturnValue(of(new Blob(['receipt content'], { type: 'application/pdf' })))
    };
    queryParamsSubject = new BehaviorSubject<any>({});

    await TestBed.configureTestingModule({
      imports: [PaymentResultComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: { queryParams: queryParamsSubject.asObservable() } },
        { provide: FeeService, useValue: mockFeeService },
        { provide: HttpClient, useValue: mockHttpClient }
      ]
    }).compileComponents();
  });

  beforeEach(async () => {
    fixture = TestBed.createComponent(PaymentResultComponent);
    component = fixture.componentInstance;
    
    // Default to success
    queryParamsSubject.next({ status: 'success', session_id: 'cs_test_123' });
    fixture.detectChanges();
    
    await fixture.whenStable();
  });

  describe('Initialization and State', () => {
    it('should fetch session details if status is success and session_id is provided', () => {
      expect(component.status()).toBe('success');
      expect(component.sessionId()).toBe('cs_test_123');
      expect(mockFeeService.getCheckoutSessionDetails).toHaveBeenCalledWith('cs_test_123');
      expect(component.sessionDetails()?.transactionId).toBe('txn_123');
      expect(component.loading()).toBe(false);
    });

    it('should set loading to false immediately if status is not success', () => {
      mockFeeService.getCheckoutSessionDetails.mockClear();
      
      queryParamsSubject.next({ status: 'cancel' });
      fixture.detectChanges();
      
      expect(component.status()).toBe('cancel');
      expect(mockFeeService.getCheckoutSessionDetails).not.toHaveBeenCalled();
      expect(component.loading()).toBe(false);
    });

    it('should handle getCheckoutSessionDetails error', () => {
      vi.spyOn(mockFeeService, 'getCheckoutSessionDetails').mockReturnValue(throwError(() => new Error('API Error')));
      fixture = TestBed.createComponent(PaymentResultComponent);
      component = fixture.componentInstance;
      queryParamsSubject.next({ status: 'success', session_id: 'err_session' });
      fixture.detectChanges();
      
      expect(component.errorMessage()).toBe('API Error');
      expect(component.loading()).toBe(false);
    });
  });

  describe('Methods', () => {
    it('should navigate to /fees on goToFees()', () => {
      component.goToFees();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/fees']);
    });

    it('should download receipt successfully', () => {
      window.URL.createObjectURL = vi.fn().mockReturnValue('blob:url');
      window.URL.revokeObjectURL = vi.fn();
      const createElementSpy = vi.spyOn(document, 'createElement');
      
      component.downloadReceipt();
      
      expect(component.downloading()).toBe(false);
      expect(mockHttpClient.get).toHaveBeenCalled();
      expect(createElementSpy).toHaveBeenCalledWith('a');
      expect(window.URL.createObjectURL).toHaveBeenCalled();
      expect(window.URL.revokeObjectURL).toHaveBeenCalled();
    });

    it('should fallback to window.open if download fails', () => {
      vi.spyOn(mockHttpClient, 'get').mockReturnValue(throwError(() => new Error('Download err')));
      vi.spyOn(window, 'open').mockImplementation(() => null);
      
      component.downloadReceipt();
      
      expect(component.downloading()).toBe(false);
      expect(window.open).toHaveBeenCalledWith(expect.stringContaining('receipt/txn_123'), '_blank');
    });

    it('should not download receipt if no transactionId is present', () => {
      component.sessionDetails.set(null);
      component.downloadReceipt();
      expect(mockHttpClient.get).not.toHaveBeenCalled();
    });
  });

  describe('HTML Template rendering', () => {
    it('should handle template interactions for success state', () => {
      fixture.detectChanges();
      
      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      
      // Download button
      const downloadBtn = buttons.find(b => b.textContent?.includes('Download Receipt'));
      expect(downloadBtn).toBeTruthy();
      downloadBtn?.dispatchEvent(new Event('click'));
      
      // Back to Fees button
      const backBtn = buttons.find(b => b.textContent?.includes('Back to Fees'));
      expect(backBtn).toBeTruthy();
      backBtn?.dispatchEvent(new Event('click'));
    });

    it('should handle template interactions for failed state', () => {
      queryParamsSubject.next({ status: 'failed' });
      fixture.detectChanges();
      
      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      
      // Retry Payment button
      const retryBtn = buttons.find(b => b.textContent?.includes('Retry Payment'));
      expect(retryBtn).toBeTruthy();
      retryBtn?.dispatchEvent(new Event('click'));
      
      // Back to Dashboard button (same function calls)
      const backBtn = buttons.find(b => b.textContent?.includes('Back to Dashboard'));
      expect(backBtn).toBeTruthy();
      backBtn?.dispatchEvent(new Event('click'));
    });

    it('should render loading and downloading states', () => {
      // Test loading state
      component.loading.set(true);
      fixture.detectChanges();
      const loadingText = fixture.nativeElement.querySelector('.loading-text');
      expect(loadingText).toBeTruthy();
      expect(loadingText.textContent).toContain('Confirming payment logs...');

      // Test downloading state
      component.loading.set(false);
      component.status.set('success');
      component.downloading.set(true);
      fixture.detectChanges();
      const spinner = fixture.nativeElement.querySelector('.spinner-border');
      expect(spinner).toBeTruthy();
    });
  });
});
