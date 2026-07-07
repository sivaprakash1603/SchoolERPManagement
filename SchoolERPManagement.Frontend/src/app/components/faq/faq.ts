import { Component, signal, OnInit, inject } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ThemeService } from '../../services/theme';

interface FAQItem {
  question: string;
  answer: string;
  category: string;
}

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './faq.html',
  styleUrl: './faq.css',
})
export class FAQ implements OnInit {
  themeService = inject(ThemeService);
  isLoggedIn = signal(false);

  // Accordion active state index signal
  openIndex = signal<number | null>(null);
  selectedCategory = signal<string>('all');

  categories = [
    { id: 'all', name: 'All Topics' },
    { id: 'account', name: 'Account & Access' },
    { id: 'academics', name: 'Academics & Timetable' },
    { id: 'billing', name: 'Fees & Payments' },
  ];

  faqs: FAQItem[] = [
    {
      question: 'How do I sign in as a Parent, Teacher, or Student?',
      answer: 'Your account credentials are provided by the school administration. Depending on your role, your username starts with a specific prefix: "ST" followed by numbers for Students, "T" for Teachers, and "P" for Parents. Enter your credentials on the login screen to enter your customized portal.',
      category: 'account',
    },
    {
      question: 'Can I reset my password if I forget it?',
      answer: 'Yes! Click on the "Forgot Password?" link on the sign-in page. Enter your registered email address, and we will send you a secure link to reset your password. If you do not receive the email, please contact the administration desk.',
      category: 'account',
    },
    {
      question: 'How does the timetable validation prevent scheduling conflicts?',
      answer: 'Our smart Timetable Engine automatically validates teacher assignments. If you assign a teacher to multiple classes at the same time, or exceed the maximum allowed periods per day, the system will highlight the conflict and block saving until it is resolved.',
      category: 'academics',
    },
    {
      question: 'How are grades and exam sheets locked after submission?',
      answer: 'Once teachers submit final grades for an exam session, the system locks editing privileges to ensure grading integrity. If adjustments are required after submission, a formal change request must be approved by the principal or head administrator.',
      category: 'academics',
    },
    {
      question: 'What online payment methods are supported for school fees?',
      answer: 'We support all major credit/debit cards and net banking transfers. Once a payment is completed successfully, the system immediately generates a downloadable receipt for your records under the "Fee Management" tab.',
      category: 'billing',
    },
    {
      question: 'Can parents monitor multiple children using a single login?',
      answer: 'Absolutely. Parents can link multiple student profiles to their single account. Once logged in, a dropdown appears at the top of the page allowing you to toggle between profiles to monitor grades, attendance, and assignments for each child.',
      category: 'account',
    },
  ];

  private route = inject(ActivatedRoute);

  ngOnInit() {
    const fromLanding = this.route.snapshot.queryParamMap.get('from') === 'landing';
    if (fromLanding) {
      this.isLoggedIn.set(false);
    } else {
      this.isLoggedIn.set(!!sessionStorage.getItem('token'));
    }
  }

  toggleAccordion(index: number) {
    if (this.openIndex() === index) {
      this.openIndex.set(null);
    } else {
      this.openIndex.set(index);
    }
  }

  getFilteredFaqs() {
    const cat = this.selectedCategory();
    if (cat === 'all') return this.faqs;
    return this.faqs.filter(item => item.category === cat);
  }
}
