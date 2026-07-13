import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Auth } from './auth';
import { environment } from '../../environments/environment';

describe('Auth', () => {
  let service: Auth;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        Auth,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(Auth);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call loginApiCall', () => {
    service.loginApiCall({ username: 'u', password: 'p' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call forgotPassword', () => {
    service.forgotPassword('test@test.com').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/forgot-password`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call resetPassword', () => {
    service.resetPassword({ email: 'e', token: 't', newPassword: 'p' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/reset-password`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
