import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';

@Component({
  selector: 'app-academic-sessions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './academic-sessions.html',
  styleUrl: './academic-sessions.css'
})
export class AcademicSessions implements OnInit {
  private academicYearService = inject(AcademicYearService);

  sessions = signal<AcademicYearResponseDTO[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  // Modal State
  showCreateModal = signal(false);
  isSaving = signal(false);
  
  createForm = signal({
    yearName: '',
    startDate: '',
    endDate: ''
  });

  ngOnInit() {
    this.fetchSessions();
  }

  fetchSessions() {
    this.loading.set(true);
    this.error.set(null);
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (data) => {
        this.sessions.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load academic sessions', err);
        this.error.set('Failed to load academic sessions. Please try again.');
        this.loading.set(false);
      }
    });
  }

  openCreateModal() {
    this.createForm.set({
      yearName: '',
      startDate: '',
      endDate: ''
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  saveSession() {
    const form = this.createForm();
    if (!form.yearName || !form.startDate || !form.endDate) return;

    this.isSaving.set(true);
    const dto = {
      yearName: form.yearName,
      startDate: form.startDate,
      endDate: form.endDate
    };

    this.academicYearService.createAcademicYear(dto).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchSessions();
      },
      error: (err) => {
        console.error('Failed to create academic session', err);
        alert('Failed to create academic session. Check the inputs.');
        this.isSaving.set(false);
      }
    });
  }

  setAsActive(id: number) {
    this.academicYearService.setCurrentAcademicYear(id).subscribe({
      next: () => {
        this.fetchSessions();
      },
      error: (err) => {
        console.error('Failed to activate session', err);
        alert('Failed to set the academic session as active.');
      }
    });
  }
}
