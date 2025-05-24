using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface ICourseRepository
    {
        Task AddCourseAsync(Course course);
        Task DeleteCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task InsertAsync(Course course);
        Task<List<Course>> GetCoursesByTermAsync(int termId);
        Task<List<Course>> GetAllAsync();
        Task<Course?> GetCourseAsync(int courseId);
        Task<int> GetLastInsertedCourseIdAsync();
        Task<Course> AddNewCourseWithAssessmentsAsync(int termId);
        Task DeleteAllAsync();
    }
}