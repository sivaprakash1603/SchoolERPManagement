import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeeService, FeeSummaryDTO, FeeComponentDTO, FeePaymentResponseDTO, ClassFeeSummaryDTO } from '../../services/fee.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { StudentService, StudentQueryResponseDTO } from '../../services/student.service';
import { ParentService } from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-fees',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fees.html',
  styleUrl: './fees.css',
})
export class Fees implements OnInit {
  private feeService = inject(FeeService);
  private academicYearService = inject(AcademicYearService);
  private classService = inject(ClassService);
  private studentService = inject(StudentService);
  private parentService = inject(ParentService);
  private toastService = inject(ToastService);

  // User Role Info
  userRole = signal<string>('Student');
  currentUserId = signal<number | null>(null);
  currentStudentId = signal<number | null>(null);
  currentParentId = signal<number | null>(null);

  // Selection signals
  selectedAcademicYearId = signal<number | null>(null);
  selectedClassId = signal<number | null>(null);
  selectedStudentId = signal<number | null>(null);

  // Filter signals
  studentSearchQuery = signal<string>('');
  feeStatusFilter = signal<'all' | 'paid' | 'pending'>('all');

  // Lists
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);
  classStudentSummaries = signal<ClassFeeSummaryDTO[]>([]);
  filteredStudentSummaries = signal<ClassFeeSummaryDTO[]>([]);
  children = signal<any[]>([]); // Parent's children list
  feeComponents = signal<FeeComponentDTO[]>([]);
  payments = signal<FeePaymentResponseDTO[]>([]);

  // Fee Details Summary Signals
  totalFeeAmount = signal<number | null>(null);
  totalPaid = signal<number>(0);
  pendingAmount = signal<number>(0);

  // Loader states
  loadingYears = signal(false);
  loadingClasses = signal(false);
  loadingStudents = signal(false);
  loadingSummary = signal(false);
  isSavingStructure = signal(false);
  isSavingPayment = signal(false);
  isProcessingStripe = signal(false);

  // Modal controls
  showCreateStructureModal = signal(false);
  showPaymentModal = signal(false);

  // Forms
  structureForm = signal({
    classId: null as number | null,
    feeName: '',
    totalAmount: 0,
    dueDate: '',
  });

  paymentForm = signal({
    feeStructureId: null as number | null,
    amountPaid: 0,
    paymentDate: '',
    paymentMethod: 'cash',
    transactionId: '',
  });

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);

    const uidStr = sessionStorage.getItem('userId');
    const uid = uidStr ? parseInt(uidStr, 10) : null;
    this.currentUserId.set(uid);

    if (role === 'Admin') {
      this.fetchAcademicYears();
    } else if (role === 'Parent') {
      if (uid) {
        this.fetchParentAndChildren(uid);
      }
    } else if (role === 'Student') {
      if (uid) {
        this.fetchStudentProfile(uid);
      }
    }
  }

  // --- ADMIN FLOW ---
  fetchAcademicYears() {
    this.loadingYears.set(true);
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        this.loadingYears.set(false);
        const currentYear = years.find((y) => y.isCurrent) || years[0];
        if (currentYear) {
          this.selectedAcademicYearId.set(currentYear.id);
          this.fetchClasses(currentYear.id);
        }
      },
      error: (err) => {
        console.error('Failed to load academic years', err);
        this.toastService.error('Failed to load academic sessions.');
        this.loadingYears.set(false);
      }
    });
  }

  fetchClasses(yearId: number) {
    this.loadingClasses.set(true);
    this.classService.getAllClasses(yearId).subscribe({
      next: (res) => {
        this.classes.set(res);
        this.loadingClasses.set(false);
        if (res.length > 0) {
          this.selectedClassId.set(res[0].id);
          this.fetchStudents(res[0].id);
        } else {
          this.selectedClassId.set(null);
          this.classStudentSummaries.set([]);
          this.filteredStudentSummaries.set([]);
          this.clearSummary();
        }
      },
      error: (err) => {
        console.error('Failed to load classes', err);
        this.loadingClasses.set(false);
      }
    });
  }

  fetchStudents(classId: number) {
    const yearId = this.selectedAcademicYearId();
    if (!yearId) return;

    this.loadingStudents.set(true);
    this.feeService.getClassFeeSummaries(classId, yearId).subscribe({
      next: (res) => {
        this.classStudentSummaries.set(res);
        this.updateFilteredStudents(true);
        this.loadingStudents.set(false);
      },
      error: (err) => {
        console.error('Failed to load class student summaries', err);
        this.classStudentSummaries.set([]);
        this.filteredStudentSummaries.set([]);
        this.clearSummary();
        this.loadingStudents.set(false);
      }
    });
  }

  updateFilteredStudents(resetSelection = false) {
    let list = this.classStudentSummaries();
    const query = this.studentSearchQuery().trim().toLowerCase();
    const filter = this.feeStatusFilter();

    if (query) {
      list = list.filter(s => s.studentName.toLowerCase().includes(query) || s.regNo.toLowerCase().includes(query));
    }

    if (filter === 'paid') {
      list = list.filter(s => s.pendingAmount === 0);
    } else if (filter === 'pending') {
      list = list.filter(s => s.pendingAmount > 0);
    }

    this.filteredStudentSummaries.set(list);

    if (resetSelection) {
      if (list.length > 0) {
        this.selectedStudentId.set(list[0].studentId);
        this.fetchFeeSummary(list[0].studentId);
      } else {
        this.selectedStudentId.set(null);
        this.clearSummary();
      }
    } else {
      const currentlySelected = list.find(s => s.studentId === this.selectedStudentId());
      if (!currentlySelected && list.length > 0) {
        this.selectedStudentId.set(list[0].studentId);
        this.fetchFeeSummary(list[0].studentId);
      } else if (list.length === 0) {
        this.selectedStudentId.set(null);
        this.clearSummary();
      }
    }
  }

  onSearchQueryChange(query: string) {
    this.studentSearchQuery.set(query);
    this.updateFilteredStudents(false);
  }

  onFilterStatusChange(status: 'all' | 'paid' | 'pending') {
    this.feeStatusFilter.set(status);
    this.updateFilteredStudents(false);
  }

  onYearChange(val: number) {
    this.selectedAcademicYearId.set(val);
    this.fetchClasses(val);
  }

  onClassChange(val: number) {
    this.selectedClassId.set(val);
    this.fetchStudents(val);
  }

  onStudentSelect(studentId: number) {
    this.selectedStudentId.set(studentId);
    this.fetchFeeSummary(studentId);
  }

  // --- STUDENT FLOW ---
  fetchStudentProfile(userId: number) {
    this.loadingSummary.set(true);
    this.studentService.getStudentByUserId(userId).subscribe({
      next: (profile) => {
        this.currentStudentId.set(profile.id);
        this.selectedStudentId.set(profile.id);
        this.fetchFeeSummary(profile.id);
      },
      error: (err) => {
        console.error('Failed to load student profile', err);
        this.toastService.error('Failed to load student profile.');
        this.loadingSummary.set(false);
      }
    });
  }

  // --- PARENT FLOW ---
  fetchParentAndChildren(userId: number) {
    this.loadingSummary.set(true);
    this.parentService.getParentByUserId(userId).subscribe({
      next: (parent) => {
        this.currentParentId.set(parent.id);
        this.parentService.getParentChildren(parent.id).subscribe({
          next: (kids) => {
            this.children.set(kids);
            if (kids.length > 0) {
              this.selectedStudentId.set(kids[0].studentId);
              this.fetchFeeSummary(kids[0].studentId);
            } else {
              this.loadingSummary.set(false);
            }
          },
          error: (err) => {
            console.error('Failed to load children', err);
            this.loadingSummary.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Failed to load parent profile', err);
        this.toastService.error('Failed to load parent profile.');
        this.loadingSummary.set(false);
      }
    });
  }

  onChildChange(studentId: number) {
    this.selectedStudentId.set(studentId);
    this.fetchFeeSummary(studentId);
  }

  // --- FEE SUMMARY FETCH ---
  fetchFeeSummary(studentId: number) {
    this.loadingSummary.set(true);
    this.feeService.getFeeDetails(studentId).subscribe({
      next: (summary: FeeSummaryDTO) => {
        this.totalFeeAmount.set(summary.totalFeeAmount);
        this.totalPaid.set(summary.totalPaid);
        this.pendingAmount.set(summary.pendingAmount);
        this.feeComponents.set(summary.feeComponents);
        this.payments.set(summary.payments);
        this.loadingSummary.set(false);
      },
      error: (err) => {
        console.error('Failed to load fee details', err);
        this.clearSummary();
        this.loadingSummary.set(false);
      }
    });
  }

  clearSummary() {
    this.totalFeeAmount.set(null);
    this.totalPaid.set(0);
    this.pendingAmount.set(0);
    this.feeComponents.set([]);
    this.payments.set([]);
  }

  // --- ADD FEE STRUCTURE MODAL ---
  openCreateStructureModal() {
    const defaultClassId = this.classes().length > 0 ? this.classes()[0].id : null;
    const today = new Date().toISOString().split('T')[0];
    
    this.structureForm.set({
      classId: defaultClassId,
      feeName: '',
      totalAmount: 0,
      dueDate: today,
    });
    this.showCreateStructureModal.set(true);
  }

  closeCreateStructureModal() {
    this.showCreateStructureModal.set(false);
  }

  saveFeeStructure() {
    const form = this.structureForm();
    const yearId = this.selectedAcademicYearId();
    if (!form.classId || !yearId || !form.feeName.trim() || form.totalAmount <= 0) return;

    this.isSavingStructure.set(true);
    this.feeService.addFeeStructure({
      classId: form.classId,
      academicYearId: yearId,
      feeName: form.feeName.trim(),
      totalAmount: form.totalAmount,
      dueDate: form.dueDate || undefined,
    }).subscribe({
      next: () => {
        this.toastService.success('Fee structure added successfully.');
        this.isSavingStructure.set(false);
        this.closeCreateStructureModal();
        const classId = this.selectedClassId();
        if (classId) {
          this.fetchStudents(classId);
        }
      },
      error: (err) => {
        console.error('Failed to save fee structure', err);
        this.toastService.error(err.error?.message || 'Failed to add fee structure.');
        this.isSavingStructure.set(false);
      }
    });
  }

  // --- RECORD MANUAL PAYMENT ---
  openPaymentModal() {
    const today = new Date().toISOString().split('T')[0];
    const defaultComponentId = this.feeComponents().length > 0 ? this.feeComponents()[0].id : null;

    this.paymentForm.set({
      feeStructureId: defaultComponentId,
      amountPaid: 0,
      paymentDate: today,
      paymentMethod: 'cash',
      transactionId: '',
    });
    this.showPaymentModal.set(true);
  }

  closePaymentModal() {
    this.showPaymentModal.set(false);
  }

  savePayment() {
    const studentId = this.selectedStudentId();
    const form = this.paymentForm();
    if (!studentId || !form.feeStructureId || form.amountPaid <= 0) return;

    this.isSavingPayment.set(true);
    this.feeService.payFees({
      studentId,
      feeStructureId: form.feeStructureId,
      amountPaid: form.amountPaid,
      paymentDate: form.paymentDate || undefined,
      paymentMethod: form.paymentMethod,
      transactionId: form.transactionId || undefined,
    }).subscribe({
      next: () => {
        this.toastService.success('Payment recorded successfully.');
        this.isSavingPayment.set(false);
        this.closePaymentModal();
        
        // Refresh summaries
        const classId = this.selectedClassId();
        if (classId) {
          this.fetchStudents(classId);
        } else {
          this.fetchFeeSummary(studentId);
        }
      },
      error: (err) => {
        console.error('Failed to save payment', err);
        this.toastService.error(err.error?.message || 'Failed to record payment.');
        this.isSavingPayment.set(false);
      }
    });
  }

  // --- STRIPE CHECKOUT ---
  payViaStripe(component: FeeComponentDTO) {
    const studentId = this.selectedStudentId();
    if (!studentId) return;

    const relevantPayments = this.payments().filter(p => p.feeStructureId === component.id);
    const paidForComponent = relevantPayments.reduce((sum, p) => sum + p.amountPaid, 0);
    const balance = component.amount - paidForComponent;

    if (balance <= 0) {
      this.toastService.info('This fee component is already fully paid.');
      return;
    }

    this.isProcessingStripe.set(true);
    this.feeService.createCheckoutSession({
      studentId,
      feeStructureId: component.id,
      amount: balance,
      successUrl: window.location.origin + '/fees?payment=success',
      cancelUrl: window.location.origin + '/fees?payment=cancelled'
    }).subscribe({
      next: (res) => {
        if (res.url) {
          window.location.href = res.url;
        } else {
          this.toastService.error('Failed to retrieve checkout url.');
          this.isProcessingStripe.set(false);
        }
      },
      error: (err) => {
        console.error('Failed to generate stripe session', err);
        this.toastService.error('Error contacting payment gateway.');
        this.isProcessingStripe.set(false);
      }
    });
  }

  // Helper to calculate outstanding balance for a fee component
  getOutstandingBalance(component: FeeComponentDTO): number {
    const componentPayments = this.payments().filter(p => p.feeStructureId === component.id);
    const sumPaid = componentPayments.reduce((sum, p) => sum + p.amountPaid, 0);
    return Math.max(component.amount - sumPaid, 0);
  }

  getComponentName(id: number): string {
    const comp = this.feeComponents().find(c => c.id === id);
    return comp ? (comp.name || 'Fee Component') : 'General Fee';
  }
}
