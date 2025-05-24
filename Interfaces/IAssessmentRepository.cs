using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface IAssessmentRepository
    {
        Task InsertAsync(Assessment assessment);
        Task UpdateAssessment(Assessment assessment);
        Task DeleteAssessment(Assessment assessment);
        Task DeleteAllAsync();
        Task<List<Assessment>> GetAllAsync();
        Task<List<Assessment>> GetAssessmentsByCourseAsync(int courseId);
        Task<int> GetLastInsertedAssessmentIdAsync();
        Task<IEnumerable<Assessment>> GetAssessmentsByTermAsync(int termId);
    }
}