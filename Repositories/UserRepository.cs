using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class UserRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public UserRepository(SQLiteAsyncConnection db)
        {
            _db = db;
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _db.Table<User>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public Task<int> InsertAsync(User user) => _db.InsertAsync(user);

    }
}
