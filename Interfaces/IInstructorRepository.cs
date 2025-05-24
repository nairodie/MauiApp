using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface IInstructorRepository
    {
        Task<int> InsertAsync(Instructor instructor);
        Task UpdateInstructor(Instructor instructor);
        Task DeleteAllAsync();
        Task<List<Instructor>> GetAllAsync();
        Task<Instructor?> GetByIdAsync(int instructorId);
    }
}