
namespace MauiApp2.Interfaces
{
    public interface IUserRepository
    {
        Task GetByUsernameAsync(string username);
    }
}
