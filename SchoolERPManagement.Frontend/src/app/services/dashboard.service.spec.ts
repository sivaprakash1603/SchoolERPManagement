import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DashboardService } from './dashboard.service';
import { environment } from '../../environments/environment';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAdminMetrics without academicYearId', () => {
    service.getAdminMetrics().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Dashboard/admin`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getAdminMetrics with academicYearId', () => {
    service.getAdminMetrics(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Dashboard/admin?academicYearId=1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getTeacherMetrics without academicYearId', () => {
    service.getTeacherMetrics(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Dashboard/teacher/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getTeacherMetrics with academicYearId', () => {
    service.getTeacherMetrics(1, 2).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Dashboard/teacher/1?academicYearId=2`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
