import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AcademicCalendarService } from './academic-calendar.service';
import { environment } from '../../environments/environment';

describe('AcademicCalendarService', () => {
  let service: AcademicCalendarService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AcademicCalendarService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AcademicCalendarService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAcademicCalendarSummary', () => {
    service.getAcademicCalendarSummary(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicCalendar/year/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call createCalendarEvent', () => {
    service.createCalendarEvent({ date: '2023-10-10', description: 'desc', isHoliday: true, academicYearId: 1 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicCalendar`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call deleteCalendarEvent', () => {
    service.deleteCalendarEvent(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AcademicCalendar/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
