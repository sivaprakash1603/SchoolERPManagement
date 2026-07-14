import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { catchError, throwError, timeout, TimeoutError, of } from "rxjs";
import { environment } from "../environments/environment";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const token = sessionStorage.getItem('token');
    let requestToSend = req;

    if (token) {
        requestToSend = req.clone({
            headers: req.headers.set('Authorization', `Bearer ${token}`)
        });
    }

    // Set a reasonable timeout of 12 seconds for all normal API requests
    // AI API requests (RAG, NLQ) are highly CPU-bound and can take much longer, so we bypass this timeout.
    let stream = next(requestToSend);
    
    if (!req.url.includes(environment.aiApiUrl)) {
        stream = stream.pipe(timeout(12000));
    }

    return stream.pipe(
        catchError((error: any) => {
            let errorMessage = 'An unexpected error occurred.';

            if (error instanceof TimeoutError || error.name === 'TimeoutError') {
                errorMessage = 'Connection timed out. The server is taking too long to respond. Please try again.';
                
                const timeoutError = new HttpErrorResponse({
                    error: { message: errorMessage },
                    status: 408,
                    statusText: 'Request Timeout',
                    url: req.url
                });
                return throwError(() => timeoutError);
            }

            if (error instanceof HttpErrorResponse) {
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
