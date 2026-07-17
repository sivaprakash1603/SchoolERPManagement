import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TeacherService, TeacherResponseDTO, TeacherQueryRequest, PagedResponse } from '../../services/teacher.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';
import { ClassService } from '../../services/class.service';
import { FilterStateService } from '../../services/filter-state.service';
import { TimetableService, TeacherRequirementDTO } from '../../services/timetable.service';
import { environment } from '../../../environments/environment';

interface TeacherUI extends TeacherResponseDTO {
  email: string;
  avatarUrl: string;
  assignmentsCount?: number;
}

@Component({
  selector: 'app-teachers',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teachers.html',
  styleUrl: './teachers.css',
})
export class Teachers implements OnInit {
  private teacherService = inject(TeacherService);
  private toastService = inject(ToastService);
  private notificationService = inject(NotificationService);
  private classService = inject(ClassService);
  private filterStateService = inject(FilterStateService);
  private timetableService = inject(TimetableService);
  
  teachers = signal<TeacherUI[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  teacherStats = signal<any>(null);

  showViewModal = signal(false);
  showEditModal = signal(false);
  showDeleteModal = signal(false);
  showAssignmentsModal = signal(false);
  selectedTeacher = signal<TeacherUI | null>(null);
  selectedTeacherIds = signal<number[]>([]);
  selectedTeacherAssignments = signal<any[]>([]);
  availableClasses = signal<any[]>([]);
  availableSubjects = signal<any[]>([]);
  assignmentForm = signal({
    classId: null as number | null,
    subjectId: null as number | null
  });
  isAssigning = signal(false);
  
  showAutoAssignModal = signal(false);
  isAutoAssigning = signal(false);
  
  isSaving = signal(false);
  isDeleting = signal(false);

  editForm = signal({
    firstName: '',
    lastName: '',
    phonenumber: '',
    qualifications: '',
    subjectSpecialtyId: null as number | null
  });

  showNotificationModal = signal(false);
  notificationTitle = signal('');
  notificationMessage = signal('');
  notificationTargetUserIds = signal<number[]>([]);
  notificationTargetNames = signal('');
  isSendingNotification = signal(false);

  Math = Math;

  // Pagination state
  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = signal(0);

  // Filters state
  // Add Requirements Analysis state
  showTeacherRequirements = signal(false);
  periodsPerDay = signal<number>(8);
  freePeriodsPerStaff = signal<number>(2);
  teacherRequirements = signal<TeacherRequirementDTO[]>([]);
  isLoadingRequirements = signal(false);
  requirementsError = signal<string | null>(null);

  searchQuery = signal('');
  status = signal('All');

  // To debounce search
  private searchTimeout: any;

  ngOnInit() {
    this.loadFilterState();
    this.fetchTeachers();
    this.teacherService.getAllSubjects().subscribe({
      next: (subjects) => this.availableSubjects.set(subjects),
      error: () => console.error('Failed to load subjects')
    });
  }

  loadFilterState() {
    const state = this.filterStateService.getState('teachers');
    if (state) {
      this.searchQuery.set(state.searchQuery || '');
      this.status.set(state.status || 'All');
      this.pageNumber.set(state.pageNumber || 1);
    }
  }

  saveFilterState() {
    this.filterStateService.saveState('teachers', {
      searchQuery: this.searchQuery(),
      status: this.status(),
      pageNumber: this.pageNumber()
    });
  }

  fetchTeachers() {
    this.loading.set(true);
    this.error.set(null);
    
    const request: TeacherQueryRequest = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.teacherService.getAllTeachers(request).subscribe({
      next: (response: PagedResponse<TeacherResponseDTO>) => {
        const mappedData = response.items.map(dto => ({
          ...dto,
          email: dto.email || '',
          avatarUrl: dto.profilePhotoUrl 
            ? (dto.profilePhotoUrl.startsWith('http') ? dto.profilePhotoUrl : `${environment.baseUrl}${dto.profilePhotoUrl}`)
            : 'https://ui-avatars.com/api/?name=' + encodeURIComponent(dto.firstName + ' ' + dto.lastName) + '&background=random'
        }));
        
        this.teachers.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
        console.log("[DEBUG] Teachers Data:", mappedData);

      },
      error: (err) => {
        console.error('Failed to fetch teachers', err);
        this.error.set('Failed to load teachers. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  fetchTeacherRequirements() {
    if (this.periodsPerDay() <= this.freePeriodsPerStaff()) {
      this.toastService.error('Free periods must be less than total periods per day');
      return;
    }
    
    this.isLoadingRequirements.set(true);
    this.requirementsError.set(null);

    this.timetableService.getTeacherRequirements(this.periodsPerDay(), this.freePeriodsPerStaff()).subscribe({
      next: (data) => {
        this.teacherRequirements.set(data);
        this.isLoadingRequirements.set(false);
        this.showTeacherRequirements.set(true);
      },
      error: (err) => {
        console.error('Failed to fetch requirements', err);
        this.requirementsError.set(err.error?.message || 'Failed to analyze requirements');
        this.isLoadingRequirements.set(false);
      }
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
    this.saveFilterState();
    this.fetchTeachers();
  }

  onSearchChange() {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.onFilterChange();
    }, 500);
  }

  buildQueryRequest(): TeacherQueryRequest {
    return {
      searchQuery: this.searchQuery(),
      status: this.status()
    };
  }

  exportPdf() {
    this.teacherService.exportTeachersPdf(this.buildQueryRequest()).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `teachers_export_${new Date().toISOString().split('T')[0]}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => this.toastService.error('Failed to export PDF')
    });
  }

  openAutoAssignModal() {
    this.showAutoAssignModal.set(true);
  }

  closeAutoAssignModal() {
    this.showAutoAssignModal.set(false);
  }

  confirmAutoAssign() {
    this.isAutoAssigning.set(true);
    this.teacherService.autoAssignTeachers().subscribe({
      next: (result) => {
        this.toastService.success(`Auto-assignment complete. ${result.totalAssignmentsMade} assignments were made.`);
        if (result.messages && result.messages.length > 0) {
          console.log('Auto-Assign Log:', result.messages);
        }
        this.isAutoAssigning.set(false);
        this.closeAutoAssignModal();
        this.fetchTeachers(); // refresh assignment counts
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to auto-assign teachers.');
        this.isAutoAssigning.set(false);
      }
    });
  }

  previousPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.fetchTeachers();
    }
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.fetchTeachers();
    }
  }

  changePage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageNumber.set(page);
      this.saveFilterState();
      this.fetchTeachers();
    }
  }

  openViewModal(teacher: TeacherUI) {
    this.selectedTeacher.set(teacher);
    this.showViewModal.set(true);
  }

  closeViewModal() {
    this.showViewModal.set(false);
    this.selectedTeacher.set(null);
  }

  openEditModal(teacher: TeacherUI) {
    this.selectedTeacher.set(teacher);
    this.editForm.set({
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      phonenumber: teacher.phonenumber || '',
      qualifications: teacher.qualifications || '',
      subjectSpecialtyId: teacher.subjectSpecialtyId ?? null
    });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.selectedTeacher.set(null);
  }

  saveTeacher() {
    const teacherId = this.selectedTeacher()?.id;
    if (!teacherId) return;

    this.isSaving.set(true);
    const form = this.editForm();
    this.teacherService.updateTeacher(teacherId, {
      firstName: form.firstName,
      lastName: form.lastName,
      phonenumber: form.phonenumber,
      qualifications: form.qualifications,
      subjectSpecialtyId: form.subjectSpecialtyId ?? undefined
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.toastService.success('Teacher updated successfully');
        this.closeEditModal();
        this.fetchTeachers();
      },
      error: (err) => {
        console.error('Failed to update teacher', err);
        this.toastService.error(err.error?.message || 'Failed to update teacher');
        this.isSaving.set(false);
      }
    });
  }

  openDeleteModal(teacher: TeacherUI) {
    this.selectedTeacher.set(teacher);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.selectedTeacher.set(null);
  }

  openAssignmentsModal(teacher: TeacherUI) {
    this.selectedTeacher.set(teacher);
    this.showAssignmentsModal.set(true);
    this.selectedTeacherAssignments.set([]);
    this.assignmentForm.set({ classId: null, subjectId: null });
    
    // Load available classes and subjects
    this.classService.getAllClasses().subscribe({
      next: (classes) => this.availableClasses.set(classes),
      error: () => this.toastService.error('Failed to load classes')
    });

    this.loadTeacherAssignments(teacher.id);
  }

  closeAssignmentsModal() {
    this.showAssignmentsModal.set(false);
    this.selectedTeacher.set(null);
  }

  loadTeacherAssignments(teacherId: number) {
    this.teacherService.getTeacherAssignments(teacherId).subscribe({
      next: (assignments) => this.selectedTeacherAssignments.set(assignments),
      error: () => this.toastService.error('Failed to load assignments')
    });
  }

  addAssignment() {
    const teacher = this.selectedTeacher();
    const form = this.assignmentForm();
    if (!teacher || !form.classId || !form.subjectId) return;

    const cls = this.availableClasses().find((c: any) => c.id === form.classId);
    const sub = this.availableSubjects().find((s: any) => s.id === form.subjectId);

    if (cls && sub) {
      if (!cls.subjects || !cls.subjects.some((s: any) => s.id === form.subjectId)) {
        this.toastService.warning(`Subject '${sub.subjectName || sub.subjectname}' is not assigned to Class '${cls.classname}'. Please map it in the Classes page first.`);
        return;
      }
    }

    this.isAssigning.set(true);
    this.teacherService.assignSubject({ teacherId: teacher.id, classId: form.classId, subjectId: form.subjectId }).subscribe({
      next: () => {
        this.toastService.success('Subject assigned successfully');
        this.loadTeacherAssignments(teacher.id);
        this.assignmentForm.set({ classId: null, subjectId: null });
        this.isAssigning.set(false);
      },
      error: (err) => {
        this.toastService.error(err.error?.Message || err.error?.message || 'Failed to assign subject');
        this.isAssigning.set(false);
      }
    });
  }

  removeAssignment(classId: number, subjectId: number) {
    const teacher = this.selectedTeacher();
    if (!teacher) return;
    
    if (confirm('Are you sure you want to remove this assignment?')) {
      this.teacherService.unassignSubject(teacher.id, classId, subjectId).subscribe({
        next: () => {
          this.toastService.success('Assignment removed successfully');
          this.loadTeacherAssignments(teacher.id);
        },
        error: () => this.toastService.error('Failed to remove assignment')
      });
    }
  }

  confirmDelete() {
    const teacherId = this.selectedTeacher()?.id;
    if (!teacherId) return;

    this.isDeleting.set(true);
    this.teacherService.deleteTeacher(teacherId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.toastService.success('Teacher deactivated successfully');
        this.closeDeleteModal();
        this.fetchTeachers();
      },
      error: (err) => {
        console.error('Failed to delete teacher', err);
        this.toastService.error(err.error?.message || 'Failed to deactivate teacher');
        this.isDeleting.set(false);
      }
    });
  }

  openNotificationModal(targetName: string, targetUserIds: number[]) {
    this.notificationTitle.set('');
    this.notificationMessage.set('');
    this.notificationTargetNames.set(targetName);
    this.notificationTargetUserIds.set(targetUserIds);
    this.showNotificationModal.set(true);
  }

  closeNotificationModal() {
    this.showNotificationModal.set(false);
  }

  sendNotification() {
    if (!this.notificationTitle() || !this.notificationMessage() || this.notificationTargetUserIds().length === 0) {
      return;
    }

    this.isSendingNotification.set(true);
    const dto = {
      title: this.notificationTitle(),
      message: this.notificationMessage(),
      targetUserIds: this.notificationTargetUserIds()
    };

    this.notificationService.sendNotification(dto).subscribe({
      next: () => {
        this.isSendingNotification.set(false);
        this.toastService.success('Notification sent successfully');
        this.closeNotificationModal();
        this.clearSelection();
      },
      error: (err) => {
        console.error('Failed to send notification', err);
        this.toastService.error(err.error?.message || 'Failed to send notification');
        this.isSendingNotification.set(false);
      }
    });
  }

  toggleSelectTeacher(id: number) {
    this.selectedTeacherIds.update(ids => 
      ids.includes(id) ? ids.filter(x => x !== id) : [...ids, id]
    );
  }

  isTeacherSelected(id: number): boolean {
    return this.selectedTeacherIds().includes(id);
  }

  toggleSelectAll(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      const pageIds = this.teachers().map(t => t.id);
      this.selectedTeacherIds.set(pageIds);
    } else {
      this.selectedTeacherIds.set([]);
    }
  }

  clearSelection() {
    this.selectedTeacherIds.set([]);
  }

  openBulkNotificationModal() {
    const ids = this.selectedTeacherIds();
    const selectedTeachers = this.teachers().filter(t => ids.includes(t.id));
    const targetUserIds = selectedTeachers.map(t => t.userId);
    const targetNames = `${selectedTeachers.length} Selected Teachers`;
    this.openNotificationModal(targetNames, targetUserIds);
  }
}
