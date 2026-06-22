import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TeacherService, TeacherResponseDTO, TeacherQueryRequest, PagedResponse } from '../../services/teacher.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';

interface TeacherUI extends TeacherResponseDTO {
  email: string;
  avatarUrl: string;
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
  
  teachers = signal<TeacherUI[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  showViewModal = signal(false);
  showEditModal = signal(false);
  showDeleteModal = signal(false);
  selectedTeacher = signal<TeacherUI | null>(null);
  selectedTeacherIds = signal<number[]>([]);
  
  isSaving = signal(false);
  isDeleting = signal(false);

  editForm = signal({
    name: '',
    phonenumber: '',
    qualifications: ''
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
  searchQuery = signal('');
  status = signal('All');

  // To debounce search
  private searchTimeout: any;

  ngOnInit() {
    this.fetchTeachers();
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
            ? (dto.profilePhotoUrl.startsWith('http') ? dto.profilePhotoUrl : `http://localhost:5203${dto.profilePhotoUrl}`)
            : 'https://ui-avatars.com/api/?name=' + encodeURIComponent(dto.name) + '&background=random'
        }));
        
        this.teachers.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch teachers', err);
        this.error.set('Failed to load teachers. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
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

  exportPdf() {
    const request: TeacherQueryRequest = {
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.teacherService.exportTeachersPdf(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'teachers-directory.pdf';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to export PDF', err);
        alert('Failed to generate PDF report.');
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
      name: teacher.name,
      phonenumber: teacher.phonenumber || '',
      qualifications: teacher.qualifications || ''
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
      name: form.name,
      phonenumber: form.phonenumber,
      qualifications: form.qualifications
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
