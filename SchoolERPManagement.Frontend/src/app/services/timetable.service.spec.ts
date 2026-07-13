import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TimetableService } from './timetable.service';
import { environment } from '../../environments/environment';

describe('TimetableService', () => {
  let service: TimetableService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TimetableService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(TimetableService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getClassTimetable', () => {
    service.getClassTimetable(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/class/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getTeacherTimetable', () => {
    service.getTeacherTimetable(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/teacher/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call createTimetable', () => {
    service.createTimetable({ classId: 1, subjectId: 1, teacherId: 1, dayOfWeek: 'Monday', startTime: '09:00', endTime: '10:00' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getTeacherRequirements', () => {
    service.getTeacherRequirements(5, 2).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/teacher-requirements?periodsPerDay=5&freePeriodsPerStaff=2`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call generateTimetable', () => {
    service.generateTimetable({ classIds: [1], periodsPerDay: 5, freePeriodsPerStaff: 2, timings: [] }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/generate`);
    expect(req.request.method).toBe('POST');
    req.flush([]);
  });

  it('should call saveGeneratedTimetable', () => {
    service.saveGeneratedTimetable([]).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/save-generated`);
    expect(req.request.method).toBe('POST');
    req.flush(null);
  });

  it('should call updateTimetableSlot', () => {
    service.updateTimetableSlot(1, { subjectId: 2, teacherId: 3 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Timetable/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });
});
