import { OrderStatusEnum } from './order-status.enum';

export interface OrderHistoryItem {
    orderNumber: string;
    status: OrderStatusEnum;
    createdAt: Date;
    deliveredAt?: Date;
} 