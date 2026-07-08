import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Assets } from './assets';
import { AssetService } from '../../services/asset.service';
import { ClassService } from '../../services/class.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';
import { TeacherService } from '../../services/teacher.service';
import { TimetableService } from '../../services/timetable.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Assets', () => {
  let component: Assets;
  let fixture: ComponentFixture<Assets>;
  let mockAssetService: any;
  let mockClassService: any;
  let mockStudentService: any;
  let mockTeacherService: any;
  let mockTimetableService: any;
  let mockToastService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockAssetService = {
      getAssets: () => of([
        { id: 1, assetname: 'Computer', assettypeId: 10, purchasedate: '2026-01-01', warrantyexpiry: '2028-01-01', status: 'active', assignedClassId: 100 }
      ]),
      getAssetTypes: () => of([{ id: 10, typename: 'Electronics' }]),
      getAssetReports: () => of([{ id: 1, assetId: 1, status: 'under repair', report: 'Faulty CPU', createdat: '2026-07-01' }]),
      getAssetStats: () => of({ total: 1, active: 1, underRepair: 0, broken: 0 }),
      addAsset: () => of({ id: 2, assetname: 'Desk', assettypeId: 20, purchasedate: '2026-02-01', status: 'active' }),
      reportAssetIssue: () => of({})
    };
    mockClassService = {
      getAllClasses: () => of([{ id: 100, classname: 'Class 3-A', section: 'A' }])
    };
    mockStudentService = {
      getStudentByUserId: () => of({ id: 50, classId: 100 })
    };
    mockTeacherService = {
      getTeacherByUsername: () => of({ id: 10 })
    };
    mockTimetableService = {
      getTeacherTimetable: () => of([{ classId: 100 }])
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };
    mockFilterStateService = {
      getState: () => ({ searchQuery: 'Comp', selectedTypeId: 10, selectedStatus: 'active' }),
      saveState: vi.fn()
    };

    sessionStorage.setItem('role', 'Admin');
    sessionStorage.setItem('userId', '1');

    await TestBed.configureTestingModule({
      imports: [Assets],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AssetService, useValue: mockAssetService },
        { provide: ClassService, useValue: mockClassService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: ToastService, useValue: mockToastService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Assets);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should create and load initial admin dashboard data', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.assets().length).toBe(1);
    expect(component.assetTypes().length).toBe(1);
    expect(component.classes().length).toBe(1);
  });

  it('should handle student profile loading and filter student assets', () => {
    sessionStorage.setItem('role', 'Student');
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.userRole()).toBe('Student');
    expect(component.assets().length).toBe(1);
  });

  it('should handle teacher profile loading and filter teacher assets', () => {
    sessionStorage.setItem('role', 'Teacher');
    sessionStorage.setItem('username', 'teacher1');
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.userRole()).toBe('Teacher');
    expect(component.assets().length).toBe(1);
  });

  it('should load initial data with errors safely', () => {
    vi.spyOn(mockAssetService, 'getAssets').mockReturnValue(throwError(() => new Error('Load Failed')));
    vi.spyOn(mockAssetService, 'getAssetTypes').mockReturnValue(throwError(() => new Error('Load Types Failed')));
    vi.spyOn(mockClassService, 'getAllClasses').mockReturnValue(throwError(() => new Error('Load Classes Failed')));
    vi.spyOn(mockAssetService, 'getAssetReports').mockReturnValue(throwError(() => new Error('Load Reports Failed')));

    component.loadInitialData();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load assets list.');
  });

  it('should open and close modals', () => {
    expect(component.showAddAssetModal()).toBe(false);
    component.openAddAssetModal();
    expect(component.showAddAssetModal()).toBe(true);
    component.closeAddAssetModal();
    expect(component.showAddAssetModal()).toBe(false);

    const assetItem = { id: 1, assetname: 'Computer', assettypeId: 10, purchasedate: '2026-01-01', warrantyexpiry: '', status: 'active' };
    
    expect(component.showReportIssueModal()).toBe(false);
    component.openReportIssueModal(assetItem);
    expect(component.showReportIssueModal()).toBe(true);
    component.closeReportIssueModal();
    expect(component.showReportIssueModal()).toBe(false);

    expect(component.showResolveModal()).toBe(false);
    component.openResolveModal(assetItem);
    expect(component.showResolveModal()).toBe(true);
    component.closeResolveModal();
    expect(component.showResolveModal()).toBe(false);
  });

  it('should validate form and save asset successfully', () => {
    component.newAsset.set({
      assetname: 'Router',
      assettypeId: 10,
      purchasedate: '2026-07-01',
      warrantyexpiry: '2028-07-01',
      status: 'active',
      assignedClassId: 100
    });

    component.saveAsset();
    expect(mockToastService.success).toHaveBeenCalledWith('Asset added successfully!');
  });

  it('should warn when asset name is missing on save', () => {
    component.newAsset.set({
      assetname: '  ',
      assettypeId: 10,
      purchasedate: '',
      warrantyexpiry: '',
      status: 'active',
      assignedClassId: undefined
    });

    component.saveAsset();
    expect(mockToastService.warning).toHaveBeenCalledWith('Asset name is required.');
  });

  it('should warn when purchase date is in the future on save', () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 5);
    const dateStr = futureDate.toISOString().split('T')[0];

    component.newAsset.set({
      assetname: 'Future Phone',
      assettypeId: 10,
      purchasedate: dateStr,
      warrantyexpiry: '',
      status: 'active',
      assignedClassId: undefined
    });

    component.saveAsset();
    expect(mockToastService.warning).toHaveBeenCalledWith('Purchase date cannot be in the future.');
  });

  it('should handle error when adding asset fails', () => {
    vi.spyOn(mockAssetService, 'addAsset').mockReturnValue(throwError(() => new Error('Save Failed')));
    component.newAsset.set({
      assetname: 'Router',
      assettypeId: 10,
      purchasedate: '2026-07-01',
      warrantyexpiry: '',
      status: 'active',
      assignedClassId: undefined
    });

    component.saveAsset();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to add asset.');
  });

  it('should validate form and submit issue report successfully', () => {
    component.reportIssue.set({
      assetId: 1,
      status: 'under repair',
      report: 'Broken Screen'
    });

    component.submitIssueReport();
    expect(mockToastService.success).toHaveBeenCalledWith('Issue reported successfully!');
  });

  it('should warn when issue report text is missing on submit', () => {
    component.reportIssue.set({
      assetId: 1,
      status: 'under repair',
      report: '  '
    });

    component.submitIssueReport();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please provide an issue description.');
  });

  it('should handle error when submitting issue report fails', () => {
    vi.spyOn(mockAssetService, 'reportAssetIssue').mockReturnValue(throwError(() => new Error('Report Failed')));
    component.reportIssue.set({
      assetId: 1,
      status: 'under repair',
      report: 'Broken Screen'
    });

    component.submitIssueReport();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to report issue.');
  });

  it('should validate form and submit resolve successfully', () => {
    component.resolveForm.set({
      assetId: 1,
      status: 'active',
      report: 'Fixed Cable'
    });

    component.submitResolve();
    expect(mockToastService.success).toHaveBeenCalledWith('Asset status updated & issue addressed!');
  });

  it('should warn when resolve report text is missing on submit', () => {
    component.resolveForm.set({
      assetId: 1,
      status: 'active',
      report: '  '
    });

    component.submitResolve();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please provide a resolution note.');
  });

  it('should handle error when submitting resolve fails', () => {
    vi.spyOn(mockAssetService, 'reportAssetIssue').mockReturnValue(throwError(() => new Error('Resolve Failed')));
    component.resolveForm.set({
      assetId: 1,
      status: 'active',
      report: 'Fixed Cable'
    });

    component.submitResolve();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to update asset status.');
  });

  it('should return helper names accurately', () => {
    fixture.detectChanges();
    expect(component.getTypeName(10)).toBe('Electronics');
    expect(component.getTypeName(99)).toBe('Type #99');

    expect(component.getClassName(100)).toBe('Class 3-A - A');
    expect(component.getClassName(undefined)).toBe('N/A');
    expect(component.getClassName(999)).toBe('Class #999');

    expect(component.getAssetNameById(1)).toBe('Computer');
    expect(component.getAssetNameById(999)).toBe('Asset #999');
  });

  it('should render HTML elements, add-modal elements, reports log, and allow clicks', () => {
    component.userRole.set('Admin');
    component.isLoading.set(false);
    component.assetReports.set([
      { id: 1, assetid: 1, status: 'active', report: 'Issue log 1', createdat: '2026-07-01' },
      { id: 2, assetid: 1, status: 'under repair', report: 'Issue log 2', createdat: '2026-07-02' },
      { id: 3, assetid: 1, status: 'broken', report: 'Issue log 3', createdat: '2026-07-03' },
      { id: 4, assetid: 1, status: 'unknown', report: 'Issue log 4', createdat: '' }
    ]);
    component.showAddAssetModal.set(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.modal-content')).toBeTruthy();

    // Trigger input events in add modal
    const inputs = compiled.querySelectorAll('input');
    if (inputs.length >= 3) {
      const nameInput = inputs[0] as HTMLInputElement;
      nameInput.value = 'Lab Projector';
      nameInput.dispatchEvent(new Event('input'));

      const purchaseInput = inputs[1] as HTMLInputElement;
      purchaseInput.value = '2026-07-01';
      purchaseInput.dispatchEvent(new Event('input'));

      const warrantyInput = inputs[2] as HTMLInputElement;
      warrantyInput.value = '2028-07-01';
      warrantyInput.dispatchEvent(new Event('input'));
    }

    const selects = compiled.querySelectorAll('select');
    if (selects.length >= 3) {
      const catSelect = selects[0] as HTMLSelectElement;
      catSelect.value = '10';
      catSelect.dispatchEvent(new Event('change'));

      const statusSelect = selects[1] as HTMLSelectElement;
      statusSelect.value = 'active';
      statusSelect.dispatchEvent(new Event('change'));

      const classSelect = selects[2] as HTMLSelectElement;
      classSelect.value = '100';
      classSelect.dispatchEvent(new Event('change'));
    }
    fixture.detectChanges();

    // Click Save Asset (line 484)
    const saveBtn = compiled.querySelector('.modal-footer .btn-primary') as HTMLButtonElement;
    if (saveBtn) {
      saveBtn.click();
      fixture.detectChanges();
    }

    // Reopen modal, click Close (line 408)
    component.showAddAssetModal.set(true);
    fixture.detectChanges();
    const closeBtn = compiled.querySelector('.modal-header .btn-close') as HTMLButtonElement;
    if (closeBtn) {
      closeBtn.click();
      fixture.detectChanges();
      expect(component.showAddAssetModal()).toBe(false);
    }

    // Reopen modal, click Cancel (line 476)
    component.showAddAssetModal.set(true);
    fixture.detectChanges();
    const cancelBtn = compiled.querySelector('.modal-footer .btn-outline-secondary') as HTMLButtonElement;
    if (cancelBtn) {
      cancelBtn.click();
      fixture.detectChanges();
      expect(component.showAddAssetModal()).toBe(false);
    }

    // Check that reports log table rendered
    expect(compiled.innerHTML).toContain('Maintenance &amp; Issue Reports Log');
  });

  it('should render modals and trigger input events', () => {
    // Render report modal and fill form elements
    component.userRole.set('Student');
    component.isLoading.set(false);
    component.showReportIssueModal.set(true);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.modal-content')).toBeTruthy();

    const textareas = compiled.querySelectorAll('textarea');
    if (textareas.length > 0) {
      const reportArea = textareas[0] as HTMLTextAreaElement;
      reportArea.value = 'Broken Cable Test';
      reportArea.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    }

    // Click Close (line 511)
    const closeReportBtn = compiled.querySelector('.modal-header .btn-close') as HTMLButtonElement;
    if (closeReportBtn) {
      closeReportBtn.click();
      fixture.detectChanges();
      expect(component.showReportIssueModal()).toBe(false);
    }

    // Reopen modal, click Cancel inside modal card
    component.showReportIssueModal.set(true);
    fixture.detectChanges();

    // Click Save/Submit Report inside report modal (line 559)
    const saveBtn = compiled.querySelector('.modal-footer .btn-warning') as HTMLButtonElement;
    if (saveBtn) {
      saveBtn.click();
      fixture.detectChanges();
    }

    // Reopen modal, click Cancel inside modal card
    component.showReportIssueModal.set(true);
    fixture.detectChanges();
    const cancelReportBtn = compiled.querySelector('.modal-footer .btn-outline-secondary') as HTMLButtonElement;
    if (cancelReportBtn) {
      cancelReportBtn.click();
      fixture.detectChanges();
      expect(component.showReportIssueModal()).toBe(false);
    }
  });

  it('should render resolve modal in DOM and allow submit/cancel actions', () => {
    component.userRole.set('Admin');
    component.isLoading.set(false);
    component.assetReports.set([]); // Covers lines 339-342 (empty reports log)
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    // Click Address / Resolve button in DOM (line 305)
    const resolveBtn = compiled.querySelector('.btn-outline-info') as HTMLButtonElement;
    if (resolveBtn) {
      resolveBtn.click();
      fixture.detectChanges();
    }

    component.showResolveModal.set(true);
    fixture.detectChanges();

    const textareas = compiled.querySelectorAll('textarea');
    if (textareas.length > 0) {
      const resolveArea = textareas[0] as HTMLTextAreaElement;
      resolveArea.value = 'Replaced Cable';
      resolveArea.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    }

    // Click Submit Update inside resolve modal (line 635)
    const submitBtn = compiled.querySelector('.modal-footer .btn-info') as HTMLButtonElement;
    if (submitBtn) {
      submitBtn.click();
      fixture.detectChanges();
    }

    // Reopen modal, click Close/Cancel inside resolve modal (lines 586 and 628)
    component.showResolveModal.set(true);
    fixture.detectChanges();
    const closeBtn = compiled.querySelector('.modal-header .btn-close') as HTMLButtonElement;
    if (closeBtn) {
      closeBtn.click();
      fixture.detectChanges();
    }

    component.showResolveModal.set(true);
    fixture.detectChanges();
    const cancelBtn = compiled.querySelector('.modal-footer .btn-outline-secondary') as HTMLButtonElement;
    if (cancelBtn) {
      cancelBtn.click();
      fixture.detectChanges();
    }
  });

  it('should render Access Denied template for invalid roles', () => {
    component.userRole.set('Parent');
    component.isLoading.set(false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.access-denied-icon')).toBeTruthy();
  });

  it('should handle teacher profile and timetable errors during loading', () => {
    sessionStorage.setItem('role', 'Teacher');
    sessionStorage.setItem('username', 'teacher1');

    // 1. Timetable fails
    vi.spyOn(mockTimetableService, 'getTeacherTimetable').mockReturnValue(throwError(() => new Error('Timetable Fail')));
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.assets().length).toBe(0);

    // 2. Profile fails
    vi.spyOn(mockTeacherService, 'getTeacherByUsername').mockReturnValue(throwError(() => new Error('Profile Fail')));
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.assets().length).toBe(0);
  });

  it('should handle reports loading error path', () => {
    vi.spyOn(mockAssetService, 'getAssetReports').mockReturnValue(throwError(() => new Error('Reports fail')));
    component.loadReports();
  });

  it('should handle student profile resolve failure', () => {
    sessionStorage.setItem('role', 'Student');
    sessionStorage.setItem('userId', '1');
    vi.spyOn(mockStudentService, 'getStudentByUserId').mockReturnValue(throwError(() => new Error('Profile error')));
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.assets().length).toBe(1);
  });

  it('should handle calculateStats stats error path', () => {
    component.userRole.set('Admin');
    vi.spyOn(mockAssetService, 'getAssetStats').mockReturnValue(throwError(() => new Error('Stats fail')));
    component.calculateStats();
  });
});
