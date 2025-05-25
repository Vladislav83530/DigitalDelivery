using DigitalDelivery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/route")]
    public class RouteController : Controller
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetRoute(int orderId)
        {
            var route = await _routeService.GetShortestPathAsync(orderId);

            if (route.Item1 == null || route.Item2 == null || !route.Item1.Any() || !route.Item2.Any())
            {
                return NotFound();
            }

            return Ok(new {
                Nodes = route.Item1,
                Edges = route.Item2
            });
        }
    }
}
