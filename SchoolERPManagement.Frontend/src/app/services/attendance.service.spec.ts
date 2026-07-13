import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AttendanceService } from './attendance.service';
import { environment } from '../../environments/environment';

describe('AttendanceService', () => {
  let service: AttendanceService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AttendanceService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AttendanceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getAttendanceByClass', () => {
    service.getAttendanceByClass(1, '2023-10-10').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Attendance/class/1?date=2023-10-10`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call markAttendance', () => {
    service.markAttendance({ studentId: 1, date: '2023-10-10', status: 'present' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Attendance`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getAttendanceByStudent', () => {
    service.getAttendanceByStudent(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Attendance/student/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getStaffAttendanceByDate', () => {
    service.getStaffAttendanceByDate('2023-10-10').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/StaffAttendance/date/2023-10-10`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getStaffAttendanceByUser', () => {
    service.getStaffAttendanceByUser(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/StaffAttendance/user/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call markStaffAttendance', () => {
    service.markStaffAttendance({ userId: 1, date: '2023-10-10', status: 'Present', attendanceType: 'Daily' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/StaffAttendance`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
