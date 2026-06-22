import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TimetableResponseDTO {
  id: number;
  classId: number;
  subjectId: number;
  teacherId: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  roomNo?: string;
}

export interface CreateTimetableDTO {
  classId: number;
  subjectId: number;
  teacherId: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  roomNo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TimetableService {
  private readonly baseUrl = 'http://localhost:5203/api/Timetable';

  constructor(private http: HttpClient) {}

  getClassTimetable(classId: number): Observable<TimetableResponseDTO[]> {
    return this.http.get<TimetableResponseDTO[]>(`${this.baseUrl}/class/${classId}`);
  }

  getTeacherTimetable(teacherId: number): Observable<TimetableResponseDTO[]> {
    return this.http.get<TimetableResponseDTO[]>(`${this.baseUrl}/teacher/${teacherId}`);
  }

  createTimetable(dto: CreateTimetableDTO): Observable<TimetableResponseDTO> {
    return this.http.post<TimetableResponseDTO>(this.baseUrl, dto);
  }
}
