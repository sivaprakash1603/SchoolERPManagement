import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Fees } from './fees';
import { FeeService, FeeComponentDTO } from '../../services/fee.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ClassService } from '../../services/class.service';
import { ToastService } from '../../services/toast.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Fees', () => {
  let component: Fees;
  let fixture: ComponentFixture<Fees>;
  let mockFeeService: any;
  let mockToastService: any;
  let mockFilterStateService: any;
  let mockAcademicYearService: any;
  let mockClassService: any;

  beforeEach(async () => {
    mockFeeService = {
      getClassFeeSummaries: () => of([]),
      addFeeStructure: () => of({}),
      payFees: () => of({}),
      createCheckoutSession: () => of({ url: 'https://stripe-redirect-url' })
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn()
    };

    mockFilterStateService = {
      getState: vi.fn().mockReturnValue({
        selectedAcademicYearId: 1,
        selectedClassId: 2,
        selectedStudentId: 3,
        feeStatusFilter: 'pending',
        studentSearchQuery: 'John'
      }),
      saveState: vi.fn()
    };

    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 1, name: '2026-2027', isCurrent: true }])
    };

    mockClassService = {
      getAllClasses: () => of([{ id: 2, classname: 'Grade 10', section: 'A' }])
    };

    await TestBed.configureTestingModule({
      imports: [Fees],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: FeeService, useValue: mockFeeService },
        { provide: ToastService, useValue: mockToastService },
        { provide: FilterStateService, useValue: mockFilterStateService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ClassService, useValue: mockClassService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Fees);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create the component and load saved filters', () => {
    expect(component).toBeTruthy();
    expect(mockFilterStateService.getState).toHaveBeenCalledWith('fees');
    expect(component.selectedAcademicYearId()).toBe(1);
    expect(component.selectedClassId()).toBe(2);
    expect(component.selectedStudentId()).toBe(3);
    expect(component.feeStatusFilter()).toBe('pending');
    expect(component.studentSearchQuery()).toBe('John');
  });

  it('should validate and warn when saving fee structure with missing classId', () => {
    component.structureForm.set({
      classId: null,
      feeName: 'Tuition Fee',
      totalAmount: 1000,
      dueDate: '2026-07-06'
    });
    component.saveFeeStructure();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please select a class.');
  });

  it('should validate and warn when saving fee structure with empty name', () => {
    component.structureForm.set({
      classId: 2,
      feeName: '   ',
      totalAmount: 1000,
      dueDate: '2026-07-06'
    });
    component.saveFeeStructure();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please enter a Fee Name.');
  });

  it('should validate and warn when saving fee structure with invalid amount', () => {
    component.structureForm.set({
      classId: 2,
      feeName: 'Tuition Fee',
      totalAmount: 0,
      dueDate: '2026-07-06'
    });
    component.saveFeeStructure();
    expect(mockToastService.warning).toHaveBeenCalledWith('Amount must be greater than zero.');
  });

  it('should validate and warn when recording payment with invalid amount', () => {
    component.selectedStudentId.set(3);
    component.paymentForm.set({
      feeStructureId: 10,
      amountPaid: -50,
      paymentDate: '2026-07-06',
      paymentMethod: 'cash',
      transactionId: ''
    });
    component.savePayment();
    expect(mockToastService.warning).toHaveBeenCalledWith('Payment amount must be greater than zero.');
  });

  it('should validate and warn when payment amount exceeds outstanding balance', () => {
    const compMock: FeeComponentDTO = { id: 10, name: 'Tuition', amount: 500 };
    component.feeComponents.set([compMock]);
    component.payments.set([{ id: 1, studentId: 3, feeStructureId: 10, amountPaid: 200, paymentDate: '2026-07-06', paymentMethod: 'cash', transactionId: 'tx_123' }]);
    
    component.selectedStudentId.set(3);
    component.paymentForm.set({
      feeStructureId: 10,
      amountPaid: 400, // exceeds outstanding 300
      paymentDate: '2026-07-06',
      paymentMethod: 'cash',
      transactionId: ''
    });
    
    component.savePayment();
    expect(mockToastService.warning).toHaveBeenCalledWith('Payment amount exceeds the outstanding balance of ₹300.');
  });

  it('should pre-fill payment modal when opened for a specific row component', () => {
    const compMock: FeeComponentDTO = { id: 10, name: 'Tuition', amount: 500 };
    component.feeComponents.set([compMock]);
    component.payments.set([{ id: 1, studentId: 3, feeStructureId: 10, amountPaid: 150, paymentDate: '2026-07-06', paymentMethod: 'cash', transactionId: 'tx_123' }]);
    
    component.openPaymentModal(compMock);
    
    const form = component.paymentForm();
    expect(form.feeStructureId).toBe(10);
    expect(form.amountPaid).toBe(350); // 500 - 150
  });

  it('should open Stripe modal and checkout successfully', () => {
    const sessionSpy = vi.spyOn(mockFeeService, 'createCheckoutSession').mockReturnValue(of({ url: 'https://stripe-redirect-url' }));
    const originalLocation = window.location;
    delete (window as any).location;
    window.location = { ...originalLocation, href: '' } as any;

    const compMock: FeeComponentDTO = { id: 10, name: 'Tuition', amount: 500 };
    component.feeComponents.set([compMock]);
    component.selectedStudentId.set(3);

    component.openStripeModal(compMock);
    expect(component.showStripeModal()).toBe(true);
    expect(component.stripePaymentAmount()).toBe(500);

    component.confirmStripePayment();
    expect(sessionSpy).toHaveBeenCalled();
    expect(window.location.href).toBe('https://stripe-redirect-url');

    (window as any).location = originalLocation;
  });
});
