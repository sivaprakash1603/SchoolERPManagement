import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TeacherResponseDTO {
  id: number;
  userId: number;
  name: string;
  phonenumber: string;
  joiningdate: Date;
  qualifications?: string;
  username: string;
  className?: string;
  section?: string;
  email?: string;
  profilePhotoUrl?: string;
  subjectSpecialtyId?: number;
  subjectSpecialtyName?: string;
  assignments?: { classId: number; subjectId: number }[];
}

export interface TeacherQueryRequest {
  searchQuery?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface TeacherStatsDTO {
  totalTeachers: number;
  activeTeachers: number;
  inactiveTeachers: number;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private readonly baseUrl = `${environment.apiUrl}/Teachers`;

  constructor(private http: HttpClient) {}

  getAllTeachers(request: TeacherQueryRequest): Observable<PagedResponse<TeacherResponseDTO>> {
    let params = new HttpParams();
    Object.keys(request).forEach(key => {
      const value = (request as any)[key];
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<PagedResponse<TeacherResponseDTO>>(this.baseUrl, { params });
  }

  exportTeachersPdf(request: TeacherQueryRequest): Observable<Blob> {
    let params = new HttpParams();
    Object.keys(request).forEach(key => {
      const value = (request as any)[key];
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get(`${this.baseUrl}/export/pdf`, { 
      params,
      responseType: 'blob' 
    });
  }

  addTeacher(dto: { email: string, name: string, phonenumber?: string, qualifications?: string, subjectSpecialtyId?: number }): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  getTeacherStats(): Observable<TeacherStatsDTO> {
    return this.http.get<TeacherStatsDTO>(`${this.baseUrl}/stats`);
  }

  assignSubject(dto: { teacherId: number, subjectId: number, classId: number }): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/assign-subject`, dto);
  }

  getAllSubjects(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/Subjects`);
  }

  getTeacherAssignments(teacherId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${teacherId}/assignments`);
  }

  unassignSubject(teacherId: number, classId: number, subjectId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teacherId}/assignments/${classId}/${subjectId}`);
  }

  updateTeacher(id: number, dto: { name: string, phonenumber?: string, qualifications?: string, subjectSpecialtyId?: number }): Observable<TeacherResponseDTO> {
    return this.http.patch<TeacherResponseDTO>(`${this.baseUrl}/${id}`, dto);
  }

  deleteTeacher(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getTeacherByUsername(username: string): Observable<TeacherResponseDTO> {
    return this.http.get<TeacherResponseDTO>(`${this.baseUrl}/username/${username}`);
  }

  autoAssignTeachers(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auto-assign`, {});
  }
}
