import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExamService, ExamResponseDTO, ExamScheduleResponseDTO } from '../../services/exam.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { SubjectService, SubjectResponseDTO } from '../../services/subject.service';
import { StudentService, StudentQueryResponseDTO } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';

interface StudentExamUI extends StudentQueryResponseDTO {
  marks: number | null;
  isPublishing: boolean;
  isPublished: boolean;
}

@Component({
  selector: 'app-exams',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './exams.html',
  styleUrl: './exams.css',
})
export class Exams implements OnInit {
  private examService = inject(ExamService);
  private academicYearService = inject(AcademicYearService);
  private classService = inject(ClassService);
  private subjectService = inject(SubjectService);
  private studentService = inject(StudentService);
  private toastService = inject(ToastService);

  // Lists
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  subjects = signal<SubjectResponseDTO[]>([]);
  exams = signal<ExamResponseDTO[]>([]);
  schedules = signal<ExamScheduleResponseDTO[]>([]);
  students = signal<StudentExamUI[]>([]);
  modalSubjects = signal<SubjectResponseDTO[]>([]);

  // Selection signals
  selectedAcademicYearId = signal<number | null>(null);
  selectedExam = signal<ExamResponseDTO | null>(null);
  selectedSchedule = signal<ExamScheduleResponseDTO | null>(null);

  // Modals
  showCreateExamModal = signal(false);
  showScheduleModal = signal(false);

  // Loaders
  loadingExams = signal(false);
  loadingSchedules = signal(false);
  loadingStudents = signal(false);
  isSavingExam = signal(false);
  isSavingSchedule = signal(false);

  isAdminOrTeacher = signal(false);
  studentId = signal<number | null>(null);
  studentClassId = signal<number | null>(null);
  studentResults = signal<any[]>([]);

  // Form states
  examForm = signal({
    examname: '',
  });

  scheduleForm = signal({
    classId: null as number | null,
    subjectId: null as number | null,
    examDate: '',
    durationMinutes: 120,
    session: 'Morning',
  });

