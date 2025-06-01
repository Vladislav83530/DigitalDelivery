using DigitalDelivery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/robot")]
    public class RobotController : Controller
    {
        private readonly IRobotService _robotService;

        public RobotController(IRobotService robotService)
        {
            _robotService = robotService;
        }

        [HttpGet("{robotId}/location")]
        public async Task<IActionResult> GetLocation(Guid robotId)
        {
            var currentLocation = await _robotService.GetCurrentLocationAsync(robotId);

            return Ok(currentLocation);
        }
    }
}
