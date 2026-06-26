import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';

export interface SendNotificationDTO {
  title: string;
  message: string;
  createdByUserId?: number;
  targetUserIds: number[];
}

export interface UserNotificationResponseDTO {
  id: number;
  notificationId: number;
  title: string;
  message: string;
  createdAt: string;
  isRead: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly baseUrl = 'http://localhost:5203/api/Notifications';
  private readonly hubUrl = 'http://localhost:5203/hubs/notification';
  private http = inject(HttpClient);
  
  private hubConnection: signalR.HubConnection | null = null;
  
  // Reactive state
  unreadCount = signal<number>(0);
  latestNotification = signal<UserNotificationResponseDTO | null>(null);

  startConnection() {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = sessionStorage.getItem('token');
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('Error while starting SignalR connection:', err));

    this.hubConnection.on('ReceiveNotification', (notification: UserNotificationResponseDTO) => {
      this.latestNotification.set(notification);
      this.unreadCount.update(c => c + 1);
    });
  }

  stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  sendNotification(dto: SendNotificationDTO): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  getUserNotifications(userId: number): Observable<UserNotificationResponseDTO[]> {
    return this.http.get<UserNotificationResponseDTO[]>(`${this.baseUrl}/user/${userId}`);
  }

  markAsRead(userId: number, notificationId: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/user/${userId}/read/${notificationId}`, {});
  }

  markAllAsRead(userId: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/user/${userId}/readAll`, {});
  }
}
