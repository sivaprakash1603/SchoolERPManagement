import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ReportService } from './report.service';
import { environment } from '../../environments/environment';

describe('ReportService', () => {
  let service: ReportService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ReportService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ReportService);
    httpMock = TestBed.inject(HttpTestingController);
    
    // Mock window.URL.createObjectURL and a.click()
    window.URL.createObjectURL = vi.fn(() => 'mock-url');
    window.URL.revokeObjectURL = vi.fn();
  });

  afterEach(() => {
    httpMock.verify();
    vi.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getExamPerformanceReport', () => {
    service.getExamPerformanceReport(1).subscribe(res => {
      expect(res).toBeTruthy();
    });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/exams/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call exportFeeCollectionPdf', () => {
    service.exportFeeCollectionPdf({ classId: 1 });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/fees/export/pdf?classId=1`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call exportStudentAttendancePdf', () => {
    service.exportStudentAttendancePdf({ classId: 1 });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/attendance/students/export/pdf?classId=1`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call exportStaffAttendancePdf', () => {
    service.exportStaffAttendancePdf();
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/attendance/staff/export/pdf`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call exportExamResultsPdf', () => {
    service.exportExamResultsPdf({ examId: 1 });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/exams/results/export/pdf?examId=1`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call exportAssetsPdf', () => {
    service.exportAssetsPdf({ status: 'Active' });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/assets/export/pdf?status=Active`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should skip null undefined or empty filters in buildParams', () => {
    service.exportAssetsPdf({ a: null, b: undefined, c: '', d: 'Valid' });
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/assets/export/pdf?d=Valid`);
    req.flush(new Blob());
  });

  it('should handle null or undefined filters gracefully', () => {
    service.exportFeeCollectionPdf(null);
    const req = httpMock.expectOne(`${environment.apiUrl}/reports/fees/export/pdf`);
    req.flush(new Blob());
  });
});
