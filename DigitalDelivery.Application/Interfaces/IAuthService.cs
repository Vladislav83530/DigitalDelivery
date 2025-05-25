using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Models.User;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(UserLogin user);
        Task<Result<string>> RegisterAsync(UserRegister user);
        Task<Result<LoginResponse>> RefreshAsync(LoginResponse loginResponse);
    }
}
