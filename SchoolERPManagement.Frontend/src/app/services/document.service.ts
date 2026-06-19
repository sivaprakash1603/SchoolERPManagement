import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentResponseDTO {
  id: number;
  blobUrl: string;
  documentType: string;
  documentName: string;
  uploadedAt: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5203/api';

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

  deleteDocument(blobUrl: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/documents?blobUrl=${encodeURIComponent(blobUrl)}`);
  }
}
