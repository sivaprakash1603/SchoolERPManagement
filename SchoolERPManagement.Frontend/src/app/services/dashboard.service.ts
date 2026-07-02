import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminDashboardDTO, TeacherDashboardDTO } from '../models/dashboard.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly baseUrl = `${environment.apiUrl}/Dashboard`;

  constructor(private http: HttpClient) {}

  getAdminMetrics(academicYearId?: number): Observable<AdminDashboardDTO> {
    const url = academicYearId ? `${this.baseUrl}/admin?academicYearId=${academicYearId}` : `${this.baseUrl}/admin`;
    return this.http.get<AdminDashboardDTO>(url);
  }

  getTeacherMetrics(teacherId: number, academicYearId?: number): Observable<TeacherDashboardDTO> {
    const url = academicYearId ? `${this.baseUrl}/teacher/${teacherId}?academicYearId=${academicYearId}` : `${this.baseUrl}/teacher/${teacherId}`;
    return this.http.get<TeacherDashboardDTO>(url);
  }
}
