using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class CourseRepository(SQLiteAsyncConnection db) : ICourseRepository
    {
        private readonly SQLiteAsyncConnection _db = db;

        public Task DeleteCourseAsync(Course course) => _db.DeleteAsync(course);

        public Task AddCourseAsync(Course course) => _db.InsertAsync(course);

        public Task UpdateCourseAsync(Course course) => _db.UpdateAsync(course);

        public Task DeleteAllAsync() => _db.DeleteAllAsync<Course>();

        public Task<List<Course>> GetAllAsync() => _db.Table<Course>().ToListAsync();

        public Task InsertAsync(Course course) => _db.InsertAsync(course);

        public async Task<List<Course>> GetCoursesByTermAsync(int termId)
        {
            return await _db.Table<Course>().Where(c => c.TermId == termId).ToListAsync();
        }

        public Task<int> GetLastInsertedCourseIdAsync()
        {
            return _db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
        }

        public async Task<Course?> GetCourseAsync(int courseId)
        {
            return await _db.Table<Course>().FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course> AddNewCourseWithAssessmentsAsync(int termId)
        {
            var newCourse = new Course
            {
                TermId = termId,
                InstructorId = 1,
                CourseName = "NewCourse101",
                Start = DateTime.Now,
                End = DateTime.Now.AddMonths(4),
                Status = "Plan to Take",
                CourseDetails = "Enter Course Details Here:",
                PerformanceAssessmentId = 0,
                ObjectiveAssessmentId = 0
            };

            await _db.InsertAsync(newCourse);
            newCourse.CourseId = await _db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            var pa = new Assessment
            {
                Type = Assessment.AssessmentType.Performance,
                Name = "Performance Assessment",
                Start = DateTime.Now,
                Details = "Performance details...",
                CourseId = newCourse.CourseId
            };

            await _db.InsertAsync(pa);
            pa.AssessmentId = await _db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            var oa = new Assessment
            {
                Type = Assessment.AssessmentType.Objective,
                Name = "Objective Assessment",
                Start = DateTime.Now,
                Details = "Objective details...",
                CourseId = newCourse.CourseId
            };

            await _db.InsertAsync(oa);
            oa.AssessmentId = await _db.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            newCourse.PerformanceAssessmentId = pa.AssessmentId;
            newCourse.ObjectiveAssessmentId = oa.AssessmentId;

            await _db.UpdateAsync(newCourse);

            return newCourse;
        }
    }
}
