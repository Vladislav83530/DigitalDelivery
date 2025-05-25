import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'
@Injectable({
  providedIn: 'root',
})
export class MapService {
    private baseUrl: string = 'https://localhost:7207/api/map'

    public constructor(private http: HttpClient) {}

    getGraph() {
        return this.http.get<any>(`${this.baseUrl}/graph`)
    }
}
