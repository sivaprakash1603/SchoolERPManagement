import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DocumentResponseDTO {
  id: number;
  blobUrl: string;
  documentType: string;
  documentName: string;
  uploadedAt: string;
  status: string;
}

export interface PendingDocumentDTO {
  id: number;
  documentName: string;
  documentType: string;
  blobUrl: string;
  uploadedAt?: string;
  ownerId: number;
  ownerName: string;
  ownerType: string;
  ownerIdentifier: string;
  classId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  uploadStudentDocument(studentId: number, file: File, documentName?: string): Observable<DocumentResponseDTO> {
    const formData = new FormData();
    formData.append('file', file);
    if (documentName) {
      formData.append('documentName', documentName);
    }

    return this.http.post<DocumentResponseDTO>(`${this.apiUrl}/documents/student/${studentId}`, formData);
  }

  getStudentDocuments(studentId: number): Observable<DocumentResponseDTO[]> {
    return this.http.get<DocumentResponseDTO[]>(`${this.apiUrl}/documents/student/${studentId}`);
  }

  uploadTeacherDocument(teacherId: number, file: File, documentName?: string): Observable<DocumentResponseDTO> {
    const formData = new FormData();
    formData.append('file', file);
    if (documentName) {
      formData.append('documentName', documentName);
    }
    return this.http.post<DocumentResponseDTO>(`${this.apiUrl}/documents/teacher/${teacherId}`, formData);
  }

  getTeacherDocuments(teacherId: number): Observable<DocumentResponseDTO[]> {
    return this.http.get<DocumentResponseDTO[]>(`${this.apiUrl}/documents/teacher/${teacherId}`);
  }

  deleteDocument(blobUrl: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/documents?blobUrl=${encodeURIComponent(blobUrl)}`);
  }

  verifyDocument(dto: { documentType: string; documentId: number; status: string }): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/documents/verify`, dto);
  }

  getPendingDocuments(): Observable<PendingDocumentDTO[]> {
    return this.http.get<PendingDocumentDTO[]>(`${this.apiUrl}/documents/pending`);
  }
}
