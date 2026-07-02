import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface HomeworkResponseDTO {
  id: number;
  subjectId: number;
  subjectName?: string;
  teacherId: number;
  teacherName?: string;
  classId: number;
  classname?: string;
  section?: string;
  title: string;
  description: string;
  attachmentUrl?: string;
  createdAt: string;
  dueDate: string;
  submissions?: HomeworkSubmissionResponseDTO[];
  submission?: HomeworkSubmissionResponseDTO;
}

export interface HomeworkSubmissionResponseDTO {
  id: number;
  homeworkId: number;
  studentId: number;
  studentName?: string;
  uploadedFileUrl?: string;
  submittedAt: string;
  marks?: number;
  remarks?: string;
  verificationStatus?: string; // 'pending', 'approved', 'rejected'
}

export interface HomeworkSubmissionDetailsDTO {
  id: number;
  homeworkId: number;
  studentId: number;
  studentName: string;
  uploadedFileUrl?: string;
  verificationStatus?: string;
  marks?: number;
  remarks?: string;
  submittedAt?: string;
}

export interface EvaluateHomeworkDTO {
  homeworkSubmissionId: number;
  marks: number;
  remarks: string;
  verificationStatus: string;
}

@Injectable({
  providedIn: 'root'
})
export class HomeworkService {
  private readonly baseUrl = `${environment.apiUrl}/Homework`;

  constructor(private http: HttpClient) {}

  getHomeworks(classId: number, subjectId?: number): Observable<HomeworkResponseDTO[]> {
    let url = `${this.baseUrl}/class/${classId}`;
    if (subjectId) {
      url += `?subjectId=${subjectId}`;
    }
    return this.http.get<HomeworkResponseDTO[]>(url);
  }

  getHomeworkSubmissions(homeworkId: number): Observable<HomeworkSubmissionDetailsDTO[]> {
    return this.http.get<HomeworkSubmissionDetailsDTO[]>(`${this.baseUrl}/${homeworkId}/submissions`);
  }

  getHomeworksByUser(userId: number): Observable<HomeworkResponseDTO[]> {
    return this.http.get<HomeworkResponseDTO[]>(`${this.baseUrl}/user/${userId}`);
  }

  getHomeworksByStudentId(studentId: number): Observable<HomeworkResponseDTO[]> {
    return this.http.get<HomeworkResponseDTO[]>(`${this.baseUrl}/student/${studentId}`);
  }

  createHomework(formData: FormData): Observable<HomeworkResponseDTO> {
    return this.http.post<HomeworkResponseDTO>(this.baseUrl, formData);
  }

  submitHomework(formData: FormData): Observable<HomeworkSubmissionResponseDTO> {
    return this.http.post<HomeworkSubmissionResponseDTO>(`${this.baseUrl}/submit`, formData);
  }

  evaluateHomework(dto: EvaluateHomeworkDTO): Observable<HomeworkSubmissionResponseDTO> {
    return this.http.post<HomeworkSubmissionResponseDTO>(`${this.baseUrl}/evaluate`, dto);
  }
}
