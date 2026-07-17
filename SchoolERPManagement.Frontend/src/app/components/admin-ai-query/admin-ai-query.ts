import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService } from '../../services/ai.service';
import { ToastService } from '../../services/toast.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-admin-ai-query',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-ai-query.html',
  styleUrl: './admin-ai-query.css'
})
export class AdminAiQuery {
  aiService = inject(AiService);
  toastService = inject(ToastService);

  searchQuery: string = '';
  isSearching: boolean = false;

  suggestedQueries: string[] = [
    "How many students are enrolled in 10th grade?",
    "Show me the total fees collected last month",
    "List all teachers who joined in the last year",
    "Which classes have less than 20 students?"
  ];

  tableHeaders: string[] = [];
  tableData: any[] = [];
  rawBlob: Blob | null = null;

  setQuery(query: string) {
    this.searchQuery = query;
    this.executeSearch();
  }

  executeSearch() {
    if (!this.searchQuery.trim()) return;

    this.isSearching = true;
    const queryToExecute = this.searchQuery;

    this.aiService.adminSearch(queryToExecute).subscribe({
      next: async (blob: Blob) => {
        this.isSearching = false;
        this.rawBlob = blob;
        this.searchQuery = '';
        
        try {
          // Parse the Excel blob to JSON for preview
          const arrayBuffer = await blob.arrayBuffer();
          const workbook = XLSX.read(arrayBuffer, { type: 'array', cellDates: true });
          const firstSheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[firstSheetName];
          
          const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1, raw: false, dateNF: 'yyyy-mm-dd' }) as any[][];
          
          if (jsonData && jsonData.length > 0) {
            this.tableHeaders = jsonData[0] || [];
            this.tableData = jsonData.slice(1);
            this.toastService.success("Report generated successfully!");
          } else {
            this.tableHeaders = [];
            this.tableData = [];
            this.toastService.success("Report generated (No data found).");
          }
        } catch (e) {
          console.error("Error parsing Excel blob", e);
          this.toastService.error("Report generated but could not be previewed.");
        }
      },
      error: async (err) => {
        this.isSearching = false;
        console.error(err);
        let errorMsg = "Failed to generate report. Ensure you have Admin privileges or valid API setup.";
        
        if (err.error instanceof Blob) {
            try {
                const text = await err.error.text();
                const json = JSON.parse(text);
                if (json.detail) errorMsg = json.detail;
                else if (json.message) errorMsg = json.message;
            } catch (e) {}
        } else if (err.error) {
            if (err.error.detail) errorMsg = err.error.detail;
            else if (err.error.message) errorMsg = err.error.message;
        }
        
        this.toastService.error(errorMsg);
      }
    });
  }

  downloadReport() {
    if (!this.rawBlob) return;
    const url = window.URL.createObjectURL(this.rawBlob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `AI_Report_${new Date().getTime()}.xlsx`;
    document.body.appendChild(anchor);
    anchor.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(anchor);
  }
}
