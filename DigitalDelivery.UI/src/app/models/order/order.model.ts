import { UUID } from "crypto";
import { Contact } from "./contact.model";
import { OrderStatus } from "./order-status.model";
import { Package } from "./package.model";

export interface Order {
    orderNumber: number;
    estimatedDelivery: Date;
    robotId: UUID;
    package: Package;
    orderStatuses: OrderStatus[];
    sender: Contact;
    recipient: Contact;
}