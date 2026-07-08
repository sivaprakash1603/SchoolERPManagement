import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should format time correctly', () => {
    expect(component.formatTime('09:30:00')).toBe('09:30 AM');
    expect(component.formatTime('14:45:00')).toBe('02:45 PM');
    expect(component.formatTime('')).toBe('');
  });

  it('should evaluate slot activity correctly', () => {
    const slotActive = { startTime: '00:00:00', endTime: '23:59:59' };
    const slotInactive = { startTime: '23:59:58', endTime: '23:59:59' };

    expect(component.isSlotActive(slotActive)).toBe(true);
    // Unless current time is exactly 23:59:58/59, this should be false:
    const now = new Date();
    const currentStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
    if (currentStr < '23:59:58') {
      expect(component.isSlotActive(slotInactive)).toBe(false);
    }
  });

  it('should evaluate if slot is over correctly', () => {
    const slotOver = { startTime: '00:00:00', endTime: '00:00:01' };
    const now = new Date();
    const currentStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
    if (currentStr > '00:00:01') {
      expect(component.isSlotOver(slotOver)).toBe(true);
    }
  });
});
