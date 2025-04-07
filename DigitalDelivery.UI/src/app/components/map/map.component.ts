import { Component, inject, OnInit, PLATFORM_ID, forwardRef, Output, EventEmitter } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Address } from '../../models/create-order/address.model';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MapStage } from '../../models/map/map-stage.model';
import { SearchResult } from '../../models/map/search-result.model';
import { MapMarkers } from '../../models/map/map-markers.model';

@Component({
    selector: 'app-map',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './map.component.html',
    styleUrl: './map.component.css',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => MapComponent),
            multi: true
        }
    ]
})
export class MapComponent implements OnInit, ControlValueAccessor {
    @Output() addressesChange = new EventEmitter<any>();
    
    private map: any;
    private platformId = inject(PLATFORM_ID);
    private markers: MapMarkers = { pickup: null, delivery: null };
    private polygon: any;
    private polygonBounds: any;
    private onChange: any = () => {};
    private onTouched: any = () => {};

    public pickupCoordinates: Address | null = null;
    public deliveryCoordinates: Address | null = null;
    public pickupAddress: string = '';
    public deliveryAddress: string = '';
    public streetName: string = '';
    public houseNumber: string = '';
    public searchResults: SearchResult[] = [];
    public isSearching: boolean = false;
    public currentStage: 'pickup' | 'delivery' = 'pickup';
  
    private readonly POLYGON_COORDINATES: [number, number][] = [
        [50.284444, 26.857222],
        [50.289444, 26.841111],
        [50.301667, 26.856389],
        [50.297222, 26.876389],
        [50.284444, 26.857222]
    ];

    public stages: Record<'pickup' | 'delivery', MapStage> = {
        pickup: { 
            title: 'Select Pickup Location', 
            description: 'Choose where to pick up the package' 
        },
        delivery: { 
            title: 'Select Delivery Location', 
            description: 'Choose where to deliver the package' 
        }
    };

    public async ngOnInit(): Promise<void> {
        if (isPlatformBrowser(this.platformId)) {
            await this.configMap();
        }
    }

    public writeValue(value: any): void {
        if (value) {
            this.pickupCoordinates = value.pickup;
            this.deliveryCoordinates = value.delivery;
            this.pickupAddress = value.pickupAddress;
            this.deliveryAddress = value.deliveryAddress;
        }
    }

    public registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    public registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    private updateValue(): void {
        const value = {
            pickup: this.pickupCoordinates,
            delivery: this.deliveryCoordinates,
            pickupAddress: this.pickupAddress,
            deliveryAddress: this.deliveryAddress
        };
        
        this.onChange(value);
        this.onTouched();
        this.addressesChange.emit(value);
    }

    private async configMap(): Promise<void> {
        const L = await import('leaflet');
        const bounds = L.latLngBounds(this.POLYGON_COORDINATES);

        this.polygonBounds = bounds;
        
        this.map = L.map('map', {
            center: bounds.getCenter(),
            zoom: 15,
            minZoom: 14,
            maxBounds: bounds,
            maxBoundsViscosity: 1.0
        });
  
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(this.map);

        this.polygon = L.polygon(this.POLYGON_COORDINATES, {
            color: '#4285f4',
            weight: 2,
            fillColor: '#4285f4',
            fillOpacity: 0.1
        }).addTo(this.map);

        this.map.on('click', (e: any) => {
            this.handleMapClick(e.latlng);
        });
    }

    private isPointInPolygon(point: any): boolean {
        const lat = point.lat;
        const lng = point.lng;
        let inside = false;

        for (let i = 0, j = this.POLYGON_COORDINATES.length - 1; i < this.POLYGON_COORDINATES.length; j = i++) {
            const xi = this.POLYGON_COORDINATES[i][0];
            const yi = this.POLYGON_COORDINATES[i][1];
            const xj = this.POLYGON_COORDINATES[j][0];
            const yj = this.POLYGON_COORDINATES[j][1];

            const intersect = ((yi > lng) !== (yj > lng))
                && (lat < (xj - xi) * (lng - yi) / (yj - yi) + xi);
            
            if (intersect) inside = !inside;
        }

        return inside;
    }

    public async searchLocation(): Promise<void> {
        if (!this.streetName.trim()) return;

        this.isSearching = true;
        try {
            const query = this.houseNumber 
                ? `${this.streetName} ${this.houseNumber}`
                : this.streetName;

            const response = await fetch(
                `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=10&` +
                `bounded=1&viewbox=${this.polygonBounds.getWest()},${this.polygonBounds.getSouth()},${this.polygonBounds.getEast()},${this.polygonBounds.getNorth()}&` +
                `countrycodes=ua&addressdetails=1`
            );

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const results = await response.json();
            
            this.searchResults = results.filter((result: any) => {
                const point = { lat: parseFloat(result.lat), lng: parseFloat(result.lon) };
                return this.isPointInPolygon(point);
            });
        } catch (error) {
            this.searchResults = [];
        } finally {
            this.isSearching = false;
        }
    }

