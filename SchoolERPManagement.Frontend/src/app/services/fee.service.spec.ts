import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { FeeService } from './fee.service';
import { environment } from '../../environments/environment';

describe('FeeService', () => {
  let service: FeeService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FeeService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(FeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getFeeDetails', () => {
    service.getFeeDetails(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/student/1/summary`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getPaymentHistory', () => {
    service.getPaymentHistory(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/student/1/history`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call payFees', () => {
    service.payFees({ studentId: 1, feeStructureId: 1, amountPaid: 100 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/pay`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call createCheckoutSession', () => {
    service.createCheckoutSession({ studentId: 1, feeStructureId: 1, amount: 100 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/create-checkout-session`);
    expect(req.request.method).toBe('POST');
    req.flush({ url: 'http://stripe.com/session' });
  });

  it('should call getCheckoutSessionDetails', () => {
    service.getCheckoutSessionDetails('session123').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/checkout-session/session123`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call addFeeStructure', () => {
    service.addFeeStructure({ classId: 1, academicYearId: 1, feeName: 'Tuition', totalAmount: 1000 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/structure`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getClassFeeSummaries', () => {
    service.getClassFeeSummaries(1, 2).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Fees/class/1/summaries?academicYearId=2`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
