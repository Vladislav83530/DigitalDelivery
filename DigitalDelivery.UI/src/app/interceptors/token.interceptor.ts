import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, Observable, throwError } from 'rxjs';
import { Router } from '@angular/router';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
    constructor (
        private auth: AuthService,
        private router: Router) {}
    
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
       const token = this.auth.getToken();

       if (token) {
        req = req.clone({
            setHeaders: {Authorization: `Bearer ${token}`}
        })
       }

       return next.handle(req).pipe(
        catchError((error: any) => {
            if (error instanceof HttpErrorResponse && error.status === 401) {
                this.router.navigate(['login'])
            }

            return throwError(() => new Error('Some other error occured.'))
        })
       ); 
    }
 }