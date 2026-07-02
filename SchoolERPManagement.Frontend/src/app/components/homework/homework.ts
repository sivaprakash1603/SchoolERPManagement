import { environment } from '../../../environments/environment';
import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HomeworkService, HomeworkResponseDTO, HomeworkSubmissionDetailsDTO } from '../../services/homework.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { SubjectService, SubjectResponseDTO } from '../../services/subject.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';
import { TimetableService } from '../../services/timetable.service';
import { ParentService } from '../../services/parent.service';
import { FilterStateService } from '../../services/filter-state.service';

@Component({
  selector: 'app-homework',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './homework.html',
  styleUrl: './homework.css',
})
export class Homework implements OnInit {
  baseUrl = environment.baseUrl;
  private homeworkService = inject(HomeworkService);
  private academicYearService = inject(AcademicYearService);
  private classService = inject(ClassService);
  private subjectService = inject(SubjectService);
  private teacherService = inject(TeacherService);
  private studentService = inject(StudentService);
  private toastService = inject(ToastService);
  private timetableService = inject(TimetableService);
  private parentService = inject(ParentService);
  private filterStateService = inject(FilterStateService);

  constructor() {
    const savedState = this.filterStateService.getState('homework');
    if (savedState) {
      if (savedState.selectedAcademicYearId !== undefined) this.selectedAcademicYearId.set(savedState.selectedAcademicYearId);
      if (savedState.selectedClassId !== undefined) this.selectedClassId.set(savedState.selectedClassId);
      if (savedState.selectedSubjectId !== undefined) this.selectedSubjectId.set(savedState.selectedSubjectId);
      if (savedState.studentFilterTab !== undefined) this.studentFilterTab.set(savedState.studentFilterTab);
    }

    effect(() => {
      this.filterStateService.saveState('homework', {
        selectedAcademicYearId: this.selectedAcademicYearId(),
        selectedClassId: this.selectedClassId(),
        selectedSubjectId: this.selectedSubjectId(),
        studentFilterTab: this.studentFilterTab()
      });
    });
  }

  // Selector data
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  subjects = signal<SubjectResponseDTO[]>([]);
  teachers = signal<TeacherResponseDTO[]>([]);
  resolvedTeacherId = signal<number | null>(null);

  selectedAcademicYearId = signal<number | null>(null);
  selectedClassId = signal<number | null>(null);
  selectedSubjectId = signal<number | null>(null);

  // Parent specific
  parentChildren = signal<any[]>([]);
  selectedChildId = signal<number | null>(null);

  // Homework list
  homeworks = signal<HomeworkResponseDTO[]>([]);

  // Selected Homework and Submissions
  selectedHomework = signal<HomeworkResponseDTO | null>(null);
  submissions = signal<HomeworkSubmissionDetailsDTO[]>([]);

  // Modals
  showCreateModal = signal(false);
  showSubmissionsModal = signal(false);

  // Loaders
  loading = signal(true);
  loadingSubmissions = signal(false);
  isSaving = signal(false);
  isEvaluating = signal(false);

  isAdminOrTeacher = signal(false);
  currentUserId = signal<number | null>(null);
  userRole = signal<string>('Student');

  // Student-specific
  resolvedStudentId = signal<number | null>(null);
  studentHomeworks = signal<HomeworkResponseDTO[]>([]);
  studentFilterTab = signal<'all' | 'pending' | 'submitted' | 'graded'>('all');
  filteredStudentHomeworks = signal<HomeworkResponseDTO[]>([]);
  studentStats = signal({ total: 0, pending: 0, submitted: 0, graded: 0 });
  showSubmitModal = signal(false);
  selectedHomeworkForSubmit = signal<HomeworkResponseDTO | null>(null);
  studentSubmitFile: File | null = null;
  isSubmitting = signal(false);
  expandedHomeworkId = signal<number | null>(null);

  // View UI Helpers
  get pageTitle(): string {
    if (this.userRole() === 'Student') return 'My Homework';
    if (this.userRole() === 'Parent') return 'Child Homework';
    return 'Homework & Assignments';
  }

  get pageDescription(): string {
    if (this.userRole() === 'Student') return 'Track your assignments, submit work, and view grades.';
    if (this.userRole() === 'Parent') return "Track your child's assignments, and view their grades.";
    return 'Assign and evaluate student homework assignments.';
  }

