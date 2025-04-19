using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Order;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Result<string>> CreateAsync(CreateOrderModel model);
        Task<Result<string>> ChangeStatusAsync(Guid id, OrderStatusEnum status);
    }
}