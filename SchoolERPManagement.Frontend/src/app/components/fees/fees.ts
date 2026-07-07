import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeeService, FeeSummaryDTO, FeeComponentDTO, FeePaymentResponseDTO, ClassFeeSummaryDTO } from '../../services/fee.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { StudentService, StudentQueryResponseDTO } from '../../services/student.service';
import { ParentService } from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';
import { TeacherService } from '../../services/teacher.service';
import { TimetableService } from '../../services/timetable.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FilterStateService } from '../../services/filter-state.service';
import { DebounceClickDirective } from '../../directives/debounce-click.directive';

@Component({
  selector: 'app-fees',
  standalone: true,
  imports: [CommonModule, FormsModule, DebounceClickDirective],
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
  private teacherService = inject(TeacherService);
  private timetableService = inject(TimetableService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private filterStateService = inject(FilterStateService);

  constructor() {
    const savedState = this.filterStateService.getState('fees');
    if (savedState) {
      if (savedState.selectedAcademicYearId !== undefined) this.selectedAcademicYearId.set(savedState.selectedAcademicYearId);
      if (savedState.selectedClassId !== undefined) this.selectedClassId.set(savedState.selectedClassId);
      if (savedState.selectedStudentId !== undefined) this.selectedStudentId.set(savedState.selectedStudentId);
      if (savedState.feeStatusFilter !== undefined) this.feeStatusFilter.set(savedState.feeStatusFilter);
      if (savedState.studentSearchQuery !== undefined) this.studentSearchQuery.set(savedState.studentSearchQuery);
    }

    effect(() => {
      this.filterStateService.saveState('fees', {
        selectedAcademicYearId: this.selectedAcademicYearId(),
        selectedClassId: this.selectedClassId(),
        selectedStudentId: this.selectedStudentId(),
        feeStatusFilter: this.feeStatusFilter(),
        studentSearchQuery: this.studentSearchQuery()
      });
    });
  }

  // User Role Info
  userRole = signal<string>('Student');
  currentUserId = signal<number | null>(null);
  currentStudentId = signal<number | null>(null);
  currentParentId = signal<number | null>(null);
  currentTeacher = signal<any | null>(null);
  teacherClassIds = signal<number[]>([]);

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
  showStripeModal = signal(false);
  stripePaymentComponent = signal<FeeComponentDTO | null>(null);
  stripePaymentAmount = signal<number>(0);
  stripePaymentMaxAmount = signal<number>(0);

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
    } else if (role === 'Teacher') {
      const username = sessionStorage.getItem('username') || '';
      this.teacherService.getTeacherByUsername(username).subscribe({
        next: (teacher) => {
          this.currentTeacher.set(teacher);
          this.timetableService.getTeacherTimetable(teacher.id).subscribe({
            next: (slots) => {
              const classIds = Array.from(new Set<number>(slots.map(s => s.classId)));
              this.teacherClassIds.set(classIds);
              this.fetchAcademicYears();
            },
            error: (err) => {
              console.error('Failed to load teacher timetable', err);
              this.fetchAcademicYears();
            }
          });
        },
        error: (err) => {
          console.error('Failed to load teacher profile', err);
          this.fetchAcademicYears();
        }
      });
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

  // --- ADMIN & TEACHER FLOW ---
  fetchAcademicYears() {
    this.loadingYears.set(true);
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        this.loadingYears.set(false);
        const savedYearId = this.selectedAcademicYearId();
        const currentYear = years.find((y) => y.isCurrent) || years[0];
        const yearIdToUse = savedYearId !== null ? savedYearId : (currentYear ? currentYear.id : null);
        if (yearIdToUse) {
          this.selectedAcademicYearId.set(yearIdToUse);
          this.fetchClasses(yearIdToUse);
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
        if (this.userRole() === 'Teacher') {
          const teacher = this.currentTeacher();
          const filtered = res.filter(c => 
            this.teacherClassIds().includes(c.id) || 
            (teacher && teacher.className && c.classname.toLowerCase() === teacher.className.toLowerCase() && 
             (!teacher.section || c.section?.toLowerCase() === teacher.section.toLowerCase()))
          );
          this.classes.set(filtered);
        } else {
          this.classes.set(res);
        }
        this.loadingClasses.set(false);
        const activeClasses = this.classes();
        const savedClassId = this.selectedClassId();
        const classIdToUse = (savedClassId !== null && activeClasses.some(c => c.id === savedClassId)) ? savedClassId : (activeClasses.length > 0 ? activeClasses[0].id : null);
        if (classIdToUse) {
          this.selectedClassId.set(classIdToUse);
          this.fetchStudents(classIdToUse);
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
      const savedStudentId = this.selectedStudentId();
      const hasSavedStudent = savedStudentId !== null && list.some(s => s.studentId === savedStudentId);
      if (hasSavedStudent) {
        this.selectedStudentId.set(savedStudentId);
        this.fetchFeeSummary(savedStudentId);
      } else if (list.length > 0) {
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

  onYearChange(val: any) {
    const yearId = typeof val === 'string' ? parseInt(val, 10) : val;
    this.selectedAcademicYearId.set(yearId);
    this.fetchClasses(yearId);
  }

  onClassChange(val: any) {
    const classId = typeof val === 'string' ? parseInt(val, 10) : val;
    this.selectedClassId.set(classId);
    this.fetchStudents(classId);
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
              const savedId = this.parentService.selectedChildId;
              const child = (savedId && kids.find(k => k.studentId === savedId)) || kids[0];
              this.selectedStudentId.set(child.studentId);
              this.parentService.selectedChildId = child.studentId;
              this.fetchFeeSummary(child.studentId);
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

  onChildChange(studentId: any) {
    const parsedId = Number(studentId);
    this.selectedStudentId.set(parsedId);
    this.parentService.selectedChildId = parsedId;
    this.fetchFeeSummary(parsedId);
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

    if (!form.classId) {
      this.toastService.warning('Please select a class.');
      return;
    }
    if (!yearId) {
      this.toastService.warning('Academic Session is missing.');
      return;
    }
    if (!form.feeName || !form.feeName.trim()) {
      this.toastService.warning('Please enter a Fee Name.');
      return;
    }
    if (form.totalAmount <= 0) {
      this.toastService.warning('Amount must be greater than zero.');
      return;
    }

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
  openPaymentModal(comp?: FeeComponentDTO) {
    const today = new Date().toISOString().split('T')[0];
    const defaultComponentId = comp ? comp.id : (this.feeComponents().length > 0 ? this.feeComponents()[0].id : null);
    const defaultAmount = comp ? this.getOutstandingBalance(comp) : 0;

    this.paymentForm.set({
      feeStructureId: defaultComponentId,
      amountPaid: defaultAmount,
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

    if (!studentId) {
      this.toastService.warning('Please select a student.');
      return;
    }
    if (!form.feeStructureId) {
      this.toastService.warning('Please select a fee component.');
      return;
    }
    if (form.amountPaid <= 0) {
      this.toastService.warning('Payment amount must be greater than zero.');
      return;
    }
    if (form.paymentDate) {
      const selectedDate = new Date(form.paymentDate);
      const today = new Date();
      selectedDate.setHours(0,0,0,0);
      today.setHours(0,0,0,0);
      if (selectedDate > today) {
        this.toastService.warning('Payment date cannot be in the future.');
        return;
      }
    }

    const selectedComp = this.feeComponents().find(c => c.id === form.feeStructureId);
    if (selectedComp) {
      const balance = this.getOutstandingBalance(selectedComp);
      if (form.amountPaid > balance) {
        this.toastService.warning(`Payment amount exceeds the outstanding balance of ₹${balance}.`);
        return;
      }
    }

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
  openStripeModal(component: FeeComponentDTO) {
    const balance = this.getOutstandingBalance(component);
    if (balance <= 0) {
      this.toastService.info('This fee component is already fully paid.');
      return;
    }
    this.stripePaymentComponent.set(component);
    this.stripePaymentMaxAmount.set(balance);
    this.stripePaymentAmount.set(balance); // Default to full balance
    this.showStripeModal.set(true);
  }

  closeStripeModal() {
    this.showStripeModal.set(false);
    this.stripePaymentComponent.set(null);
  }

  confirmStripePayment() {
    const component = this.stripePaymentComponent();
    const amount = this.stripePaymentAmount();
    const studentId = this.selectedStudentId();

    if (!component || !studentId) return;

    if (amount <= 0 || amount > this.stripePaymentMaxAmount()) {
      this.toastService.warning(`Please enter a valid amount between 1 and ${this.stripePaymentMaxAmount()}`);
      return;
    }

    this.isProcessingStripe.set(true);
    this.feeService.createCheckoutSession({
      studentId,
      feeStructureId: component.id,
      amount: amount,
      successUrl: window.location.origin + '/payment-result?status=success&session_id={CHECKOUT_SESSION_ID}',
      cancelUrl: window.location.origin + '/payment-result?status=failed'
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