  // Forms
  createForm = signal({
    subjectId: null as number | null,
    teacherId: null as number | null,
    title: '',
    description: '',
    dueDate: '',
  });
  selectedFile: File | null = null;

  evaluationForm = signal({
    submissionId: null as number | null,
    marks: 0,
    remarks: '',
    verificationStatus: 'approved' // 'approved' | 'rejected'
  });

  // Metrics
  totalHomework = signal(0);
  pendingEvaluations = signal(0);
  completedEvaluations = signal(0);

  ngOnInit() {
    const role = sessionStorage.getItem('role');
    this.isAdminOrTeacher.set(role === 'Admin' || role === 'Teacher');

    const uid = sessionStorage.getItem('userId');
    this.currentUserId.set(uid ? parseInt(uid, 10) : null);
    this.userRole.set(role || 'Student');

    if (role === 'Student' && uid) {
      this.studentService.getStudentByUserId(parseInt(uid, 10)).subscribe({
        next: (student: any) => {
          this.resolvedStudentId.set(student.id);
          this.fetchStudentHomeworks(student.id);
        },
        error: (err: any) => {
          console.error('Failed to resolve student profile', err);
          this.toastService.error('Failed to resolve your student profile.');
        }
      });
    } else if (role === 'Parent' && uid) {
      this.parentService.getParentByUserId(parseInt(uid, 10)).subscribe({
        next: (parent) => {
          this.parentService.getParentChildren(parent.id).subscribe({
            next: (children) => {
              this.parentChildren.set(children);
              if (children.length > 0) {
                this.selectedChildId.set(children[0].studentId);
                this.resolvedStudentId.set(children[0].studentId);
                this.fetchStudentHomeworks(children[0].studentId);
              }
            },
            error: (err) => console.error('Failed to load parent children', err)
          });
        },
        error: (err) => {
          console.error('Failed to resolve parent profile', err);
          this.toastService.error('Failed to resolve parent profile.');
            this.loading.set(false);
        }
      });
    } else {
      this.fetchAcademicYears();
      this.fetchTeachers();
      if (role === 'Teacher') {
        const username = sessionStorage.getItem('username') || '';
        this.teacherService.getTeacherByUsername(username).subscribe({
          next: (teacher) => {
            this.resolvedTeacherId.set(teacher.id);
          },
          error: (err) => console.error('Failed to resolve teacher profile', err)
        });
      }
    }
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const savedId = this.selectedAcademicYearId();
        const currentYear = (savedId && years.find((y) => y.id === savedId)) 
          || years.find((y) => y.isCurrent) 
          || years[0];
          
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.fetchClasses(currentYear.id);
        } else {
          this.loading.set(false);
        }
      },
      error: (err) => {
        console.error('Failed to load academic years', err);
        this.toastService.error('Failed to load academic years.');
      }
    });
  }

  onChildChange(studentId: number) {
    this.selectedChildId.set(studentId);
    this.resolvedStudentId.set(studentId);
    
    const child = this.parentChildren().find(c => c.studentId === studentId);
    this.fetchStudentHomeworks(studentId);
  }

  teacherAssignments = signal<{classId: number, subjectId: number}[]>([]);
  teacherTimetableSlots = signal<any[]>([]);

  fetchClasses(yearId: number) {
    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => {
        if (this.userRole() === 'Teacher') {
          const username = sessionStorage.getItem('username') || '';
          this.teacherService.getTeacherByUsername(username).subscribe({
            next: (teacher) => {
              this.resolvedTeacherId.set(teacher.id);
              this.teacherAssignments.set(teacher.assignments || []);

              this.timetableService.getTeacherTimetable(teacher.id).subscribe({
                next: (slots) => {
                  this.teacherTimetableSlots.set(slots);
                  const assignedClassIds = new Set<number>([
                    ...slots.map(s => s.classId),
                    ...(teacher.assignments || []).map(a => a.classId)
                  ]);

                  const filtered = res.filter(c => 
                    assignedClassIds.has(c.id)
                  );
                  
                  this.classes.set(filtered);
                  if (filtered.length > 0) {
                    const savedId = this.selectedClassId();
                    const validId = (savedId && filtered.some(c => c.id === savedId)) ? savedId : filtered[0].id;
                    this.selectedClassId.set(validId);
                    this.fetchSubjectsForClass();
                    this.fetchHomeworks();
                  } else {
                    this.selectedClassId.set(null);
                    this.subjects.set([]);
                    this.homeworks.set([]);
                    this.calculateMetrics();
                    this.loading.set(false);
                  }
                },
                error: (err) => {
                  console.error('Failed to load teacher timetable', err);
                  const assignedClassIds = new Set<number>((teacher.assignments || []).map(a => a.classId));
                  const filtered = res.filter(c => assignedClassIds.has(c.id));
                  this.classes.set(filtered);
                  if (filtered.length > 0) {
                    const savedId = this.selectedClassId();
                    const validId = (savedId && filtered.some(c => c.id === savedId)) ? savedId : filtered[0].id;
                    this.selectedClassId.set(validId);
                    this.fetchSubjectsForClass();
                    this.fetchHomeworks();
                  } else {
                    this.selectedClassId.set(null);
                    this.subjects.set([]);
                    this.homeworks.set([]);
                    this.calculateMetrics();
                    this.loading.set(false);
                  }
                }
              });
            },
            error: (err) => {
              console.error('Failed to load teacher profile', err);
              this.classes.set([]);
              this.selectedClassId.set(null);
              this.subjects.set([]);
              this.homeworks.set([]);
              this.calculateMetrics();
                    this.loading.set(false);
            }
          });
        } else {
          this.classes.set(res);
          if (res.length > 0) {
            const savedId = this.selectedClassId();
            const validId = (savedId && res.some(c => c.id === savedId)) ? savedId : res[0].id;
            this.selectedClassId.set(validId);
            this.fetchSubjectsForClass();
            this.fetchHomeworks();
          } else {
            this.selectedClassId.set(null);
            this.subjects.set([]);
            this.homeworks.set([]);
            this.calculateMetrics();
                    this.loading.set(false);
          }
        }
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.toastService.error('Failed to load classes.');
        this.loading.set(false);
      }
    });
  }

  fetchSubjectsForClass() {
    const classId = this.selectedClassId();
    if (!classId) {
      this.subjects.set([]);
      return;
    }

    this.subjectService.getSubjectsByClass(classId).subscribe({
      next: (res) => {
        let filteredSubjects = res;
        
        if (this.userRole() === 'Teacher') {
          const allowedSubjectIds = new Set<number>([
            ...this.teacherTimetableSlots().filter(s => s.classId === classId).map(s => s.subjectId),
            ...this.teacherAssignments().filter(a => a.classId === classId).map(a => a.subjectId)
          ]);
          filteredSubjects = res.filter(s => allowedSubjectIds.has(s.id));
        }

        this.subjects.set(filteredSubjects);
        // Reset selected subject filter if it's no longer available
        const currentSubId = this.selectedSubjectId();
        if (currentSubId && !filteredSubjects.some((s) => s.id === currentSubId)) {
          this.selectedSubjectId.set(null);
          this.fetchHomeworks();
        }
      },
      error: (err) => {
        console.error('Failed to load subjects for class', err);
        this.subjects.set([]);
      }
    });
  }

  fetchTeachers() {
    this.teacherService.getAllTeachers({ pageSize: 1000 }).subscribe({
      next: (res) => this.teachers.set(res.items),
      error: (err) => console.error('Failed to load teachers', err)
    });
  }

  fetchHomeworks() {
    const classId = this.selectedClassId();
    if (!classId) return;

    this.loading.set(true);
    const subId = this.selectedSubjectId() || undefined;

    this.homeworkService.getHomeworks(classId, subId).subscribe({
      next: (res) => {
        this.homeworks.set(res);
        this.calculateMetrics();
                    this.loading.set(false);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load homeworks', err);
        this.toastService.error('Failed to load homework assignments.');
        this.loading.set(false);
      }
    });
  }

  calculateMetrics() {
    const list = this.homeworks();
    this.totalHomework.set(list.length);
    // Note: detailed submission count requires checking submissions of each homework,
    // which we will calculate dynamically as submissions are loaded, or default to placeholder
    this.pendingEvaluations.set(0);
    this.completedEvaluations.set(0);
  }

  onYearSelect(val: number) {
    this.selectedAcademicYearId.set(val);
    this.fetchClasses(val);
  }

  onClassSelect() {
    this.fetchSubjectsForClass();
    this.fetchHomeworks();
  }

  onSubjectFilterSelect() {
    this.fetchHomeworks();
  }

  openCreateModal() {
    const today = new Date().toISOString().split('T')[0];
    const defaultTeacherId = this.userRole() === 'Teacher' && this.resolvedTeacherId() 
      ? this.resolvedTeacherId() 
      : (this.teachers().length > 0 ? this.teachers()[0].id : null);
    this.createForm.set({
      subjectId: this.subjects().length > 0 ? this.subjects()[0].id : null,
      teacherId: defaultTeacherId,
      title: '',
      description: '',
      dueDate: today,
    });
    this.selectedFile = null;
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  saveHomework() {
    const form = this.createForm();
    const classId = this.selectedClassId();

    if (!classId || !form.subjectId || !form.teacherId || !form.title.trim()) {
      this.toastService.warning('Please fill in all required fields.');
      return;
    }

    this.isSaving.set(true);

    const formData = new FormData();
    formData.append('classId', classId.toString());
    formData.append('subjectId', form.subjectId.toString());
    formData.append('teacherId', form.teacherId.toString());
    formData.append('title', form.title.trim());
    formData.append('description', form.description.trim());
    formData.append('dueDate', form.dueDate);

    if (this.selectedFile) {
      formData.append('attachment', this.selectedFile);
    }

    this.homeworkService.createHomework(formData).subscribe({
      next: () => {
        this.toastService.success('Homework assignment created successfully!');
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchHomeworks();
      },
      error: (err) => {
        console.error('Failed to create homework', err);
        const errMsg = this.getErrorMessage(err, 'Failed to create homework.');
        this.toastService.error(errMsg);
        this.isSaving.set(false);
      }
    });
  }

  openSubmissionsModal(homework: HomeworkResponseDTO) {
    this.selectedHomework.set(homework);
    this.submissions.set([]);
    this.showSubmissionsModal.set(true);
    this.fetchSubmissions(homework.id);
  }

  closeSubmissionsModal() {
    this.showSubmissionsModal.set(false);
    this.selectedHomework.set(null);
  }

  fetchSubmissions(homeworkId: number) {
    this.loadingSubmissions.set(true);
    this.homeworkService.getHomeworkSubmissions(homeworkId).subscribe({
      next: (res) => {
        this.submissions.set(res);
        this.loadingSubmissions.set(false);
        this.updateSubmissionMetrics(res);
      },
      error: (err) => {
        console.error('Failed to load submissions', err);
        this.toastService.error('Failed to load submissions.');
        this.loadingSubmissions.set(false);
      }
    });
  }

  updateSubmissionMetrics(subs: HomeworkSubmissionDetailsDTO[]) {
    const pending = subs.filter(s => s.verificationStatus?.toLowerCase() === 'pending').length;
    const completed = subs.filter(s => s.verificationStatus?.toLowerCase() === 'approved' || s.verificationStatus?.toLowerCase() === 'rejected').length;
    this.pendingEvaluations.set(pending);
    this.completedEvaluations.set(completed);
  }

  selectSubmissionForEvaluation(sub: HomeworkSubmissionDetailsDTO) {
    this.evaluationForm.set({
      submissionId: sub.id,
      marks: sub.marks || 0,
      remarks: sub.remarks || '',
      verificationStatus: sub.verificationStatus?.toLowerCase() === 'rejected' ? 'rejected' : 'approved'
    });
  }

  submitEvaluation() {
    const form = this.evaluationForm();
    if (!form.submissionId) return;

    this.isEvaluating.set(true);

    const dto = {
      homeworkSubmissionId: form.submissionId,
      marks: form.marks,
      remarks: form.remarks,
      verificationStatus: form.verificationStatus
    };

    this.homeworkService.evaluateHomework(dto).subscribe({
      next: () => {
        this.toastService.success('Submission evaluated successfully!');
        this.isEvaluating.set(false);
        const hw = this.selectedHomework();
        if (hw) {
          this.fetchSubmissions(hw.id);
        }
        // Reset form
        this.evaluationForm.set({
          submissionId: null,
          marks: 0,
          remarks: '',
          verificationStatus: 'approved'
        });
      },
      error: (err) => {
        console.error('Failed to save evaluation', err);
        const errMsg = this.getErrorMessage(err, 'Failed to save evaluation.');
        this.toastService.error(errMsg);
        this.isEvaluating.set(false);
      }
    });
  }

  private getErrorMessage(err: any, fallback: string): string {
    if (err.error?.errors) {
      const validationErrors = Object.values(err.error.errors).flat();
      if (validationErrors.length > 0) {
        return validationErrors.join(' ');
      }
    }
    return err.error?.message || fallback;
  }

  // ==============================
  // STUDENT-SPECIFIC METHODS
  // ==============================

  fetchStudentHomeworks(studentId: number) {
    this.loading.set(true);
    this.homeworkService.getHomeworksByStudentId(studentId).subscribe({
      next: (res) => {
        this.studentHomeworks.set(res);
        this.calculateStudentStats(res);
        this.applyStudentFilter();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load student homeworks', err);
        this.toastService.error('Failed to load your homework assignments.');
        this.loading.set(false);
      }
    });
  }

  calculateStudentStats(homeworks: HomeworkResponseDTO[]) {
    const total = homeworks.length;
    const submitted = homeworks.filter(h => h.submission && h.submission.verificationStatus?.toLowerCase() === 'pending').length;
    const graded = homeworks.filter(h => h.submission && (h.submission.verificationStatus?.toLowerCase() === 'approved' || h.submission.verificationStatus?.toLowerCase() === 'rejected')).length;
    const pending = total - submitted - graded;
    this.studentStats.set({ total, pending, submitted, graded });
  }

  applyStudentFilter() {
    const tab = this.studentFilterTab();
    const homeworks = this.studentHomeworks();
    let filtered: HomeworkResponseDTO[];

    switch (tab) {
      case 'pending':
        filtered = homeworks.filter(h => !h.submission);
        break;
      case 'submitted':
        filtered = homeworks.filter(h => h.submission && h.submission.verificationStatus?.toLowerCase() === 'pending');
        break;
      case 'graded':
        filtered = homeworks.filter(h => h.submission && (h.submission.verificationStatus?.toLowerCase() === 'approved' || h.submission.verificationStatus?.toLowerCase() === 'rejected'));
        break;
      default:
        filtered = homeworks;
    }
    this.filteredStudentHomeworks.set(filtered);
  }

  onStudentFilterChange(tab: 'all' | 'pending' | 'submitted' | 'graded') {
    this.studentFilterTab.set(tab);
    this.applyStudentFilter();
  }

  getHomeworkStatus(hw: HomeworkResponseDTO): string {
    if (!hw.submission) return 'pending';
    const vs = hw.submission.verificationStatus?.toLowerCase();
    if (vs === 'approved') return 'approved';
    if (vs === 'rejected') return 'rejected';
    return 'submitted';
  }

  getHomeworkStatusLabel(hw: HomeworkResponseDTO): string {
    const status = this.getHomeworkStatus(hw);
    switch (status) {
      case 'approved': return 'Graded ✓';
      case 'rejected': return 'Redo Required';
      case 'submitted': return 'Submitted';
      default: return 'Not Submitted';
    }
  }

  isOverdue(hw: HomeworkResponseDTO): boolean {
    if (hw.submission) return false;
    const today = new Date().toISOString().split('T')[0];
    return hw.dueDate < today;
  }

  openSubmitHomeworkModal(hw: HomeworkResponseDTO) {
    this.selectedHomeworkForSubmit.set(hw);
    this.studentSubmitFile = null;
    this.showSubmitModal.set(true);
  }

  closeSubmitModal() {
    this.showSubmitModal.set(false);
    this.selectedHomeworkForSubmit.set(null);
    this.studentSubmitFile = null;
  }

  onStudentFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.studentSubmitFile = input.files[0];
    }
  }

  submitStudentHomework() {
    const hw = this.selectedHomeworkForSubmit();
    const studentId = this.resolvedStudentId();
    if (!hw || !studentId) return;

    this.isSubmitting.set(true);
    const formData = new FormData();
    formData.append('homeworkId', hw.id.toString());
    formData.append('studentId', studentId.toString());
    if (this.studentSubmitFile) {
      formData.append('uploadedFile', this.studentSubmitFile);
    }

    this.homeworkService.submitHomework(formData).subscribe({
      next: () => {
        this.toastService.success('Homework submitted successfully!');
        this.isSubmitting.set(false);
        this.closeSubmitModal();
        this.fetchStudentHomeworks(studentId);
      },
      error: (err) => {
        console.error('Failed to submit homework', err);
        this.toastService.error(this.getErrorMessage(err, 'Failed to submit homework.'));
        this.isSubmitting.set(false);
      }
    });
  }

  toggleHomeworkExpand(hwId: number) {
    this.expandedHomeworkId.set(this.expandedHomeworkId() === hwId ? null : hwId);
  }
}
