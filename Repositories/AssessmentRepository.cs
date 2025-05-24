using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly ICourseRepository _courseRepository;

        public AssessmentRepository(SQLiteAsyncConnection db, ICourseRepository courseRepository)
        {
            _db = db;
            _courseRepository = courseRepository;
        }

        public Task InsertAsync(Assessment assessment)
        {
            return _db.InsertAsync(assessment);
        }

        public Task UpdateAssessment(Assessment assessment)
        {
            return _db.UpdateAsync(assessment);
        }

        public Task DeleteAssessment(Assessment assessment)
        {
            return _db.DeleteAsync(assessment);
        }

        public Task DeleteAllAsync()
        {
            return _db.DeleteAllAsync<Assessment>();
        }

        public Task<List<Assessment>> GetAllAsync()
        {
            return _db.Table<Assessment>().ToListAsync();
        }

        public Task<List<Assessment>> GetAssessmentsByCourseAsync(int courseId)
        {
            return _db.Table<Assessment>().Where(a => a.CourseId == courseId).ToListAsync();
        }

        public Task<int> GetLastInsertedAssessmentIdAsync()
        {
            return _db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByTermAsync(int termId)
        {
            var courses = await _courseRepository.GetCoursesByTermAsync(termId);
            var courseIds = courses.Select(c => c.CourseId);

            var allAssessments = await GetAllAsync();
            return allAssessments.Where(a => courseIds.Contains(a.CourseId));
        }
    }
}
