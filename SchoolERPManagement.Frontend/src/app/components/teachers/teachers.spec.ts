import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Teachers } from './teachers';

describe('Teachers', () => {
  let component: Teachers;
  let fixture: ComponentFixture<Teachers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Teachers],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Teachers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should build query requests correctly', () => {
    component.searchQuery.set('Smith');
    component.status.set('Active');
    const req = component.buildQueryRequest();
    expect(req.searchQuery).toBe('Smith');
    expect(req.status).toBe('Active');
  });

  it('should toggle modals and forms correctly', () => {
    expect(component.showViewModal()).toBe(false);
    expect(component.showEditModal()).toBe(false);

    const dummyTeacher = {
      id: 1,
      userId: 5,
      name: 'John Doe',
      username: 'john',
      phonenumber: '123456',
      joiningdate: new Date('2024-01-01'),
      qualifications: 'B.Ed',
      status: 'Active',
      profilePhotoUrl: undefined,
      email: 'john@test.com',
      avatarUrl: '',
      joinDate: '2024-01-01',
      gender: 'Male',
      dateOfBirth: '1985-05-12'
    };

    component.openViewModal(dummyTeacher);
    expect(component.showViewModal()).toBe(true);
    expect(component.selectedTeacher()).toEqual(dummyTeacher);

    component.closeViewModal();
    expect(component.showViewModal()).toBe(false);
    expect(component.selectedTeacher()).toBeNull();

    component.openEditModal(dummyTeacher);
    expect(component.showEditModal()).toBe(true);
    expect(component.editForm().name).toBe('John Doe');

    component.closeEditModal();
    expect(component.showEditModal()).toBe(false);
  });

  it('should update pagination pages correctly', () => {
    component.pageNumber.set(2);
    component.totalPages.set(5);

    component.previousPage();
    expect(component.pageNumber()).toBe(1);

    component.nextPage();
    expect(component.pageNumber()).toBe(2);

    component.changePage(4);
    expect(component.pageNumber()).toBe(4);
  });
});
