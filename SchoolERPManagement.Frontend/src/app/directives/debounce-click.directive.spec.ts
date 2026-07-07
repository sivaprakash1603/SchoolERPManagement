import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DebounceClickDirective } from './debounce-click.directive';

@Component({
  template: `
    <button appDebounceClick [debounceTime]="100" (debounceClick)="onClick()">Click Me</button>
  `,
  imports: [DebounceClickDirective],
  standalone: true
})
class TestComponent {
  clickCount = 0;
  onClick() {
    this.clickCount++;
  }
}

describe('DebounceClickDirective', () => {
  let component: TestComponent;
  let fixture: ComponentFixture<TestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(TestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should trigger click immediately and throttle subsequent clicks within 100ms', async () => {
    const button = fixture.nativeElement.querySelector('button');
    
    // First click
    button.click();
    fixture.detectChanges();
    expect(component.clickCount).toBe(1);

    // Consecutive clicks should be ignored
    button.click();
    button.click();
    fixture.detectChanges();
    expect(component.clickCount).toBe(1);

    // Wait for debounce time to pass
    await new Promise(resolve => setTimeout(resolve, 150));

    // Click again after 100ms
    button.click();
    fixture.detectChanges();
    expect(component.clickCount).toBe(2);
  });
});
