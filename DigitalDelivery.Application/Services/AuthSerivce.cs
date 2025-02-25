using DigitalDelivery.Application.Models.User;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace DigitalDelivery.Application.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(UserLogin user);
        Task<bool> RegisterAsync(UserRegister user);
    }

    public class AuthSerivce : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthSerivce(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginAsync(UserLogin user)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == u.Password);

            return userEntity != null;
        }

        public async Task<bool> RegisterAsync(UserRegister user)
        {
            var userEntity = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password
            };

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}