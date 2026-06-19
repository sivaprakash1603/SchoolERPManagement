import { Component, OnInit, ViewChild, ElementRef, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
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

  @ViewChild('demographicsChart') demographicsChartRef!: ElementRef;
  @ViewChild('revenueChart') revenueChartRef!: ElementRef;

  private demographicsChartInstance: Chart | null = null;
  private revenueChartInstance: Chart | null = null;

  metrics = signal<AdminDashboardDTO | null>(null);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  academicYears = signal<AcademicYearResponseDTO[]>([]);
  selectedAcademicYearId = signal<number | undefined>(undefined);

  get userName() {
    return sessionStorage.getItem('name') || 'Admin';
  }

  get salutation() {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  ngOnInit(): void {
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
