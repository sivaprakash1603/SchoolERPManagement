import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FeeService, CheckoutSessionResultDTO } from '../../services/fee.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';


@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-result.html',
  styleUrls: ['./payment-result.css']
})
export class PaymentResultComponent implements OnInit {
  status = signal<'success' | 'failed' | null>(null);
  sessionId = signal<string | null>(null);
  sessionDetails = signal<CheckoutSessionResultDTO | null>(null);
  loading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);
  downloading = signal<boolean>(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private feeService: FeeService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.status.set(params['status']);
      this.sessionId.set(params['session_id']);

      if (this.status() === 'success' && this.sessionId()) {
        this.fetchSessionDetails();
      } else {
        this.loading.set(false);
      }
    });
  }

  fetchSessionDetails(): void {
    this.feeService.getCheckoutSessionDetails(this.sessionId()!).subscribe({
      next: (details) => {
        this.sessionDetails.set(details);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching session details', err);
        this.errorMessage.set(err.message || 'Unknown error');
        this.loading.set(false);
      }
    });
  }

  downloadReceipt(): void {
    const details = this.sessionDetails();
    if (!details || !details.transactionId) return;

    this.downloading.set(true);
    // Because this returns a file, we use HttpClient directly to get a Blob
    const url = `${environment.apiUrl}/Fees/receipt/${details.transactionId}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const downloadUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = `Receipt_${details.transactionId}.pdf`;
        link.click();
        window.URL.revokeObjectURL(downloadUrl);
        this.downloading.set(false);
      },
      error: (err) => {
        console.error('Failed to download receipt', err);
        // Fallback: Just open the url in a new tab if blob download fails due to CORS or something
        window.open(url, '_blank');
        this.downloading.set(false);
      }
    });
  }

  goToFees(): void {
    this.router.navigate(['/fees']);
  }
}
