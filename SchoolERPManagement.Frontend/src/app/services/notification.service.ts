import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SendNotificationDTO {
  title: string;
  message: string;
  createdByUserId?: number;
  targetUserIds: number[];
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly baseUrl = 'http://localhost:5203/api/Notifications';
  private http = inject(HttpClient);

  sendNotification(dto: SendNotificationDTO): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }
}
