import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService, DocumentResponseDTO, PendingDocumentDTO } from '../../services/document.service';
import { StudentService, StudentQueryResponseDTO } from '../../services/student.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { ParentService } from '../../services/parent.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { ToastService } from '../../services/toast.service';
import { TimetableService } from '../../services/timetable.service';
import { FilterStateService } from '../../services/filter-state.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.html',
  styleUrl: './documents.css'
})
export class Documents implements OnInit {
  private documentService = inject(DocumentService);
  private studentService = inject(StudentService);
  private teacherService = inject(TeacherService);
  private parentService = inject(ParentService);
  private classService = inject(ClassService);
  private toastService = inject(ToastService);
  private timetableService = inject(TimetableService);
  private filterStateService = inject(FilterStateService);

  constructor() {
    const savedState = this.filterStateService.getState('documents');
    if (savedState) {
      if (savedState.activeDirectoryTab !== undefined) this.activeDirectoryTab.set(savedState.activeDirectoryTab);
      if (savedState.selectedClassId !== undefined) this.selectedClassId.set(savedState.selectedClassId);
    }

    effect(() => {
      this.filterStateService.saveState('documents', {
        activeDirectoryTab: this.activeDirectoryTab(),
        selectedClassId: this.selectedClassId()
      });
    });
  }

  // Auth & Roles
  userRole = signal<string>('Student');
  currentUserId = signal<number | null>(null);
  teacherClassIds = signal<number[]>([]);

  // Profile references for Student / Parent
  resolvedStudentId = signal<number | null>(null);
  resolvedTeacherId = signal<number | null>(null);
  currentTeacher = signal<TeacherResponseDTO | null>(null);
  parentChildren = signal<any[]>([]); // list of children for parent
  selectedChildId = signal<number | null>(null);

  // Directory lists (Admin & Teacher)
  activeDirectoryTab = signal<'students' | 'teachers'>('students');
  classesList = signal<ClassResponseDTO[]>([]);
  selectedClassId = signal<number | null>(null);
  searchQuery = signal<string>('');
  
  studentsDirectory = signal<StudentQueryResponseDTO[]>([]);
  teachersDirectory = signal<TeacherResponseDTO[]>([]);

  // Selected Directory Item
  selectedStudent = signal<StudentQueryResponseDTO | null>(null);
  selectedTeacher = signal<TeacherResponseDTO | null>(null);

  // Document Lists
  activeDocumentsList = signal<DocumentResponseDTO[]>([]);

  // Stats
  totalDocsCount = signal<number>(0);
  verifiedDocsCount = signal<number>(0);
  pendingDocsCount = signal<number>(0);
  systemPendingCount = signal<number>(0);
  pendingDocuments = signal<PendingDocumentDTO[]>([]);
  onlyShowPending = signal<boolean>(true);

  // Teacher self-management
  teacherViewMode = signal<'verify' | 'self'>('verify');

  // Modals Visibility
  showUploadModal = signal<boolean>(false);

  // Upload Form
  uploadForm = signal({
    documentName: '',
    selectedFile: null as File | null
  });

