using DigitalDelivery.Application.Helper;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.User;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalDelivery.Application.Services
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(UserLogin user);
        Task<Result<string>> RegisterAsync(UserRegister user);
    }

    public class AuthSerivce : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly AuthSettings _authSettings;

        public AuthSerivce(
            AppDbContext context,
            IOptions<AuthSettings> authSettings)
        {
            _context = context;
            _authSettings = authSettings.Value;
        }

        public async Task<Result<LoginResponse>> LoginAsync(UserLogin user)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            if (userEntity == null)
            {
                return new Result<LoginResponse>(false, "User not found.");
            }

            if (!PasswordHasher.VerifyPassword(user.Password, userEntity.Password))
            {
                return new Result<LoginResponse>(false, "Password is incorrect");
            }

            var response = new LoginResponse
            {
                Token = CreateJwt(userEntity)
            };

            return new Result<LoginResponse>(true, null, data: response);
        }

        public async Task<Result<string>> RegisterAsync(UserRegister user)
        {
            if (user == null)
            {
                return new Result<string>(false, "User is required.");
            }

            if (await CheckEmailExistAsync(user.Email))
            {
                return new Result<string>(false, "Email is already exists.");
            }

            var passwordValidation = CheckPasswordStrength(user.Password);
            if (!string.IsNullOrEmpty(passwordValidation))
            {
                return new Result<string>(false, passwordValidation);
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

            return new Result<string>(true);
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

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDesciptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDesciptor);

            return jwtTokenHandler.WriteToken(token);
        }
    }
}