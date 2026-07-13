import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DocumentService } from './document.service';
import { environment } from '../../environments/environment';

describe('DocumentService', () => {
  let service: DocumentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DocumentService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DocumentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call uploadStudentDocument', () => {
    const file = new File([''], 'test.pdf', { type: 'application/pdf' });
    service.uploadStudentDocument(1, file, 'TestDoc').subscribe(res => {
      expect(res).toBeTruthy();
    });
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/student/1`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.get('documentName')).toBe('TestDoc');
    req.flush({});
  });

  it('should call uploadStudentDocument without doc name', () => {
    const file = new File([''], 'test.pdf', { type: 'application/pdf' });
    service.uploadStudentDocument(1, file).subscribe(res => {
      expect(res).toBeTruthy();
    });
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/student/1`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.has('documentName')).toBe(false);
    req.flush({});
  });

  it('should call getStudentDocuments', () => {
    service.getStudentDocuments(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/student/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call uploadTeacherDocument', () => {
    const file = new File([''], 'test.pdf', { type: 'application/pdf' });
    service.uploadTeacherDocument(1, file, 'Doc').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/teacher/1`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.get('documentName')).toBe('Doc');
    req.flush({});
  });

  it('should call uploadTeacherDocument without doc name', () => {
    const file = new File([''], 'test.pdf', { type: 'application/pdf' });
    service.uploadTeacherDocument(1, file).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/teacher/1`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.has('documentName')).toBe(false);
    req.flush({});
  });

  it('should call getTeacherDocuments', () => {
    service.getTeacherDocuments(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/teacher/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call deleteDocument', () => {
    service.deleteDocument('test-url').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents?blobUrl=test-url`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should call verifyDocument', () => {
    service.verifyDocument({ documentId: 1, documentType: 'T', status: 'S' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/verify`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call getPendingDocuments', () => {
    service.getPendingDocuments().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/documents/pending`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
