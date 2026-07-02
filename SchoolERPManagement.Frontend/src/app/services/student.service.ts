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

export interface StudentQueryResponseDTO {
  id: number;
  userId: number;
  regNo: string;
  name: string;
  parentName?: string;
  className?: string;
  section?: string;
  gender?: string;
  admissionDate?: Date;
  status?: string;
  profilePhotoUrl?: string;
  bloodgroup?: string;
  dateofbirth?: string;
  parents?: ParentSelection[];
  classId?: number;
}

export interface ParentSelection {
  parentId: number;
  relation: string;
}

export interface CreateStudentDTO {
  email: string;
  name: string;
  classId: number;
  academicYearId: number;
  parents?: ParentSelection[];
  gender?: string;
  bloodgroup?: string;
  dateofbirth?: string; // YYYY-MM-DD
  admissiondate?: string; // YYYY-MM-DD
}

export interface StudentQueryRequest {
  searchQuery?: string;
  classId?: number;
  academicYearId?: number;
  gender?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface StudentStatsDTO {
  totalStudents: number;
  activeStudents: number;
  inactiveStudents: number;
  boys: number;
  girls: number;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private readonly baseUrl = `${environment.apiUrl}/Students`;

  constructor(private http: HttpClient) {}

  getAllStudents(request: StudentQueryRequest): Observable<PagedResponse<StudentQueryResponseDTO>> {
    let params = new HttpParams();
    Object.keys(request).forEach(key => {
      const value = (request as any)[key];
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<PagedResponse<StudentQueryResponseDTO>>(this.baseUrl, { params });
  }

  addStudent(dto: CreateStudentDTO): Observable<any> {
    return this.http.post(this.baseUrl, dto);
  }

  getStudentStats(): Observable<StudentStatsDTO> {
    return this.http.get<StudentStatsDTO>(`${this.baseUrl}/stats`);
  }

  exportStudentsPdf(request: StudentQueryRequest): Observable<Blob> {
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

  getStudentById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  updateStudent(id: number, dto: any): Observable<any> {
    return this.http.patch<any>(`${this.baseUrl}/${id}`, dto);
  }

  deleteStudent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  enrollStudent(studentId: number, dto: { classId: number; academicYearId: number }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${studentId}/enroll`, dto);
  }

  bulkEnrollStudents(dto: { studentIds: number[]; classId: number; academicYearId: number }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/bulk-enroll`, dto);
  }

  getStudentsByClassId(classId: number): Observable<StudentQueryResponseDTO[]> {
    return this.http.get<StudentQueryResponseDTO[]>(`${this.baseUrl}/class/${classId}`);
  }

  getStudentByUserId(userId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/by-user/${userId}`);
  }
}
