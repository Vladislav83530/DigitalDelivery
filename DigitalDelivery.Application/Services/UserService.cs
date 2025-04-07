using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DigitalDelivery.Application.Helpers;

namespace DigitalDelivery.Application.Services
{
    public interface IUserService
    {
        User GetCurrentUser();
        User GetUserByPhoneNumber(string phoneNumber);
    }

    public class UserService : IUserService
    {
        private AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public User GetCurrentUser()
        {
            var currentUserPhoneNumber = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.MobilePhone)?.Value;

            return _context.Users.FirstOrDefault(u => u.PhoneNumber == currentUserPhoneNumber);
        }

        public User GetUserByPhoneNumber(string phoneNumber)
        {
            var clearPhoneNumber = Helper.CleanPhoneNumber(phoneNumber);
            return _context.Users.FirstOrDefault(u => u.PhoneNumber == clearPhoneNumber);
        }
    }
}