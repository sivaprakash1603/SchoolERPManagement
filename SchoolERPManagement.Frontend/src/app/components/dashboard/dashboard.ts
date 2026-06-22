import { Component, OnInit, ViewChild, ElementRef, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { StudentService } from '../../services/student.service';
import { AttendanceService } from '../../services/attendance.service';
import { HomeworkService, HomeworkResponseDTO } from '../../services/homework.service';
import { FeeService, FeeSummaryDTO } from '../../services/fee.service';
import { ExamService, ExamResultResponseDTO, ExamResponseDTO } from '../../services/exam.service';
import { SubjectService } from '../../services/subject.service';
import { AdminDashboardDTO } from '../../models/dashboard.model';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);
  private academicYearService = inject(AcademicYearService);
  private studentService = inject(StudentService);
  private attendanceService = inject(AttendanceService);
  private homeworkService = inject(HomeworkService);
  private feeService = inject(FeeService);
  private examService = inject(ExamService);
  private subjectService = inject(SubjectService);

  @ViewChild('demographicsChart') demographicsChartRef!: ElementRef;
  @ViewChild('revenueChart') revenueChartRef!: ElementRef;

  private demographicsChartInstance: Chart | null = null;
  private revenueChartInstance: Chart | null = null;

  // Role and status
  userRole = signal<string>('Student');
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  // Admin/Teacher Metrics
  metrics = signal<AdminDashboardDTO | null>(null);
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | undefined>(undefined);

  // Student metrics
  studentData = signal<any>(null);
  studentAttendanceRate = signal<number>(0);
  pendingHomeworkCount = signal<number>(0);
  upcomingHomework = signal<HomeworkResponseDTO[]>([]);
  feeSummary = signal<FeeSummaryDTO | null>(null);
  recentResults = signal<any[]>([]);

  get userName() {
    if (this.userRole() === 'Student' && this.studentData()) {
      return this.studentData().name;
    }
    return sessionStorage.getItem('username') || sessionStorage.getItem('name') || 'User';
  }

  get salutation() {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  ngOnInit(): void {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);

    if (role === 'Admin' || role === 'Teacher') {
      this.academicYearService.getAllAcademicYears().subscribe({
        next: (years) => {
          this.academicYears.set(years);
          const currentYear = years.find(y => y.isCurrent);
          if (currentYear) {
            this.selectedAcademicYearId.set(currentYear.id);
            this.loadMetrics(currentYear.id);
          } else if (years.length > 0) {
            this.selectedAcademicYearId.set(years[0].id);
            this.loadMetrics(years[0].id);
          } else {
            this.loadMetrics();
          }
        },
        error: (err) => {
          console.error('Failed to load academic years', err);
          this.loadMetrics();
        }
      });
    } else if (role === 'Student') {
      const uidStr = sessionStorage.getItem('userId');
      const uid = uidStr ? parseInt(uidStr, 10) : null;
      if (uid) {
        this.loadStudentDashboard(uid);
      } else {
        this.loading.set(false);
      }
    } else {
      this.loading.set(false);
    }
  }

  loadMetrics(academicYearId?: number): void {
    this.loading.set(true);
    this.dashboardService.getAdminMetrics(academicYearId).subscribe({
      next: (data) => {
        this.metrics.set(data);
        this.loading.set(false);
        setTimeout(() => {
          this.initDemographicsChart(data);
          this.initRevenueChart(data);
        }, 100);
      },
      error: (err) => {
        console.error('Failed to load dashboard metrics', err);
        this.error.set('Could not load dashboard metrics. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  loadStudentDashboard(userId: number) {
    this.loading.set(true);
    this.error.set(null);
    
    this.studentService.getStudentByUserId(userId).subscribe({
      next: (student) => {
        this.studentData.set(student);
        const studentId = student.id;

        let completedRequests = 0;
        const totalRequests = 4;
        const checkLoading = () => {
          completedRequests++;
          if (completedRequests >= totalRequests) {
            this.loading.set(false);
          }
        };

        // 1. Fetch attendance stats
        this.attendanceService.getAttendanceByStudent(studentId).subscribe({
          next: (attendance) => {
            const total = attendance.length;
            if (total > 0) {
              const present = attendance.filter(r => r.status.toLowerCase() === 'present' || r.status.toLowerCase() === 'late').length;
              this.studentAttendanceRate.set(Math.round((present / total) * 100));
            }
            checkLoading();
          },
          error: (err) => {
            console.error('Failed to load attendance', err);
            checkLoading();
          }
        });

        // 2. Fetch homework
        this.homeworkService.getHomeworksByStudentId(studentId).subscribe({
          next: (homeworkList) => {
            const today = new Date().toISOString().split('T')[0];
            
            // Map subject names to homework list
            this.subjectService.getAllSubjects().subscribe({
              next: (subList) => {
                const mapped = homeworkList.map(h => {
                  const sub = subList.find(s => s.id === h.subjectId);
                  return {
                    ...h,
                    subjectName: sub ? sub.subjectName : `Subject #${h.subjectId}`
                  };
                });
                this.upcomingHomework.set(mapped.filter(h => !(h as any).submission || h.dueDate >= today));
              }
            });
            
            const pendingCount = homeworkList.filter(h => {
              const hasSubmitted = !!(h as any).submission;
              return !hasSubmitted;
            }).length;
            this.pendingHomeworkCount.set(pendingCount);
            checkLoading();
          },
          error: (err) => {
            console.error('Failed to load homework', err);
            checkLoading();
          }
        });

        // 3. Fetch fees summary
        this.feeService.getFeeDetails(studentId).subscribe({
          next: (summary) => {
            this.feeSummary.set(summary);
            checkLoading();
          },
          error: (err) => {
            console.error('Failed to load fees', err);
            checkLoading();
          }
        });

        // 4. Fetch exam results
        this.examService.getStudentResults(studentId).subscribe({
          next: (results) => {
            this.recentResults.set(results.slice(0, 5));
            
            // Map names asynchronously
            this.examService.getAllExams().subscribe({
              next: (examList) => {
                const resultsWithNames = this.recentResults().map(r => {
                  const exam = examList.find(e => e.id === r.examId);
                  return {
                    ...r,
                    examName: exam ? exam.examname : `Exam #${r.examId}`
                  };
                });
                this.recentResults.set(resultsWithNames);
              }
            });

            this.subjectService.getAllSubjects().subscribe({
              next: (subList) => {
                const resultsWithSubjects = this.recentResults().map(r => {
                  const sub = subList.find(s => s.id === r.subjectId);
                  return {
                    ...r,
                    subjectName: sub ? sub.subjectName : `Subject #${r.subjectId}`
                  };
                });
                this.recentResults.set(resultsWithSubjects);
                checkLoading();
              },
              error: () => checkLoading()
            });
          },
          error: (err) => {
            console.error('Failed to load exam results', err);
            checkLoading();
          }
        });
      },
      error: (err) => {
        console.error('Failed to load student profile', err);
        this.error.set('Failed to load student profile details.');
        this.loading.set(false);
      }
    });
  }

  onYearChange(event: Event): void {
    const selectEl = event.target as HTMLSelectElement;
    const yearId = Number(selectEl.value);
    this.selectedAcademicYearId.set(yearId);
    this.loadMetrics(yearId);
  }

  private initDemographicsChart(data: AdminDashboardDTO) {
    if (!this.demographicsChartRef) return;
    
    if (this.demographicsChartInstance) {
      this.demographicsChartInstance.destroy();
    }

    this.demographicsChartInstance = new Chart(this.demographicsChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Students', 'Teachers', 'Parents'],
        datasets: [{
          data: [data.totalStudents, data.totalTeachers, data.totalParents],
          backgroundColor: ['#6366f1', '#eab308', '#ec4899'],
          borderWidth: 0,
          hoverOffset: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom' }
        },
        cutout: '70%'
      }
    });
  }

  private initRevenueChart(data: AdminDashboardDTO) {
    if (!this.revenueChartRef || !data.revenueTrends || data.revenueTrends.length === 0) return;

    if (this.revenueChartInstance) {
      this.revenueChartInstance.destroy();
    }

    const labels = data.revenueTrends.map(t => t.month);
    const values = data.revenueTrends.map(t => t.amount);

    this.revenueChartInstance = new Chart(this.revenueChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Revenue (₹)',
          data: values,
          backgroundColor: '#3730a3',
          borderRadius: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: { beginAtZero: true, grid: { display: false } },
          x: { grid: { display: false } }
        },
        plugins: {
          legend: { display: false }
        }
      }
    });
  }
}
