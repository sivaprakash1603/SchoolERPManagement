import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { StudentService } from './student.service';
import { environment } from '../../environments/environment';

describe('StudentService', () => {
  let service: StudentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StudentService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(StudentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all students with query params', () => {
    service.getAllStudents({ classId: 1, gender: 'Male', status: undefined }).subscribe();
    const req = httpMock.expectOne(request => request.url === `${environment.apiUrl}/Students`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('classId')).toBe('1');
    expect(req.request.params.get('gender')).toBe('Male');
    expect(req.request.params.has('status')).toBe(false);
    req.flush({ items: [], totalCount: 0 });
  });

  it('should add student', () => {
    const dto: any = { name: 'John', classId: 1 };
    service.addStudent(dto).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should get student stats', () => {
    service.getStudentStats().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should export pdf', () => {
    service.exportStudentsPdf({ classId: 1, status: undefined, gender: '' }).subscribe();
    const req = httpMock.expectOne(request => request.url === `${environment.apiUrl}/Students/export/pdf`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('classId')).toBe('1');
    expect(req.request.params.has('status')).toBe(false);
    expect(req.request.params.has('gender')).toBe(false);
    req.flush(new Blob());
  });

  it('should get student by id', () => {
    service.getStudentById(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should update student', () => {
    service.updateStudent(1, { name: 'John' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should delete student', () => {
    service.deleteStudent(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should enroll student', () => {
    service.enrollStudent(1, { classId: 1, academicYearId: 1 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/1/enroll`);
    expect(req.request.method).toBe('POST');
    req.flush(null);
  });

  it('should bulk enroll students', () => {
    service.bulkEnrollStudents({ studentIds: [1, 2], classId: 1, academicYearId: 1 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/bulk-enroll`);
    expect(req.request.method).toBe('POST');
    req.flush(null);
  });

  it('should get students by class id', () => {
    service.getStudentsByClassId(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/class/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get student by user id', () => {
    service.getStudentByUserId(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Students/by-user/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
