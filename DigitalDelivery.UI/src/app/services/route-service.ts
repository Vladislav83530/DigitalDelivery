import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'
@Injectable({
  providedIn: 'root',
})
export class RouteService {
    private baseUrl: string = 'https://localhost:7207/api/route'

    public constructor(private http: HttpClient) {}

    getRoute(orderId: number | undefined) {
        return this.http.get<any>(`${this.baseUrl}/${orderId}`)
    }
}
