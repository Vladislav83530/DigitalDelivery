import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class OrderService {
    private baseUrl = 'https://localhost:7207/api/order';

    constructor(private http: HttpClient) { }

    public createOrder(orderData: any): Observable<any> {
        return this.http.post<any>(`${this.baseUrl}`, orderData);
    }
} 