using DigitalDelivery.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDelivery.API.Controllers
{
    [Authorize, ApiController, Route("api/account")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Account()
       
        {
            return Ok(new Result<bool>(true, null, data: true));
        }
    }
}
