namespace MauiApp2.Interfaces
{
    public interface IHashingService
    {
        (string hash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, string storedHash, string storedSalt);
    }
}
