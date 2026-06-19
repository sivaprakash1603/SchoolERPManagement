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

export interface ParentResponseDTO {
  id: number;
  userId: number;
  name: string;
  relation: string;
  phonenumber: string;
  email: string;
  username: string;
}

export interface ParentQueryRequest {
  searchQuery?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface CreateParentDTO {
  email: string;
  name: string;
  relation?: string;
  phonenumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class ParentService {
  private readonly baseUrl = 'http://localhost:5203/api/Parents';

  constructor(private http: HttpClient) {}

  addParent(dto: CreateParentDTO): Observable<ParentResponseDTO> {
    return this.http.post<ParentResponseDTO>(this.baseUrl, dto);
  }

  getAllParents(request: ParentQueryRequest): Observable<PagedResponse<ParentResponseDTO>> {
    let params = new HttpParams();
    Object.keys(request).forEach(key => {
      const value = (request as any)[key];
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<PagedResponse<ParentResponseDTO>>(this.baseUrl, { params });
  }

  exportParentsPdf(request: ParentQueryRequest): Observable<Blob> {
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
