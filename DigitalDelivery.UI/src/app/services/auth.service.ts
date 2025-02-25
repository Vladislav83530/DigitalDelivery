import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'

@Injectable({
  providedIn: 'root',
})
export class AuthService {
    private baseUrl: string = 'https://localhost:7207/api/auth'

    public constructor(private http: HttpClient) {}

    signUp(userRegister: any) {
        return this.http.post<any>(`${this.baseUrl}/register`, userRegister)
    }

    login(userLogin: any) {
        return this.http.post<any>(`${this.baseUrl}/login`, userLogin)
    }
}
