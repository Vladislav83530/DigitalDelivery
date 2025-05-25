using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Order;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderViewModel> GetAsync(int id);
        Task<PaginationResponse<OrderHistoryItem>> GetOrdersAsync(int userId, PaginationRequest requestS);
        Task<Result<int>> CreateAsync(CreateOrderModel model);
        Task<Result<string>> ChangeStatusAsync(int id, OrderStatusEnum status);
        Task<Result<string>> SetDeliveryAstimateAsync(List<double> distances, Guid robotId, int orderId);
        Task<List<Order>> GetAllActiveOrdersAsync();
        Task CreateSimulatedOrderAsync(DateTime simulatedTime, CreateOrderModel orderModel);
    }
}