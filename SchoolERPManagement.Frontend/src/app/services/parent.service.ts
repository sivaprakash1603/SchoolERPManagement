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

export interface ParentStatsDTO {
  totalParents: number;
  activeParents: number;
  inactiveParents: number;
}

@Injectable({
  providedIn: 'root'
})
export class ParentService {
  private readonly baseUrl = `${environment.apiUrl}/Parents`;

  constructor(private http: HttpClient) {}

  addParent(dto: CreateParentDTO): Observable<ParentResponseDTO> {
    return this.http.post<ParentResponseDTO>(this.baseUrl, dto);
  }

  getParentStats(): Observable<ParentStatsDTO> {
    return this.http.get<ParentStatsDTO>(`${this.baseUrl}/stats`);
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

  updateParent(id: number, dto: { name: string, email: string, phonenumber: string }): Observable<ParentResponseDTO> {
    return this.http.patch<ParentResponseDTO>(`${this.baseUrl}/${id}`, dto);
  }

  deleteParent(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }

  getParentByUserId(userId: number): Observable<ParentResponseDTO> {
    return this.http.get<ParentResponseDTO>(`${this.baseUrl}/by-user/${userId}`);
  }

  getParentChildren(parentId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${parentId}/children`);
  }
}
