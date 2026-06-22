import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-classes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './classes.html',
  styleUrl: './classes.css'
})
export class Classes implements OnInit {
  private classService = inject(ClassService);
  private teacherService = inject(TeacherService);
  private academicYearService = inject(AcademicYearService);
  private toastService = inject(ToastService);

  classes = signal<ClassResponseDTO[]>([]);
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  teachers = signal<TeacherResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | null>(null);

  loading = signal(false);
  error = signal<string | null>(null);
  showCreateModal = signal(false);
  isSaving = signal(false);

  showEditModal = signal(false);
  showDeleteModal = signal(false);
  editingClass = signal<ClassResponseDTO | null>(null);
  deletingClass = signal<ClassResponseDTO | null>(null);
  editForm = signal({
    classname: '',
    section: '',
    classteacherId: null as number | null,
    academicyearId: null as number | null
  });
  isUpdating = signal(false);
  isDeleting = signal(false);

  createForm = signal({
    classname: '',
    section: '',
    classteacherId: null as number | null,
    academicyearId: null as number | null
  });

  ngOnInit() {
    this.fetchAcademicYears();
    this.fetchTeachers();
  }

  fetchAcademicYears() {
    this.loading.set(true);
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find(y => y.isCurrent);
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.createForm.update(form => ({ ...form, academicyearId: currentYear.id }));
        } else if (years.length > 0) {
          this.selectedAcademicYearId.set(years[0].id);
          this.createForm.update(form => ({ ...form, academicyearId: years[0].id }));
        }
        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to load academic sessions', err);
        this.error.set('Failed to load academic sessions.');
        this.loading.set(false);
      }
    });
  }

  fetchClasses() {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) return;

    this.loading.set(true);
    this.error.set(null);
    this.classService.getAllClasses(yearId).subscribe({
      next: (data) => {
        this.classes.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.error.set('Failed to load classes.');
        this.loading.set(false);
      }
    });
  }

  fetchTeachers() {
    this.teacherService.getAllTeachers({ pageSize: 100 }).subscribe({
      next: (res) => {
        this.teachers.set(res.items);
      },
      error: (err) => {
        console.error('Failed to load teachers', err);
      }
    });
  }

  getTeacherName(teacherId?: number): string {
    if (!teacherId) return 'Not Assigned';
    const teacher = this.teachers().find(t => t.id === teacherId);
    return teacher ? teacher.name : 'Unknown';
  }

  getAvailableTeachersForCreate(): TeacherResponseDTO[] {
    const assignedTeacherIds = this.classes()
      .map(c => c.classteacherId)
      .filter((id): id is number => !!id);
    return this.teachers().filter(t => !assignedTeacherIds.includes(t.id));
  }

  getAvailableTeachersForEdit(currentClassteacherId?: number): TeacherResponseDTO[] {
    const assignedTeacherIds = this.classes()
      .map(c => c.classteacherId)
      .filter((id): id is number => !!id && id !== currentClassteacherId);
    return this.teachers().filter(t => !assignedTeacherIds.includes(t.id));
  }

  getYearName(yearId?: number): string {
    if (!yearId) return 'N/A';
    const year = this.academicYears().find(y => y.id === yearId);
    return year ? year.yearName : 'N/A';
  }

  onYearChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const val = select.value ? parseInt(select.value, 10) : null;
    this.selectedAcademicYearId.set(val);
    this.createForm.update(form => ({ ...form, academicyearId: val }));
    this.fetchClasses();
  }

  openCreateModal() {
    this.createForm.set({
      classname: '',
      section: '',
      classteacherId: null,
      academicyearId: this.selectedAcademicYearId()
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  saveClass() {
    const form = this.createForm();
    if (!form.classname || !form.section) {
      this.toastService.warning('Please fill in both Class Name and Section.');
      return;
    }

    this.isSaving.set(true);
    const dto = {
      classname: form.classname,
      section: form.section,
      classteacherId: form.classteacherId || undefined,
      academicyearId: form.academicyearId || undefined
    };

    this.classService.createClass(dto).subscribe({
      next: () => {
        this.toastService.success('Class created successfully!');
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to save class', err);
        this.isSaving.set(false);
        this.toastService.error(err.error?.message || 'Failed to save class. A teacher might already be assigned to another class.');
      }
    });
  }

  openEditModal(cls: ClassResponseDTO) {
    this.editingClass.set(cls);
    this.editForm.set({
      classname: cls.classname,
      section: cls.section || '',
      classteacherId: cls.classteacherId || null,
      academicyearId: cls.academicyearId || null
    });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingClass.set(null);
  }

  saveEdit() {
    const cls = this.editingClass();
    if (!cls) return;

    const form = this.editForm();
    if (!form.classname || !form.section) {
      this.toastService.warning('Please fill in both Class Name and Section.');
      return;
    }

    this.isUpdating.set(true);
    const dto = {
      classname: form.classname,
      section: form.section,
      classteacherId: form.classteacherId || undefined,
      academicyearId: form.academicyearId || undefined
    };

    this.classService.updateClass(cls.id, dto).subscribe({
      next: () => {
        this.toastService.success('Class updated successfully!');
        this.isUpdating.set(false);
        this.closeEditModal();
        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to update class', err);
        this.isUpdating.set(false);
        this.toastService.error(err.error?.message || 'Failed to update class.');
      }
    });
  }

  openDeleteModal(cls: ClassResponseDTO) {
    this.deletingClass.set(cls);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.deletingClass.set(null);
  }

  confirmDelete() {
    const cls = this.deletingClass();
    if (!cls) return;

    this.isDeleting.set(true);
    this.classService.deleteClass(cls.id).subscribe({
      next: () => {
        this.toastService.success('Class deleted successfully!');
        this.isDeleting.set(false);
        this.closeDeleteModal();
        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to delete class', err);
        this.isDeleting.set(false);
        this.toastService.error(err.error?.message || 'Failed to delete class.');
      }
    });
  }
}
