import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PtmMeetingResponseDTO {
  id: number;
  academicCalendarId: number | null;
  eventDate: string;
  startTime: string;
  endTime: string;
  description: string;
  isActive: boolean;
}

export interface PtmSlotResponseDTO {
  id: number;
  meetingId: number;
  teacherId: number;
  teacherName: string;
  startTime: string;
  endTime: string;
  status: string;
  parentId: number | null;
  parentName: string | null;
  studentId: number | null;
  studentName: string | null;
}

export interface BookSlotRequestDTO {
  slotId: number;
  studentId: number;
}

@Injectable({ providedIn: 'root' })
export class ParentTeacherMeetingService {
  private baseUrl = `${environment.apiUrl}/ParentTeacherMeeting`;

  constructor(private http: HttpClient) {}

  getUpcoming(): Observable<PtmMeetingResponseDTO[]> {
    return this.http.get<PtmMeetingResponseDTO[]>(`${this.baseUrl}/upcoming`);
  }

  getSlots(meetingId: number, teacherId?: number, studentId?: number): Observable<PtmSlotResponseDTO[]> {
    const params: any = {};
    if (teacherId) params.teacherId = teacherId;
    if (studentId) params.studentId = studentId;
    return this.http.get<PtmSlotResponseDTO[]>(`${this.baseUrl}/${meetingId}/slots`, { params });
  }

  bookSlot(dto: BookSlotRequestDTO): Observable<PtmSlotResponseDTO> {
    return this.http.post<PtmSlotResponseDTO>(`${this.baseUrl}/book`, dto);
  }

  cancelSlot(slotId: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/cancel/${slotId}`, {});
  }

  getMyBookings(): Observable<PtmSlotResponseDTO[]> {
    return this.http.get<PtmSlotResponseDTO[]>(`${this.baseUrl}/my-bookings`);
  }
}
