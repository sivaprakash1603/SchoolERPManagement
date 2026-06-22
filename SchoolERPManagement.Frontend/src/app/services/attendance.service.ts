import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AttendanceResponseDTO {
  id: number;
  studentId: number;
  date: string;
  status: string; // e.g. "present", "absent", "late"
  markedByTeacherId?: number;
  remarks?: string;
}

export interface MarkAttendanceDTO {
  studentId: number;
  date: string; // YYYY-MM-DD
  status: string; // e.g. "present", "absent"
  markedByTeacherId?: number;
  remarks?: string;
}

export interface StaffAttendanceResponseDTO {
  id: number;
  userId: number;
  username?: string;
  date: string;
  status: string;
  attendanceType: string;
  remarks?: string;
}

export interface StaffAttendanceRequestDTO {
  userId: number;
  date: string; // YYYY-MM-DD
  status: string; // Present, Absent, Late, OnLeave
  attendanceType: string; // Daily, HalfDay, etc.
  remarks?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly baseUrl = 'http://localhost:5203/api/Attendance';
  private readonly staffBaseUrl = 'http://localhost:5203/api/StaffAttendance';

  constructor(private http: HttpClient) {}

  getAttendanceByClass(classId: number, date: string): Observable<AttendanceResponseDTO[]> {
    return this.http.get<AttendanceResponseDTO[]>(`${this.baseUrl}/class/${classId}?date=${date}`);
  }

  markAttendance(dto: MarkAttendanceDTO): Observable<AttendanceResponseDTO> {
    return this.http.post<AttendanceResponseDTO>(this.baseUrl, dto);
  }

  getAttendanceByStudent(studentId: number): Observable<AttendanceResponseDTO[]> {
    return this.http.get<AttendanceResponseDTO[]>(`${this.baseUrl}/student/${studentId}`);
  }

  // --- STAFF ATTENDANCE ---
  getStaffAttendanceByDate(date: string): Observable<StaffAttendanceResponseDTO[]> {
    return this.http.get<StaffAttendanceResponseDTO[]>(`${this.staffBaseUrl}/date/${date}`);
  }

  getStaffAttendanceByUser(userId: number): Observable<StaffAttendanceResponseDTO[]> {
    return this.http.get<StaffAttendanceResponseDTO[]>(`${this.staffBaseUrl}/user/${userId}`);
  }

  markStaffAttendance(dto: StaffAttendanceRequestDTO): Observable<StaffAttendanceResponseDTO> {
    return this.http.post<StaffAttendanceResponseDTO>(this.staffBaseUrl, dto);
  }
}
