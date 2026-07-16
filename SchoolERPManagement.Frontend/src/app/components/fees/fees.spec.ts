import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError, Subject } from 'rxjs';
import { Fees } from './fees';
import { FeeService, FeeComponentDTO } from '../../services/fee.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ClassService } from '../../services/class.service';
import { StudentService } from '../../services/student.service';
import { ParentService } from '../../services/parent.service';
import { TeacherService } from '../../services/teacher.service';
import { TimetableService } from '../../services/timetable.service';
import { ToastService } from '../../services/toast.service';
import { FilterStateService } from '../../services/filter-state.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('Fees', () => {
  let component: Fees;
  let fixture: ComponentFixture<Fees>;
  
  let mockFeeService: any;
  let mockAcademicYearService: any;
  let mockClassService: any;
  let mockStudentService: any;
  let mockParentService: any;
  let mockTeacherService: any;
  let mockTimetableService: any;
  let mockToastService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockFeeService = {
      getClassFeeSummaries: vi.fn().mockReturnValue(of([
        { studentId: 10, studentName: 'Alice', regNo: 'S001', totalAmount: 1000, paidAmount: 0, pendingAmount: 1000 },
        { studentId: 11, studentName: 'Bob', regNo: 'S002', totalAmount: 1000, paidAmount: 1000, pendingAmount: 0 }
      ])),
      getFeeDetails: vi.fn().mockReturnValue(of({
        totalFeeAmount: 1000,
        totalPaid: 500,
        pendingAmount: 500,
        feeComponents: [{ id: 1, feeName: 'Tuition', amount: 1000 }],
        payments: [{ id: 100, studentId: 10, feeStructureId: 1, amountPaid: 500, paymentDate: '2026-07-06T00:00:00Z', paymentMethod: 'cash', transactionId: 'TX1' }]
      })),
      addFeeStructure: vi.fn().mockReturnValue(of({})),
      payFees: vi.fn().mockReturnValue(of({})),
      createCheckoutSession: vi.fn().mockReturnValue(of({ url: 'https://stripe-redirect-url' }))
    };

    mockAcademicYearService = {
      getAllAcademicYears: vi.fn().mockReturnValue(of([{ id: 1, yearName: '2026-2027', isCurrent: true }]))
    };

    mockClassService = {
      getAllClasses: vi.fn().mockReturnValue(of([{ id: 2, classfeeName: 'Grade 10', section: 'A' }]))
    };

    mockStudentService = {
      getStudentByUserId: vi.fn().mockReturnValue(of({ id: 10, feeName: 'Alice', classId: 2 }))
    };

    mockParentService = {
      getParentByUserId: vi.fn().mockReturnValue(of({ id: 20 })),
      getParentChildren: vi.fn().mockReturnValue(of([
        { studentId: 10, feeName: 'Alice', regNo: 'S001', className: 'Grade 10' }
      ])),
      selectedChildId: null
    };

    mockTeacherService = {
      getTeacherByUserfeeName: vi.fn().mockReturnValue(of({ id: 30, className: 'Grade 10', section: 'A' }))
    };

    mockTimetableService = {
      getTeacherTimetable: vi.fn().mockReturnValue(of([{ classId: 2, subjectId: 100 }]))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn()
    };

    mockFilterStateService = {
      getState: vi.fn().mockReturnValue(undefined),
      saveState: vi.fn()
    };

    sessionStorage.clear();
    sessionStorage.setItem('role', 'Admin');
    sessionStorage.setItem('userId', '1');

    await TestBed.configureTestingModule({
      imports: [Fees, CommonModule, FormsModule],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: FeeService, useValue: mockFeeService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ClassService, useValue: mockClassService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: ParentService, useValue: mockParentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: ToastService, useValue: mockToastService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Fees);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  describe('Initialization and Roles', () => {
    it('should load state from FilterStateService on construct', () => {
      mockFilterStateService.getState.mockReturnValue({
        selectedAcademicYearId: 99,
        selectedClassId: 88,
        selectedStudentId: 77,
        feeStatusFilter: 'paid',
        studentSearchQuery: 'Test'
      });
      const newFixture = TestBed.createComponent(Fees);
      const newComponent = newFixture.componentInstance;
      
      expect(newComponent.selectedAcademicYearId()).toBe(99);
      expect(newComponent.selectedClassId()).toBe(88);
      expect(newComponent.feeStatusFilter()).toBe('paid');
      
      newFixture.detectChanges();
      expect(mockFilterStateService.saveState).toHaveBeenCalled();
    });

    it('should initialize Admin role correctly', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      expect(component.userRole()).toBe('Admin');
      expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
      expect(mockClassService.getAllClasses).toHaveBeenCalledWith(1);
    });

    it('should initialize Teacher role correctly', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 't1');
      fixture.detectChanges();
      expect(component.userRole()).toBe('Teacher');
      expect(mockTeacherService.getTeacherByUsername).toHaveBeenCalledWith('t1');
      expect(mockTimetableService.getTeacherTimetable).toHaveBeenCalledWith(30);
      expect(component.teacherClassIds()).toContain(2);
      expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
    });

    it('should fallback if Teacher profile or timetable fails', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 't1');
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalled();
      expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
      
      mockTeacherService.getTeacherByUsername.mockReturnValue(of({ id: 30 }));
      mockTimetableService.getTeacherTimetable.mockReturnValue(throwError(() => new Error('err')));
      component.ngOnInit();
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should initialize Student role correctly', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.userRole()).toBe('Student');
      expect(mockStudentService.getStudentByUserId).toHaveBeenCalledWith(1);
      expect(component.selectedStudentId()).toBe(10);
      expect(mockFeeService.getFeeDetails).toHaveBeenCalledWith(10);
    });

    it('should handle Student profile error', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load student profile.');
      consoleSpy.mockRestore();
    });

    it('should initialize Parent role correctly', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.userRole()).toBe('Parent');
      expect(mockParentService.getParentByUserId).toHaveBeenCalledWith(1);
      expect(mockParentService.getParentChildren).toHaveBeenCalledWith(20);
      expect(component.selectedStudentId()).toBe(10);
    });

    it('should handle Parent profile error', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load parent profile.');
      consoleSpy.mockRestore();
    });

    it('should handle Parent children error', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentChildren.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should handle Parent with no children', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentChildren.mockReturnValue(of([]));
      fixture.detectChanges();
      expect(component.children().length).toBe(0);
      expect(component.loadingSummary()).toBe(false);
    });
  });

  describe('Data Loading and Filtering', () => {
    beforeEach(() => {
      fixture.detectChanges(); // defaults to Admin
    });

    it('should handle fetchAcademicYears error', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchAcademicYears();
      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load academic sessions.');
      consoleSpy.mockRestore();
    });

    it('should filter classes for Teacher based on id or name', () => {
      component.userRole.set('Teacher');
      component.currentTeacher.set({ className: 'Grade 10', section: 'A' });
      component.teacherClassIds.set([99]); // ID doesn't match, but name matches
      mockClassService.getAllClasses.mockReturnValue(of([{ id: 2, classfeeName: 'Grade 10', section: 'A' }, { id: 3, classfeeName: 'Grade 11' }]));
      component.fetchClasses(1);
      expect(component.classes().length).toBe(1);
      expect(component.classes()[0].id).toBe(2);
    });

    it('should handle fetchClasses error', () => {
      mockClassService.getAllClasses.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchClasses(1);
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should handle fetchStudents error', () => {
      component.selectedAcademicYearId.set(1);
      mockFeeService.getClassFeeSummaries.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchStudents(2);
      expect(consoleSpy).toHaveBeenCalled();
      expect(component.classStudentSummaries().length).toBe(0);
      consoleSpy.mockRestore();
    });

    it('should filter students by search query', () => {
      component.onSearchQueryChange('Bob');
      expect(component.filteredStudentSummaries().length).toBe(1);
      expect(component.filteredStudentSummaries()[0].studentName).toBe('Bob');
    });

    it('should handle search query yielding no results', () => {
      component.onSearchQueryChange('Nobody');
      expect(component.filteredStudentSummaries().length).toBe(0);
      expect(component.selectedStudentId()).toBeNull();
      expect(component.totalFeeAmount()).toBeNull();
    });

    it('should filter students by status (paid vs pending)', () => {
      component.onFilterStatusChange('paid');
      expect(component.filteredStudentSummaries().length).toBe(1);
      expect(component.filteredStudentSummaries()[0].studentName).toBe('Bob'); // Bob has 0 pending
      
      component.onFilterStatusChange('pending');
      expect(component.filteredStudentSummaries().length).toBe(1);
      expect(component.filteredStudentSummaries()[0].studentName).toBe('Alice'); // Alice has pending
    });

    it('should handle year and class change', () => {
      component.onYearChange('1');
      expect(component.selectedAcademicYearId()).toBe(1);
      expect(mockClassService.getAllClasses).toHaveBeenCalledWith(1);
      
      component.onClassChange('2');
      expect(component.selectedClassId()).toBe(2);
      expect(mockFeeService.getClassFeeSummaries).toHaveBeenCalledWith(2, 1);
    });

    it('should handle student selection', () => {
      component.onStudentSelect(11);
      expect(component.selectedStudentId()).toBe(11);
      expect(mockFeeService.getFeeDetails).toHaveBeenCalledWith(11);
    });

    it('should handle child change for Parent', () => {
      component.onChildChange('11');
      expect(component.selectedStudentId()).toBe(11);
      expect(mockParentService.selectedChildId).toBe(11);
    });

    it('should handle fee details fetch error', () => {
      mockFeeService.getFeeDetails.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchFeeSummary(10);
      expect(consoleSpy).toHaveBeenCalled();
      expect(component.totalFeeAmount()).toBeNull(); // cleared
      consoleSpy.mockRestore();
    });
  });

  describe('Modals and Forms', () => {
    beforeEach(() => {
      fixture.detectChanges(); // defaults Admin
    });

    it('should toggle Create Structure Modal', () => {
      component.openCreateStructureModal();
      expect(component.showCreateStructureModal()).toBe(true);
      expect(component.structureForm().classId).toBe(2); // default class
      
      component.closeCreateStructureModal();
      expect(component.showCreateStructureModal()).toBe(false);
    });

    it('should save fee structure successfully', () => {
      component.selectedAcademicYearId.set(1);
      component.showCreateStructureModal.set(true);
      component.structureForm.set({ classId: 2, feeName: 'Library', totalAmount: 500, dueDate: '2026-07-06' });
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const saveBtn = compiled.querySelector('.modal-footer-custom .btn-primary') as HTMLButtonElement;
      saveBtn.click();
      
      expect(mockFeeService.addFeeStructure).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalledWith('Fee structure added successfully.');
    });

    it('should validate fee structure form inputs', () => {
      component.selectedAcademicYearId.set(1);
      
      component.structureForm.set({ classId: null, feeName: 'Lib', totalAmount: 500, dueDate: '' });
      component.saveFeeStructure();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select a class.');
      
      component.structureForm.set({ classId: 2, feeName: '   ', totalAmount: 500, dueDate: '' });
      component.saveFeeStructure();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please enter a Fee Name.');
      
      component.structureForm.set({ classId: 2, feeName: 'Lib', totalAmount: 0, dueDate: '' });
      component.saveFeeStructure();
      expect(mockToastService.warning).toHaveBeenCalledWith('Amount must be greater than zero.');
    });

    it('should warn if Academic Session is missing when saving structure', () => {
      component.selectedAcademicYearId.set(null);
      component.structureForm.set({ classId: 2, feeName: 'Library', totalAmount: 500, dueDate: '2026-07-06' });
      component.saveFeeStructure();
      expect(mockToastService.warning).toHaveBeenCalledWith('Academic Session is missing.');
    });

    it('should handle save structure error', () => {
      component.selectedAcademicYearId.set(1);
      component.structureForm.set({ classId: 2, feeName: 'Library', totalAmount: 500, dueDate: '2026-07-06' });
      mockFeeService.addFeeStructure.mockReturnValue(throwError(() => ({ error: { message: 'err msg' } })));
      component.saveFeeStructure();
      expect(mockToastService.error).toHaveBeenCalledWith('err msg');
    });

    it('should toggle Payment Modal', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([]);
      
      component.openPaymentModal(comp);
      expect(component.showPaymentModal()).toBe(true);
      expect(component.paymentForm().feeStructureId).toBe(1);
      expect(component.paymentForm().amountPaid).toBe(1000); // full amount outstanding
      
      component.closePaymentModal();
      expect(component.showPaymentModal()).toBe(false);
    });

    it('should save payment successfully', () => {
      component.selectedStudentId.set(10);
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      component.payments.set([]);
      
      component.showPaymentModal.set(true);
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 500, paymentDate: '2026-07-06', paymentMethod: 'cash', transactionId: '' });
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const saveBtn = compiled.querySelector('.modal-footer-custom .btn-primary') as HTMLButtonElement;
      saveBtn.click();
      
      expect(mockFeeService.payFees).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalledWith('Payment recorded successfully.');
    });

    it('should save payment successfully without classId selected', () => {
      component.selectedClassId.set(null);
      component.selectedStudentId.set(10);
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      component.payments.set([]);
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 500, paymentDate: '2026-07-06', paymentMethod: 'cash', transactionId: '' });
      
      component.savePayment();
      expect(mockFeeService.payFees).toHaveBeenCalled();
      expect(mockFeeService.getFeeDetails).toHaveBeenCalledWith(10);
      expect(mockToastService.success).toHaveBeenCalledWith('Payment recorded successfully.');
    });

    it('should validate payment form inputs', () => {
      component.selectedStudentId.set(null);
      component.savePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select a student.');
      
      component.selectedStudentId.set(10);
      component.paymentForm.set({ feeStructureId: null, amountPaid: 500, paymentDate: '', paymentMethod: 'cash', transactionId: '' });
      component.savePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select a fee component.');
      
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 0, paymentDate: '', paymentMethod: 'cash', transactionId: '' });
      component.savePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Payment amount must be greater than zero.');
    });

    it('should validate payment amount against outstanding balance', () => {
      component.selectedStudentId.set(10);
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      component.payments.set([{ id: 100, studentId: 10, feeStructureId: 1, amountPaid: 800, paymentDate: '', paymentMethod: '', transactionId: '' }]);
      
      // Balance is 200
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 300, paymentDate: '', paymentMethod: 'cash', transactionId: '' });
      component.savePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Payment amount exceeds the outstanding balance of ₹200.');
    });

    it('should validate payment date is not in future', () => {
      component.selectedStudentId.set(10);
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      component.payments.set([]);
      
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 10); // 10 days in future
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 100, paymentDate: futureDate.toISOString().split('T')[0], paymentMethod: 'cash', transactionId: '' });
      
      component.savePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Payment date cannot be in the future.');
    });

    it('should handle save payment error', () => {
      component.selectedStudentId.set(10);
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      component.payments.set([]);
      component.paymentForm.set({ feeStructureId: 1, amountPaid: 500, paymentDate: '2026-07-06', paymentMethod: 'cash', transactionId: '' });
      
      mockFeeService.payFees.mockReturnValue(throwError(() => ({ error: { message: 'pay err' } })));
      component.savePayment();
      expect(mockToastService.error).toHaveBeenCalledWith('pay err');
    });

    it('should open Stripe Modal and confirm payment', () => {
      const originalLocation = window.location;
      delete (window as any).location;
      window.location = { ...originalLocation, href: '' } as any;

      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([]);
      component.selectedStudentId.set(10);
      
      component.openStripeModal(comp);
      expect(component.showStripeModal()).toBe(true);
      expect(component.stripePaymentMaxAmount()).toBe(1000);
      
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      const payBtn = compiled.querySelector('.modal-footer .btn-primary') as HTMLButtonElement;
      payBtn.click();
      
      expect(mockFeeService.createCheckoutSession).toHaveBeenCalled();
      expect(window.location.href).toBe('https://stripe-redirect-url');
      
      component.closeStripeModal();
      expect(component.showStripeModal()).toBe(false);

      (window as any).location = originalLocation;
    });

    it('should show info if Stripe modal opened for fully paid component', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([{ id: 100, studentId: 10, feeStructureId: 1, amountPaid: 1000, paymentDate: '', paymentMethod: '', transactionId: '' }]);
      
      component.openStripeModal(comp);
      expect(mockToastService.info).toHaveBeenCalledWith('This fee component is already fully paid.');
      expect(component.showStripeModal()).toBe(false);
    });

    it('should validate Stripe payment amount', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([]);
      component.selectedStudentId.set(10);
      
      component.openStripeModal(comp);
      
      component.stripePaymentAmount.set(1500); // exceeds max
      component.confirmStripePayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please enter a valid amount between 1 and 1000');
    });

    it('should handle Stripe session error', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([]);
      component.selectedStudentId.set(10);
      
      component.openStripeModal(comp);
      mockFeeService.createCheckoutSession.mockReturnValue(throwError(() => new Error('err')));
      component.confirmStripePayment();
      
      expect(mockToastService.error).toHaveBeenCalledWith('Error contacting payment gateway.');
    });

    it('should handle missing URL from Stripe session', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([]);
      component.selectedStudentId.set(10);
      
      component.openStripeModal(comp);
      mockFeeService.createCheckoutSession.mockReturnValue(of({ url: null }));
      component.confirmStripePayment();
      
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to retrieve checkout url.');
    });
    
    it('should calculate outstanding balance correctly', () => {
      const comp: FeeComponentDTO = { id: 1, feeName: 'Tuition', amount: 1000 };
      component.feeComponents.set([comp]);
      component.payments.set([
        { id: 100, studentId: 10, feeStructureId: 1, amountPaid: 300, paymentDate: '', paymentMethod: '', transactionId: '' },
        { id: 101, studentId: 10, feeStructureId: 1, amountPaid: 200, paymentDate: '', paymentMethod: '', transactionId: '' },
        { id: 102, studentId: 10, feeStructureId: 2, amountPaid: 500, paymentDate: '', paymentMethod: '', transactionId: '' }
      ]);
      expect(component.getOutstandingBalance(comp)).toBe(500); // 1000 - 500
    });
    
    it('should get component name correctly', () => {
      component.feeComponents.set([{ id: 1, feeName: 'Tuition', amount: 1000 }]);
      expect(component.getComponentName(1)).toBe('Tuition');
      expect(component.getComponentName(99)).toBe('General Fee');
    });
  });

  describe('DOM Rendering', () => {
    it('should render loading state initially', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(new Subject());
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Consulting the ledger...');
    });

    it('should render Admin UI correctly', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Define Fee Structure');
      expect(compiled.textContent).toContain('Record Payment');
      expect(compiled.textContent).toContain('Academic Session');
      expect(compiled.textContent).toContain('Class / Grade');
    });

    it('should render Student list and summary in Admin view', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingSummary.set(false);
      component.loadingStudents.set(false);
      component.loadingClasses.set(false);
      component.selectedClassId.set(2);
      component.selectedStudentId.set(10);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Alice');
      expect(compiled.textContent).toContain('PENDING'); // Alice has pending 1000 in mock
      expect(compiled.textContent).toContain('Total Fee');
      expect(compiled.textContent).toContain('Tuition');
    });

    it('should render Parent UI with Child selector', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      component.loadingSummary.set(false);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Select Child');
      expect(compiled.textContent).toContain('Alice (S001 - Grade 10)');
    });

    it('should render Student UI summary', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      component.loadingSummary.set(false);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('My Fee Details');
      expect(compiled.textContent).toContain('Total Fee');
    });
    
    it('should render Create Structure modal and handle state', () => {
      component.showCreateStructureModal.set(true);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Define Class Fee Structure');
      expect(compiled.textContent).toContain('Grade 10 - A');
      
      // Trigger ngModelChange on classId select
      const selects = compiled.querySelectorAll('select');
      if (selects.length > 0) {
        selects[0].value = '2';
        selects[0].dispatchEvent(new Event('change'));
      }
      
      // Trigger keypress on totalAmount
      const amountInput = compiled.querySelector('input[type="number"]') as HTMLInputElement;
      if (amountInput) {
        amountInput.dispatchEvent(new KeyboardEvent('keypress', { charCode: 48 })); // valid
        amountInput.dispatchEvent(new KeyboardEvent('keypress', { charCode: 65 })); // invalid
      }
      
      // Save state
      component.isSavingStructure.set(true);
      fixture.detectChanges();
      expect(compiled.innerHTML).toContain('spinner-border');
    });

    it('should render Record Payment modal and handle state', () => {
      component.showPaymentModal.set(true);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Record Manual Fee Payment');
      expect(compiled.textContent).toContain('Tuition (Bal: ₹500)');
      
      // Trigger select change for fee component
      const selects = compiled.querySelectorAll('.modal-body select') as NodeListOf<HTMLSelectElement>;
      if (selects.length > 0) {
        // Fee Component select
        selects[0].value = '1';
        selects[0].dispatchEvent(new Event('change'));
      }
      if (selects.length > 1) {
        // Payment Method select
        selects[1].value = 'cheque';
        selects[1].dispatchEvent(new Event('change'));
      }

      // Trigger amountPaid keypress
      const amountInput = compiled.querySelector('input[type="number"]') as HTMLInputElement;
      if (amountInput) {
        amountInput.dispatchEvent(new KeyboardEvent('keypress', { charCode: 48 })); // valid
      }

      // Trigger paymentDate and transactionId inputs
      const dateInput = compiled.querySelector('input[type="date"]') as HTMLInputElement;
      if (dateInput) {
        dateInput.value = '2026-10-10';
        dateInput.dispatchEvent(new Event('input'));
        dateInput.dispatchEvent(new Event('change'));
      }

      const textInputs = compiled.querySelectorAll('.modal-body input[type="text"]');
      if (textInputs.length > 0) {
        const txnInput = textInputs[textInputs.length - 1] as HTMLInputElement;
        txnInput.value = 'TXN123';
        txnInput.dispatchEvent(new Event('input'));
      }
      
      component.isSavingPayment.set(true);
      fixture.detectChanges();
      expect(compiled.innerHTML).toContain('spinner-border');
      
      component.isSavingPayment.set(false);
      fixture.detectChanges();
      
      // Click cancel button explicitly in the modal footer
      const cancelBtn = compiled.querySelector('.modal-footer-custom .btn-light') as HTMLButtonElement;
      if (cancelBtn) cancelBtn.click();
    });

    it('should render Stripe modal and handle state', () => {
      component.showStripeModal.set(true);
      component.stripePaymentComponent.set({ id: 1, feeName: 'Tuition Fee', amount: 1000 });
      component.stripePaymentMaxAmount.set(500);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Online Payment');
      expect(compiled.textContent).toContain('Tuition Fee');
      expect(compiled.textContent).toContain('₹500');
      
      // Trigger debounceClick on Pay button
      const payButton = compiled.querySelector('.btn-primary') as HTMLButtonElement;
      if (payButton) {
        payButton.dispatchEvent(new Event('debounceClick'));
      }
      
      component.isProcessingStripe.set(true);
      fixture.detectChanges();
      expect(compiled.textContent).toContain('Processing...');
      
      // Reset processing and trigger Stripe cancel buttons
      component.isProcessingStripe.set(false);
      fixture.detectChanges();
      
      const headerCloseBtn = compiled.querySelector('.modal-header .btn-close') as HTMLButtonElement;
      if (headerCloseBtn) headerCloseBtn.click();

      // Show it again to test the other cancel button
      component.showStripeModal.set(true);
      fixture.detectChanges();

      const footerCancelBtn = compiled.querySelector('.modal-footer .btn-secondary') as HTMLButtonElement;
      if (footerCancelBtn) footerCancelBtn.click();
    });
    
    it('should render empty components list', () => {
      sessionStorage.setItem('role', 'Student');
      fixture.detectChanges();
      component.loadingSummary.set(false);
      component.selectedStudentId.set(10);
      component.feeComponents.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No components defined.');
    });

    it('should render empty payments list', () => {
      sessionStorage.setItem('role', 'Student');
      fixture.detectChanges();
      component.loadingSummary.set(false);
      component.selectedStudentId.set(10);
      component.payments.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No transaction history.');
    });
    
    it('should render empty student list for Admin', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingStudents.set(false);
      component.filteredStudentSummaries.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No students found matching filters.');
    });
    
    it('should render no student selected message', () => {
      sessionStorage.setItem('role', 'Parent');
      fixture.detectChanges();
      component.loadingSummary.set(false);
      component.selectedStudentId.set(null);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No student selected. Please select a student/child to view fee details.');
    });
  });
});
