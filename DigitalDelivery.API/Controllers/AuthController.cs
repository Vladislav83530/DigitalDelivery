using DigitalDelivery.Application.Models.User;
using DigitalDelivery.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var result = await _authService.LoginAsync(model);

            return result ? Ok() : NotFound();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var result = await _authService.RegisterAsync(model);

            return Ok(result);
        }
    }
}
