import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/order/order.model';
import { OrderStatusEnum } from '../models/order/order-status.enum';
import { PaginatedResponse } from '../models/common/paginated-response.model';
import { OrderHistoryItem } from '../models/order/order-history-item.model';

@Injectable({
    providedIn: 'root'
})
export class OrderService {
    private baseUrl = 'https://localhost:7207/api/order';

    constructor(private http: HttpClient) { }

    public createOrder(orderData: any): Observable<any> {
        return this.http.post<any>(`${this.baseUrl}`, orderData);
    }

    public getOrder(id: string): Observable<Order> {
        return this.http.get<Order>(`${this.baseUrl}/${id}`);
    }

    public getUserOrders(page: number, pageSize: number): Observable<PaginatedResponse<OrderHistoryItem>> {
        return this.http.post<PaginatedResponse<OrderHistoryItem>>(`${this.baseUrl}/user-orders`, { page: page, pageSize: pageSize });
    }
} 