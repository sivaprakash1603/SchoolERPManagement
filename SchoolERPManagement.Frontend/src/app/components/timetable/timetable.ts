import { Component, signal, OnInit, inject, computed } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { TimetableService, TimetableResponseDTO } from '../../services/timetable.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { SubjectService, SubjectResponseDTO } from '../../services/subject.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';
import { ParentService } from '../../services/parent.service';

@Component({
  selector: 'app-timetable',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './timetable.html',
  styleUrl: './timetable.css',
})
export class Timetable implements OnInit {
  private timetableService = inject(TimetableService);
  private classService = inject(ClassService);
  private subjectService = inject(SubjectService);
  private teacherService = inject(TeacherService);
  private academicYearService = inject(AcademicYearService);
  private studentService = inject(StudentService);
  private toastService = inject(ToastService);
  private parentService = inject(ParentService);

  // Data signals
  classes = signal<ClassResponseDTO[]>([]);
  subjects = signal<SubjectResponseDTO[]>([]);
  teachers = signal<TeacherResponseDTO[]>([]);
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  slots = signal<TimetableResponseDTO[]>([]);

  // Selection signals
  selectedAcademicYearId = signal<number | null>(null);
  selectedClassId = signal<number | null>(null);

  // Parent specific
  parentChildren = signal<any[]>([]);
  selectedChildId = signal<number | null>(null);

  // States
  loading = signal(true);
  error = signal<string | null>(null);
  showCreateModal = signal(false);
  isSaving = signal(false);
  isAdmin = signal(false);
  userRole = signal<string>('Student');
  viewMode = signal<'personal' | 'class'>('personal');
  teacherId = signal<number | null>(null);

  // View UI Helpers
  get pageTitle(): string {
    if (this.viewMode() === 'personal') {
      if (this.userRole() === 'Teacher') return 'My Schedule';
      if (this.userRole() === 'Parent') return 'Child Timetable';
      return 'My Timetable';
    }
    return 'Class Timetable';
  }

  get pageDescription(): string {
    if (this.viewMode() === 'personal') {
      if (this.userRole() === 'Teacher') return 'View your personalized weekly teaching schedule.';
      if (this.userRole() === 'Parent') return "View your child's classroom schedule.";
      return 'View your classroom schedule.';
    }
    return 'View and configure weekly schedules for institutional classes.';
  }

  // Form signal
  createForm = signal({
    dayOfWeek: 'monday',
    subjectId: null as number | null,
    teacherId: null as number | null,
    startTime: '',
    endTime: '',
    roomNo: '',
  });

