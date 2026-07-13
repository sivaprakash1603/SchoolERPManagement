import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subjects } from './subjects';
import { SubjectService } from '../../services/subject.service';
import { ToastService } from '../../services/toast.service';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('Subjects', () => {
  let component: Subjects;
  let fixture: ComponentFixture<Subjects>;
  let subjectServiceMock: any;
  let toastServiceMock: any;

  const mockSubjects = [
    { id: 1, subjectName: 'Math', createdAt: new Date().toISOString() },
    { id: 2, subjectName: 'Science', createdAt: new Date().toISOString() },
  ];

  beforeEach(async () => {
    subjectServiceMock = {
      getAllSubjects: vi.fn().mockReturnValue(of(mockSubjects)),
      createSubject: vi.fn().mockReturnValue(of({ id: 3, subjectName: 'History' })),
      updateSubject: vi.fn().mockReturnValue(of({})),
      deleteSubject: vi.fn().mockReturnValue(of({})),
    };

    toastServiceMock = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [Subjects, FormsModule],
      providers: [
        { provide: SubjectService, useValue: subjectServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Subjects);
    component = fixture.componentInstance;
    // Mock sessionStorage
    vi.spyOn(Storage.prototype, 'getItem').mockReturnValue('Admin');
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with isAdmin based on session storage and load subjects', () => {
      expect(component.isAdmin()).toBe(true);
      expect(subjectServiceMock.getAllSubjects).toHaveBeenCalled();
      expect(component.subjects()).toEqual(mockSubjects);
    });

    it('should handle fetchSubjects error', () => {
      subjectServiceMock.getAllSubjects.mockReturnValueOnce(throwError(() => new Error('error')));
      component.fetchSubjects();
      expect(component.error()).toBe('Failed to load subjects. Please try again later.');
      expect(component.loading()).toBe(false);
    });
  });

  describe('Computed Properties and Filtering', () => {
    it('should return all subjects if search query is empty', () => {
      component.searchQuery.set('');
      expect(component.filteredSubjects()).toEqual(mockSubjects);
    });

    it('should filter subjects based on search query', () => {
      component.searchQuery.set('sci');
      expect(component.filteredSubjects().length).toBe(1);
      expect(component.filteredSubjects()[0].subjectName).toBe('Science');
    });
  });

  describe('Modal Operations', () => {
    it('should open and close create modal', () => {
      component.openCreateModal();
      expect(component.showCreateModal()).toBe(true);
      expect(component.createForm().subjectName).toBe('');

      component.closeCreateModal();
      expect(component.showCreateModal()).toBe(false);
    });

    it('should open and close edit modal', () => {
      component.openEditModal(mockSubjects[0] as any);
      expect(component.showEditModal()).toBe(true);
      expect(component.editingSubject()).toEqual(mockSubjects[0] as any);
      expect(component.editForm().subjectName).toBe('Math');

      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
      expect(component.editingSubject()).toBeNull();
    });

    it('should open and close delete modal', () => {
      component.openDeleteModal(mockSubjects[0] as any);
      expect(component.showDeleteModal()).toBe(true);
      expect(component.deletingSubject()).toEqual(mockSubjects[0] as any);

      component.closeDeleteModal();
      expect(component.showDeleteModal()).toBe(false);
      expect(component.deletingSubject()).toBeNull();
    });
  });

  describe('CRUD Operations', () => {
    it('should show warning if subject name is empty on save', () => {
      component.createForm.set({ subjectName: '   ' });
      component.saveSubject();
      expect(toastServiceMock.warning).toHaveBeenCalledWith('Subject Name is required.');
      expect(subjectServiceMock.createSubject).not.toHaveBeenCalled();
    });

    it('should create subject successfully', () => {
      component.createForm.set({ subjectName: 'History' });
      component.saveSubject();
      expect(subjectServiceMock.createSubject).toHaveBeenCalledWith({ subjectName: 'History' });
      expect(toastServiceMock.success).toHaveBeenCalledWith('Subject created successfully!');
      expect(component.showCreateModal()).toBe(false);
      expect(subjectServiceMock.getAllSubjects).toHaveBeenCalledTimes(2); // init + after create
    });

    it('should handle create subject error', () => {
      subjectServiceMock.createSubject.mockReturnValueOnce(throwError(() => ({ error: { message: 'Server error' } })));
      component.createForm.set({ subjectName: 'History' });
      component.saveSubject();
      expect(toastServiceMock.error).toHaveBeenCalledWith('Server error');
      expect(component.isSaving()).toBe(false);
    });

    it('should show warning if subject name is empty on edit save', () => {
      component.editingSubject.set(mockSubjects[0] as any);
      component.editForm.set({ subjectName: '   ' });
      component.saveEdit();
      expect(toastServiceMock.warning).toHaveBeenCalledWith('Subject Name is required.');
      expect(subjectServiceMock.updateSubject).not.toHaveBeenCalled();
    });

    it('should update subject successfully', () => {
      component.editingSubject.set(mockSubjects[0] as any);
      component.editForm.set({ subjectName: 'Advanced Math' });
      component.saveEdit();
      expect(subjectServiceMock.updateSubject).toHaveBeenCalledWith(1, { subjectName: 'Advanced Math' });
      expect(toastServiceMock.success).toHaveBeenCalledWith('Subject updated successfully!');
      expect(component.showEditModal()).toBe(false);
      expect(subjectServiceMock.getAllSubjects).toHaveBeenCalledTimes(2);
    });

    it('should handle update subject error', () => {
      subjectServiceMock.updateSubject.mockReturnValueOnce(throwError(() => ({ error: { message: 'Update error' } })));
      component.editingSubject.set(mockSubjects[0] as any);
      component.editForm.set({ subjectName: 'Advanced Math' });
      component.saveEdit();
      expect(toastServiceMock.error).toHaveBeenCalledWith('Update error');
      expect(component.isSaving()).toBe(false);
    });

    it('should delete subject successfully', () => {
      component.deletingSubject.set(mockSubjects[0] as any);
      component.confirmDelete();
      expect(subjectServiceMock.deleteSubject).toHaveBeenCalledWith(1);
      expect(toastServiceMock.success).toHaveBeenCalledWith('Subject deleted successfully!');
      expect(component.showDeleteModal()).toBe(false);
      expect(subjectServiceMock.getAllSubjects).toHaveBeenCalledTimes(2);
    });

    it('should handle delete subject error', () => {
      subjectServiceMock.deleteSubject.mockReturnValueOnce(throwError(() => ({ error: { message: 'Delete error' } })));
      component.deletingSubject.set(mockSubjects[0] as any);
      component.confirmDelete();
      expect(toastServiceMock.error).toHaveBeenCalledWith('Delete error');
      expect(component.isDeleting()).toBe(false);
    });
    
    it('should handle operations gracefully if subject is null', () => {
      component.editingSubject.set(null);
      component.saveEdit();
      expect(subjectServiceMock.updateSubject).not.toHaveBeenCalled();
      
      component.deletingSubject.set(null);
      component.confirmDelete();
      expect(subjectServiceMock.deleteSubject).not.toHaveBeenCalled();
    });
  });

  describe('DOM Interactions and HTML Coverage', () => {
    it('should trigger search input change', async () => {
      const searchInput = fixture.nativeElement.querySelector('input[placeholder*="Search"]');
      expect(searchInput).toBeTruthy();
      searchInput.value = 'Math';
      searchInput.dispatchEvent(new Event('input'));
      fixture.detectChanges();
      await fixture.whenStable();
      expect(component.searchQuery()).toBe('Math');
    });

    it('should trigger fetchSubjects on refresh button click', async () => {
      const refreshBtn = fixture.nativeElement.querySelector('button[title="Refresh"]');
      expect(refreshBtn).toBeTruthy();
      refreshBtn.dispatchEvent(new Event('click'));
      expect(subjectServiceMock.getAllSubjects).toHaveBeenCalled();
    });

    it('should open create modal from button click and interact with form', async () => {
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      const createBtn = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
        (b: any) => b.textContent.includes('Add New Subject')
      ) as HTMLButtonElement;
      expect(createBtn).toBeTruthy();
      createBtn.dispatchEvent(new Event('click'));
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.showCreateModal()).toBe(true);

      const input = fixture.nativeElement.querySelector('.modal-body-custom input');
      expect(input).toBeTruthy();
      input.value = 'New Subject';
      input.dispatchEvent(new Event('input'));
      
      const saveBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      expect(saveBtn).toBeTruthy();
      saveBtn.dispatchEvent(new Event('click'));
      
      expect(subjectServiceMock.createSubject).toHaveBeenCalled();
    });

    it('should trigger edit and delete modals from table row actions', async () => {
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      const actionButtons = fixture.nativeElement.querySelectorAll('tbody tr td button');
      expect(actionButtons.length).toBeGreaterThanOrEqual(2);
      
      // Click Edit
      actionButtons[0].dispatchEvent(new Event('click'));
      fixture.detectChanges();
      await fixture.whenStable();
      expect(component.showEditModal()).toBe(true);
      
      // Interact with edit form
      const editInput = fixture.nativeElement.querySelector('.modal-body-custom input');
      expect(editInput).toBeTruthy();
      editInput.value = 'Edited Subject';
      editInput.dispatchEvent(new Event('input'));
      
      const saveEditBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      expect(saveEditBtn).toBeTruthy();
      saveEditBtn.dispatchEvent(new Event('click'));
      expect(subjectServiceMock.updateSubject).toHaveBeenCalled();

      // Click Delete
      actionButtons[1].dispatchEvent(new Event('click'));
      fixture.detectChanges();
      await fixture.whenStable();
      expect(component.showDeleteModal()).toBe(true);
      
      const confirmDelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-danger');
      expect(confirmDelBtn).toBeTruthy();
      confirmDelBtn.dispatchEvent(new Event('click'));
      expect(subjectServiceMock.deleteSubject).toHaveBeenCalled();
    });
    
    it('should cover cancel buttons in modals and modal overlay clicks', async () => {
      component.openCreateModal();
      fixture.detectChanges();
      await fixture.whenStable();
      let cancelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      cancelBtn.dispatchEvent(new Event('click'));
      
      component.openEditModal(mockSubjects[0] as any);
      fixture.detectChanges();
      await fixture.whenStable();
      cancelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      cancelBtn.dispatchEvent(new Event('click'));
      
      component.openDeleteModal(mockSubjects[0] as any);
      fixture.detectChanges();
      await fixture.whenStable();
      cancelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      cancelBtn.dispatchEvent(new Event('click'));
      
      // Also close buttons on headers
      component.openCreateModal();
      fixture.detectChanges();
      await fixture.whenStable();
      let closeIcon = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      closeIcon.dispatchEvent(new Event('click'));
      
      // Overlay clicks
      component.openCreateModal();
      fixture.detectChanges();
      await fixture.whenStable();
      let overlay = fixture.nativeElement.querySelector('.modal-overlay');
      overlay.dispatchEvent(new Event('click'));
      
      component.openEditModal(mockSubjects[0] as any);
      fixture.detectChanges();
      await fixture.whenStable();
      overlay = fixture.nativeElement.querySelector('.modal-overlay');
      overlay.dispatchEvent(new Event('click'));
      
      component.openDeleteModal(mockSubjects[0] as any);
      fixture.detectChanges();
      await fixture.whenStable();
      overlay = fixture.nativeElement.querySelector('.modal-overlay');
      overlay.dispatchEvent(new Event('click'));
    });
    
    it('should cover loading state spinners in modals', async () => {
      component.openCreateModal();
      component.isSaving.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      const saveBtn1 = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      expect(saveBtn1.disabled).toBe(true);
      expect(fixture.nativeElement.querySelector('.spinner-border')).toBeTruthy();
      
      component.closeCreateModal();
      component.openEditModal(mockSubjects[0] as any);
      component.isSaving.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      const saveBtn2 = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      expect(saveBtn2.disabled).toBe(true);
      expect(fixture.nativeElement.querySelector('.spinner-border')).toBeTruthy();
      
      component.closeEditModal();
      component.openDeleteModal(mockSubjects[0] as any);
      component.isDeleting.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      const delBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-danger');
      expect(delBtn.disabled).toBe(true);
      expect(fixture.nativeElement.querySelector('.spinner-border')).toBeTruthy();
    });
    
    it('should cover global loading state', async () => {
      component.loading.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.querySelector('.loading-text')).toBeTruthy();
    });
    
    it('should cover different UI states (empty list, error, not admin)', async () => {
      component.subjects.set([]);
      component.isAdmin.set(false);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const emptyState = Array.from(fixture.nativeElement.querySelectorAll('td')).find((td: any) => td.textContent.includes('No subjects found'));
      expect(emptyState).toBeTruthy();
      
      component.error.set('Something went wrong');
      fixture.detectChanges();
      await fixture.whenStable();
      const errorAlert = fixture.nativeElement.querySelector('.alert-danger');
      expect(errorAlert).toBeTruthy();
    });
  });
});
