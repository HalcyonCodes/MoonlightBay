


using MoonlightBay.Model;
namespace MoonlightBay.Data.Interfaces;

public interface IAccountRepository
{
    public Task<ApplicationUser?> GetUserByUserNameAsync(string userName);
}