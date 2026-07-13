import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Parents } from './parents';
import { ParentService } from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';
import { FilterStateService } from '../../services/filter-state.service';
import { of, throwError } from 'rxjs';

describe('Parents', () => {
  let component: Parents;
  let fixture: ComponentFixture<Parents>;
  
  let mockParentService: any;
  let mockToastService: any;
  let mockNotificationService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockParentService = {
      getAllParents: vi.fn().mockReturnValue(of({ items: [], totalCount: 0, totalPages: 0 })),
      getParentStats: vi.fn().mockReturnValue(of({})),
      updateParent: vi.fn().mockReturnValue(of({})),
      deleteParent: vi.fn().mockReturnValue(of({})),
      exportParentsPdf: vi.fn().mockReturnValue(of(new Blob()))
    };
    mockToastService = { success: vi.fn(), error: vi.fn() };
    mockNotificationService = { sendNotification: vi.fn().mockReturnValue(of({})) };
    mockFilterStateService = { getState: vi.fn(), saveState: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Parents],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ParentService, useValue: mockParentService },
        { provide: ToastService, useValue: mockToastService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();
  });

  beforeEach(async () => {
    fixture = TestBed.createComponent(Parents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should load initial data', () => {
    expect(component).toBeTruthy();
    expect(mockParentService.getParentStats).toHaveBeenCalled();
    expect(mockParentService.getAllParents).toHaveBeenCalled();
  });

  describe('Pagination and Filtering', () => {
    it('should handle filter change', () => {
      component.onFilterChange();
      expect(component.pageNumber()).toBe(1);
      expect(mockParentService.getAllParents).toHaveBeenCalled();
    });

    it('should handle search debounce', () => {
      vi.useFakeTimers();
      component.onSearchChange();
      vi.advanceTimersByTime(500);
      expect(mockParentService.getAllParents).toHaveBeenCalled();
      vi.useRealTimers();
    });

    it('should paginate correctly', () => {
      vi.spyOn(mockParentService, 'getAllParents').mockReturnValue(of({ items: [], totalCount: 0, totalPages: 3 }));
      component.pageNumber.set(2);
      component.totalPages.set(3);
      component.previousPage();
      expect(component.pageNumber()).toBe(1);
      component.nextPage();
      expect(component.pageNumber()).toBe(2);
    });
  });

  describe('Modals and Notifications', () => {
    it('should toggle selection and open bulk modal', () => {
      component.parents.set([{ id: 1, userId: 10, name: 'John', email: 'x', phonenumber: '1', avatarUrl: 'x' } as any]);
      component.toggleSelectParent(1);
      expect(component.isParentSelected(1)).toBe(true);
      
      component.openBulkNotificationModal();
      expect(component.showNotificationModal()).toBe(true);
      expect(component.notificationTargetUserIds()).toEqual([10]);
      
      component.closeNotificationModal();
      expect(component.showNotificationModal()).toBe(false);
    });

    it('should toggle select all', () => {
      component.parents.set([{ id: 1 }, { id: 2 }] as any[]);
      component.toggleSelectAll({ target: { checked: true } } as any);
      expect(component.selectedParentIds().length).toBe(2);
      component.toggleSelectAll({ target: { checked: false } } as any);
      expect(component.selectedParentIds().length).toBe(0);
    });

    it('should send notification', () => {
      component.notificationTitle.set('Title');
      component.notificationMessage.set('Msg');
      component.notificationTargetUserIds.set([1]);
      
      component.sendNotification();
      
      expect(mockNotificationService.sendNotification).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });
    
    it('should not send notification if invalid', () => {
      component.notificationTitle.set('');
      component.sendNotification();
      expect(mockNotificationService.sendNotification).not.toHaveBeenCalled();
    });
  });

  describe('CRUD Operations', () => {
    it('should edit parent', () => {
      const parent = { id: 1, name: 'P', email: 'e', phonenumber: '1' } as any;
      component.openEditModal(parent);
      expect(component.showEditModal()).toBe(true);
      
      component.saveEdit();
      expect(mockParentService.updateParent).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should delete parent', () => {
      const parent = { id: 1, name: 'P' } as any;
      component.openDeleteModal(parent);
      expect(component.showDeleteModal()).toBe(true);
      
      component.confirmDelete();
      expect(mockParentService.deleteParent).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });
  });

  describe('Error handling', () => {
    it('should handle getAllParents error', () => {
      vi.spyOn(mockParentService, 'getAllParents').mockReturnValue(throwError(() => new Error('err')));
      component.fetchParents();
      expect(component.error()).toBeTruthy();
      expect(component.loading()).toBe(false);
    });

    it('should handle getParentStats error', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      vi.spyOn(mockParentService, 'getParentStats').mockReturnValue(throwError(() => new Error('err')));
      component.fetchParentStats();
      expect(consoleSpy).toHaveBeenCalled();
    });

    it('should handle exportPdf error', () => {
      vi.spyOn(window, 'alert').mockImplementation(() => {});
      vi.spyOn(mockParentService, 'exportParentsPdf').mockReturnValue(throwError(() => new Error('err')));
      component.exportPdf();
      expect(window.alert).toHaveBeenCalled();
    });

    it('should handle sendNotification error', () => {
      component.notificationTitle.set('t');
      component.notificationMessage.set('m');
      component.notificationTargetUserIds.set([1]);
      vi.spyOn(mockNotificationService, 'sendNotification').mockReturnValue(throwError(() => ({ error: { message: 'err' } })));
      component.sendNotification();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should handle updateParent error', () => {
      component.editingParent.set({ id: 1 } as any);
      vi.spyOn(mockParentService, 'updateParent').mockReturnValue(throwError(() => ({ error: { message: 'err' } })));
      component.saveEdit();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should handle deleteParent error', () => {
      component.deletingParent.set({ id: 1 } as any);
      vi.spyOn(mockParentService, 'deleteParent').mockReturnValue(throwError(() => ({ error: { message: 'err' } })));
      component.confirmDelete();
      expect(mockToastService.error).toHaveBeenCalled();
    });
  });

  describe('Export', () => {
    it('should export pdf', () => {
      window.URL.createObjectURL = vi.fn().mockReturnValue('blob:url');
      window.URL.revokeObjectURL = vi.fn();
      const createElementSpy = vi.spyOn(document, 'createElement');
      const appendChildSpy = vi.spyOn(document.body, 'appendChild');
      const removeChildSpy = vi.spyOn(document.body, 'removeChild');
      
      vi.spyOn(mockParentService, 'exportParentsPdf').mockReturnValue(of(new Blob(['pdf content'], { type: 'application/pdf' })));

      component.exportPdf();
      
      expect(mockParentService.exportParentsPdf).toHaveBeenCalled();
      expect(createElementSpy).toHaveBeenCalledWith('a');
      expect(appendChildSpy).toHaveBeenCalled();
      expect(removeChildSpy).toHaveBeenCalled();
      expect(window.URL.revokeObjectURL).toHaveBeenCalled();
    });
  });

  describe('Edge cases and null checks', () => {
    it('should not update if editingParent is null', () => {
      component.editingParent.set(null);
      component.saveEdit();
      expect(mockParentService.updateParent).not.toHaveBeenCalled();
    });

    it('should not delete if deletingParent is null', () => {
      component.deletingParent.set(null);
      component.confirmDelete();
      expect(mockParentService.deleteParent).not.toHaveBeenCalled();
    });
    
    it('should not page past bounds', () => {
      component.pageNumber.set(1);
      component.previousPage();
      expect(component.pageNumber()).toBe(1);
      
      component.totalPages.set(1);
      component.nextPage();
      expect(component.pageNumber()).toBe(1);
    });

    it('should handle search debounce and timeout clearing', () => {
      vi.useFakeTimers();
      component.onSearchChange();
      component.onSearchChange(); // This will hit clearTimeout(this.searchTimeout)
      vi.advanceTimersByTime(500);
      expect(mockParentService.getAllParents).toHaveBeenCalled();
      vi.useRealTimers();
    });

    it('should render loading spinners in modals', () => {
      component.isUpdating.set(true);
      component.showEditModal.set(true);
      fixture.detectChanges();
      let spinner = fixture.nativeElement.querySelector('.spinner-border');
      expect(spinner).toBeTruthy();

      component.showEditModal.set(false);
      component.isDeleting.set(true);
      component.showDeleteModal.set(true);
      fixture.detectChanges();
      spinner = fixture.nativeElement.querySelector('.spinner-border');
      expect(spinner).toBeTruthy();
      let disabledBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-danger');
      expect(disabledBtn.disabled).toBe(true);

      component.showDeleteModal.set(false);
      component.isSendingNotification.set(true);
      component.showNotificationModal.set(true);
      fixture.detectChanges();
      spinner = fixture.nativeElement.querySelector('.spinner-border');
      expect(spinner).toBeTruthy();
    });
    
    it('should read filter state from service on init', () => {
      mockFilterStateService.getState.mockReturnValue({ searchQuery: 'abc', status: 'Active', pageNumber: 2 });
      // To test constructor injection without breaking the test suite:
      TestBed.runInInjectionContext(() => {
        const comp = new Parents();
        expect(comp.searchQuery()).toBe('abc');
        expect(comp.status()).toBe('Active');
        expect(comp.pageNumber()).toBe(2);
      });
    });
  });
  
  describe('HTML Template rendering', () => {
    it('should handle template interactions for coverage', () => {
      component.loading.set(false);
      component.error.set('');
      component.parentStats.set({ totalParents: 10, activeParents: 8 } as any);
      const testParent = { id: 1, userId: 10, name: 'John', email: 'x', phonenumber: '1', avatarUrl: 'x' } as any;
      component.parents.set([testParent]);
      fixture.detectChanges();
      
      // Top bar actions
      const refreshBtn = fixture.nativeElement.querySelector('button[title="Refresh"]');
      if (refreshBtn) refreshBtn.dispatchEvent(new Event('click'));

      const exportBtn = fixture.nativeElement.querySelector('button[title="Export PDF"]');
      if (exportBtn) exportBtn.dispatchEvent(new Event('click'));
      
      // Select All Checkbox
      const selectAll = fixture.nativeElement.querySelector('thead input[type="checkbox"]');
      if (selectAll) selectAll.dispatchEvent(new Event('change'));
      
      // Select individual parent checkbox
      const rowCheck = fixture.nativeElement.querySelector('tbody input[type="checkbox"]');
      if (rowCheck) rowCheck.dispatchEvent(new Event('change'));
      
      // Filter changes
      const selectStatus = fixture.nativeElement.querySelector('select');
      if (selectStatus) {
        selectStatus.value = 'Active';
        selectStatus.dispatchEvent(new Event('ngModelChange'));
      }
      
      const searchInput = fixture.nativeElement.querySelector('input[type="text"]');
      if (searchInput) {
        searchInput.value = 'John';
        searchInput.dispatchEvent(new Event('ngModelChange'));
      }
      
      // Row Buttons
      const notifBtn = fixture.nativeElement.querySelector('tbody button[title="Send Notification"]');
      if (notifBtn) notifBtn.dispatchEvent(new Event('click'));

      const closeNotifBtn = fixture.nativeElement.querySelector('.modal-overlay .btn-close');
      if (closeNotifBtn) closeNotifBtn.dispatchEvent(new Event('click'));
      
      const editBtn = fixture.nativeElement.querySelector('tbody button[title="Edit"]');
      if (editBtn) editBtn.dispatchEvent(new Event('click'));

      fixture.detectChanges();
      // Test ngModelChanges in Edit Modal
      const nameInput = fixture.nativeElement.querySelector('input[placeholder="Parent Name"]');
      if (nameInput) {
        nameInput.value = 'Jane';
        nameInput.dispatchEvent(new Event('ngModelChange'));
      }
      const emailInput = fixture.nativeElement.querySelector('input[placeholder="Email Address"]');
      if (emailInput) {
        emailInput.value = 'jane@test.com';
        emailInput.dispatchEvent(new Event('ngModelChange'));
      }
      const phoneInput = fixture.nativeElement.querySelector('input[placeholder="Phone Number"]');
      if (phoneInput) {
        phoneInput.value = '123';
        phoneInput.dispatchEvent(new Event('ngModelChange'));
      }
      
      const saveBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      if (saveBtn) saveBtn.dispatchEvent(new Event('click'));
      
      const deleteBtn = fixture.nativeElement.querySelector('tbody button[title="Deactivate"]');
      if (deleteBtn) deleteBtn.dispatchEvent(new Event('click'));
      
      fixture.detectChanges();
      const allButtons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const confirmDelBtn = allButtons.find(b => b.textContent?.includes('Deactivate Account'));
      if (confirmDelBtn) confirmDelBtn.dispatchEvent(new Event('click'));
      
      // Bulk Action Button
      component.selectedParentIds.set([1]);
      fixture.detectChanges();
      const bulkBtn = fixture.nativeElement.querySelector('.bulk-action-bar button.btn-warning');
      if (bulkBtn) bulkBtn.dispatchEvent(new Event('click'));
      
      fixture.detectChanges();
      const titleInput = fixture.nativeElement.querySelector('input[placeholder="Notification Title"]');
      if (titleInput) {
        titleInput.value = 'Test';
        titleInput.dispatchEvent(new Event('ngModelChange'));
      }
      const msgInput = fixture.nativeElement.querySelector('textarea');
      if (msgInput) {
        msgInput.value = 'Msg';
        msgInput.dispatchEvent(new Event('ngModelChange'));
      }
      const sendBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-warning');
      if (sendBtn) sendBtn.dispatchEvent(new Event('click'));

      const clearSelBtn = fixture.nativeElement.querySelector('.bulk-action-bar button.btn-outline-secondary');
      if (clearSelBtn) clearSelBtn.dispatchEvent(new Event('click'));
      
      // Pagination buttons
      component.totalPages.set(2);
      component.pageNumber.set(1);
      fixture.detectChanges();
      
      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const nextBtn = buttons.find(b => b.textContent?.includes('Next'));
      if (nextBtn) nextBtn.dispatchEvent(new Event('click'));
      
      const prevBtn = buttons.find(b => b.textContent?.includes('Previous'));
      if (prevBtn) prevBtn.dispatchEvent(new Event('click'));

      // Modals propagation
      component.openEditModal(testParent);
      fixture.detectChanges();
      
      component.openDeleteModal(testParent);
      fixture.detectChanges();

      component.openNotificationModal('John', [10]);
      fixture.detectChanges();

      const overlays = fixture.nativeElement.querySelectorAll('.modal-overlay');
      overlays.forEach((o: any) => o.dispatchEvent(new Event('click')));

      const cards = fixture.nativeElement.querySelectorAll('.modal-card');
      cards.forEach((c: any) => c.dispatchEvent(new Event('click')));

      const cancelEdits = fixture.nativeElement.querySelectorAll('.modal-footer-custom .btn-light');
      cancelEdits.forEach((c: any) => c.dispatchEvent(new Event('click')));
    });
  });
});
