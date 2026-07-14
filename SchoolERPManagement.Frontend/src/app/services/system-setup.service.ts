import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface SetupStatusResponse {
  isComplete: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SystemSetupService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/SystemSetup`;
  
  private setupStatusCache: boolean | null = null;

  getSetupStatus(): Observable<SetupStatusResponse> {
    if (this.setupStatusCache !== null) {
      return of({ isComplete: this.setupStatusCache });
    }

    return this.http.get<SetupStatusResponse>(`${this.apiUrl}/status`).pipe(
      tap(res => this.setupStatusCache = res.isComplete),
      catchError(err => {
        console.error('Failed to get setup status', err);
        return of({ isComplete: false });
      })
    );
  }
  
  markSetupComplete(): void {
    this.setupStatusCache = true;
  }
}
