using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IUserService
    {
        User GetCurrentUser();
        User GetUserByPhoneNumber(string phoneNumber);
        User GetRandomUser();
    }
}