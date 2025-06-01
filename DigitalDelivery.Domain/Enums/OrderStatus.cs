namespace DigitalDelivery.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending,
        Processing,
        Processed,
        MoveToPickupPoint,
        MoveToDeliveryPoint,
        Delivered,
        Cancelled
    }
}