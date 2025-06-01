using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/order")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var result = await _orderService.CreateAsync(model);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost("user-orders")]
        public async Task<IActionResult> GetUserOrders(PaginationRequest request)
        {
            var currentUser = _userService.GetCurrentUser();

            var orders = await _orderService.GetOrdersAsync(currentUser.Id, request);

            return Ok(orders);
        }
    }
}