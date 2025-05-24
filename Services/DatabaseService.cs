using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;
using System.Diagnostics;

namespace MauiApp2.Services
{
    public class DatabaseService(ITermRepository termRepository, ICourseRepository courseRepository, IAssessmentRepository assessmentRepository,
        IInstructorRepository instructorRepository, INoteRepository noteRepository, SQLiteAsyncConnection db) : IDatabaseService
    {
        private readonly SQLiteAsyncConnection _db = db;

        public ITermRepository TermRepository { get; } = termRepository;
        public ICourseRepository CourseRepository { get; } = courseRepository;
        public IAssessmentRepository AssessmentRepository { get; } = assessmentRepository;
        public IInstructorRepository InstructorRepository { get; } = instructorRepository;
        public INoteRepository NoteRepository { get; } = noteRepository;
        public IUserRepository UserRepository => throw new NotImplementedException();

        public async Task CreateTablesAsync()
        {
            await _db.CreateTableAsync<Term>();
            await _db.CreateTableAsync<Course>();
            await _db.CreateTableAsync<Assessment>();
            await _db.CreateTableAsync<Instructor>();
            await _db.CreateTableAsync<Note>();
        }

        public async Task ResetTermsAutoIncrementAsync()
        {
            try
            {
                await _db.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Term';");
                Debug.WriteLine("Auto-increment for 'Term' has been reset.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resetting auto-increment: {ex.Message}");
            }

        }
    }
}