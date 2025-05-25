import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NgIf, NgFor, DatePipe } from '@angular/common';
import { Order } from '../../models/order/order.model';
import { MapComponent } from '../map/map.component';
import { GraphData } from '../../models/map/graph-data.model';
import { RouteService } from '../../services/route-service';
import { OrderStatusEnum } from '../../models/order/order-status.enum';
import { GraphNode } from '../../models/map/graph-node.model';
import { Address } from '../../models/create-order/address.model';
import { RobotService } from '../../services/robot.service';
import { OrderService } from '../../services/order.service';

@Component({
    selector: 'app-order',
    standalone: true,
    imports: [NgIf, NgFor, DatePipe, MapComponent],
    templateUrl: './order.component.html',
    styleUrl: './order.component.css'
})
export class OrderComponent implements OnInit, OnDestroy {
    orderData: Order | null = null;
    routeData: GraphData | null = null;
    terminalNodes: GraphNode[] = [];
    OrderStatusEnum = OrderStatusEnum;
    senderFullAddress: string = '';
    recipientFullAddress: string = '';
    robotLocation: Address | null = null;
    private locationUpdateInterval: any;
    private readonly ROBOT_ID = '0f9e2aff-490f-4d30-9c0e-35bd8e14b720';

    private readonly statusDisplayNames: Record<OrderStatusEnum, string> = {
        [OrderStatusEnum.Pending]: 'Pending Processing',
        [OrderStatusEnum.Processing]: 'In Processing',
        [OrderStatusEnum.Processed]: 'Processed',
        [OrderStatusEnum.MoveToPickupPoint]: 'Moving to Pickup Point',
        [OrderStatusEnum.MoveToDeliveryPoint]: 'Moving to Delivery Point',
        [OrderStatusEnum.Delivered]: 'Delivered',
        [OrderStatusEnum.Cancelled]: 'Cancelled'
    };

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private routeService: RouteService,
        private robotService: RobotService,
        private orderService: OrderService
    ) {
        const navigation = this.router.getCurrentNavigation();
        if (navigation?.extras.state) {
            this.orderData = navigation.extras.state['orderData'];
            
            if (this.orderData?.sender.address && this.orderData?.recipient.address) {
                this.terminalNodes = [this.orderData.sender.address, this.orderData.recipient.address];
                this.loadAddresses();
                this.loadGraph();
                this.startRobotLocationUpdates();
            }
        }
    }

    ngOnInit(): void {
        // Get order number from URL parameters
        this.route.params.subscribe(params => {
            const orderNumber = params['orderNumber'];
            if (orderNumber) {
                this.loadOrderData(orderNumber);
            } else if (!this.orderData) {
                this.router.navigate(['/search']);
            }
        });
    }

    private loadOrderData(orderNumber: string): void {
        this.orderService.getOrder(orderNumber).subscribe({
            next: (order) => {
                this.orderData = order;
                if (this.orderData?.sender.address && this.orderData?.recipient.address) {
                    this.terminalNodes = [this.orderData.sender.address, this.orderData.recipient.address];
                    this.loadAddresses();
                }
                this.loadGraph();
                this.startRobotLocationUpdates();
            },
            error: (error) => {
                console.error('Error loading order:', error);
                this.router.navigate(['/not-found']);
            }
        });
    }

    ngOnDestroy(): void {
        if (this.locationUpdateInterval) {
            clearInterval(this.locationUpdateInterval);
        }
    }

    private startRobotLocationUpdates(): void {
        // Initial update
        this.updateRobotLocation();

        // Set up interval for updates every minute
        this.locationUpdateInterval = setInterval(() => {
            this.updateRobotLocation();
        }, 60000); // 60000 ms = 1 minute
    }

    private updateRobotLocation(): void {
        console.log('Fetching robot location...');
        this.robotService.getRobotLocationAsAddress(this.ROBOT_ID).subscribe({
            next: (location) => {
                console.log('Robot location received:', location);
                this.robotLocation = location;
            },
            error: (error) => {
                console.error('Error fetching robot location:', error);
                this.robotLocation = null;
            }
        });
    }

    private async loadAddresses(): Promise<void> {
        if (this.orderData?.sender.address) {
            this.senderFullAddress = await this.getAddressFromCoordinates(
                this.orderData.sender.address.latitude,
                this.orderData.sender.address.longitude
            );
        }
        
        if (this.orderData?.recipient.address) {
            this.recipientFullAddress = await this.getAddressFromCoordinates(
                this.orderData.recipient.address.latitude,
                this.orderData.recipient.address.longitude
            );
        }
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

    private loadGraph(): void {
        this.routeService.getRoute(this.orderData?.orderNumber).subscribe({
            next: (data: GraphData) => {
                if (data.nodes && data.edges) {
                    this.routeData = data;
                }
            },
            error: (error) => {
                console.error('Error loading graph:', error);
            }
        });
    }

    getStatusDisplayName(status: OrderStatusEnum): string {
        return this.statusDisplayNames[status] || 'Невідомий статус';
    }
} 