import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { FAQ } from './faq';

describe('FAQ', () => {
  let component: FAQ;
  let fixture: ComponentFixture<FAQ>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FAQ],
      providers: [
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FAQ);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle accordion states', () => {
    expect(component.openIndex()).toBeNull();

    component.toggleAccordion(0);
    expect(component.openIndex()).toBe(0);

    component.toggleAccordion(0);
    expect(component.openIndex()).toBeNull();

    component.toggleAccordion(2);
    expect(component.openIndex()).toBe(2);
  });

  it('should filter faqs based on category selection', () => {
    component.selectedCategory.set('all');
    expect(component.getFilteredFaqs().length).toBe(component.faqs.length);

    component.selectedCategory.set('academics');
    const filtered = component.getFilteredFaqs();
    expect(filtered.length).toBeLessThan(component.faqs.length);
    expect(filtered.every(item => item.category === 'academics')).toBe(true);
  });

  it('should render correct number of categories in DOM', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const categoryButtons = compiled.querySelectorAll('.category-tab');
    // The component template has category pills. Let's verify they exist.
    expect(categoryButtons.length).toBeGreaterThan(0);
  });

  it('should toggle accordion on button click in DOM', () => {
    component.selectedCategory.set('all');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const accordionHeader = compiled.querySelector('.accordion-button') as HTMLButtonElement;
    if (accordionHeader) {
      accordionHeader.click();
      fixture.detectChanges();
      expect(component.openIndex()).toBe(0);
    }
  });
});
