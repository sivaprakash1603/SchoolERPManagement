import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentService, StudentQueryResponseDTO, StudentQueryRequest, PagedResponse, ParentSelection } from '../../services/student.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { ParentService, ParentResponseDTO } from '../../services/parent.service';
import { DocumentService, DocumentResponseDTO } from '../../services/document.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';

interface StudentUI extends StudentQueryResponseDTO {
  email: string;
  avatarUrl: string;
}

import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './students.html',
  styleUrl: './students.css',
})
export class Students implements OnInit {
  private studentService = inject(StudentService);
  private classService = inject(ClassService);
  private parentService = inject(ParentService);
  private documentService = inject(DocumentService);
  private academicYearService = inject(AcademicYearService);
  private toastService = inject(ToastService);
  private notificationService = inject(NotificationService);

  showNotificationModal = signal(false);
  notificationTitle = signal('');
  notificationMessage = signal('');
  notificationTargetUserIds = signal<number[]>([]);
  notificationTargetNames = signal('');
  isSendingNotification = signal(false);
  isAdmin = signal(false);
  userRole = signal<string>('Student');
  
  students = signal<StudentUI[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | null>(null);

  loading = signal(true);
  error = signal<string | null>(null);

  selectedStudent = signal<StudentUI | null>(null);
  showViewModal = signal(false);
  showEditModal = signal(false);
  showDeleteModal = signal(false);
  isSaving = signal(false);
  isDeleting = signal(false);

  // Edit parents and docs state
  allParentsList = signal<ParentResponseDTO[]>([]);
  selectedParents = signal<{ parentId: number; name: string; relation: string }[]>([]);
  relationOptions = ['Father', 'Mother', 'Guardian', 'Other'];
  parentSearchQuery = signal('');
  currentSessionName = signal('2024-2025');

  filteredParents() {
    const query = this.parentSearchQuery().toLowerCase();
    return this.allParentsList().filter(p => 
      p.name.toLowerCase().includes(query) || 
      p.phonenumber.includes(query) || 
      p.email.toLowerCase().includes(query)
    );
  }

  // Documents state for edit modal
  currentDocuments = signal<DocumentResponseDTO[]>([]);
  selectedFiles = signal<{ file: File; type: string }[]>([]);

  // Edit form state
  editForm = signal({
    name: '',
    gender: 'Male',
    bloodgroup: '',
    dateofbirth: '',
    admissiondate: ''
  });



  // Pagination state
  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = signal(0);

  // Filters state
  searchQuery = signal('');
  classId = signal<number | null>(null);
  gender = signal('Any Gender');
  status = signal('All');

  // To debounce search
  private searchTimeout: any;

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);
    this.isAdmin.set(role === 'Admin');
    this.loadFilterState();
    this.fetchCurrentAcademicSession();
    this.fetchParents();
  }

  loadFilterState() {
    const savedState = sessionStorage.getItem('students_filter_state');
    if (savedState) {
      try {
        const state = JSON.parse(savedState);
        this.searchQuery.set(state.searchQuery || '');
        this.status.set(state.status || 'All');
        this.gender.set(state.gender || 'Any Gender');
        this.pageNumber.set(state.pageNumber || 1);
        if (state.classId !== undefined) {
          this.classId.set(state.classId);
        }
        if (state.selectedAcademicYearId !== undefined) {
          this.selectedAcademicYearId.set(state.selectedAcademicYearId);
        }
      } catch (e) {
        console.error('Failed to parse saved filter state', e);
      }
    }
  }

  saveFilterState() {
    const state = {
      searchQuery: this.searchQuery(),
      status: this.status(),
      gender: this.gender(),
      pageNumber: this.pageNumber(),
      classId: this.classId(),
      selectedAcademicYearId: this.selectedAcademicYearId()
    };
    sessionStorage.setItem('students_filter_state', JSON.stringify(state));
  }

  fetchCurrentAcademicSession() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        
        // Only set current session if we don't have a saved one from filter state
        if (this.selectedAcademicYearId() === null) {
          const current = years.find(y => y.isCurrent);
          if (current) {
            this.currentSessionName.set(current.yearName);
            this.selectedAcademicYearId.set(current.id);
          } else if (years.length > 0) {
            this.currentSessionName.set(years[0].yearName);
            this.selectedAcademicYearId.set(years[0].id);
          }
        } else {
          // Sync name with saved ID
          const matched = years.find(y => y.id === this.selectedAcademicYearId());
          if (matched) {
            this.currentSessionName.set(matched.yearName);
          }
        }
        this.fetchClasses();
        this.fetchStudents();
      },
      error: (err) => console.error('Failed to fetch academic session', err)
    });
  }

  fetchParents() {
    this.parentService.getAllParents({ pageSize: 1000 }).subscribe({
      next: (data) => this.allParentsList.set(data.items),
      error: (err) => console.error('Failed to fetch parents', err)
    });
  }

  fetchClasses() {
    const yearId = this.selectedAcademicYearId();
    this.classService.getAllClasses(yearId || undefined).subscribe({
      next: (data) => {
        this.classes.set(data);
      },
      error: (err) => {
        console.error('Failed to fetch classes', err);
      }
    });
  }

  onYearChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const val = select.value ? parseInt(select.value, 10) : null;
    this.selectedAcademicYearId.set(val);
    const matched = this.academicYears().find(y => y.id === val);
    if (matched) {
      this.currentSessionName.set(matched.yearName);
    }
    this.classId.set(null); // reset class filter since classes change per year
    this.saveFilterState();
    this.fetchClasses();
    this.onFilterChange();
  }

  fetchStudents() {
    this.loading.set(true);
    this.error.set(null);
    
    const request: StudentQueryRequest = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
      gender: this.gender(),
      status: this.status()
    };

    if (this.selectedAcademicYearId() !== null) {
      request.academicYearId = this.selectedAcademicYearId()!;
    }

    if (this.classId() !== null) {
      request.classId = this.classId()!;
    }

    this.studentService.getAllStudents(request).subscribe({
      next: (response: PagedResponse<StudentQueryResponseDTO>) => {
        const mappedData = response.items.map(dto => ({
          ...dto,
          email: `${dto.name.split(' ')[0].toLowerCase()}@edupro.in`,
          avatarUrl: dto.profilePhotoUrl 
            ? `http://localhost:5203${dto.profilePhotoUrl}` 
            : 'https://ui-avatars.com/api/?name=' + encodeURIComponent(dto.name) + '&background=random'
        }));
        
        this.students.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch students', err);
        this.error.set('Failed to load students. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
    this.saveFilterState();
    this.fetchStudents();
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
    const request: StudentQueryRequest = {
      searchQuery: this.searchQuery(),
      gender: this.gender(),
      status: this.status()
    };
    if (this.selectedAcademicYearId() !== null) {
      request.academicYearId = this.selectedAcademicYearId()!;
    }
    if (this.classId() !== null) {
      request.classId = this.classId()!;
    }

    this.studentService.exportStudentsPdf(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'students-directory.pdf';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to export PDF', err);
        this.toastService.error('Failed to generate PDF report.');
      }
    });
  }

  showEnrollModal = signal(false);
  enrollClasses = signal<ClassResponseDTO[]>([]);
  isBulkEnroll = signal(false);
  selectedStudentIds = signal<number[]>([]);
  enrollForm = signal({
    studentId: 0,
    studentName: '',
    academicYearId: null as number | null,
    classId: null as number | null
  });

  toggleSelectStudent(id: number) {
    this.selectedStudentIds.update(ids => 
      ids.includes(id) ? ids.filter(x => x !== id) : [...ids, id]
    );
  }

  isStudentSelected(id: number): boolean {
    return this.selectedStudentIds().includes(id);
  }

  toggleSelectAll(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    if (checkbox.checked) {
      const pageIds = this.students().map(s => s.id);
      this.selectedStudentIds.set(pageIds);
    } else {
      this.selectedStudentIds.set([]);
    }
  }

  clearSelection() {
    this.selectedStudentIds.set([]);
  }

  openEnrollModal(student: StudentUI) {
    this.isBulkEnroll.set(false);
    this.enrollForm.set({
      studentId: student.id,
      studentName: student.name,
      academicYearId: this.selectedAcademicYearId(),
      classId: null
    });
    this.showEnrollModal.set(true);
    this.fetchEnrollClasses();
  }

  openBulkEnrollModal() {
    const selectedCount = this.selectedStudentIds().length;
    if (selectedCount === 0) return;
    this.isBulkEnroll.set(true);
    this.enrollForm.set({
      studentId: 0,
      studentName: `${selectedCount} selected students`,
      academicYearId: this.selectedAcademicYearId(),
      classId: null
    });
    this.showEnrollModal.set(true);
    this.fetchEnrollClasses();
  }

  closeEnrollModal() {
    this.showEnrollModal.set(false);
  }

  fetchEnrollClasses() {
    const yearId = this.enrollForm().academicYearId;
    if (!yearId) {
      this.enrollClasses.set([]);
      return;
    }
    this.classService.getAllClasses(yearId).subscribe({
      next: (data) => this.enrollClasses.set(data),
      error: (err) => console.error('Failed to load enrollment classes', err)
    });
  }

  onEnrollYearChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const val = select.value ? parseInt(select.value, 10) : null;
    this.enrollForm.update(form => ({ ...form, academicYearId: val, classId: null }));
    this.fetchEnrollClasses();
  }

  saveEnrollment() {
    const form = this.enrollForm();
    if (!form.academicYearId || !form.classId) {
      this.toastService.warning('Please select both Academic Session and Class.');
      return;
    }

    if (this.isBulkEnroll()) {
      this.studentService.bulkEnrollStudents({
        studentIds: this.selectedStudentIds(),
        classId: form.classId,
        academicYearId: form.academicYearId
      }).subscribe({
        next: () => {
          this.toastService.success('Students successfully enrolled/promoted in bulk!');
          this.closeEnrollModal();
          this.clearSelection();
          this.fetchStudents();
        },
        error: (err: any) => {
          console.error('Failed to bulk enroll students', err);
          this.toastService.error(err.error?.message || 'Failed to bulk enroll students.');
        }
      });
    } else {
      this.studentService.enrollStudent(form.studentId, {
        classId: form.classId,
        academicYearId: form.academicYearId
      }).subscribe({
        next: () => {
          this.toastService.success('Student successfully enrolled/promoted!');
          this.closeEnrollModal();
          this.fetchStudents();
        },
        error: (err) => {
          console.error('Failed to enroll student', err);
          this.toastService.error(err.error?.message || 'Failed to enroll student.');
        }
      });
    }
  }

  openViewModal(student: StudentUI) {
    this.selectedStudent.set(student);
    this.showViewModal.set(true);
  }

  closeViewModal() {
    this.showViewModal.set(false);
    this.selectedStudent.set(null);
  }

  openEditModal(student: StudentUI) {
    this.selectedStudent.set(student);
    
    // Format dates to YYYY-MM-DD
    const dob = student.dateofbirth ? new Date(student.dateofbirth).toISOString().split('T')[0] : '';
    const adm = student.admissionDate ? new Date(student.admissionDate).toISOString().split('T')[0] : '';

    this.editForm.set({
      name: student.name,
      gender: student.gender || 'Male',
      bloodgroup: student.bloodgroup || '',
      dateofbirth: dob,
      admissiondate: adm
    });

    // Reset document file cache
    this.selectedFiles.set([]);
    this.currentDocuments.set([]);

    // Fetch documents
    this.documentService.getStudentDocuments(student.id).subscribe({
      next: (docs) => this.currentDocuments.set(docs),
      error: (err) => console.error('Failed to load documents', err)
    });

    // Load pre-selected parents
    const preSelected = (student.parents || []).map(p => {
      const matchedParent = this.allParentsList().find(ap => ap.id === p.parentId);
      return {
        parentId: p.parentId,
        name: matchedParent ? matchedParent.name : 'Unknown Parent',
        relation: p.relation
      };
    });
    this.selectedParents.set(preSelected);

    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.selectedStudent.set(null);
  }

  onFileSelected(event: Event, type: string) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const current = this.selectedFiles();
      const existingIndex = current.findIndex(d => d.type === type);
      
      if (existingIndex >= 0) {
        current[existingIndex].file = file;
      } else {
        current.push({ file, type });
      }
      this.selectedFiles.set([...current]);
    }
  }

  getFileName(type: string): string | null {
    const doc = this.selectedFiles().find(d => d.type === type);
    return doc ? doc.file.name : null;
  }

  getDocumentUrl(type: string): string | null {
    const doc = this.currentDocuments().find(d => d.documentName === type);
    return doc ? `http://localhost:5203${doc.blobUrl}` : null;
  }

  isParentSelected(parentId: number): boolean {
    return this.selectedParents().some(p => p.parentId === parentId);
  }

  toggleParentSelection(parentId: number, parentName: string) {
    if (this.isParentSelected(parentId)) {
      this.selectedParents.update(list => list.filter(p => p.parentId !== parentId));
    } else {
      this.selectedParents.update(list => [...list, { parentId, name: parentName, relation: 'Father' }]);
    }
  }

  updateParentRelation(parentId: number, relation: string) {
    this.selectedParents.update(list =>
      list.map(p => p.parentId === parentId ? { ...p, relation } : p)
    );
  }

  saveStudent() {
    const studentId = this.selectedStudent()?.id;
    if (!studentId) return;

    this.isSaving.set(true);
    const updateDto = {
      name: this.editForm().name,
      gender: this.editForm().gender,
      bloodgroup: this.editForm().bloodgroup || undefined,
      dateofbirth: this.editForm().dateofbirth || undefined,
      admissiondate: this.editForm().admissiondate || undefined,
      parents: this.selectedParents().map(p => ({ parentId: p.parentId, relation: p.relation }))
    };

    this.studentService.updateStudent(studentId, updateDto).subscribe({
      next: async () => {
        // Upload new files
        const filesToUpload = this.selectedFiles();
        for (const f of filesToUpload) {
          await new Promise<void>((resolve) => {
            this.documentService.uploadStudentDocument(studentId, f.file, f.type).subscribe({
              next: () => resolve(),
              error: (err) => {
                console.error(`Failed to upload ${f.type}`, err);
                resolve();
              }
            });
          });
        }
        this.isSaving.set(false);
        this.closeEditModal();
        this.fetchStudents(); // Refresh the list
      },
      error: (err) => {
        console.error('Failed to update student', err);
        this.toastService.error(err.error?.message || 'Failed to update student. Please check the inputs.');
        this.isSaving.set(false);
      }
    });
  }

  openDeleteModal(student: StudentUI) {
    this.selectedStudent.set(student);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.selectedStudent.set(null);
  }

  confirmDelete() {
    const studentId = this.selectedStudent()?.id;
    if (!studentId) return;

    this.isDeleting.set(true);
    this.studentService.deleteStudent(studentId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.closeDeleteModal();
        this.fetchStudents(); // Refresh list
      },
      error: (err) => {
        console.error('Failed to delete student', err);
        this.toastService.error(err.error?.message || 'Failed to delete student.');
        this.isDeleting.set(false);
      }
    });
  }

  getStatusClass(status: string | undefined): string {
    switch (status) {
      case 'Active': return 'bg-success-subtle text-success';
      case 'On Leave': return 'bg-warning-subtle text-warning';
      case 'Withdrawn':
      case 'Inactive': return 'bg-secondary-subtle text-secondary';
      default: return 'bg-light text-dark';
    }
  }

  previousPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.fetchStudents();
    }
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.fetchStudents();
    }
  }

  changePage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageNumber.set(page);
      this.saveFilterState();
      this.fetchStudents();
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

  openBulkNotificationModal() {
    const ids = this.selectedStudentIds();
    const selectedStudents = this.students().filter(s => ids.includes(s.id));
    const targetUserIds = selectedStudents.map(s => s.userId);
    const targetNames = `${selectedStudents.length} Selected Students`;
    this.openNotificationModal(targetNames, targetUserIds);
  }
}
