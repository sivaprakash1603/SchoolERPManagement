import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Documents } from './documents';

describe('Documents', () => {
  let component: Documents;
  let fixture: ComponentFixture<Documents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Documents],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Documents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle modal states and form model', () => {
    expect(component.showUploadModal()).toBe(false);
    
    component.openUploadModal();
    expect(component.showUploadModal()).toBe(true);

    component.closeUploadModal();
    expect(component.showUploadModal()).toBe(false);
  });

  it('should switch teacher view mode', () => {
    component.switchTeacherViewMode('self');
    expect(component.teacherViewMode()).toBe('self');

    component.switchTeacherViewMode('verify');
    expect(component.teacherViewMode()).toBe('verify');
  });

  it('should toggle directory tabs correctly', () => {
    component.activeDirectoryTab.set('students');
    expect(component.activeDirectoryTab()).toBe('students');

    component.activeDirectoryTab.set('teachers');
    expect(component.activeDirectoryTab()).toBe('teachers');
  });
});
