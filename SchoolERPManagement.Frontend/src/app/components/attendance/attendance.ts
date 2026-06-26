import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService, AttendanceResponseDTO } from '../../services/attendance.service';
import { StudentService, StudentQueryResponseDTO } from '../../services/student.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { AcademicCalendarService } from '../../services/academic-calendar.service';
import { ToastService } from '../../services/toast.service';
import { TeacherService, TeacherResponseDTO } from '../../services/teacher.service';
import { TimetableService } from '../../services/timetable.service';


import { ParentService } from '../../services/parent.service';

interface StudentAttendanceUI extends StudentQueryResponseDTO {
  status: string; // 'present' | 'absent' | 'late' | 'unmarked'
  remarks: string;
  isUpdatingStatus: boolean;
  isSavingRemarks: boolean;
}

interface TeacherAttendanceUI extends TeacherResponseDTO {
  status: string; // 'present' | 'absent' | 'late' | 'onleave' | 'unmarked'
  remarks: string;
  isUpdatingStatus: boolean;
  isSavingRemarks: boolean;
}

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance.html',
  styleUrl: './attendance.css',
})
export class Attendance implements OnInit {
  private attendanceService = inject(AttendanceService);
  private studentService = inject(StudentService);
  private teacherService = inject(TeacherService);
  private classService = inject(ClassService);
  private academicYearService = inject(AcademicYearService);
  private calendarService = inject(AcademicCalendarService);
  private toastService = inject(ToastService);
  private timetableService = inject(TimetableService);
  private parentService = inject(ParentService);
  
  // Date constraints
  minDate = signal<string>('');
  maxDate = signal<string>('');
  holidays = signal<string[]>([]);
  previousDate = '';

  // Auth & Role
  userRole = signal<string>('Student');
  currentUserId = signal<number | null>(null);
  resolvedStudentId = signal<number | null>(null);
  resolvedTeacherId = signal<number | null>(null);
  
  // Parent specific
  parentChildren = signal<any[]>([]);
  selectedChildId = signal<number | null>(null);

  studentAttendanceRecords = signal<any[]>([]);
  studentAttendanceStats = signal({ present: 0, absent: 0, leave: 0, late: 0, total: 0, rate: 0 });
  teacherAttendanceStats = signal({ present: 0, absent: 0, leave: 0, late: 0, total: 0, rate: 0 });
  teacherHistoryFilterMonth = signal<string>('all');
  filteredTeacherAttendanceRecords = signal<any[]>([]);

  // View UI Helpers
  get pageTitle(): string {
    if (this.userRole() === 'Student') return 'My Attendance Dashboard';
    if (this.userRole() === 'Parent') return 'Child Attendance Dashboard';
    if (this.activeTab() === 'students') return 'Daily Student Attendance';
    if (this.userRole() === 'Teacher') return 'My Attendance Logs';
    return 'Teacher & Staff Attendance';
  }

  get pageDescription(): string {
    if (this.userRole() === 'Student') return 'Overview of your personal attendance performance.';
    if (this.userRole() === 'Parent') return "Overview of your child's attendance performance.";
    if (this.activeTab() === 'students') return 'Record and review student attendance.';
    if (this.userRole() === 'Teacher') return 'Overview of your personal staff attendance logs.';
    return 'Record, review, and manage teacher/staff daily attendance.';
  }

  // Student Calendar
  calendarMonth = signal<number>(new Date().getMonth()); // 0-indexed
  calendarYear = signal<number>(new Date().getFullYear());
  calendarDays = signal<{ date: number; status: string; fullDate: string; isToday: boolean }[]>([]);
  calendarBlanks = signal<number[]>([]);
  monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
  dayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  // Filtered history
  historyFilterMonth = signal<string>('all');
  filteredAttendanceRecords = signal<any[]>([]);

  // Tab switcher
  activeTab = signal<'students' | 'teachers'>('students');

