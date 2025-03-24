import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'
@Injectable({
  providedIn: 'root',
})
export class AccountService {
    private baseUrl: string = 'https://localhost:7207/api/account'

    public constructor(private http: HttpClient) {}

    getAccount() {
        return this.http.get<any>(`${this.baseUrl}`)
    }
}
