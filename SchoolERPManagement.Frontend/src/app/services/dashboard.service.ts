import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminDashboardDTO } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = 'http://localhost:5203/api/Dashboard';

  constructor(private http: HttpClient) {}

  getAdminMetrics(academicYearId?: number): Observable<AdminDashboardDTO> {
    const url = academicYearId ? `${this.baseUrl}/admin?academicYearId=${academicYearId}` : `${this.baseUrl}/admin`;
    return this.http.get<AdminDashboardDTO>(url);
  }
}
