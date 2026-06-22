import { Component, signal, OnInit, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SubjectService, SubjectResponseDTO } from '../../services/subject.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-subjects',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './subjects.html',
  styleUrl: './subjects.css',
})
export class Subjects implements OnInit {
  private subjectService = inject(SubjectService);
  private toastService = inject(ToastService);

  subjects = signal<SubjectResponseDTO[]>([]);
  searchQuery = signal('');

  loading = signal(false);
  error = signal<string | null>(null);

  // Authorization flag
  isAdmin = signal(false);

  // Modals state
  showCreateModal = signal(false);
  showEditModal = signal(false);
  showDeleteModal = signal(false);
  isSaving = signal(false);
  isDeleting = signal(false);

  // Active items being edited or deleted
  editingSubject = signal<SubjectResponseDTO | null>(null);
  deletingSubject = signal<SubjectResponseDTO | null>(null);

  // Form states
  createForm = signal({
    subjectName: ''
  });
  editForm = signal({
    subjectName: ''
  });

  // Computed signal for filtered subjects list
  filteredSubjects = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    if (!query) return this.subjects();
    return this.subjects().filter(s => s.subjectName.toLowerCase().includes(query));
  });

  ngOnInit() {
    const role = sessionStorage.getItem('role');
    this.isAdmin.set(role === 'Admin');
    this.fetchSubjects();
  }

  fetchSubjects() {
    this.loading.set(true);
    this.error.set(null);
    this.subjectService.getAllSubjects().subscribe({
      next: (data) => {
        this.subjects.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load subjects', err);
        this.error.set('Failed to load subjects. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  openCreateModal() {
    this.createForm.set({ subjectName: '' });
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  saveSubject() {
    const form = this.createForm();
    if (!form.subjectName.trim()) {
      this.toastService.warning('Subject Name is required.');
      return;
    }

    this.isSaving.set(true);
    this.subjectService.createSubject({ subjectName: form.subjectName }).subscribe({
      next: () => {
        this.toastService.success('Subject created successfully!');
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchSubjects();
      },
      error: (err) => {
        console.error('Failed to create subject', err);
        this.isSaving.set(false);
        this.toastService.error(err.error?.message || 'Failed to create subject.');
      }
    });
  }

  openEditModal(subject: SubjectResponseDTO) {
    this.editingSubject.set(subject);
    this.editForm.set({ subjectName: subject.subjectName });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingSubject.set(null);
  }

  saveEdit() {
    const subject = this.editingSubject();
    if (!subject) return;

    const form = this.editForm();
    if (!form.subjectName.trim()) {
      this.toastService.warning('Subject Name is required.');
      return;
    }

    this.isSaving.set(true);
    this.subjectService.updateSubject(subject.id, { subjectName: form.subjectName }).subscribe({
      next: () => {
        this.toastService.success('Subject updated successfully!');
        this.isSaving.set(false);
        this.closeEditModal();
        this.fetchSubjects();
      },
      error: (err) => {
        console.error('Failed to update subject', err);
        this.isSaving.set(false);
        this.toastService.error(err.error?.message || 'Failed to update subject.');
      }
    });
  }

  openDeleteModal(subject: SubjectResponseDTO) {
    this.deletingSubject.set(subject);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.deletingSubject.set(null);
  }

  confirmDelete() {
    const subject = this.deletingSubject();
    if (!subject) return;

    this.isDeleting.set(true);
    this.subjectService.deleteSubject(subject.id).subscribe({
      next: () => {
        this.toastService.success('Subject deleted successfully!');
        this.isDeleting.set(false);
        this.closeDeleteModal();
        this.fetchSubjects();
      },
      error: (err) => {
        console.error('Failed to delete subject', err);
        this.isDeleting.set(false);
        this.toastService.error(err.error?.message || 'Failed to delete subject. It might be assigned to a teacher or timetable.');
      }
    });
  }
}
