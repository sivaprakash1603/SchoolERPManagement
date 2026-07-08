import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Students } from './students';

describe('Students', () => {
  let component: Students;
  let fixture: ComponentFixture<Students>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Students],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Students);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle modals correctly', () => {
    expect(component.showViewModal()).toBe(false);
    expect(component.showEditModal()).toBe(false);
    expect(component.showDeleteModal()).toBe(false);

    component.openViewModal({
      id: 1,
      userId: 101,
      regNo: 'S001',
      name: 'John Doe',
      className: '3A',
      status: 'Active',
      gender: 'Male',
      dateofbirth: '2015-05-10',
      admissionDate: new Date('2025-06-01'),
      email: 'john@edu.com',
      avatarUrl: '',
      classId: 1,
      bloodgroup: 'O+',
      parents: []
    });
    expect(component.showViewModal()).toBe(true);
    expect(component.selectedStudent()?.name).toBe('John Doe');

    component.closeViewModal();
    expect(component.showViewModal()).toBe(false);
  });

  it('should filter parents list correctly', () => {
    const parentList = [
      { id: 1, userId: 10, name: 'Alice Smith', email: 'alice@test.com', phonenumber: '1234567890', relation: 'Father', username: 'alice' },
      { id: 2, userId: 11, name: 'Bob Johnson', email: 'bob@test.com', phonenumber: '9876543210', relation: 'Father', username: 'bob' }
    ];
    component.allParentsList.set(parentList);
    component.parentSearchQuery.set('Alice');

    const result = component.filteredParents();
    expect(result.length).toBe(1);
    expect(result[0].name).toBe('Alice Smith');
  });

  it('should toggle parent selection correctly', () => {
    component.selectedParents.set([]);
    
    // Add parent
    component.toggleParentSelection(5, 'Chris Evans');
    expect(component.isParentSelected(5)).toBe(true);
    expect(component.selectedParents().length).toBe(1);
    expect(component.selectedParents()[0].name).toBe('Chris Evans');

    // Update parent relation
    component.updateParentRelation(5, 'Mother');
    expect(component.selectedParents()[0].relation).toBe('Mother');

    // Remove parent (toggle off)
    component.toggleParentSelection(5, 'Chris Evans');
    expect(component.isParentSelected(5)).toBe(false);
    expect(component.selectedParents().length).toBe(0);
  });
});
