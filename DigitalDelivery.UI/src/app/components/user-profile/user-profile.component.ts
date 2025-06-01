import { Component, OnInit } from '@angular/core';
import { NgFor, DatePipe, NgClass, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { OrderStatusEnum } from '../../models/order/order-status.enum';
import { OrderService } from '../../services/order.service';
import { UserProfile } from '../../models/user/user-profile.model';
import { OrderHistoryItem } from '../../models/order/order-history-item.model';
import { PaginatedResponse } from '../../models/common/paginated-response.model';
import { AccountService } from '../../services/account.service';
import { PhoneFormatPipe } from '../../pipes/phone-format.pipe';

@Component({
    selector: 'app-user-profile',
    standalone: true,
    imports: [NgFor, DatePipe, NgClass, NgIf, PhoneFormatPipe],
    templateUrl: './user-profile.component.html',
    styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
    userProfile: UserProfile = {
        fullName: '',
        email: '',
        phoneNumber: ''
    };

    orderHistory: OrderHistoryItem[] = [];
    currentPage: number = 1;
    pageSize: number = 5;
    totalItems: number = 0;
    totalPages: number = 0;
    isLoadingProfile: boolean = false;
    isLoadingOrders: boolean = false;

    OrderStatusEnum = OrderStatusEnum;

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
        private orderService: OrderService,
        private accountService: AccountService
    ) {}

    ngOnInit(): void {
        this.loadAcountData();
        this.loadOrderHistory();
    }

    loadOrderHistory(): void {
        this.isLoadingOrders = true;
        this.orderService.getUserOrders(this.currentPage, this.pageSize).subscribe({
            next: (response: PaginatedResponse<OrderHistoryItem>) => {
                this.orderHistory = response.items;
                this.totalItems = response.totalCount;
                this.totalPages = response.totalPages;
                this.isLoadingOrders = false;
            },
            error: (error) => {
                console.error('Error loading order history:', error);
                this.isLoadingOrders = false;
            }
        });
    }

    loadAcountData(): void {
        this.isLoadingProfile = true;
        this.accountService.getAccount().subscribe({
            next: (response: UserProfile) => {
                this.userProfile = response;
                this.isLoadingProfile = false;
            },
            error: (error) => {
                console.error('Error loading user data:', error);
                this.isLoadingProfile = false;
            }
        });
    }

    onPageChange(page: number): void {
        this.currentPage = page;
        this.loadOrderHistory();
    }

    getStatusDisplayName(status: OrderStatusEnum): string {
        return this.statusDisplayNames[status] || 'Unknown Status';
    }

    navigateToOrder(orderNumber: string): void {
        this.router.navigate(['/order', orderNumber]);
    }
}
