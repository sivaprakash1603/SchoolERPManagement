import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChatRequest {
  query: string;
  role?: string;
}

export interface ChatResponse {
  answer: string;
}

export interface SqlSearchRequest {
  query: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private http = inject(HttpClient);
  private aiApiUrl = environment.aiApiUrl;

  chatFaq(query: string, role?: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.aiApiUrl}/chat/faq`, { query, role });
  }

  uploadFaq(file: File): Observable<{ message: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ message: string }>(`${this.aiApiUrl}/chat/upload`, formData);
  }

  adminSearch(query: string): Observable<Blob> {
    // We expect a blob (Excel file) response
    return this.http.post(`${this.aiApiUrl}/admin/search`, { query }, {
      responseType: 'blob'
    });
  }
}
