import { Component, OnInit, ViewChild, ElementRef, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
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

  @ViewChild('demographicsChart') demographicsChartRef!: ElementRef;
  @ViewChild('revenueChart') revenueChartRef!: ElementRef;

  metrics = signal<AdminDashboardDTO | null>(null);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.dashboardService.getAdminMetrics().subscribe({
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

  private initDemographicsChart(data: AdminDashboardDTO) {
    if (!this.demographicsChartRef) return;
    
    new Chart(this.demographicsChartRef.nativeElement, {
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

    const labels = data.revenueTrends.map(t => t.month);
    const values = data.revenueTrends.map(t => t.amount);

    new Chart(this.revenueChartRef.nativeElement, {
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