    private createCheckpointIcon(color: string): any {
        const L = (window as any).L;
        return L.divIcon({
            className: 'custom-div-icon',
            html: `
                <div style="
                    width: 24px;
                    height: 24px;
                    background-color: white;
                    border: 2px solid ${color};
                    border-radius: 50%;
                    position: relative;
                ">
                    <div style="
                        width: 12px;
                        height: 12px;
                        background-color: ${color};
                        border-radius: 50%;
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                    "></div>
                </div>
            `,
            iconSize: [24, 24],
            iconAnchor: [12, 12]
        });
    }

    public async selectSearchResult(result: SearchResult): Promise<void> {
        const L = (window as any).L;
        const point = L.latLng(result.lat, result.lon);
        
        if (!this.isPointInPolygon(point)) {
            alert('Selected location is outside the delivery area. Please select a location within the blue polygon.');
            return;
        }
        
        if (this.currentStage === 'pickup') {
            this.setPickupLocation(point, result);
        } 
        else if (this.currentStage === 'delivery') {
            this.setDeliveryLocation(point, result);
        }

        this.clearSearch();
        this.updateValue();
    }

    private setPickupLocation(point: any, result: SearchResult): void {
        const L = (window as any).L;
        
        if (this.markers.pickup) {
            this.map.removeLayer(this.markers.pickup);
        }
        
        this.markers.pickup = L.marker(point, {
            icon: this.createCheckpointIcon('#4285f4')
        }).addTo(this.map);

        this.pickupCoordinates = { 
            latitude: parseFloat(result.lat), 
            longitude: parseFloat(result.lon) 
        };
        this.pickupAddress = result.display_name;
        this.map.setView(point, 19);
    }

    private setDeliveryLocation(point: any, result: SearchResult): void {
        const L = (window as any).L;
        
        if (this.markers.delivery) {
            this.map.removeLayer(this.markers.delivery);
        }
        
        this.markers.delivery = L.marker(point, {
            icon: this.createCheckpointIcon('#ea4335')
        }).addTo(this.map);

        this.deliveryCoordinates = { 
            latitude: parseFloat(result.lat), 
            longitude: parseFloat(result.lon) 
        };
        this.deliveryAddress = result.display_name;
        this.map.setView(point, 19);
    }

    private clearSearch(): void {
        this.searchResults = [];
        this.streetName = '';
        this.houseNumber = '';
    }

    private async getAddressFromCoordinates(lat: number, lng: number): Promise<string> {
        try {
            const response = await fetch(
                `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`
            );
            const data = await response.json();
            if (data && data.display_name) {
                return data.display_name;
            }
            return 'Address not found';
        } catch (error) {
            return 'Address not found';
        }
    }

    private async handleMapClick(point: any): Promise<void> {
        if (!this.isPointInPolygon(point)) {
            alert('Selected location is outside the delivery area. Please select a location within the blue polygon.');
            return;
        }

        if (this.currentStage === 'pickup') {
            if (this.markers.pickup) {
                this.map.removeLayer(this.markers.pickup);
            }
            
            this.markers.pickup = (window as any).L.marker(point, {
                icon: this.createCheckpointIcon('#4285f4')
            }).addTo(this.map);

            this.pickupCoordinates = { latitude: point.lat, longitude: point.lng };
            this.pickupAddress = await this.getAddressFromCoordinates(point.lat, point.lng);
        } 
        else if (this.currentStage === 'delivery') {
            if (this.markers.delivery) {
                this.map.removeLayer(this.markers.delivery);
            }
            
            this.markers.delivery = (window as any).L.marker(point, {
                icon: this.createCheckpointIcon('#ea4335')
            }).addTo(this.map);

            this.deliveryCoordinates = { latitude: point.lat, longitude: point.lng };
            this.deliveryAddress = await this.getAddressFromCoordinates(point.lat, point.lng);
        }

        this.updateValue();
    }

    public goToStage(stage: 'pickup' | 'delivery'): void {
        if (stage === 'pickup') {
            this.currentStage = 'pickup';
        } else if (stage === 'delivery' && this.pickupCoordinates) {
            this.currentStage = 'delivery';
        }
    }

    public goToPreviousStage(): void {
        if (this.currentStage === 'delivery') {
            this.currentStage = 'pickup';
        }
    }

    public proceedToNextStage(): void {
        if (this.currentStage === 'pickup' && this.pickupCoordinates) {
            this.currentStage = 'delivery';
        }

        this.updateValue();
    }

    public resetMarkers(): void {
        if (!this.map) {
            return;
        }

        if (this.markers.pickup) {
            this.map.removeLayer(this.markers.pickup);
        }
        
        if (this.markers.delivery) {
            this.map.removeLayer(this.markers.delivery);
        }

        this.markers = { pickup: null, delivery: null };
        this.pickupCoordinates = null; 
        this.deliveryCoordinates = null;
        this.pickupAddress = '';
        this.deliveryAddress = '';
        this.searchResults = [];
        this.currentStage = 'pickup';

        this.updateValue();
    }
}

