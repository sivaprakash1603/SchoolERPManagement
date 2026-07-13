import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { NotificationService } from './notification.service';
import { environment } from '../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { vi } from 'vitest';

describe('NotificationService', () => {
  let service: NotificationService;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        NotificationService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    vi.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start connection if token exists', () => {
    sessionStorage.setItem('token', 'abc');
    const onMock = vi.fn();
    const startMock = vi.fn().mockResolvedValue(true);
    const stopMock = vi.fn();
    const mockHubConnection: any = {
      state: signalR.HubConnectionState.Disconnected,
      start: startMock,
      stop: stopMock,
      on: onMock
    };

    const builderMock = {
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      build: vi.fn().mockReturnValue(mockHubConnection)
    };
    vi.spyOn(signalR, 'HubConnectionBuilder').mockImplementation(function() {
      return builderMock;
    } as any);

    service.startConnection();
    expect(startMock).toHaveBeenCalled();
    expect(onMock).toHaveBeenCalledWith('ReceiveNotification', expect.any(Function));
    
    // Test receiving notification
    const callback = onMock.mock.calls[0][1];
    callback({ id: 1, title: 'T', message: 'M' });
    expect(service.unreadCount()).toBe(1);
    expect(service.latestNotification()?.id).toBe(1);

    // Call stop
    service.stopConnection();
    expect(stopMock).toHaveBeenCalled();
  });

  it('should not start connection if token missing', () => {
    const builderMock = vi.spyOn(signalR, 'HubConnectionBuilder');
    service.startConnection();
    expect(builderMock).not.toHaveBeenCalled();
  });
  
  it('should not start connection if already connected', () => {
    sessionStorage.setItem('token', 'abc');
    (service as any).hubConnection = { state: signalR.HubConnectionState.Connected };
    const builderMock = vi.spyOn(signalR, 'HubConnectionBuilder');
    service.startConnection();
    expect(builderMock).not.toHaveBeenCalled();
  });

  it('should call sendNotification', () => {
    service.sendNotification({ title: 'T', message: 'M', targetUserIds: [1] }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call getUserNotifications', () => {
    service.getUserNotifications(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications/user/1`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should call markAsRead', () => {
    service.markAsRead(1, 2).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications/user/1/read/2`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should call markAllAsRead', () => {
    service.markAllAsRead(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications/user/1/readAll`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });
});
