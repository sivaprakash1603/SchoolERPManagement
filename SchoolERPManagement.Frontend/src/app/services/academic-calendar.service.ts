import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CalendarEventResponseDTO {
  id: number;
  date: string;
  description: string;
  isHoliday: boolean;
  academicYearId: number;
}

export interface CreateCalendarEventDTO {
  date: string; // YYYY-MM-DD
  endDate?: string; // YYYY-MM-DD
  description: string;
  isHoliday: boolean;
  academicYearId: number;
}

export interface AcademicCalendarSummaryDTO {
  events: CalendarEventResponseDTO[];
  startDate: string;
  endDate: string;
  totalDays: number;
  weekendDays: number;
  holidayDays: number;
  workingDays: number;
}

@Injectable({
  providedIn: 'root'
})
export class AcademicCalendarService {
  private readonly baseUrl = `${environment.apiUrl}/AcademicCalendar`;

  constructor(private http: HttpClient) {}

  getAcademicCalendarSummary(academicYearId: number): Observable<AcademicCalendarSummaryDTO> {
    return this.http.get<AcademicCalendarSummaryDTO>(`${this.baseUrl}/year/${academicYearId}`);
  }

  createCalendarEvent(dto: CreateCalendarEventDTO): Observable<CalendarEventResponseDTO> {
    return this.http.post<CalendarEventResponseDTO>(this.baseUrl, dto);
  }

  deleteCalendarEvent(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }
}
