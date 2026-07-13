import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AcademicYearService } from './academic-year.service';
import { environment } from '../../environments/environment';

describe('AcademicYearService', () => {
  let service: AcademicYearService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AcademicYearService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AcademicYearService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAllAcademicYears', () => {
    service.getAllAcademicYears().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicYears`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call createAcademicYear', () => {
    service.createAcademicYear({ yearName: '2023', startDate: '2023-01-01', endDate: '2023-12-31' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicYears`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call setCurrentAcademicYear', () => {
    service.setCurrentAcademicYear(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicYears/1/set-current`);
    expect(req.request.method).toBe('PATCH');
    req.flush(null);
  });
});
