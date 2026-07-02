import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AcademicYearResponseDTO {
  id: number;
  yearName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

export interface CreateAcademicYearDTO {
  yearName: string;
  startDate: string;
  endDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class AcademicYearService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/AcademicYears`;

  getAllAcademicYears(): Observable<AcademicYearResponseDTO[]> {
    return this.http.get<AcademicYearResponseDTO[]>(this.baseUrl);
  }

  createAcademicYear(dto: CreateAcademicYearDTO): Observable<AcademicYearResponseDTO> {
    return this.http.post<AcademicYearResponseDTO>(this.baseUrl, dto);
  }

  setCurrentAcademicYear(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/set-current`, {});
  }
}
