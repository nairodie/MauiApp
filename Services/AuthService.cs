using MauiApp2.Interfaces;
using MauiApp2.Models;

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

        public Task<bool> LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}