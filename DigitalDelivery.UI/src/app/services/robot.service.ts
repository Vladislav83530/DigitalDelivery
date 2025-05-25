import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RobotLocation } from '../models/robot/robot-location.model';
import { Address } from '../models/create-order/address.model';

@Injectable({
    providedIn: 'root'
})
export class RobotService {
    private baseUrl = 'https://localhost:7207/api/robot';

    constructor(private http: HttpClient) { }

    public getRobotLocation(robotId: string): Observable<RobotLocation> {
        return this.http.get<RobotLocation>(`${this.baseUrl}/${robotId}/location`);
    }

    public getRobotLocationAsAddress(robotId: string): Observable<Address> {
        return new Observable<Address>(observer => {
            this.getRobotLocation(robotId).subscribe({
                next: (location) => {
                    observer.next({
                        latitude: location.latitude,
                        longitude: location.longitude
                    });
                    observer.complete();
                },
                error: (error) => {
                    console.error('Error getting robot location:', error);
                    observer.error(error);
                }
            });
        });
    }
} 