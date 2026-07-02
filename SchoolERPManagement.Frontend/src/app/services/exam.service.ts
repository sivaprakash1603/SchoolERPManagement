import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ExamResponseDTO {
  id: number;
  examname: string;
  academicyearId?: number;
}

export interface ExamScheduleResponseDTO {
  id: number;
  examId: number;
  subjectId: number;
  subjectName: string;
  classId: number;
  className: string;
  classSection: string;
  examDate: string;
  durationMinutes: number;
  session?: string;
}

export interface CreateExamDTO {
  examname: string;
  academicyearId: number;
}

export interface CreateExamScheduleDTO {
  examId: number;
  subjectId: number;
  classId: number;
  examDate: string;
  durationMinutes: number;
  session: string;
}

export interface UpdateExamScheduleDTO {
  classId: number;
  subjectId: number;
  examDate: string;
  durationMinutes: number;
  session: string;
}

export interface PublishResultDTO {
  examId: number;
  subjectId: number;
  studentId: number;
  marks: number;
  uploadedCorrectedAnswerSheetUrl?: string;
}

export interface ExamResultResponseDTO {
  id: number;
  examId: number;
  subjectId: number;
  studentId: number;
  marks?: number;
  uploadedCorrectedAnswerSheetUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExamService {
  private readonly baseUrl = `${environment.apiUrl}/Exams`;

  constructor(private http: HttpClient) {}

  getAllExams(): Observable<ExamResponseDTO[]> {
    return this.http.get<ExamResponseDTO[]>(this.baseUrl);
  }

  createExam(dto: CreateExamDTO): Observable<ExamResponseDTO> {
    return this.http.post<ExamResponseDTO>(this.baseUrl, dto);
  }

  createExamSchedule(dto: CreateExamScheduleDTO): Observable<ExamScheduleResponseDTO> {
    return this.http.post<ExamScheduleResponseDTO>(`${this.baseUrl}/schedule`, dto);
  }

  updateExamSchedule(scheduleId: number, dto: UpdateExamScheduleDTO): Observable<ExamScheduleResponseDTO> {
    return this.http.put<ExamScheduleResponseDTO>(`${this.baseUrl}/schedules/${scheduleId}`, dto);
  }

  getExamSchedules(examId: number): Observable<ExamScheduleResponseDTO[]> {
    return this.http.get<ExamScheduleResponseDTO[]>(`${this.baseUrl}/${examId}/schedules`);
  }

  getExamResultsByClass(examId: number, classId: number, subjectId: number): Observable<ExamResultResponseDTO[]> {
    return this.http.get<ExamResultResponseDTO[]>(`${this.baseUrl}/results?examId=${examId}&classId=${classId}&subjectId=${subjectId}`);
  }

  publishResult(dto: PublishResultDTO): Observable<ExamResultResponseDTO> {
    return this.http.post<ExamResultResponseDTO>(`${this.baseUrl}/publish-result`, dto);
  }

  getStudentResults(studentId: number): Observable<ExamResultResponseDTO[]> {
    return this.http.get<ExamResultResponseDTO[]>(`${this.baseUrl}/student/${studentId}/results`);
  }
}
