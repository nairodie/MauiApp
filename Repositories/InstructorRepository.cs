using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class InstructorRepository(SQLiteAsyncConnection db) : IInstructorRepository
    {
        private readonly SQLiteAsyncConnection _db = db;

        public async Task<int> InsertAsync(Instructor instructor)
        {
            await _db.InsertAsync(instructor);
            return instructor.InstructorId;
        }

        public Task UpdateInstructor(Instructor instructor)
        {
            return _db.UpdateAsync(instructor);
        }

        public Task DeleteAllAsync()
        {
            return _db.DeleteAllAsync<Instructor>();
        }

        public Task<List<Instructor>> GetAllAsync()
        {
            return _db.Table<Instructor>().ToListAsync();
        }

        public async Task<Instructor?> GetByIdAsync(int instructorId)
        {
            return await _db.Table<Instructor>().Where(i => i.InstructorId == instructorId).FirstOrDefaultAsync();
        }        
    }
}
