import { Component, inject, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService } from '../../services/ai.service';
import { ToastService } from '../../services/toast.service';
import { trigger, state, style, animate, transition } from '@angular/animations';

interface ChatMessage {
  text: string;
  sender: 'user' | 'bot';
  timestamp: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
  animations: [
    trigger('messageAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('300ms cubic-bezier(0.25, 0.8, 0.25, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class Chatbot implements OnInit, AfterViewChecked {
  @ViewChild('chatScroll') private chatScrollContainer!: ElementRef;

  aiService = inject(AiService);
  toastService = inject(ToastService);
  
  messages: ChatMessage[] = [];
  currentInput: string = '';
  isTyping: boolean = false;
  
  suggestedQuestions: string[] = [
    "What are the school timings?",
    "How do I pay fees online?",
    "When is the next parent-teacher meeting?"
  ];

  ngOnInit() {
    this.messages.push({
      text: "Hello! I'm your EduControl AI Assistant. How can I help you today?",
      sender: 'bot',
      timestamp: new Date()
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.chatScrollContainer.nativeElement.scrollTop = this.chatScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }

  sendSuggestion(suggestion: string) {
    this.currentInput = suggestion;
    this.sendMessage();
  }

  sendMessage() {
    if (!this.currentInput.trim()) return;

    const userQuery = this.currentInput;
    this.messages.push({
      text: userQuery,
      sender: 'user',
      timestamp: new Date()
    });
    this.currentInput = '';
    this.isTyping = true;

    this.aiService.chatFaq(userQuery).subscribe({
      next: (res) => {
        this.isTyping = false;
        this.messages.push({
          text: res.answer,
          sender: 'bot',
          timestamp: new Date()
        });
      },
      error: (err) => {
        this.isTyping = false;
        console.error(err);
        this.toastService.error("Failed to get response from AI Assistant.");
        this.messages.push({
          text: "I'm sorry, I'm having trouble connecting to my brain right now. Please try again later.",
          sender: 'bot',
          timestamp: new Date()
        });
      }
    });
  }
}
