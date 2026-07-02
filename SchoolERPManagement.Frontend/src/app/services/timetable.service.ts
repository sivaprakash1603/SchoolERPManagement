import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

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

export interface TeacherRequirementDTO {
  subjectId: number;
  subjectName: string;
  totalClassesTakingSubject: number;
  requiredTeachers: number;
  availableTeachers: number;
  status: string;
}

export interface PeriodTimingDTO {
  periodNumber: number;
  startTime: string;
  endTime: string;
}

export interface GenerateTimetableRequestDTO {
  classIds: number[];
  periodsPerDay: number;
  freePeriodsPerStaff: number;
  timings: PeriodTimingDTO[];
}

@Injectable({
  providedIn: 'root'
})
export class TimetableService {
  private readonly baseUrl = `${environment.apiUrl}/Timetable`;

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

  getTeacherRequirements(periodsPerDay: number, freePeriodsPerStaff: number): Observable<TeacherRequirementDTO[]> {
    return this.http.get<TeacherRequirementDTO[]>(`${this.baseUrl}/teacher-requirements?periodsPerDay=${periodsPerDay}&freePeriodsPerStaff=${freePeriodsPerStaff}`);
  }

  generateTimetable(request: GenerateTimetableRequestDTO): Observable<TimetableResponseDTO[]> {
    return this.http.post<TimetableResponseDTO[]>(`${this.baseUrl}/generate`, request);
  }

  saveGeneratedTimetable(generatedTimetable: TimetableResponseDTO[]): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/save-generated`, generatedTimetable);
  }

  updateTimetableSlot(id: number, dto: { subjectId: number, teacherId: number, roomNo?: string }): Observable<TimetableResponseDTO> {
    return this.http.patch<TimetableResponseDTO>(`${this.baseUrl}/${id}`, dto);
  }
}
