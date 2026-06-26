import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = sessionStorage.getItem('token');

  if (token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      const payload = JSON.parse(jsonPayload);
      
      // Check expiration
      if (payload.exp) {
        const currentTime = Math.floor(Date.now() / 1000);
        if (payload.exp > currentTime) {
          return true; // Token is valid
        }
      }
    } catch (e) {
      console.error('Invalid token format in auth guard', e);
    }
  }

  // Not logged in or token expired/invalid, redirect to login page
  sessionStorage.clear(); // Clear any invalid/expired session data
  router.navigate(['/login']);
  return false;
};
