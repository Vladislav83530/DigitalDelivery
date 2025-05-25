using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Application.Models.Order;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.EF;
using DigitalDelivery.Infrastructure.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading;

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
        private readonly ILogger<OrderService> _logger;
        private readonly ISimulatedClock _clock;

        public OrderService(
            AppDbContext context,
            IUserService userService,
            IOrderQueue orderQueue,
            ISimulatedClock clock,
            ILogger<OrderService> logger,
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
            _logger = logger;
            _clock = clock;
        }

        public async Task<Result<int>> CreateAsync(CreateOrderModel model)
        {
            var sender = _userService.GetCurrentUser();
            var recipient = _userService.GetUserByPhoneNumber(model.RecipientPhoneNumber);

            var result = ValidateCreatingOrder(recipient, model);
            if (!result.Success)
            {
                return new Result<int>(false, result.Message);
            }

            var orderEntity = CreateOrderEntity(sender, recipient, model);
            await _context.Orders.AddAsync(orderEntity);
            await _context.SaveChangesAsync();

            await _orderQueue.EnqueueAsync(orderEntity);

            return new Result<int>(true, "Success", orderEntity.Id);
        }

        public async Task<OrderViewModel> GetAsync(int id)
        {
            var currentUser = _userService.GetCurrentUser();

            return await _context.Orders
                .Where(o => o.Id == id &&
                    (o.SenderId == currentUser.Id || o.RecipientId == currentUser.Id)
                )
                .Select(o => new OrderViewModel
                {
                    OrderNumber = o.Id,
                    EstimatedDelivery = o.EstimatedDelivery,
                    Package = new PackageViewModel
                    {
                        WeightKg = o.PackageDetails.WeightKg,
                        WidthCm = o.PackageDetails.WidthCm,
                        HeightCm = o.PackageDetails.HeightCm,
                        DepthCm = o.PackageDetails.DepthCm
                    },
                    OrderStatuses = o.OrderStatuses
                        .Select(os => new OrderStatusViewModel
                        {
                            DateIn = os.DateIn,
                            Status = os.Status
                        })
                        .ToList(),
                    Sender = new ContactViewModel
                    {
                        PhoneNumber = o.Sender.PhoneNumber,
                        FirstName = o.Sender.FirstName,
                        LastName = o.Sender.LastName,
                        Address = new Node
                        {
                            Latitude = o.PickupAddress.Latitude,
                            Longitude = o.PickupAddress.Longitude,
                        }
                    },
                    Recipient = new ContactViewModel
                    {
                        PhoneNumber = o.Recipient.PhoneNumber,
                        FirstName = o.Recipient.FirstName,
                        LastName = o.Recipient.LastName,
                        Address = new Node
                        {
                            Latitude = o.DeliveryAddress.Latitude,
                            Longitude = o.DeliveryAddress.Longitude,
                        }
                    }

                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaginationResponse<OrderHistoryItem>> GetOrdersAsync(int userId, PaginationRequest request)
        {
            var query = _context.Orders
              .Where(o => o.RecipientId == userId || o.SenderId == userId)
              .OrderByDescending(o => o.CreatedAt);

            var count = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(count / (double)request.PageSize);

            var orders = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrderHistoryItem
                {
                    OrderNumber = o.Id,
                    Status = o.OrderStatuses
                        .OrderByDescending(s => s.DateIn)
                        .FirstOrDefault().Status,
                    CreatedAt = o.CreatedAt,
                    DeliveredAt = o.CompletedAt == DateTime.MinValue ? null : o.CompletedAt
                })
                .ToListAsync();

            return new PaginationResponse<OrderHistoryItem>
            {
                Items = orders,
                PageNumber = request.Page,
                PageSize = request.PageSize,
                TotalCount = count,
                TotalPages = totalPages
            };
        }

        public async Task<Result<string>> ChangeStatusAsync(int id, OrderStatusEnum status)
        {
            var order = _context.Orders
                .Include(o => o.OrderStatuses)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return new Result<string>(false, "Order not found.");
            }

            if (order.OrderStatuses.Where(s => s.Status == status).Any())
            {
                return new Result<string>(true);
            }

            _context.OrderStatuses.Add(new OrderStatus
            {
                OrderId = order.Id,
                Status = status,
                DateIn = _clock.Now,
            });
            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        public async Task<Result<string>> SetDeliveryAstimateAsync(List<double> distances, Guid robotId, int orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return new Result<string>(false, "Order not found.");
            }

            var robotSpeed = await _context.RobotTelemetries
                .Where(r => r.RobotId == robotId)
                .Select(r => r.SpeedKhp)
                .FirstOrDefaultAsync();

            var totalDistance = distances.Sum() / 1000;
            var deliveryTime = totalDistance / robotSpeed;
            const double pickUpWaitingTime = 0.1;
            var totalDeliveryTime = deliveryTime + pickUpWaitingTime;

            order.EstimatedDelivery = _clock.Now.AddHours(totalDeliveryTime);

            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        public async Task<List<Order>> GetAllActiveOrdersAsync()
        {
            var activeOrderStatuses = new[] { OrderStatusEnum.Processed, OrderStatusEnum.MoveToDeliveryPoint, OrderStatusEnum.MoveToPickupPoint };

            return await _context.Orders
                .Include(o => o.OrderStatuses)
                .Include(o => o.RobotAssignments)
                .ThenInclude(r => r.Robot)
                .ThenInclude(r => r.Telemetry)
                .Include(o => o.RobotAssignments)
                .ThenInclude(r => r.Robot)
                .ThenInclude(r => r.Specification)
                .Include(o => o.Routes)
                .Include(o => o.PickupAddress)
                .Include(o => o.DeliveryAddress)
                .Where(o => activeOrderStatuses.Contains(o.OrderStatuses.OrderByDescending(s => s.DateIn).FirstOrDefault().Status))
                .ToListAsync();
        }

        public async Task CreateSimulatedOrderAsync(DateTime simulatedTime, CreateOrderModel orderModel)
        {
            var sender = _userService.GetRandomUser();
            var recipient = _userService.GetUserByPhoneNumber(orderModel.RecipientPhoneNumber);

            var validation = ValidateCreatingOrder(recipient, orderModel);
            if (!validation.Success)
            {
                _logger.LogWarning($"[SIM] Помилка створення замовлення: {validation.Message}");
                return;
            }

            var orderEntity = CreateOrderEntity(sender, recipient, orderModel);
            orderEntity.CreatedAt = _clock.Now.ToUniversalTime();
            await _context.Orders.AddAsync(orderEntity);
            await _context.SaveChangesAsync();

            await _orderQueue.EnqueueAsync(orderEntity);

            _logger.LogInformation($"[SIM] Створено замовлення #{orderEntity.Id} в {simulatedTime}");
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
                OrderStatuses = new List<OrderStatus> {
                    new OrderStatus
                    {
                        Status = OrderStatusEnum.Pending,
                        DateIn = _clock.Now
                    }
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

        private static GeoCoordinate GetRandomPointInPolygon(List<(double, double)> polygon)
        {
            var bbox = GetBoundingBox(polygon);
            GeoCoordinate point;

            int maxAttempts = 1000;
            do
            {
                var lat = Random.Shared.NextDouble() * (bbox.MaxLat - bbox.MinLat) + bbox.MinLat;
                var lon = Random.Shared.NextDouble() * (bbox.MaxLon - bbox.MinLon) + bbox.MinLon;
                point = new GeoCoordinate(lat, lon);
                maxAttempts--;
            }
            while (!DeliveryHelper.IsPointInPolygon(point.Latitude, point.Longitude, polygon) && maxAttempts > 0);

            return point;
        }

        private static (double MinLat, double MaxLat, double MinLon, double MaxLon) GetBoundingBox(List<(double, double)> polygon)
        {
            return (
                polygon.Min(p => p.Item1),
                polygon.Max(p => p.Item1),
                polygon.Min(p => p.Item2),
                polygon.Max(p => p.Item2)
            );
        }
    }
}