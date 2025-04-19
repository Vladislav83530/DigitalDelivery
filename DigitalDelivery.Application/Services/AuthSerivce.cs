using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DigitalDelivery.Application.Services
{
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

            userEntity.Token = CreateJwt(userEntity);
            userEntity.RefreshToken = CreateRefreshToken();
            userEntity.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5);
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                AccessToken = userEntity.Token,
                RefreshToken = userEntity.RefreshToken
            };

            return new Result<LoginResponse>(true, null, data: response);
        }

        public async Task<Result<string>> RegisterAsync(UserRegister user)
        {
            if (user == null)
            {
                return new Result<string>(false, "User is required.");
            }

            if (await CheckEmailOrPhoneExistAsync(user.Email, user.PhoneNumber))
            {
                return new Result<string>(false, "Email or phone number is already exists.");
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
                PhoneNumber = Helper.CleanPhoneNumber(user.PhoneNumber),
                Password = PasswordHasher.HashPassword(user.Password),
                Token = string.Empty,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = DateTime.MinValue
            };

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        public async Task<Result<LoginResponse>> RefreshAsync(LoginResponse model)
        {
            string accessToken = model.AccessToken;
            string refreshToken = model.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            var email = principal.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new Result<LoginResponse>(false, "Invalid request");
            }

            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return new Result<LoginResponse>(true, null, new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }

        private async Task<bool> CheckEmailOrPhoneExistAsync(string email, string phoneNumber)
        {
            var clearPhoneNumber = Helper.CleanPhoneNumber(phoneNumber);
            return await _context.Users.AnyAsync(u => u.Email == email || u.PhoneNumber == clearPhoneNumber);
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
                builder.Append("Password should be Alphanumeric");
            }

            return builder.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, Helper.CleanPhoneNumber(user.PhoneNumber))
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

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _context.Users
                .Any(u => u.RefreshToken == refreshToken);

            if (tokenInUser)
            {
                return CreateRefreshToken();
            }

            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This is invalid token!");
            }

            return principal;
        }
    }
}