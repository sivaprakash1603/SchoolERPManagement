import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: number;
  message: string;
  type: 'success' | 'danger' | 'info' | 'warning';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts = signal<ToastMessage[]>([]);
  private nextId = 0;

  show(message: string, type: 'success' | 'danger' | 'info' | 'warning' = 'info') {
    const id = this.nextId++;
    this.toasts.update(list => [...list, { id, message, type }]);

    // Auto-remove after 4 seconds
    setTimeout(() => {
      this.remove(id);
    }, 4000);
  }

  success(message: string) {
    this.show(message, 'success');
  }

  error(message: string) {
    this.show(message, 'danger');
  }

  warning(message: string) {
    this.show(message, 'warning');
  }

  info(message: string) {
    this.show(message, 'info');
  }

  remove(id: number) {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}
