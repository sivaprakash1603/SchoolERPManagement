import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ExamService } from './exam.service';
import { environment } from '../../environments/environment';

describe('ExamService', () => {
  let service: ExamService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ExamService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ExamService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getAllExams without classId', () => {
    service.getAllExams().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getAllExams with classId', () => {
    service.getAllExams(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams?classId=1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call createExam', () => {
    service.createExam({ examname: 'Midterm', academicyearId: 1 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call createExamSchedule', () => {
    service.createExamSchedule({ examId: 1, subjectId: 1, classId: 1, examDate: '2023-10-10', durationMinutes: 60, session: 'Morning' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/schedule`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call updateExamSchedule', () => {
    service.updateExamSchedule(1, { subjectId: 1, classId: 1, examDate: '2023-10-10', durationMinutes: 60, session: 'Morning' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/schedules/1`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should call getExamSchedules', () => {
    service.getExamSchedules(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/1/schedules`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getExamResultsByClass', () => {
    service.getExamResultsByClass(1, 2, 3).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/results?examId=1&classId=2&subjectId=3`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call publishResult', () => {
    service.publishResult({ examId: 1, subjectId: 2, studentId: 3, marks: 95 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/publish-result`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getStudentResults', () => {
    service.getStudentResults(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Exams/student/1/results`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
