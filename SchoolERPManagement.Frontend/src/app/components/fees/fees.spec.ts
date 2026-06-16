import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Fees } from './fees';

describe('Fees', () => {
  let component: Fees;
  let fixture: ComponentFixture<Fees>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Fees],
    }).compileComponents();

    fixture = TestBed.createComponent(Fees);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
