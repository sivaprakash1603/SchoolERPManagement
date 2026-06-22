import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { catchError, throwError } from "rxjs";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const token = sessionStorage.getItem('token');
    let requestToSend = req;

    if (token) {
        requestToSend = req.clone({
            headers: req.headers.set('Authorization', `Bearer ${token}`)
        });
    }

    return next(requestToSend).pipe(
        catchError((error: HttpErrorResponse) => {
            let errorMessage = 'An unexpected error occurred.';

            if (error.error) {
                if (typeof error.error === 'string') {
                    errorMessage = error.error;
                } else if (error.error.Message) {
                    errorMessage = error.error.Message;
                } else if (error.error.message) {
                    errorMessage = error.error.message;
                } else if (error.error.errors) {
                    const validationErrors = error.error.errors;
                    const messages = [];
                    for (const key in validationErrors) {
                        if (validationErrors.hasOwnProperty(key)) {
                            messages.push(...validationErrors[key]);
                        }
                    }
                    if (messages.length > 0) {
                        errorMessage = messages.join(' ');
                    }
                }
            } else if (error.message) {
                errorMessage = error.message;
            }

            const normalizedError = new HttpErrorResponse({
                error: { message: errorMessage },
                headers: error.headers,
                status: error.status,
                statusText: error.statusText,
                url: error.url || undefined
            });

            return throwError(() => normalizedError);
        })
    );
};
