import { Component, inject, OnInit, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService } from '../../services/ai.service';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../environments/environment';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { Pipe, PipeTransform } from '@angular/core';
import { marked } from 'marked';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';
    return marked.parse(value) as string;
  }
}

interface ChatMessage {
  text: string;
  sender: 'user' | 'bot';
  timestamp: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, MarkdownPipe],
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
  cdr = inject(ChangeDetectorRef);
  
  messages: ChatMessage[] = [];
  currentInput: string = '';
  isTyping: boolean = false;
  
  suggestedQuestions: string[] = [
    "What are the school timings?",
    "How do I pay fees online?",
    "When is the next parent-teacher meeting?"
  ];

  ngOnInit() {
    const savedHistory = localStorage.getItem('chatbot_history');
    if (savedHistory) {
      try {
        const parsed = JSON.parse(savedHistory);
        this.messages = parsed.map((m: any) => ({
          ...m,
          timestamp: new Date(m.timestamp)
        }));
      } catch (e) {
        console.error('Failed to parse chat history', e);
        this.initDefaultMessage();
      }
    } else {
      this.initDefaultMessage();
    }
  }

  initDefaultMessage() {
    this.messages = [{
      text: "Hello! I'm your EduControl AI Assistant. How can I help you today?",
      sender: 'bot',
      timestamp: new Date()
    }];
    this.saveHistory();
  }

  saveHistory() {
    localStorage.setItem('chatbot_history', JSON.stringify(this.messages));
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
    this.saveHistory();
    this.currentInput = '';
    this.isTyping = true;

    const role = sessionStorage.getItem('role') || 'Guest';
    const token = sessionStorage.getItem('token');
    
    // Add a placeholder bot message
    const botMsg: ChatMessage = {
      text: '',
      sender: 'bot',
      timestamp: new Date()
    };
    this.messages.push(botMsg);
    this.saveHistory();

    fetch(`${environment.aiApiUrl}/chat/faq/stream`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ query: userQuery, role: role })
    }).then(async (response) => {
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        
        this.isTyping = false; // Stop typing indicator as soon as stream starts
        this.cdr.detectChanges();
        
        const reader = response.body?.getReader();
        const decoder = new TextDecoder();

        if (reader) {
            while (true) {
                const { done, value } = await reader.read();
                if (done) {
                    this.saveHistory();
                    break;
                }
                botMsg.text += decoder.decode(value, { stream: true });
                this.cdr.detectChanges();
                this.scrollToBottom();
            }
        }
    }).catch((err) => {
        this.isTyping = false;
        console.error(err);
        this.toastService.error("Failed to get response from AI Assistant.");
        if (!botMsg.text) {
            botMsg.text = "I'm sorry, I'm having trouble connecting to my brain right now. Please try again later.";
        }
        this.saveHistory();
        this.cdr.detectChanges();
    });
  }
}