  // Timetable days
  days = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];

  // Computed signal for selected class name
  selectedClassName = computed(() => {
    const classId = this.selectedClassId();
    if (!classId) return '';
    const cls = this.classes().find((c) => c.id === classId);
    return cls ? `${cls.classname} ${cls.section ? '- ' + cls.section : ''}` : '';
  });

  getClassNameForSlot(classId: number): string {
    const cls = this.classes().find((c) => c.id === classId);
    return cls ? `${cls.classname} ${cls.section ? '- ' + cls.section : ''}` : `Class #${classId}`;
  }

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);
    this.isAdmin.set(role === 'Admin');

    if (role === 'Teacher') {
      this.viewMode.set('personal');
    } else {
      this.viewMode.set(role === 'Student' || role === 'Parent' ? 'personal' : 'class');
    }

    this.fetchSubjects();
    this.fetchTeachers();

    if (role === 'Student') {
      const uidStr = sessionStorage.getItem('userId');
      const uid = uidStr ? parseInt(uidStr, 10) : null;
      if (uid) {
        this.studentService.getStudentByUserId(uid).subscribe({
          next: (student) => {
            if (student.classId) {
              this.selectedClassId.set(student.classId);
              this.fetchTimetable();
            } else {
              this.toastService.warning('You are not currently enrolled in any class.');
              this.loading.set(false);
            }
          },
          error: (err) => {
            console.error('Failed to resolve student profile', err);
            this.toastService.error('Failed to resolve student profile.');
            this.loading.set(false);
          },
        });
      }
    } else if (role === 'Parent') {
      const uidStr = sessionStorage.getItem('userId');
      const uid = uidStr ? parseInt(uidStr, 10) : null;
      if (uid) {
        this.parentService.getParentByUserId(uid).subscribe({
          next: (parent) => {
            this.parentService.getParentChildren(parent.id).subscribe({
              next: (children) => {
                this.parentChildren.set(children);
                if (children.length > 0) {
                  const child = children[0];
                  this.selectedChildId.set(child.studentId);
                  if (child.classId) {
                    this.selectedClassId.set(child.classId);
                    this.fetchTimetable();
                  } else {
                    this.toastService.warning('Selected child is not enrolled in a class.');
                  }
                }
              },
              error: (err) => console.error('Failed to load parent children', err),
            });
          },
          error: (err) => {
            console.error('Failed to resolve parent profile', err);
            this.toastService.error('Failed to resolve parent profile.');
            this.loading.set(false);
          },
        });
      }
    } else if (role === 'Teacher') {
      this.fetchAcademicYears();
      const username = sessionStorage.getItem('username') || '';
      this.teacherService.getTeacherByUsername(username).subscribe({
        next: (res) => {
          this.teacherId.set(res.id);
          this.fetchTeacherTimetable(res.id);
        },
        error: (err) => {
          console.error('Failed to load teacher profile', err);
          this.toastService.error('Failed to resolve teacher profile.');
          this.loading.set(false);
        },
      });
    } else {
      this.fetchAcademicYears();
    }
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find((y) => y.isCurrent);
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
        } else if (years.length > 0) {
          this.selectedAcademicYearId.set(years[0].id);
        }
        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to load academic sessions', err);
        this.toastService.error('Failed to load academic sessions.');
        this.loading.set(false);
      },
    });
  }

  onChildChange(studentId: number) {
    this.selectedChildId.set(studentId);

    const child = this.parentChildren().find((c) => c.studentId === studentId);
    if (child && child.classId) {
      this.selectedClassId.set(child.classId);
      this.fetchTimetable();
    } else {
      this.selectedClassId.set(null);
      this.slots.set([]);
      this.loading.set(false);
      this.toastService.warning('Selected child is not enrolled in a class.');
    }
  }

  fetchClasses() {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) {
      this.loading.set(false);
      return;
    }

    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => {
        this.classes.set(res);
        if (res.length > 0) {
          this.selectedClassId.set(res[0].id);
          if (this.viewMode() === 'class') {
            this.fetchTimetable();
          } else {
            this.loading.set(false);
          }
        } else {
          this.selectedClassId.set(null);
          if (this.viewMode() === 'class') {
            this.slots.set([]);
            this.loading.set(false);
          }
        }
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.toastService.error('Failed to load classes.');
        this.loading.set(false);
      },
    });
  }

  fetchSubjects() {
    this.subjectService.getAllSubjects().subscribe({
      next: (res) => this.subjects.set(res),
      error: (err) => console.error('Failed to load subjects', err),
    });
  }

  fetchTeachers() {
    this.teacherService.getAllTeachers({ pageSize: 1000 }).subscribe({
      next: (res) => this.teachers.set(res.items),
      error: (err) => console.error('Failed to load teachers', err),
    });
  }

  fetchTimetable() {
    const classId = this.selectedClassId();
    if (!classId) {
      this.slots.set([]);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.timetableService.getClassTimetable(classId).subscribe({
      next: (data) => {
        this.slots.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load timetable', err);
        this.error.set('Failed to load class timetable.');
        this.loading.set(false);
      },
    });
  }

  fetchTeacherTimetable(teacherId: number) {
    this.loading.set(true);
    this.error.set(null);
    this.timetableService.getTeacherTimetable(teacherId).subscribe({
      next: (data) => {
        this.slots.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load teacher timetable', err);
        this.error.set('Failed to load teaching timetable.');
        this.loading.set(false);
      },
    });
  }

  switchViewMode(mode: 'personal' | 'class') {
    this.viewMode.set(mode);
    if (mode === 'personal') {
      if (this.teacherId()) {
        this.fetchTeacherTimetable(this.teacherId()!);
      }
    } else {
      this.fetchTimetable();
    }
  }

  onYearSelect(val: number | null) {
    this.selectedAcademicYearId.set(val);
    this.fetchClasses();
  }

  onClassChange() {
    this.fetchTimetable();
  }

  getSlotsForDay(day: string): TimetableResponseDTO[] {
    return this.slots().filter((s) => s.dayOfWeek.toLowerCase() === day.toLowerCase());
  }

  getSubjectName(subjectId: number): string {
    const sub = this.subjects().find((s) => s.id === subjectId);
    return sub ? sub.subjectName : 'Unknown';
  }

  getTeacherName(teacherId: number): string {
    const teach = this.teachers().find((t) => t.id === teacherId);
    return teach ? teach.name : 'Unknown';
  }

  formatTime(timeStr: string): string {
    if (!timeStr) return '';
    const parts = timeStr.split(':');
    let hour = parseInt(parts[0], 10);
    const minute = parts[1] || '00';
    const ampm = hour >= 12 ? 'PM' : 'AM';
    hour = hour % 12;
    hour = hour ? hour : 12;
    return `${hour.toString().padStart(2, '0')}:${minute} ${ampm}`;
  }

  openCreateModal() {
    this.createForm.set({
      dayOfWeek: 'monday',
      subjectId: this.subjects().length > 0 ? this.subjects()[0].id : null,
      teacherId: this.teachers().length > 0 ? this.teachers()[0].id : null,
      startTime: '09:00',
      endTime: '10:00',
      roomNo: '',
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  saveTimetableEntry() {
    const classId = this.selectedClassId();
    if (!classId) return;

    const form = this.createForm();
    if (!form.subjectId || !form.teacherId || !form.startTime || !form.endTime) {
      this.toastService.warning('Please fill in all required fields.');
      return;
    }

    this.isSaving.set(true);

    const dto = {
      classId: classId,
      subjectId: form.subjectId,
      teacherId: form.teacherId,
      dayOfWeek: form.dayOfWeek,
      startTime: form.startTime,
      endTime: form.endTime,
      roomNo: form.roomNo || undefined,
    };

    this.timetableService.createTimetable(dto).subscribe({
      next: () => {
        this.toastService.success('Timetable slot added successfully!');
        this.isSaving.set(false);
        this.closeCreateModal();
        this.fetchTimetable();
      },
      error: (err) => {
        console.error('Failed to create slot', err);
        this.isSaving.set(false);
        this.toastService.error(err.error?.message || 'Failed to add timetable slot.');
      },
    });
  }
}
