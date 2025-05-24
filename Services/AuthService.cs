using MauiApp2.Interfaces;
using MauiApp2.Models;
using System.Security.Cryptography;
using System.Text;

namespace MauiApp2.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _db;
        private readonly IHashingService _hashingService;
        private const int IterationCount = 100000;

        public AuthService(IDatabaseService db, IHashingService hashingService)
        {
            _db = db;
            _hashingService = hashingService;
        }

        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return AuthResult.InvalidInput;

            var existing = await _db.UserRepository.GetByUsernameAsync(username);
            if (existing != null) return AuthResult.UsernameTaken;

            var (hash, salt) = _hashingService.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Salt = salt,
                PasswordHash = hash
            };

            try
            {
                await _db.UserRepository.InsertAsync(newUser);
                return AuthResult.Success;
            }
            catch
            {
                return AuthResult.DatabaseError;
            }
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return AuthResult.InvalidInput;

            var user = await _db.UserRepository.GetByUsernameAsync(username);
            if (user == null) return AuthResult.InvalidCredentials;

            if (!_hashingService.VerifyPassword(password, user.PasswordHash, user.Salt))
                return AuthResult.InvalidCredentials;

            await SecureStorage.SetAsync("logged_in_user", user.Id.ToString());
            return AuthResult.Success;
        }
    }
}