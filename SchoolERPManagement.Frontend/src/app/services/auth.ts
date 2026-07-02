import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginRequest, AuthResponse } from '../models/login.model';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) {}

  loginApiCall(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, data);
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(data: { email: string, token: string, newPassword: string }): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/reset-password`, data);
  }
}
