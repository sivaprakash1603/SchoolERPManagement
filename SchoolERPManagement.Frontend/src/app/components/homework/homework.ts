import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HomeworkService, HomeworkResponseDTO, HomeworkSubmissionDetailsDTO } from '../../services/homework.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { SubjectService, SubjectResponseDTO } from '../../services/subject.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-homework',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './homework.html',
  styleUrl: './homework.css',
})
export class Homework implements OnInit {
  private homeworkService = inject(HomeworkService);
  private academicYearService = inject(AcademicYearService);
  private classService = inject(ClassService);
  private subjectService = inject(SubjectService);
  private teacherService = inject(TeacherService);
  private studentService = inject(StudentService);
  private toastService = inject(ToastService);

  // Selector data
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  subjects = signal<SubjectResponseDTO[]>([]);
  teachers = signal<TeacherResponseDTO[]>([]);

  selectedAcademicYearId = signal<number | null>(null);
  selectedClassId = signal<number | null>(null);
  selectedSubjectId = signal<number | null>(null);

  // Homework list
  homeworks = signal<HomeworkResponseDTO[]>([]);

  // Selected Homework and Submissions
  selectedHomework = signal<HomeworkResponseDTO | null>(null);
  submissions = signal<HomeworkSubmissionDetailsDTO[]>([]);

  // Modals
  showCreateModal = signal(false);
  showSubmissionsModal = signal(false);

  // Loaders
  loading = signal(false);
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
    } else {
      this.fetchAcademicYears();
      this.fetchTeachers();
    }
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find((y) => y.isCurrent) || years[0];
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.fetchClasses(currentYear.id);
        }
      },
      error: (err) => {
        console.error('Failed to load academic years', err);
        this.toastService.error('Failed to load academic years.');
      }
    });
  }

  fetchClasses(yearId: number) {
    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => {
        this.classes.set(res);
        if (res.length > 0) {
          this.selectedClassId.set(res[0].id);
          this.fetchSubjectsForClass();
          this.fetchHomeworks();
        } else {
          this.selectedClassId.set(null);
          this.subjects.set([]);
          this.homeworks.set([]);
          this.calculateMetrics();
        }
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.toastService.error('Failed to load classes.');
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
        this.subjects.set(res);
        // Reset selected subject filter if it's no longer available
        const currentSubId = this.selectedSubjectId();
        if (currentSubId && !res.some((s) => s.id === currentSubId)) {
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
    this.createForm.set({
      subjectId: this.subjects().length > 0 ? this.subjects()[0].id : null,
      teacherId: this.teachers().length > 0 ? this.teachers()[0].id : null,
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
