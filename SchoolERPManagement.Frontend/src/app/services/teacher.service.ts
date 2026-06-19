import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

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
}

export interface TeacherQueryRequest {
  searchQuery?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private readonly baseUrl = 'http://localhost:5203/api/Teachers';

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
}
