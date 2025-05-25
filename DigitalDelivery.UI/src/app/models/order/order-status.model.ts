import { OrderStatusEnum } from "./order-status.enum";

export interface OrderStatus {
    status: OrderStatusEnum;
    dateIn: Date;
}