  // Selector data
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | null>(null);
  selectedClassId = signal<number | null>(null);
  selectedDate = signal<string>('');

  // Holiday states
  isHoliday = signal(false);
  holidayDescription = signal('');

  // Table data
  students = signal<StudentAttendanceUI[]>([]);
  teachers = signal<TeacherAttendanceUI[]>([]);
  personalStaffAttendance = signal<any[]>([]);

  // States
  loading = signal(true);
  error = signal<string | null>(null);
  isAdminOrTeacher = signal(false);

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);
    this.isAdminOrTeacher.set(role === 'Admin' || role === 'Teacher');

    const uidStr = sessionStorage.getItem('userId');
    const uid = uidStr ? parseInt(uidStr, 10) : null;
    this.currentUserId.set(uid);

    if (role === 'Student' && uid) {
      this.studentService.getStudentByUserId(uid).subscribe({
        next: (student) => {
          this.resolvedStudentId.set(student.id);
          this.fetchStudentAttendance(student.id);
        },
        error: (err) => {
          console.error('Failed to resolve student profile', err);
          this.error.set('Failed to resolve student profile.');
        }
      });
    } else if (role === 'Parent' && uid) {
      this.parentService.getParentByUserId(uid).subscribe({
        next: (parent) => {
          this.parentService.getParentChildren(parent.id).subscribe({
            next: (children) => {
              this.parentChildren.set(children);
              if (children.length > 0) {
                this.selectedChildId.set(children[0].studentId);
                this.resolvedStudentId.set(children[0].studentId);
                this.fetchStudentAttendance(children[0].studentId);
              }
            },
            error: (err) => console.error('Failed to load parent children', err)
          });
        },
        error: (err) => {
          console.error('Failed to resolve parent profile', err);
          this.error.set('Failed to resolve parent profile.');
        }
      });
    } else {
      // Default to today's date
      const today = new Date().toISOString().split('T')[0];
      this.selectedDate.set(today);
      this.fetchAcademicYears();
    }
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const currentYear = years.find(y => y.isCurrent) || years[0];
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.loadYearDetails(currentYear);
        } else {
          this.loading.set(false);
        }
      },
      error: (err) => {
        console.error('Failed to load academic sessions', err);
        this.toastService.error('Failed to load academic sessions.');
        this.loading.set(false);
      }
    });
  }

  onChildChange(studentId: number) {
    this.selectedChildId.set(studentId);
    this.resolvedStudentId.set(studentId);
    
    const child = this.parentChildren().find(c => c.studentId === studentId);
    this.fetchStudentAttendance(studentId);
  }

  loadYearDetails(year: AcademicYearResponseDTO) {
    const min = this.formatDateForInput(year.startDate);
    const max = this.formatDateForInput(year.endDate);
    this.minDate.set(min);
    this.maxDate.set(max);

    this.calendarService.getAcademicCalendarSummary(year.id).subscribe({
      next: (cal) => {
        const holidayDates = cal.events
          .filter(e => e.isHoliday)
          .map(e => this.formatDateForInput(e.date));
        this.holidays.set(holidayDates);

        let defaultDate = this.selectedDate();
        const today = new Date().toISOString().split('T')[0];

        if (!defaultDate || defaultDate < min || defaultDate > max) {
          defaultDate = (today >= min && today <= max) ? today : min;
        }

        if (holidayDates.includes(defaultDate)) {
          defaultDate = this.findNextWorkingDay(defaultDate, min, max, holidayDates);
        }

        this.selectedDate.set(defaultDate);
        this.previousDate = defaultDate;

        this.fetchClasses();
      },
      error: (err) => {
        console.error('Failed to load calendar summary', err);
        this.fetchClasses();
      }
    });
  }

  private formatDateForInput(dateStr: string): string {
    if (!dateStr) return '';
    return dateStr.split('T')[0];
  }

  private findNextWorkingDay(current: string, min: string, max: string, holidays: string[]): string {
    let currDate = new Date(current);
    const maxDate = new Date(max);
    const minDate = new Date(min);

    while (currDate <= maxDate) {
      const dateStr = currDate.toISOString().split('T')[0];
      if (!holidays.includes(dateStr)) {
        return dateStr;
      }
      currDate.setDate(currDate.getDate() + 1);
    }

    currDate = new Date(current);
    while (currDate >= minDate) {
      const dateStr = currDate.toISOString().split('T')[0];
      if (!holidays.includes(dateStr)) {
        return dateStr;
      }
      currDate.setDate(currDate.getDate() - 1);
    }

    return current;
  }

  private getErrorMessage(err: any, fallback: string): string {
    if (err.error?.errors) {
      const validationErrors = Object.values(err.error.errors).flat();
      if (validationErrors.length > 0) {
        return validationErrors.join(' ');
      }
    }
    if (err.error?.Errors) {
      const validationErrors = Object.values(err.error.Errors).flat();
      if (validationErrors.length > 0) {
        return validationErrors.join(' ');
      }
    }
    return err.error?.message || err.error?.Message || fallback;
  }



  fetchClasses() {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) {
      this.loading.set(false);
      return;
    }

    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => {
        if (this.userRole() === 'Teacher') {
          const username = sessionStorage.getItem('username') || '';
          this.teacherService.getTeacherByUsername(username).subscribe({
            next: (teacher) => {
              this.resolvedTeacherId.set(teacher.id);
              this.timetableService.getTeacherTimetable(teacher.id).subscribe({
                next: (slots) => {
                  const assignedClassIds = new Set<number>(slots.map(s => s.classId));
                  const filtered = res.filter(c => 
                    assignedClassIds.has(c.id) || 
                    (teacher.className && c.classname.toLowerCase() === teacher.className.toLowerCase() && 
                     (!teacher.section || c.section?.toLowerCase() === teacher.section.toLowerCase()))
                  );
                  this.classes.set(filtered);
                  if (filtered.length > 0) {
                    this.selectedClassId.set(filtered[0].id);
                  } else {
                    this.selectedClassId.set(null);
                  }
                  this.fetchAttendanceSheet();
                },
                error: (err) => {
                  console.error('Failed to load teacher timetable', err);
                  const filtered = res.filter(c => 
                    teacher.className && c.classname.toLowerCase() === teacher.className.toLowerCase() && 
                    (!teacher.section || c.section?.toLowerCase() === teacher.section.toLowerCase())
                  );
                  this.classes.set(filtered);
                  if (filtered.length > 0) {
                    this.selectedClassId.set(filtered[0].id);
                  } else {
                    this.selectedClassId.set(null);
                  }
                  this.fetchAttendanceSheet();
                }
              });
            },
            error: (err) => {
              console.error('Failed to load teacher profile', err);
              this.classes.set([]);
              this.selectedClassId.set(null);
              this.fetchAttendanceSheet();
            }
          });
        } else {
          this.classes.set(res);
          if (res.length > 0) {
            this.selectedClassId.set(res[0].id);
          } else {
            this.selectedClassId.set(null);
          }
          this.fetchAttendanceSheet();
        }
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.toastService.error('Failed to load classes.');
        this.fetchAttendanceSheet();
      }
    });
  }

  fetchAttendanceSheet() {
    if (this.activeTab() === 'students') {
      this.fetchStudentsAndAttendance();
    } else {
      if (this.userRole() === 'Admin') {
        this.fetchTeachersAndAttendance();
      } else if (this.userRole() === 'Teacher') {
        this.fetchPersonalStaffAttendance();
      }
    }
  }

  fetchStudentsAndAttendance() {
    const classId = this.selectedClassId();
    const date = this.selectedDate();
    if (!classId || !date) {
      this.students.set([]);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const yearId = this.selectedAcademicYearId();
    if (yearId) {
      this.calendarService.getAcademicCalendarSummary(yearId).subscribe({
        next: (cal) => {
          const holiday = cal.events.find(e => this.formatDateForInput(e.date) === date && e.isHoliday);
          if (holiday) {
            this.isHoliday.set(true);
            this.holidayDescription.set(holiday.description);
          } else {
            this.isHoliday.set(false);
            this.holidayDescription.set('');
          }
        },
        error: (err) => console.error('Failed to fetch calendar', err)
      });
    }

    // 1. Fetch students in the class
    this.studentService.getStudentsByClassId(classId).subscribe({
      next: (studentList) => {
        // 2. Fetch existing attendance records
        this.attendanceService.getAttendanceByClass(classId, date).subscribe({
          next: (attendanceRecords) => {
            const uiList: StudentAttendanceUI[] = studentList.map(s => {
              const record = attendanceRecords.find(r => r.studentId === s.id);
              return {
                ...s,
                status: record ? record.status.toLowerCase() : 'unmarked',
                remarks: record?.remarks || '',
                isUpdatingStatus: false,
                isSavingRemarks: false
              };
            });
            this.students.set(uiList);
            this.loading.set(false);
          },
          error: (err) => {
            console.error('Failed to load attendance records', err);
            this.error.set('Failed to load class attendance records.');
            this.loading.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Failed to load students', err);
        this.error.set('Failed to load class students.');
        this.loading.set(false);
      }
    });
  }

  fetchTeachersAndAttendance() {
    const date = this.selectedDate();
    if (!date) return;

    this.loading.set(true);
    this.error.set(null);

    const yearId = this.selectedAcademicYearId();
    if (yearId) {
      this.calendarService.getAcademicCalendarSummary(yearId).subscribe({
        next: (cal) => {
          const holiday = cal.events.find(e => this.formatDateForInput(e.date) === date && e.isHoliday);
          if (holiday) {
            this.isHoliday.set(true);
            this.holidayDescription.set(holiday.description);
          } else {
            this.isHoliday.set(false);
            this.holidayDescription.set('');
          }
        },
        error: (err) => console.error('Failed to fetch calendar', err)
      });
    }

    // 1. Fetch all teachers
    this.teacherService.getAllTeachers({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        const teacherList = res.items;
        // 2. Fetch existing staff attendance records
        this.attendanceService.getStaffAttendanceByDate(date).subscribe({
          next: (attendanceRecords) => {
            const uiList: TeacherAttendanceUI[] = teacherList.map(t => {
              const record = attendanceRecords.find(r => r.userId === t.userId);
              return {
                ...t,
                status: record ? record.status.toLowerCase() : 'unmarked',
                remarks: record?.remarks || '',
                isUpdatingStatus: false,
                isSavingRemarks: false
              };
            });
            this.teachers.set(uiList);
            this.loading.set(false);
          },
          error: (err) => {
            console.error('Failed to load staff attendance records', err);
            this.error.set('Failed to load staff attendance records.');
            this.loading.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Failed to load teachers', err);
        this.error.set('Failed to load teachers list.');
        this.loading.set(false);
      }
    });
  }

  fetchPersonalStaffAttendance() {
    const userId = this.currentUserId();
    if (!userId) { this.loading.set(false); return; }

    this.loading.set(true);
    this.error.set(null);

    this.attendanceService.getStaffAttendanceByUser(userId).subscribe({
      next: (records) => {
        const normalized = records.map(r => ({
          ...r,
          status: r.status.toLowerCase()
        }));
        this.personalStaffAttendance.set(normalized);

        const total = normalized.length;
        if (total > 0) {
          const present = normalized.filter(r => r.status === 'present').length;
          const late = normalized.filter(r => r.status === 'late').length;
          const absent = normalized.filter(r => r.status === 'absent').length;
          const leave = normalized.filter(r => r.status === 'onleave' || r.status === 'leave').length;
          const rate = Math.round(((present + late) / total) * 100);
          this.teacherAttendanceStats.set({ present, absent, leave, late, total, rate });
        } else {
          this.teacherAttendanceStats.set({ present: 0, absent: 0, leave: 0, late: 0, total: 0, rate: 0 });
        }

        this.buildCalendar();
        this.applyTeacherHistoryFilter();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load personal staff attendance', err);
        this.error.set('Failed to load personal staff attendance.');
        this.loading.set(false);
      }
    });
  }

  onTabChange(tab: 'students' | 'teachers') {
    this.activeTab.set(tab);
    this.fetchAttendanceSheet();
  }

  onYearSelect(val: number | null) {
    this.selectedAcademicYearId.set(val);
    const year = this.academicYears().find(y => y.id === Number(val));
    if (year) {
      this.loadYearDetails(year);
    }
  }

  onClassChange() {
    this.fetchAttendanceSheet();
  }

  onDateChange() {
    const selected = this.selectedDate();
    const holidaysList = this.holidays();
    
    if (holidaysList.includes(selected)) {
      this.toastService.error('Cannot select a holiday. Please select a working day.');
      setTimeout(() => {
        this.selectedDate.set(this.previousDate);
      });
      return;
    }
    
    const min = this.minDate();
    const max = this.maxDate();
    if (selected < min || selected > max) {
      this.toastService.error(`Selected date must be between ${min} and ${max}.`);
      setTimeout(() => {
        this.selectedDate.set(this.previousDate);
      });
      return;
    }

    this.previousDate = selected;
    this.fetchAttendanceSheet();
  }

  updateTeacherStatus(teacher: TeacherAttendanceUI, newStatus: string) {
    if (this.userRole() !== 'Admin') {
      this.toastService.error('Only administrators can mark staff attendance.');
      return;
    }

    teacher.isUpdatingStatus = true;
    const dto = {
      userId: teacher.userId,
      date: this.selectedDate(),
      status: newStatus,
      attendanceType: 'Daily',
      remarks: teacher.remarks || undefined
    };

    this.attendanceService.markStaffAttendance(dto).subscribe({
      next: (res) => {
        teacher.status = res.status.toLowerCase();
        teacher.isUpdatingStatus = false;
        this.toastService.success(`Marked ${teacher.name} as ${newStatus}`);
      },
      error: (err) => {
        console.error('Failed to mark staff attendance', err);
        teacher.isUpdatingStatus = false;
        this.toastService.error(this.getErrorMessage(err, 'Failed to save staff attendance.'));
      }
    });
  }

  saveTeacherRemarks(teacher: TeacherAttendanceUI) {
    if (this.userRole() !== 'Admin') return;

    teacher.isSavingRemarks = true;
    const dto = {
      userId: teacher.userId,
      date: this.selectedDate(),
      status: teacher.status === 'unmarked' ? 'present' : teacher.status,
      attendanceType: 'Daily',
      remarks: teacher.remarks
    };

    this.attendanceService.markStaffAttendance(dto).subscribe({
      next: (res) => {
        teacher.status = res.status.toLowerCase();
        teacher.isSavingRemarks = false;
        this.toastService.success(`Saved remarks for ${teacher.name}`);
      },
      error: (err) => {
        console.error('Failed to save staff remarks', err);
        teacher.isSavingRemarks = false;
        this.toastService.error(this.getErrorMessage(err, 'Failed to save staff remarks.'));
      }
    });
  }


  updateStudentStatus(student: StudentAttendanceUI, newStatus: string) {
    if (!this.isAdminOrTeacher()) {
      this.toastService.error('You do not have permission to mark attendance.');
      return;
    }

    student.isUpdatingStatus = true;
    const dto = {
      studentId: student.id,
      date: this.selectedDate(),
      status: newStatus,
      remarks: student.remarks || undefined
    };

    this.attendanceService.markAttendance(dto).subscribe({
      next: (res) => {
        student.status = res.status.toLowerCase();
        student.isUpdatingStatus = false;
        this.toastService.success(`Marked ${student.name} as ${newStatus}`);
      },
      error: (err) => {
        console.error('Failed to mark attendance', err);
        student.isUpdatingStatus = false;
        this.toastService.error(this.getErrorMessage(err, 'Failed to save attendance.'));
      }
    });
  }

  saveRemarks(student: StudentAttendanceUI) {
    if (!this.isAdminOrTeacher()) return;

    student.isSavingRemarks = true;
    const dto = {
      studentId: student.id,
      date: this.selectedDate(),
      status: student.status === 'unmarked' ? 'present' : student.status, // Default to present if they enter remarks on unmarked
      remarks: student.remarks
    };

    this.attendanceService.markAttendance(dto).subscribe({
      next: (res) => {
        student.status = res.status.toLowerCase();
        student.isSavingRemarks = false;
        this.toastService.success(`Saved remarks for ${student.name}`);
      },
      error: (err) => {
        console.error('Failed to save remarks', err);
        student.isSavingRemarks = false;
        this.toastService.error(this.getErrorMessage(err, 'Failed to save remarks.'));
      }
    });
  }


  // Quick action: Mark all as present
  markAllAsPresent() {
    if (!this.isAdminOrTeacher()) return;

    const unmarked = this.students().filter(s => s.status === 'unmarked');
    if (unmarked.length === 0) {
      this.toastService.info('All students are already marked.');
      return;
    }

    let completed = 0;
    unmarked.forEach(student => {
      student.isUpdatingStatus = true;
      const dto = {
        studentId: student.id,
        date: this.selectedDate(),
        status: 'present',
        remarks: student.remarks || undefined
      };
      this.attendanceService.markAttendance(dto).subscribe({
        next: (res) => {
          student.status = res.status.toLowerCase();
          student.isUpdatingStatus = false;
          completed++;
          if (completed === unmarked.length) {
            this.toastService.success('All unmarked students set to Present.');
          }
        },
        error: () => {
          student.isUpdatingStatus = false;
        }
      });
    });
  }

  fetchStudentAttendance(studentId: number) {
    this.loading.set(true);
    this.attendanceService.getAttendanceByStudent(studentId).subscribe({
      next: (records) => {
        // Normalize status to lowercase for all records
        const normalized = records.map(r => ({
          ...r,
          status: r.status.toLowerCase()
        }));
        this.studentAttendanceRecords.set(normalized);
        
        // Calculate statistics
        const total = normalized.length;
        if (total > 0) {
          const present = normalized.filter(r => r.status === 'present').length;
          const late = normalized.filter(r => r.status === 'late').length;
          const absent = normalized.filter(r => r.status === 'absent').length;
          const leave = normalized.filter(r => r.status === 'onleave' || r.status === 'leave').length;
          const rate = Math.round(((present + late) / total) * 100);
          this.studentAttendanceStats.set({ present, absent, leave, late, total, rate });
        } else {
          this.studentAttendanceStats.set({ present: 0, absent: 0, leave: 0, late: 0, total: 0, rate: 0 });
        }

        this.buildCalendar();
        this.applyHistoryFilter();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch student attendance logs', err);
        this.error.set('Failed to fetch attendance logs.');
        this.loading.set(false);
      }
    });
  }

  buildCalendar() {
    const month = this.calendarMonth();
    const year = this.calendarYear();
    const firstDay = new Date(year, month, 1).getDay(); // 0=Sun
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const today = new Date();
    const todayStr = today.toISOString().split('T')[0];

    const records = (this.userRole() === 'Teacher' && this.activeTab() === 'teachers') 
      ? this.personalStaffAttendance() 
      : this.studentAttendanceRecords();
    const statusMap = new Map<string, string>();
    records.forEach(r => {
      const dateKey = r.date.split('T')[0];
      statusMap.set(dateKey, r.status);
    });

    const blanks: number[] = [];
    for (let i = 0; i < firstDay; i++) blanks.push(i);
    this.calendarBlanks.set(blanks);

    const days: { date: number; status: string; fullDate: string; isToday: boolean }[] = [];
    for (let d = 1; d <= daysInMonth; d++) {
      const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
      const status = statusMap.get(dateStr) || '';
      days.push({
        date: d,
        status,
        fullDate: dateStr,
        isToday: dateStr === todayStr
      });
    }
    this.calendarDays.set(days);
  }

  navigateMonth(direction: number) {
    let month = this.calendarMonth() + direction;
    let year = this.calendarYear();
    if (month < 0) { month = 11; year--; }
    if (month > 11) { month = 0; year++; }
    this.calendarMonth.set(month);
    this.calendarYear.set(year);
    this.buildCalendar();
  }

  goToCurrentMonth() {
    this.calendarMonth.set(new Date().getMonth());
    this.calendarYear.set(new Date().getFullYear());
    this.buildCalendar();
  }

  applyHistoryFilter() {
    const filterVal = this.historyFilterMonth();
    const records = this.studentAttendanceRecords();

    if (filterVal === 'all') {
      // Sort most recent first
      this.filteredAttendanceRecords.set([...records].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()));
      return;
    }

    // filterVal format: "YYYY-MM"
    const [fy, fm] = filterVal.split('-').map(Number);
    const filtered = records.filter(r => {
      const d = new Date(r.date);
      return d.getFullYear() === fy && d.getMonth() + 1 === fm;
    });
    this.filteredAttendanceRecords.set(filtered.sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime()));
  }

  onHistoryFilterChange(value: string) {
    this.historyFilterMonth.set(value);
    this.applyHistoryFilter();
  }

  getAvailableMonths(): { label: string; value: string }[] {
    const records = this.studentAttendanceRecords();
    const monthSet = new Set<string>();
    records.forEach(r => {
      const d = new Date(r.date);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      monthSet.add(key);
    });
    const months = Array.from(monthSet).sort().reverse();
    return months.map(m => {
      const [y, mo] = m.split('-').map(Number);
      return { label: `${this.monthNames[mo - 1]} ${y}`, value: m };
    });
  }

  applyTeacherHistoryFilter() {
    const filterVal = this.teacherHistoryFilterMonth();
    const records = this.personalStaffAttendance();

    if (filterVal === 'all') {
      this.filteredTeacherAttendanceRecords.set([...records].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()));
      return;
    }

    const [fy, fm] = filterVal.split('-').map(Number);
    const filtered = records.filter(r => {
      const d = new Date(r.date);
      return d.getFullYear() === fy && d.getMonth() + 1 === fm;
    });
    this.filteredTeacherAttendanceRecords.set(filtered.sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime()));
  }

  onTeacherHistoryFilterChange(value: string) {
    this.teacherHistoryFilterMonth.set(value);
    this.applyTeacherHistoryFilter();
  }

  getAvailableTeacherMonths(): { label: string; value: string }[] {
    const records = this.personalStaffAttendance();
    const monthSet = new Set<string>();
    records.forEach(r => {
      const d = new Date(r.date);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      monthSet.add(key);
    });
    const months = Array.from(monthSet).sort().reverse();
    return months.map(m => {
      const [y, mo] = m.split('-').map(Number);
      return { label: `${this.monthNames[mo - 1]} ${y}`, value: m };
    });
  }

  getCalendarStatusClass(status: string): string {
    if (!status) return '';
    switch (status) {
      case 'present': return 'cal-present';
      case 'absent': return 'cal-absent';
      case 'late': return 'cal-late';
      case 'onleave': case 'leave': return 'cal-leave';
      default: return '';
    }
  }
}

