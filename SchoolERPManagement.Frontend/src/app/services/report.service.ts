import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private http = inject(HttpClient);
  // Base URL should match environment config. Assuming default local backend.
  private apiUrl = `${environment.apiUrl}/reports`; 

  /**
   * Helper to trigger native browser download
   */
  private downloadFile(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Converts a filter object into HttpParams
   */
  private buildParams(filters: any): HttpParams {
    let params = new HttpParams();
    if (!filters) return params;

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.append(key, filters[key]);
      }
    });
    return params;
  }

  getExamPerformanceReport(examId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/exams/${examId}`);
  }

  // --- Export Methods ---

  exportFeeCollectionPdf(filters: any = {}): void {
    const params = this.buildParams(filters);
    this.http.get(`${this.apiUrl}/fees/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'fee-collection-report.pdf'));
  }

  exportStudentAttendancePdf(filters: any = {}): void {
    const params = this.buildParams(filters);
    this.http.get(`${this.apiUrl}/attendance/students/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'student-attendance-report.pdf'));
  }

  exportStaffAttendancePdf(filters: any = {}): void {
    const params = this.buildParams(filters);
    this.http.get(`${this.apiUrl}/attendance/staff/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'staff-attendance-report.pdf'));
  }

  exportExamResultsPdf(filters: any = {}): void {
    const params = this.buildParams(filters);
    this.http.get(`${this.apiUrl}/exams/results/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'exam-results-report.pdf'));
  }

  exportAssetsPdf(filters: any = {}): void {
    const params = this.buildParams(filters);
    this.http.get(`${this.apiUrl}/assets/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'assets-inventory-report.pdf'));
  }
}
