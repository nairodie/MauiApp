namespace MauiApp2.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
    }
}
