import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ParentService, ParentResponseDTO, ParentQueryRequest, PagedResponse } from '../../services/parent.service';

interface ParentUI extends ParentResponseDTO {
  email: string;
  avatarUrl: string;
}

@Component({
  selector: 'app-parents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './parents.html',
  styleUrl: './parents.css',
})
export class Parents implements OnInit {
  private parentService = inject(ParentService);
  
  parents = signal<ParentUI[]>([]);
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
    this.fetchParents();
  }

  fetchParents() {
    this.loading.set(true);
    this.error.set(null);
    
    const request: ParentQueryRequest = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.parentService.getAllParents(request).subscribe({
      next: (response: PagedResponse<ParentResponseDTO>) => {
        const mappedData = response.items.map(dto => ({
          ...dto,
          email: dto.email || 'N/A',
          avatarUrl: 'https://ui-avatars.com/api/?name=' + encodeURIComponent(dto.name) + '&background=random'
        }));
        
        this.parents.set(mappedData);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch parents', err);
        this.error.set('Failed to load parents. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.pageNumber.set(1);
    this.fetchParents();
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
    const request: ParentQueryRequest = {
      searchQuery: this.searchQuery(),
      status: this.status()
    };

    this.parentService.exportParentsPdf(request).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'parents-directory.pdf';
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
      this.fetchParents();
    }
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.fetchParents();
    }
  }
}
