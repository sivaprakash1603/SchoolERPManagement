import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { HomeworkService } from './homework.service';
import { environment } from '../../environments/environment';

describe('HomeworkService', () => {
  let service: HomeworkService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        HomeworkService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(HomeworkService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getHomeworks without subjectId', () => {
    service.getHomeworks(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/class/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getHomeworks with subjectId', () => {
    service.getHomeworks(1, 2).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/class/1?subjectId=2`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getHomeworkSubmissions', () => {
    service.getHomeworkSubmissions(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/1/submissions`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getHomeworksByUser', () => {
    service.getHomeworksByUser(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/user/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getHomeworksByStudentId', () => {
    service.getHomeworksByStudentId(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/student/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call createHomework', () => {
    service.createHomework(new FormData()).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call submitHomework', () => {
    service.submitHomework(new FormData()).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/submit`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call evaluateHomework', () => {
    service.evaluateHomework({ homeworkSubmissionId: 1, marks: 10, remarks: 'Good', verificationStatus: 'approved' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/evaluate`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call unsubmitHomework', () => {
    service.unsubmitHomework(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Homework/submissions/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(true);
  });
});
