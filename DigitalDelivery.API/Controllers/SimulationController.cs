using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models.Order;
using DigitalDelivery.Application.Models.Simuation;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading;
using System.IO;

namespace DigitalDelivery.API.Controllers
{
    [ApiController]
    [Route("api/simulation")]
    public class SimulationController : Controller
    {
        private readonly ISimulatedClock _clock;
        private readonly IOrderService _orderService;
        private readonly List<OrderTimeSlotSettings> _timeSlots;
        private readonly ILogger<SimulationController> _logger;

        public SimulationController(
            ISimulatedClock clock,
            IOrderService orderService,
            IOptions<OrderSimulationSettings> options,
            ILogger<SimulationController> logger)
        {
            _clock = clock;
            _orderService = orderService;
            _timeSlots = options.Value.TimeSlots;
            _logger = logger;
        }

        [HttpPost("simulate-orders")]
        public async Task<ActionResult<OrderSimulationResult>> SimulateOrders(CancellationToken token)
        {
            var simDayStart = _clock.Now.Date;
            var orders = new List<CreateOrderModel>();
            var totalOrdersCreated = 0;
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "orders.json");

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string fileContent = await System.IO.File.ReadAllTextAsync(filePath);
                    orders = JsonSerializer.Deserialize<List<CreateOrderModel>>(fileContent) ?? new List<CreateOrderModel>();

                    var orderCount = _timeSlots.Sum(x => x.Orders);
                    orders = orders.Take(orderCount).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при зчитуванні ордерів з файлу: {ex.Message}");
                    return StatusCode(500, "Error reading orders from file");
                }
            }

            int globalOrderIndex = 0;

            foreach (var slot in _timeSlots.OrderBy(s => s.FromTime))
            {
                int count = slot.Orders;
                var slotDuration = slot.ToTime.ToTimeSpan() - slot.FromTime.ToTimeSpan();

                _logger.LogInformation($"[SIM] Слот {slot.FromTime}-{slot.ToTime}: {count} замовлень");

                for (int i = 0; i < count; i++)
                {
                    if (globalOrderIndex >= orders.Count)
                    {
                        _logger.LogWarning("Немає більше замовлень у orders.json");
                        break;
                    }

                    double fraction = (count == 1) ? 0 : i / (double)(count - 1);
                    var timeOffsetInSlot = TimeSpan.FromSeconds(slotDuration.TotalSeconds * fraction);
                    var orderTime = simDayStart + slot.FromTime.ToTimeSpan() + timeOffsetInSlot;

                    _clock.Pause();
                    await _orderService.CreateSimulatedOrderAsync(orderTime, orders[globalOrderIndex]);
                    _clock.Resume();

                    globalOrderIndex++;

                    // Затримка — тільки якщо не останнє в слоті
                    if (i < count - 1)
                    {
                        double nextFraction = (count == 1) ? 1 : (i + 1) / (double)(count - 1);
                        var nextOffset = TimeSpan.FromSeconds(slotDuration.TotalSeconds * nextFraction);
                        var nextOrderTime = simDayStart + slot.FromTime.ToTimeSpan() + nextOffset;

                        var simulatedDelay = nextOrderTime - orderTime;
                        await _clock.DelayAsync(simulatedDelay, token);
                    }
                    else
                    {
                        var simulatedDelay = slot.ToTime - _clock.TimeNow;
                        await _clock.DelayAsync(simulatedDelay, token);
                    }

                }

                totalOrdersCreated += count;
            }

            _logger.LogInformation($"Створено {totalOrdersCreated} нових ордерів.");

            return Ok(new OrderSimulationResult
            {
                SimulatedTime = _clock.TimeNow,
                OrdersCreated = totalOrdersCreated
            });
        }
    }
}