  // Loader state
  isLoading = signal<boolean>(false);
  isUploading = signal<boolean>(false);

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);

    const uidStr = sessionStorage.getItem('userId');
    const uid = uidStr ? parseInt(uidStr, 10) : null;
    this.currentUserId.set(uid);

    if (role === 'Admin') {
      this.loadDirectoryClasses();
      this.loadPendingDocuments();
    } else if (role === 'Teacher') {
      const username = sessionStorage.getItem('username');
      if (username) {
        this.teacherService.getTeacherByUsername(username).subscribe({
          next: (profile) => {
            this.resolvedTeacherId.set(profile.id);
            this.currentTeacher.set(profile);
            
            this.timetableService.getTeacherTimetable(profile.id).subscribe({
              next: (slots) => {
                const classIds = Array.from(new Set<number>(slots.map(s => s.classId)));
                this.teacherClassIds.set(classIds);
                this.loadDirectoryClasses();
                this.loadPendingDocuments();
              },
              error: (err) => {
                console.error(err);
                this.loadDirectoryClasses();
                this.loadPendingDocuments();
              }
            });
          },
          error: (err) => console.error(err)
        });
      }
    } else if (role === 'Student') {
      if (uid) this.fetchStudentProfileAndDocs(uid);
    } else if (role === 'Parent') {
      if (uid) this.fetchParentProfileAndChildren(uid);
    }
  }

  loadTeacherSelfDocuments() {
    this.isLoading.set(true);
    const teacherId = this.resolvedTeacherId();
    if (teacherId) {
      this.documentService.getTeacherDocuments(teacherId).subscribe({
        next: (docs) => {
          this.activeDocumentsList.set(docs);
          this.calculateStats(docs);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.toastService.error('Failed to load your documents.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }

  switchTeacherViewMode(mode: 'verify' | 'self') {
    this.teacherViewMode.set(mode);
    if (mode === 'self') {
      this.selectedStudent.set(null);
      this.selectedTeacher.set(null);
      this.loadTeacherSelfDocuments();
    } else {
      this.activeDocumentsList.set([]);
      this.calculateStats([]);
      this.loadPendingDocuments();
    }
  }

  // --- STUDENT / PARENT FLOWS ---
  fetchStudentProfileAndDocs(userId: number) {
    this.isLoading.set(true);
    this.studentService.getStudentByUserId(userId).subscribe({
      next: (student) => {
        this.resolvedStudentId.set(student.id);
        this.loadDocumentsForStudent(student.id);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to resolve student profile.');
        this.isLoading.set(false);
      }
    });
  }

  fetchParentProfileAndChildren(userId: number) {
    this.isLoading.set(true);
    this.parentService.getParentByUserId(userId).subscribe({
      next: (parent) => {
        this.parentService.getParentChildren(parent.id).subscribe({
          next: (children) => {
            this.parentChildren.set(children);
            if (children.length > 0) {
              this.selectedChildId.set(children[0].studentId);
              this.loadDocumentsForStudent(children[0].studentId);
            } else {
              this.isLoading.set(false);
            }
          },
          error: (err) => {
            console.error(err);
            this.isLoading.set(false);
          }
        });
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to resolve parent profile.');
        this.isLoading.set(false);
      }
    });
  }

  onChildChange(childId: number) {
    this.selectedChildId.set(childId);
    this.loadDocumentsForStudent(childId);
  }

  loadDocumentsForStudent(studentId: number) {
    this.isLoading.set(true);
    this.documentService.getStudentDocuments(studentId).subscribe({
      next: (docs) => {
        this.activeDocumentsList.set(docs);
        this.calculateStats(docs);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to load documents.');
        this.isLoading.set(false);
      }
    });
  }

  loadPendingDocuments() {
    this.isLoading.set(true);
    this.documentService.getPendingDocuments().subscribe({
      next: (docs) => {
        this.pendingDocuments.set(docs);
        this.systemPendingCount.set(docs.length);
        this.pendingDocsCount.set(docs.length);
        this.refreshDirectoryList();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }

  // --- ADMIN / TEACHER DIRECTORY FLOWS ---
  loadDirectoryClasses() {
    this.classService.getAllClasses().subscribe({
      next: (classes) => {
        if (this.userRole() === 'Teacher') {
          const teacher = this.currentTeacher();
          const filtered = classes.filter(c => 
            this.teacherClassIds().includes(c.id) || 
            (teacher && teacher.className && c.classname.toLowerCase() === teacher.className.toLowerCase() && 
             (!teacher.section || c.section?.toLowerCase() === teacher.section.toLowerCase()))
          );
          this.classesList.set(filtered);
        } else {
          this.classesList.set(classes);
        }
      },
      error: (err) => console.error(err)
    });
  }

  refreshDirectoryList() {
    const tab = this.activeDirectoryTab();
    const query = this.searchQuery().trim().toLowerCase();

    if (this.onlyShowPending()) {
      const pendingList = this.pendingDocuments();
      if (tab === 'students') {
        const students = pendingList
          .filter(d => d.ownerType === 'Student')
          .map(d => ({
            id: d.ownerId,
            userId: 0,
            regNo: d.ownerIdentifier,
            name: d.ownerName,
            className: 'Pending Verification',
            profilePhotoUrl: '',
            classId: d.classId // If available, or we verify by loading details
          }));

        // Let's filter pending students by class if user is Teacher
        this.studentService.getAllStudents({ pageNumber: 1, pageSize: 1000 }).subscribe({
          next: (res) => {
            const validClassIds = this.classesList().map(c => c.id);
            const allowedStudentIds = res.items
              .filter(s => this.userRole() !== 'Teacher' || (s.classId && validClassIds.includes(s.classId)))
              .map(s => s.id);

            const uniqueStudentsMap = new Map<number, any>();
            for (const s of students) {
              if (allowedStudentIds.includes(s.id) && !uniqueStudentsMap.has(s.id)) {
                if (!query || s.name.toLowerCase().includes(query) || s.regNo.toLowerCase().includes(query)) {
                  uniqueStudentsMap.set(s.id, s);
                }
              }
            }
            this.studentsDirectory.set(Array.from(uniqueStudentsMap.values()));
          },
          error: (err) => console.error(err)
        });
      } else {
        const teachers = pendingList
          .filter(d => d.ownerType === 'Teacher')
          .map(d => ({
            id: d.ownerId,
            userId: 0,
            name: d.ownerName,
            phonenumber: '',
            joiningdate: new Date(),
            username: d.ownerIdentifier
          }));

        const uniqueTeachersMap = new Map<number, any>();
        for (const t of teachers) {
          if (!uniqueTeachersMap.has(t.id)) {
            if (!query || t.name.toLowerCase().includes(query) || t.username.toLowerCase().includes(query)) {
              uniqueTeachersMap.set(t.id, t);
            }
          }
        }
        this.teachersDirectory.set(Array.from(uniqueTeachersMap.values()));
      }
    } else {
      if (tab === 'students') {
        const request: any = {
          searchQuery: query,
          pageNumber: 1,
          pageSize: 100
        };
        if (this.selectedClassId()) {
          request.classId = this.selectedClassId();
        }

        this.studentService.getAllStudents(request).subscribe({
          next: (res) => {
            if (this.userRole() === 'Teacher' && !this.selectedClassId()) {
              const validClassIds = this.classesList().map(c => c.id);
              const filtered = res.items.filter(s => s.classId && validClassIds.includes(s.classId));
              this.studentsDirectory.set(filtered);
            } else {
              this.studentsDirectory.set(res.items);
            }
          },
          error: (err) => console.error(err)
        });
      } else {
        this.teacherService.getAllTeachers({ searchQuery: query, pageNumber: 1, pageSize: 100 }).subscribe({
          next: (res) => this.teachersDirectory.set(res.items),
          error: (err) => console.error(err)
        });
      }
    }
  }

  onTabChange(tab: 'students' | 'teachers') {
    this.activeDirectoryTab.set(tab);
    this.selectedStudent.set(null);
    this.selectedTeacher.set(null);
    this.activeDocumentsList.set([]);
    this.calculateStats([]);
    this.refreshDirectoryList();
  }

  onClassChangeFilter(classId: number | null) {
    this.selectedClassId.set(classId);
    this.refreshDirectoryList();
  }

  selectStudentFromDirectory(student: StudentQueryResponseDTO) {
    this.selectedStudent.set(student);
    this.selectedTeacher.set(null);
    
    this.isLoading.set(true);
    this.documentService.getStudentDocuments(student.id).subscribe({
      next: (docs) => {
        this.activeDocumentsList.set(docs);
        this.calculateStats(docs);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to load student documents.');
        this.isLoading.set(false);
      }
    });
  }

  selectTeacherFromDirectory(teacher: TeacherResponseDTO) {
    this.selectedTeacher.set(teacher);
    this.selectedStudent.set(null);

    this.isLoading.set(true);
    this.documentService.getTeacherDocuments(teacher.id).subscribe({
      next: (docs) => {
        this.activeDocumentsList.set(docs);
        this.calculateStats(docs);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to load teacher documents.');
        this.isLoading.set(false);
      }
    });
  }

  // --- COMMON UTILITIES ---
  calculateStats(docs: DocumentResponseDTO[]) {
    this.totalDocsCount.set(docs.length);
    this.verifiedDocsCount.set(docs.filter(d => d.status?.toLowerCase() === 'verified').length);
    this.pendingDocsCount.set(docs.filter(d => d.status?.toLowerCase() === 'pending').length);
  }

  // --- UPLOAD FLOW ---
  openUploadModal() {
    this.uploadForm.set({
      documentName: '',
      selectedFile: null
    });
    this.showUploadModal.set(true);
  }

  closeUploadModal() {
    this.showUploadModal.set(false);
  }

  onFileSelected(event: any) {
    const file = event.target.files[0] as File;
    if (file) {
      this.uploadForm.update(form => ({ ...form, selectedFile: file }));
    }
  }

  submitUpload() {
    const form = this.uploadForm();
    if (!form.selectedFile) {
      this.toastService.warning('Please select a file to upload.');
      return;
    }

    const docName = form.documentName.trim() || form.selectedFile.name;
    this.isUploading.set(true);

    const role = this.userRole();
    if (role === 'Student' && this.resolvedStudentId()) {
      this.documentService.uploadStudentDocument(this.resolvedStudentId()!, form.selectedFile, docName).subscribe({
        next: () => {
          this.toastService.success('Document uploaded successfully!');
          this.loadDocumentsForStudent(this.resolvedStudentId()!);
          this.closeUploadModal();
          this.isUploading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.toastService.error('Failed to upload document.');
          this.isUploading.set(false);
        }
      });
    } else if (role === 'Parent' && this.selectedChildId()) {
      this.documentService.uploadStudentDocument(this.selectedChildId()!, form.selectedFile, docName).subscribe({
        next: () => {
          this.toastService.success('Document uploaded successfully!');
          this.loadDocumentsForStudent(this.selectedChildId()!);
          this.closeUploadModal();
          this.isUploading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.toastService.error('Failed to upload document.');
          this.isUploading.set(false);
        }
      });
    } else if (role === 'Teacher' && this.resolvedTeacherId()) {
      this.documentService.uploadTeacherDocument(this.resolvedTeacherId()!, form.selectedFile, docName).subscribe({
        next: () => {
          this.toastService.success('Document uploaded successfully!');
          this.documentService.getTeacherDocuments(this.resolvedTeacherId()!).subscribe({
            next: (docs) => {
              this.activeDocumentsList.set(docs);
              this.calculateStats(docs);
            }
          });
          this.closeUploadModal();
          this.isUploading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.toastService.error('Failed to upload document.');
          this.isUploading.set(false);
        }
      });
    }
  }

  // --- VERIFICATION & DELETE ACTIONS ---
  updateVerificationStatus(doc: DocumentResponseDTO, newStatus: 'Verified' | 'Rejected') {
    const type = this.activeDirectoryTab() === 'students' ? 'Student' : 'Teacher';
    
    this.isLoading.set(true);
    this.documentService.verifyDocument({
      documentType: type,
      documentId: doc.id,
      status: newStatus
    }).subscribe({
      next: () => {
        this.toastService.success(`Document marked as ${newStatus}!`);
        this.loadPendingDocuments();
        if (this.selectedStudent()) {
          this.selectStudentFromDirectory(this.selectedStudent()!);
        } else if (this.selectedTeacher()) {
          this.selectTeacherFromDirectory(this.selectedTeacher()!);
        }
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to verify document.');
        this.isLoading.set(false);
      }
    });
  }

  deleteDocument(doc: DocumentResponseDTO) {
    if (!confirm('Are you sure you want to permanently delete this document?')) return;

    this.isLoading.set(true);
    this.documentService.deleteDocument(doc.blobUrl).subscribe({
      next: () => {
        this.toastService.success('Document deleted successfully.');
        if (this.selectedStudent()) {
          this.selectStudentFromDirectory(this.selectedStudent()!);
        } else if (this.selectedTeacher()) {
          this.selectTeacherFromDirectory(this.selectedTeacher()!);
        }
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to delete document.');
        this.isLoading.set(false);
      }
    });
  }

  hasPhoto(): boolean {
    const studentPhoto = this.selectedStudent()?.profilePhotoUrl;
    const teacherPhoto = this.selectedTeacher()?.profilePhotoUrl;
    
    if (this.selectedStudent()) {
      return !!studentPhoto && studentPhoto !== 'null' && studentPhoto.trim() !== '';
    }
    if (this.selectedTeacher()) {
      return !!teacherPhoto && teacherPhoto !== 'null' && teacherPhoto.trim() !== '';
    }
    return false;
  }

  getPhotoUrl(): string {
    let url = '';
    if (this.selectedStudent()) {
      url = this.selectedStudent()?.profilePhotoUrl || '';
    } else if (this.selectedTeacher()) {
      url = this.selectedTeacher()?.profilePhotoUrl || '';
    }
    if (url && url.startsWith('/')) {
      return `http://localhost:5203${url}`;
    }
    return url;
  }

  hasDirectoryPhoto(item: any): boolean {
    return !!item.profilePhotoUrl && item.profilePhotoUrl !== 'null' && item.profilePhotoUrl.trim() !== '';
  }

  getDirectoryPhotoUrl(item: any): string {
    const url = item.profilePhotoUrl || '';
    if (url && url.startsWith('/')) {
      return `http://localhost:5203${url}`;
    }
    return url;
  }

  getFileUrl(blobUrl: string): string {
    if (blobUrl && blobUrl.startsWith('/')) {
      return `http://localhost:5203${blobUrl}`;
    }
    return blobUrl;
  }
}
