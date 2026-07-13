import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TeacherService } from './teacher.service';
import { environment } from '../../environments/environment';

describe('TeacherService', () => {
  let service: TeacherService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TeacherService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(TeacherService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getAllTeachers', () => {
    service.getAllTeachers({ status: 'Active' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers?status=Active`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call exportTeachersPdf', () => {
    service.exportTeachersPdf({ status: 'Active', searchQuery: undefined, sortBy: '' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/export/pdf?status=Active`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call addTeacher', () => {
    service.addTeacher({ email: 't@t.com', name: 'John' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getTeacherStats', () => {
    service.getTeacherStats().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call assignSubject', () => {
    service.assignSubject({ teacherId: 1, subjectId: 1, classId: 1 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/assign-subject`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getAllSubjects', () => {
    service.getAllSubjects().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getTeacherAssignments', () => {
    service.getTeacherAssignments(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/1/assignments`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call unassignSubject', () => {
    service.unassignSubject(1, 2, 3).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/1/assignments/2/3`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should call updateTeacher', () => {
    service.updateTeacher(1, { name: 'John' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call deleteTeacher', () => {
    service.deleteTeacher(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should call getTeacherByUsername', () => {
    service.getTeacherByUsername('john').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/username/john`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call autoAssignTeachers', () => {
    service.autoAssignTeachers().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Teachers/auto-assign`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
