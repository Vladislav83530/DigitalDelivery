namespace DigitalDelivery.Application.Models.Order
{
    public class CreateOrderModel
    {
        public string RecipientPhoneNumber { get; set; }
        public AddressModel PickupAddress { get; set; }
        public AddressModel DeliveryAddress { get; set; }
        public PackageDetailsModel PackageDetails { get; set; }
    }
}