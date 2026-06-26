import { Component, signal, OnInit, inject } from '@angular/core';

import { FormsModule } from '@angular/forms';
import {
  ParentService,
  ParentResponseDTO,
  ParentQueryRequest,
  PagedResponse,
} from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';

interface ParentUI extends ParentResponseDTO {
  email: string;
  avatarUrl: string;
}

@Component({
  selector: 'app-parents',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './parents.html',
  styleUrl: './parents.css',
})
export class Parents implements OnInit {
  private parentService = inject(ParentService);
  private toastService = inject(ToastService);
  private notificationService = inject(NotificationService);

  parents = signal<ParentUI[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  showEditModal = signal(false);
  showDeleteModal = signal(false);
  editingParent = signal<ParentUI | null>(null);
  deletingParent = signal<ParentUI | null>(null);
  editForm = signal({ name: '', email: '', phonenumber: '' });
  isUpdating = signal(false);
  isDeleting = signal(false);

  showNotificationModal = signal(false);
  notificationTitle = signal('');
  notificationMessage = signal('');
  notificationTargetUserIds = signal<number[]>([]);
  notificationTargetNames = signal('');
  selectedParentIds = signal<number[]>([]);
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
    this.fetchParents();
  }

  fetchParents() {
    this.loading.set(true);
    this.error.set(null);

    const request: ParentQueryRequest = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
      status: this.status(),
    };

    this.parentService.getAllParents(request).subscribe({
      next: (response: PagedResponse<ParentResponseDTO>) => {
        const mappedData = response.items.map((dto) => ({
          ...dto,
          email: dto.email || 'N/A',
          avatarUrl:
            'https://ui-avatars.com/api/?name=' +
            encodeURIComponent(dto.name) +
            '&background=random',
        }));

        this.parents.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch parents', err);
        this.error.set('Failed to load parents. Please try again later.');
        this.loading.set(false);
      },
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
    this.fetchParents();
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
    const request: ParentQueryRequest = {
      searchQuery: this.searchQuery(),
      status: this.status(),
    };

    this.parentService.exportParentsPdf(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'parents-directory.pdf';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to export PDF', err);
        alert('Failed to generate PDF report.');
      },
    });
  }

  previousPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((p) => p - 1);
      this.fetchParents();
    }
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((p) => p + 1);
      this.fetchParents();
    }
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
    if (
      !this.notificationTitle() ||
      !this.notificationMessage() ||
      this.notificationTargetUserIds().length === 0
    ) {
      return;
    }

    this.isSendingNotification.set(true);
    const dto = {
      title: this.notificationTitle(),
      message: this.notificationMessage(),
      targetUserIds: this.notificationTargetUserIds(),
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
      },
    });
  }

  toggleSelectParent(id: number) {
    this.selectedParentIds.update((ids) =>
      ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id],
    );
  }

  isParentSelected(id: number): boolean {
    return this.selectedParentIds().includes(id);
  }

  toggleSelectAll(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      const pageIds = this.parents().map((p) => p.id);
      this.selectedParentIds.set(pageIds);
    } else {
      this.selectedParentIds.set([]);
    }
  }

  clearSelection() {
    this.selectedParentIds.set([]);
  }

  openBulkNotificationModal() {
    const ids = this.selectedParentIds();
    const selectedParents = this.parents().filter((p) => ids.includes(p.id));
    const targetUserIds = selectedParents.map((p) => p.userId);
    const targetNames = `${selectedParents.length} Selected Parents`;
    this.openNotificationModal(targetNames, targetUserIds);
  }

  openEditModal(parent: ParentUI) {
    this.editingParent.set(parent);
    this.editForm.set({
      name: parent.name,
      email: parent.email,
      phonenumber: parent.phonenumber || '',
    });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingParent.set(null);
  }

  saveEdit() {
    const parent = this.editingParent();
    if (!parent) return;

    this.isUpdating.set(true);
    this.parentService.updateParent(parent.id, this.editForm()).subscribe({
      next: () => {
        this.isUpdating.set(false);
        this.toastService.success('Parent details updated successfully.');
        this.closeEditModal();
        this.fetchParents();
      },
      error: (err) => {
        console.error('Failed to update parent', err);
        this.toastService.error(err.error?.message || 'Failed to update parent details.');
        this.isUpdating.set(false);
      },
    });
  }

  openDeleteModal(parent: ParentUI) {
    this.deletingParent.set(parent);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.deletingParent.set(null);
  }

  confirmDelete() {
    const parent = this.deletingParent();
    if (!parent) return;

    this.isDeleting.set(true);
    this.parentService.deleteParent(parent.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.toastService.success('Parent account deactivated successfully.');
        this.closeDeleteModal();
        this.fetchParents();
      },
      error: (err) => {
        console.error('Failed to delete parent', err);
        this.toastService.error(err.error?.message || 'Failed to deactivate parent.');
        this.isDeleting.set(false);
      },
    });
  }
}
