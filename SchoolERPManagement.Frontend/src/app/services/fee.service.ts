import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface FeeComponentDTO {
  id: number;
  name: string;
  amount: number;
}

export interface FeePaymentResponseDTO {
  id: number;
  studentId: number;
  feeStructureId: number;
  amountPaid: number;
  paymentDate: string;
  paymentMethod: string;
  transactionId: string;
}

export interface FeeSummaryDTO {
  studentId: number;
  totalFeeAmount: number | null;
  totalPaid: number;
  pendingAmount: number;
  payments: FeePaymentResponseDTO[];
  feeComponents: FeeComponentDTO[];
}

export interface FeePaymentDTO {
  studentId: number;
  feeStructureId: number;
  amountPaid: number;
  paymentDate?: string;
  paymentMethod?: string;
  transactionId?: string;
}

export interface CreateCheckoutSessionDTO {
  studentId: number;
  feeStructureId: number;
  amount: number;
  successUrl?: string;
  cancelUrl?: string;
}

export interface AddFeeStructureDTO {
  classId: number;
  academicYearId: number;
  feeName: string;
  totalAmount: number;
  dueDate?: string;
}

export interface FeeStructureResponseDTO {
  id: number;
  classId: number;
  academicYearId: number;
  feeName: string;
  totalAmount: number;
  dueDate?: string;
}

export interface ClassFeeSummaryDTO {
  studentId: number;
  studentName: string;
  regNo: string;
  totalFeeAmount: number;
  totalPaid: number;
  pendingAmount: number;
}

@Injectable({
  providedIn: 'root'
})
export class FeeService {
  private readonly baseUrl = 'http://localhost:5203/api/Fees';

  constructor(private http: HttpClient) {}

  getFeeDetails(studentId: number): Observable<FeeSummaryDTO> {
    return this.http.get<FeeSummaryDTO>(`${this.baseUrl}/student/${studentId}/summary`);
  }

  getPaymentHistory(studentId: number): Observable<FeePaymentResponseDTO[]> {
    return this.http.get<FeePaymentResponseDTO[]>(`${this.baseUrl}/student/${studentId}/history`);
  }

  payFees(dto: FeePaymentDTO): Observable<FeePaymentResponseDTO> {
    return this.http.post<FeePaymentResponseDTO>(`${this.baseUrl}/pay`, dto);
  }

  createCheckoutSession(dto: CreateCheckoutSessionDTO): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(`${this.baseUrl}/create-checkout-session`, dto);
  }

  addFeeStructure(dto: AddFeeStructureDTO): Observable<FeeStructureResponseDTO> {
    return this.http.post<FeeStructureResponseDTO>(`${this.baseUrl}/structure`, dto);
  }

  getClassFeeSummaries(classId: number, academicYearId: number): Observable<ClassFeeSummaryDTO[]> {
    return this.http.get<ClassFeeSummaryDTO[]>(`${this.baseUrl}/class/${classId}/summaries?academicYearId=${academicYearId}`);
  }
}
