import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SubjectResponseDTO {
  id: number;
  subjectName: string;
}

export interface CreateSubjectDTO {
  subjectName: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  private readonly baseUrl = 'http://localhost:5203/api/Subjects';

  constructor(private http: HttpClient) {}

  getAllSubjects(): Observable<SubjectResponseDTO[]> {
    return this.http.get<SubjectResponseDTO[]>(this.baseUrl);
  }

  getSubjectsByClass(classId: number): Observable<SubjectResponseDTO[]> {
    return this.http.get<SubjectResponseDTO[]>(`${this.baseUrl}/class/${classId}`);
  }

  getSubjectById(id: number): Observable<SubjectResponseDTO> {
    return this.http.get<SubjectResponseDTO>(`${`${this.baseUrl}/${id}`}`);
  }

  createSubject(dto: CreateSubjectDTO): Observable<SubjectResponseDTO> {
    return this.http.post<SubjectResponseDTO>(this.baseUrl, dto);
  }

  updateSubject(id: number, dto: CreateSubjectDTO): Observable<SubjectResponseDTO> {
    return this.http.put<SubjectResponseDTO>(`${`${this.baseUrl}/${id}`}`, dto);
  }

  deleteSubject(id: number): Observable<any> {
    return this.http.delete<any>(`${`${this.baseUrl}/${id}`}`);
  }
}
