using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DigitalDelivery.Application.Services
{
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

        public User GetRandomUser()
        {
            var users = _context.Users.ToList();
            if (!users.Any())
                throw new InvalidOperationException("Користувачі не знайдені.");

            int index = Random.Shared.Next(users.Count);
            return users[index];
        }
    }
}