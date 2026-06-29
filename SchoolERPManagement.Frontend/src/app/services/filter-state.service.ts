import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FilterStateService {
  constructor() {}

  saveState(componentName: string, state: any): void {
    try {
      sessionStorage.setItem(`filter_state_${componentName}`, JSON.stringify(state));
    } catch (e) {
      console.warn('Could not save filter state to sessionStorage', e);
    }
  }

  getState(componentName: string): any {
    try {
      const data = sessionStorage.getItem(`filter_state_${componentName}`);
      return data ? JSON.parse(data) : null;
    } catch (e) {
      console.warn('Could not read filter state from sessionStorage', e);
      return null;
    }
  }
}
