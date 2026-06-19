import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TeacherService, TeacherResponseDTO, TeacherQueryRequest, PagedResponse } from '../../services/teacher.service';

interface TeacherUI extends TeacherResponseDTO {
  email: string;
  avatarUrl: string;
}

@Component({
  selector: 'app-teachers',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teachers.html',
  styleUrl: './teachers.css',
})
export class Teachers implements OnInit {
  private teacherService = inject(TeacherService);
  
  teachers = signal<TeacherUI[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  Math = Math;

  // Pagination state
  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = signal(0);

  // Filters state
  searchQuery = signal('');
  status = signal('All');

  // To debounce search
  private searchTimeout: any;

  ngOnInit() {
    this.fetchTeachers();
  }

  fetchTeachers() {
    this.loading.set(true);
    this.error.set(null);
    
    const request: TeacherQueryRequest = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.teacherService.getAllTeachers(request).subscribe({
      next: (response: PagedResponse<TeacherResponseDTO>) => {
        const mappedData = response.items.map(dto => ({
          ...dto,
          email: `${dto.username.toLowerCase()}@edupro.in`,
          avatarUrl: 'https://ui-avatars.com/api/?name=' + encodeURIComponent(dto.name) + '&background=random'
        }));
        
        this.teachers.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch teachers', err);
        this.error.set('Failed to load teachers. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
    this.fetchTeachers();
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
    const request: TeacherQueryRequest = {
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.teacherService.exportTeachersPdf(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'teachers-directory.pdf';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to export PDF', err);
        alert('Failed to generate PDF report.');
      }
    });
  }

  previousPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.fetchTeachers();
    }
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.fetchTeachers();
    }
  }
}
