import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private readonly baseUrl = 'http://localhost:5203/api/AcademicYears';

  getAllAcademicYears(): Observable<AcademicYearResponseDTO[]> {
    return this.http.get<AcademicYearResponseDTO[]>(this.baseUrl);
  }

  createAcademicYear(dto: CreateAcademicYearDTO): Observable<AcademicYearResponseDTO> {
    return this.http.post<AcademicYearResponseDTO>(this.baseUrl, dto);
  }

  setCurrentAcademicYear(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/set-current`, {});
  }
}
