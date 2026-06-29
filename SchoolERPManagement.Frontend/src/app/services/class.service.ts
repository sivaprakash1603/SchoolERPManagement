import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ClassResponseDTO {
  id: number;
  classname: string;
  section?: string;
  classteacherId?: number;
  academicyearId?: number;
  studentCount?: number;
  subjects?: { id: number; subjectName: string; subjectCode: string }[];
}

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private readonly baseUrl = 'http://localhost:5203/api/Classes';

  constructor(private http: HttpClient) {}

  getAllClasses(academicYearId?: number): Observable<ClassResponseDTO[]> {
    const url = academicYearId ? `${this.baseUrl}?academicYearId=${academicYearId}` : this.baseUrl;
    return this.http.get<ClassResponseDTO[]>(url);
  }

  createClass(dto: { classname: string; section: string; classteacherId?: number; academicyearId?: number; subjectIds?: number[] }): Observable<ClassResponseDTO> {
    return this.http.post<ClassResponseDTO>(this.baseUrl, dto);
  }

  updateClass(id: number, dto: { classname: string; section: string; classteacherId?: number; academicyearId?: number; subjectIds?: number[] }): Observable<ClassResponseDTO> {
    return this.http.put<ClassResponseDTO>(`${this.baseUrl}/${id}`, dto);
  }

  deleteClass(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }
}
