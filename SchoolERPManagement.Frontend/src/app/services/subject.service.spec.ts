import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SubjectService } from './subject.service';
import { environment } from '../../environments/environment';

describe('SubjectService', () => {
  let service: SubjectService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SubjectService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(SubjectService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAllSubjects', () => {
    service.getAllSubjects().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getSubjectsByClass', () => {
    service.getSubjectsByClass(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects/class/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getSubjectById', () => {
    service.getSubjectById(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call createSubject', () => {
    service.createSubject({ subjectName: 'Math' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call updateSubject', () => {
    service.updateSubject(1, { subjectName: 'Math' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call deleteSubject', () => {
    service.deleteSubject(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Subjects/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
