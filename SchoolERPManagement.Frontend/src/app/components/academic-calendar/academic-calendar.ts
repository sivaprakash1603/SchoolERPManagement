import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AcademicCalendarService, AcademicCalendarSummaryDTO, CalendarEventResponseDTO } from '../../services/academic-calendar.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-academic-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './academic-calendar.html',
  styleUrl: './academic-calendar.css',
})
export class AcademicCalendar implements OnInit {
  private calendarService = inject(AcademicCalendarService);
  private academicYearService = inject(AcademicYearService);
  private toastService = inject(ToastService);

  academicYears = signal<AcademicYearResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | null>(null);
  summary = signal<AcademicCalendarSummaryDTO | null>(null);

  loading = signal(false);
  error = signal<string | null>(null);
  isAdmin = signal(false);

  // Date constraints
  minDate = signal<string>('');
  maxDate = signal<string>('');


  // Modal States
  showCreateModal = signal(false);
  showDeleteModal = signal(false);
  isSaving = signal(false);
  isDeleting = signal(false);
  deletingEvent = signal<CalendarEventResponseDTO | null>(null);

  // Form State
  createForm = signal({
    date: '',
    description: '',
    isHoliday: true
  });

  ngOnInit() {
    const role = sessionStorage.getItem('role');
    this.isAdmin.set(role === 'Admin');
    this.fetchAcademicYears();
  }

  fetchAcademicYears() {
    this.loading.set(true);
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find(y => y.isCurrent);
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
        } else if (years.length > 0) {
          this.selectedAcademicYearId.set(years[0].id);
        }
        this.fetchCalendar();
      },
      error: (err) => {
        console.error('Failed to load academic sessions', err);
        this.error.set('Failed to load academic sessions.');
        this.loading.set(false);
      }
    });
  }

  fetchCalendar() {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) {
      this.summary.set(null);
      return;
    }

    const year = this.academicYears().find(y => y.id === yearId);
    if (year) {
      this.minDate.set(year.startDate.split('T')[0]);
      this.maxDate.set(year.endDate.split('T')[0]);
    }

    this.loading.set(true);
    this.error.set(null);
    this.calendarService.getAcademicCalendarSummary(yearId).subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load academic calendar', err);
        this.error.set('Failed to load academic calendar.');
        this.loading.set(false);
      }
    });
  }


  onYearChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const val = select.value ? parseInt(select.value, 10) : null;
    this.selectedAcademicYearId.set(val);
    this.fetchCalendar();
  }

  openCreateModal() {
    const min = this.minDate();
    const max = this.maxDate();
    const todayStr = new Date().toISOString().split('T')[0];
    let defaultDate = todayStr;
    if (defaultDate < min || defaultDate > max) {
      defaultDate = min;
    }

    this.createForm.set({
      date: defaultDate,
      description: '',
      isHoliday: true
    });
    this.showCreateModal.set(true);
  }


  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  saveCalendarEvent() {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) return;

    const form = this.createForm();
    if (!form.date || !form.description.trim()) {
      this.toastService.warning('Please fill in both Date and Description.');
      return;
    }

    this.isSaving.set(true);
    const dto = {
      date: form.date,
      description: form.description.trim(),
      isHoliday: form.isHoliday,
      academicYearId: yearId
    };

    this.calendarService.createCalendarEvent(dto).subscribe({
      next: () => {
        this.toastService.success('Academic calendar entry added successfully!');
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchCalendar();
      },
      error: (err) => {
        console.error('Failed to save calendar entry', err);
        this.isSaving.set(false);
        this.toastService.error(err.error?.message || 'Failed to save calendar entry.');
      }
    });
  }

  openDeleteModal(eventItem: CalendarEventResponseDTO) {
    this.deletingEvent.set(eventItem);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.deletingEvent.set(null);
  }

  confirmDelete() {
    const eventItem = this.deletingEvent();
    if (!eventItem) return;

    this.isDeleting.set(true);
    this.calendarService.deleteCalendarEvent(eventItem.id).subscribe({
      next: () => {
        this.toastService.success('Calendar entry deleted successfully!');
        this.isDeleting.set(false);
        this.closeDeleteModal();
        this.fetchCalendar();
      },
      error: (err) => {
        console.error('Failed to delete calendar entry', err);
        this.isDeleting.set(false);
        this.toastService.error(err.error?.message || 'Failed to delete calendar entry.');
      }
    });
  }
}
