import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, Observable, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { LoginResponse } from '../models/login-response.model';

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
                return this.handlerUnAuthrizedError(req, next)
            }

            return throwError(() => new Error('Some other error occured.'))
        })
       ); 
    }

    handlerUnAuthrizedError(req: HttpRequest<any>, next: HttpHandler) {
        let loginModel = new LoginResponse();
        loginModel.accessToken = this.auth.getToken() ?? '';
        loginModel.refreshToken = this.auth.getRefreshToken() ?? '';

        return this.auth.renewToken(loginModel)
            .pipe(
                switchMap((data: LoginResponse) => {
                    this.auth.storeRefreshToken(data.refreshToken);
                    this.auth.storeToken(data.accessToken);
                    req = req.clone({
                        setHeaders: {Authorization: `Bearer ${data.accessToken}`}
                    })
                    return next.handle(req);
                }),
                catchError((error: any) => {
                    if (error instanceof HttpErrorResponse && error.status === 401) {
                        this.router.navigate(['login'])
                    }
        
                    return throwError(() => new Error('Some other error occured.'))
                })
            );
    }
 }