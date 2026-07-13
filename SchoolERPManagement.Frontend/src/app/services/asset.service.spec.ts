import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AssetService } from './asset.service';
import { environment } from '../../environments/environment';

describe('AssetService', () => {
  let service: AssetService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AssetService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AssetService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call getAssets', () => {
    service.getAssets().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call getAssetStats', () => {
    service.getAssetStats().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should call getAssetTypes', () => {
    service.getAssetTypes().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets/types`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call addAsset', () => {
    service.addAsset({ assetname: 'Laptop' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call reportAssetIssue', () => {
    service.reportAssetIssue({ assetId: 1, status: 'Broken' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets/report`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getAssetReports', () => {
    service.getAssetReports().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assets/reports`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