  ngOnInit() {
    const role = sessionStorage.getItem('role');
    this.isAdminOrTeacher.set(role === 'Admin' || role === 'Teacher');

    const userIdStr = sessionStorage.getItem('userId');
    const userId = userIdStr ? parseInt(userIdStr, 10) : null;

    if (role === 'Student' && userId) {
      this.studentService.getStudentByUserId(userId).subscribe({
        next: (student) => {
          this.studentId.set(student.id);
          this.studentClassId.set(student.classId);
          this.fetchStudentResults(student.id);
        },
        error: (err) => console.error('Failed to resolve student profile', err)
      });
    }

    this.fetchAcademicYears();
    this.fetchSubjects();
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find((y) => y.isCurrent) || years[0];
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.fetchClasses(currentYear.id);
          this.fetchExams();
        }
      },
      error: (err) => {
        console.error('Failed to load academic years', err);
        this.toastService.error('Failed to load academic sessions.');
      }
    });
  }

  fetchClasses(yearId: number) {
    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => this.classes.set(res),
      error: (err) => console.error('Failed to load classes', err)
    });
  }

  fetchSubjects() {
    this.subjectService.getAllSubjects().subscribe({
      next: (res) => this.subjects.set(res),
      error: (err) => console.error('Failed to load subjects', err)
    });
  }

  fetchExams() {
    this.loadingExams.set(true);
    this.examService.getAllExams().subscribe({
      next: (res) => {
        // Filter exams based on current academic year (if applicable)
        const yearId = this.selectedAcademicYearId();
        const filtered = yearId ? res.filter(e => e.academicyearId === yearId) : res;
        this.exams.set(filtered);
        this.loadingExams.set(false);

        // Auto select first exam if available
        if (filtered.length > 0) {
          this.selectExam(filtered[0]);
        } else {
          this.selectedExam.set(null);
          this.schedules.set([]);
          this.students.set([]);
        }
      },
      error: (err) => {
        console.error('Failed to load exams', err);
        this.toastService.error('Failed to load exams.');
        this.loadingExams.set(false);
      }
    });
  }

  onYearSelect(val: number) {
    this.selectedAcademicYearId.set(val);
    this.fetchClasses(val);
    this.fetchExams();
  }

  selectExam(exam: ExamResponseDTO) {
    this.selectedExam.set(exam);
    this.selectedSchedule.set(null);
    this.students.set([]);
    this.fetchSchedules(exam.id);
  }

  fetchSchedules(examId: number) {
    this.loadingSchedules.set(true);
    this.examService.getExamSchedules(examId).subscribe({
      next: (res) => {
        if (!this.isAdminOrTeacher() && this.studentClassId()) {
          this.schedules.set(res.filter(s => s.classId === this.studentClassId()));
        } else {
          this.schedules.set(res);
        }
        this.loadingSchedules.set(false);
      },
      error: (err) => {
        console.error('Failed to load schedules', err);
        this.toastService.error('Failed to load exam schedules.');
        this.loadingSchedules.set(false);
      }
    });
  }

  openCreateExamModal() {
    this.examForm.set({ examname: '' });
    this.showCreateExamModal.set(true);
  }

  closeCreateExamModal() {
    this.showCreateExamModal.set(false);
  }

  saveExam() {
    const form = this.examForm();
    const yearId = this.selectedAcademicYearId();
    if (!form.examname.trim() || !yearId) return;

    this.isSavingExam.set(true);
    this.examService.createExam({ examname: form.examname.trim(), academicyearId: yearId }).subscribe({
      next: (newExam) => {
        this.toastService.success('Exam created successfully.');
        this.isSavingExam.set(false);
        this.closeCreateExamModal();
        this.fetchExams();
      },
      error: (err) => {
        console.error('Failed to save exam', err);
        this.toastService.error(this.getErrorMessage(err, 'Failed to create exam.'));
        this.isSavingExam.set(false);
      }
    });
  }

  openScheduleModal() {
    const today = new Date().toISOString().split('T')[0];
    const defaultClassId = this.classes().length > 0 ? this.classes()[0].id : null;
    
    this.scheduleForm.set({
      classId: defaultClassId,
      subjectId: null,
      examDate: today,
      durationMinutes: 120,
      session: 'Morning'
    });

    this.modalSubjects.set([]);
    if (defaultClassId) {
      this.onModalClassChange(defaultClassId);
    }
    this.showScheduleModal.set(true);
  }

  onModalClassChange(classId: number | null) {
    if (!classId) {
      this.modalSubjects.set([]);
      this.scheduleForm.set({ ...this.scheduleForm(), subjectId: null });
      return;
    }

    this.subjectService.getSubjectsByClass(classId).subscribe({
      next: (res) => {
        this.modalSubjects.set(res);
        if (res.length > 0) {
          this.scheduleForm.set({ ...this.scheduleForm(), subjectId: res[0].id });
        } else {
          this.scheduleForm.set({ ...this.scheduleForm(), subjectId: null });
        }
      },
      error: (err) => {
        console.error('Failed to load subjects for class', err);
        this.modalSubjects.set([]);
        this.scheduleForm.set({ ...this.scheduleForm(), subjectId: null });
      }
    });
  }

  closeScheduleModal() {
    this.showScheduleModal.set(false);
  }

  saveSchedule() {
    const exam = this.selectedExam();
    const form = this.scheduleForm();
    if (!exam || !form.classId || !form.subjectId || !form.examDate) return;

    this.isSavingSchedule.set(true);
    const dto = {
      examId: exam.id,
      classId: form.classId,
      subjectId: form.subjectId,
      examDate: form.examDate,
      durationMinutes: form.durationMinutes,
      session: form.session
    };

    this.examService.createExamSchedule(dto).subscribe({
      next: () => {
        this.toastService.success('Subject scheduled successfully.');
        this.isSavingSchedule.set(false);
        this.closeScheduleModal();
        this.fetchSchedules(exam.id);
      },
      error: (err) => {
        console.error('Failed to save schedule', err);
        this.toastService.error(this.getErrorMessage(err, 'Failed to schedule exam subject.'));
        this.isSavingSchedule.set(false);
      }
    });
  }

  selectSchedule(schedule: ExamScheduleResponseDTO) {
    this.selectedSchedule.set(schedule);
    this.fetchStudents(schedule);
  }

  fetchStudents(schedule: ExamScheduleResponseDTO) {
    this.loadingStudents.set(true);
    this.studentService.getStudentsByClassId(schedule.classId).subscribe({
      next: (studentList) => {
        this.examService.getExamResultsByClass(schedule.examId, schedule.classId, schedule.subjectId).subscribe({
          next: (results) => {
            const uiList: StudentExamUI[] = studentList.map(s => {
              const res = results.find(r => r.studentId === s.id);
              return {
                ...s,
                marks: res && res.marks !== undefined ? Number(res.marks) : null,
                isPublishing: false,
                isPublished: res !== undefined
              };
            });
            this.students.set(uiList);
            this.loadingStudents.set(false);
          },
          error: (err) => {
            console.error('Failed to load class exam results', err);
            const uiList: StudentExamUI[] = studentList.map(s => ({
              ...s,
              marks: null,
              isPublishing: false,
              isPublished: false
            }));
            this.students.set(uiList);
            this.loadingStudents.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Failed to load class students', err);
        this.toastService.error('Failed to load students for this class.');
        this.loadingStudents.set(false);
      }
    });
  }

  publishMarks(student: StudentExamUI) {
    const exam = this.selectedExam();
    const schedule = this.selectedSchedule();
    if (!exam || !schedule || student.marks === null || student.marks === undefined) return;

    student.isPublishing = true;
    const dto = {
      examId: exam.id,
      subjectId: schedule.subjectId,
      studentId: student.id,
      marks: student.marks
    };

    this.examService.publishResult(dto).subscribe({
      next: () => {
        student.isPublishing = false;
        student.isPublished = true;
        this.toastService.success(`Published marks for ${student.name}`);
      },
      error: (err) => {
        console.error('Failed to publish marks', err);
        this.toastService.error(this.getErrorMessage(err, 'Failed to publish results.'));
        student.isPublishing = false;
      }
    });
  }

  fetchStudentResults(studentId: number) {
    this.examService.getStudentResults(studentId).subscribe({
      next: (res) => {
        this.studentResults.set(res);
      },
      error: (err) => console.error('Failed to load student results', err)
    });
  }

  getStudentMarkForSchedule(schedule: ExamScheduleResponseDTO): string {
    const results = this.studentResults();
    const match = results.find(r => r.examId === schedule.examId && r.subjectId === schedule.subjectId);
    if (!match) return 'Pending';
    return match.marks !== undefined && match.marks !== null ? `${match.marks}` : 'N/A';
  }

  getExamPerformanceSummary() {
    const schedules = this.schedules();
    const results = this.studentResults();
    if (schedules.length === 0) return null;

    let totalPossible = 0;
    let totalObtained = 0;
    let gradedCount = 0;
    let passCount = 0;
    let failCount = 0;

    for (const sch of schedules) {
      const match = results.find(r => r.examId === sch.examId && r.subjectId === sch.subjectId);
      if (match && match.marks !== undefined && match.marks !== null) {
        totalObtained += Number(match.marks);
        totalPossible += 100;
        gradedCount++;
        if (Number(match.marks) >= 40) {
          passCount++;
        } else {
          failCount++;
        }
      }
    }

    if (gradedCount === 0) return null;

    const percentage = (totalObtained / totalPossible) * 100;
    let grade = 'F';
    let statusColor = 'text-danger';
    if (percentage >= 90) { grade = 'A+'; statusColor = 'text-success'; }
    else if (percentage >= 80) { grade = 'A'; statusColor = 'text-success'; }
    else if (percentage >= 70) { grade = 'B'; statusColor = 'text-primary'; }
    else if (percentage >= 60) { grade = 'C'; statusColor = 'text-info'; }
    else if (percentage >= 50) { grade = 'D'; statusColor = 'text-warning'; }
    else if (percentage >= 40) { grade = 'E'; statusColor = 'text-warning'; }

    return {
      totalObtained,
      totalPossible,
      gradedCount,
      totalSubjects: schedules.length,
      percentage: Math.round(percentage * 10) / 10,
      grade,
      statusColor,
      passCount,
      failCount
    };
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
}
