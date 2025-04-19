using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Order;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.EF;
using DigitalDelivery.Infrastructure.Queues;
using Microsoft.Extensions.Options;

namespace DigitalDelivery.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly PackageRestrictionSettings _packageRestrictionSettings;
        private readonly MapSettings _mapSettings;
        private readonly BaseDeliverySettings _baseDeliverySettings;
        private readonly IUserService _userService;
        private readonly IOrderQueue _orderQueue;

        public OrderService(
            AppDbContext context,
            IUserService userService,
            IOrderQueue orderQueue,
            IOptions<PackageRestrictionSettings> packageRestrictionSettings,
            IOptions<MapSettings> mapSettings,
            IOptions<BaseDeliverySettings> baseDeliverySettings)
        {
            _context = context;
            _userService = userService;
            _orderQueue = orderQueue;
            _packageRestrictionSettings = packageRestrictionSettings.Value;
            _mapSettings = mapSettings.Value;
            _baseDeliverySettings = baseDeliverySettings.Value;
        }

        public async Task<Result<string>> CreateAsync(CreateOrderModel model)
        {
            var sender = _userService.GetCurrentUser();
            var recipient = _userService.GetUserByPhoneNumber(model.RecipientPhoneNumber);

            var result = ValidateCreatingOrder(recipient, model);
            if (!result.Success)
            {
                return result;
            }

            var orderEntity = CreateOrderEntity(sender, recipient, model);
            await _context.Orders.AddAsync(orderEntity);
            await _context.SaveChangesAsync();

            await _orderQueue.EnqueueAsync(orderEntity);

            return new Result<string>(true);
        }

        public async Task<Result<string>> ChangeStatusAsync(Guid id, OrderStatus status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return new Result<string>(false, "Order not found.");
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        private Result<string> ValidateCreatingOrder(User recipient, CreateOrderModel model)
        {
            if (recipient == null)
            {
                return new Result<string>(false, $"Recipient with phone number {model.RecipientPhoneNumber} is not found.");
            }

            if (model.PackageDetails.WeightKg > _packageRestrictionSettings.MaxWeightKg)
            {
                return new Result<string>(false, $"The maximum allowed weight is {_packageRestrictionSettings.MaxWeightKg} kg.");
            }

            if (model.PackageDetails.WidthCm > _packageRestrictionSettings.MaxWidthCm ||
                model.PackageDetails.HeightCm > _packageRestrictionSettings.MaxHeightCm ||
                model.PackageDetails.DepthCm > _packageRestrictionSettings.MaxDepthCm)
            {
                return new Result<string>(false, $"The maximum allowed dimensions are {_packageRestrictionSettings.MaxWidthCm}x{_packageRestrictionSettings.MaxHeightCm}x{_packageRestrictionSettings.MaxDepthCm} cm.");
            }

            if (!IsPointInConfiguredArea(model.PickupAddress.Latitude, model.PickupAddress.Longitude))
            {
                return new Result<string>(false, "Pickup address is outside the allowed area.");
            }

            if (!IsPointInConfiguredArea(model.DeliveryAddress.Latitude, model.DeliveryAddress.Longitude))
            {
                return new Result<string>(false, "Delivery address is outside the allowed area.");
            }

            return new Result<string>(true);
        }

        private bool IsPointInConfiguredArea(double latitude, double longitude)
        {
            var polygon = _mapSettings.AreaPolygonCoordinates
                .Select(coord => (Lat: coord.Latitude, Lon: coord.Longitude))
                .ToList();

            return DeliveryHelper.IsPointInPolygon(latitude, longitude, polygon);
        }

        private Order CreateOrderEntity(User sender, User recipient, CreateOrderModel model)
        {
            return new Order
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                PickupAddress = new Address
                {
                    Latitude = model.PickupAddress.Latitude,
                    Longitude = model.PickupAddress.Longitude
                },
                DeliveryAddress = new Address
                {
                    Latitude = model.DeliveryAddress.Latitude,
                    Longitude = model.DeliveryAddress.Longitude
                },
                Status = new OrderStatus
                {
                    Status = OrderStatusEnum.Pending
                },
                PackageDetails = new PackageDetails
                {
                    WeightKg = model.PackageDetails.WeightKg,
                    WidthCm = model.PackageDetails.WidthCm,
                    HeightCm = model.PackageDetails.HeightCm,
                    DepthCm = model.PackageDetails.DepthCm
                },
                Cost = _baseDeliverySettings.DeliveryCost
            };
        }
    }
}