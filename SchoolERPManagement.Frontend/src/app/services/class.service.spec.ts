import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ClassService } from './class.service';
import { environment } from '../../environments/environment';

describe('ClassService', () => {
  let service: ClassService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ClassService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ClassService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAllClasses without academicYearId', () => {
    service.getAllClasses().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Classes`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getAllClasses with academicYearId', () => {
    service.getAllClasses(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Classes?academicYearId=1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call createClass', () => {
    service.createClass({ classname: 'A', section: 'B' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Classes`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call updateClass', () => {
    service.updateClass(1, { classname: 'A', section: 'B' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Classes/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call deleteClass', () => {
    service.deleteClass(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Classes/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
