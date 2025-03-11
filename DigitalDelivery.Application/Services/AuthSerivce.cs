using DigitalDelivery.Application.Helper;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.User;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalDelivery.Application.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(UserLogin user);
        Task<Result> RegisterAsync(UserRegister user);
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

        public async Task<Result> RegisterAsync(UserRegister user)
        {
            if (user == null)
            {
                return new Result(false, "User is required.");
            }

            if (await CheckEmailExistAsync(user.Email))
            {
                return new Result(false, "Email is already exists.");
            }

            var passwordValidation = CheckPasswordStrength(user.Password);
            if (!string.IsNullOrEmpty(passwordValidation))
            {
                return new Result(false, passwordValidation);
            }

            var userEntity = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = PasswordHasher.HashPassword(user.Password),
                Token = string.Empty
            };

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return new Result(true);
        }

        private async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            var builder = new StringBuilder();
            if (password.Length < 8)
            {
                builder.Append("Minimum password length should be 8" + Environment.NewLine);
            }

            if (!(Regex.IsMatch(password, "[a-z]")
                && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
            {
                builder.Append("Password should be Alphanumeric" + Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}