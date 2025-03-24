import { inject, Injectable, PLATFORM_ID  } from '@angular/core';
import { HttpClient } from '@angular/common/http'
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { LoginResponse } from '../models/login-response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
    private baseUrl: string = 'https://localhost:7207/api/auth'
    private platformId = inject(PLATFORM_ID);

    public constructor(
        private http: HttpClient,
        private router: Router) {}

    signUp(userRegister: any) {
        return this.http.post<any>(`${this.baseUrl}/register`, userRegister)
    }

    login(userLogin: any) {
        return this.http.post<any>(`${this.baseUrl}/login`, userLogin)
    }

    signOut() {
        localStorage.clear();
        this.router.navigate(['login'])
    }

    storeToken(token: string) {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('token', token);
        }
    }

    storeRefreshToken(token: string) {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('refreshToken', token);
        }
    }

    getToken() {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem('token');
        }

        return null;
    }

    getRefreshToken() {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem('refreshToken');
        }

        return null;
    }

    isLoggedIn() {
        if (isPlatformBrowser(this.platformId)) {
            return !!localStorage.getItem('token');
        }

        return false;
    }

    renewToken(model: LoginResponse) {
        return this.http.post<any>(`${this.baseUrl}/refresh`, model)
    }
}
