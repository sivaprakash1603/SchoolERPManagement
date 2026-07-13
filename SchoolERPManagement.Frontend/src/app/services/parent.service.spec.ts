import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ParentService } from './parent.service';
import { environment } from '../../environments/environment';

describe('ParentService', () => {
  let service: ParentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ParentService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ParentService);
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call addParent', () => {
    service.addParent({ email: 'p@p.com', name: 'John', phonenumber: '123' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getParentStats', () => {
    service.getParentStats().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getAllParents', () => {
    service.getAllParents({ status: 'Active', searchQuery: undefined }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents?status=Active`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call exportParentsPdf', () => {
    service.exportParentsPdf({ status: 'Active', searchQuery: undefined }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/export/pdf?status=Active`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });

  it('should call updateParent', () => {
    service.updateParent(1, { name: 'John', email: 'a@a.com', phonenumber: '1' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/1`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call deleteParent', () => {
    service.deleteParent(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should call getParentByUserId', () => {
    service.getParentByUserId(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/by-user/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getParentChildren', () => {
    service.getParentChildren(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Parents/1/children`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should handle selectedChildId', () => {
    expect(service.selectedChildId).toBeNull();
    service.selectedChildId = 5;
    expect(sessionStorage.getItem('selectedChildId')).toBe('5');
    expect(service.selectedChildId).toBe(5);
    service.selectedChildId = null;
    expect(sessionStorage.getItem('selectedChildId')).toBeNull();
    expect(service.selectedChildId).toBeNull();
  });
});
