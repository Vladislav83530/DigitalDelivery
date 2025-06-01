using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/account")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Account()  
        {
            var user = _userService.GetCurrentUser();

            var viewModel = new UserProfileViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return Ok(viewModel);
        }
    }
}
