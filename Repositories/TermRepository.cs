using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class TermRepository(SQLiteAsyncConnection db) : ITermRepository
    {
        private readonly SQLiteAsyncConnection _db = db;

        public Task InsertAsync(Term term)
        {
            return _db.InsertAsync(term);
        }

        public Task UpdateTermAsync(Term term)
        {
            return _db.UpdateAsync(term);
        }

        public Task<List<Term>> GetAllAsync()
        {
            return _db.Table<Term>().ToListAsync();
        }

        public Task DeleteAllAsync()
        {
            return _db.DeleteAllAsync<Term>();
        }

        public Task DeleteTermAsync(Term term)
        {
            return _db.DeleteAsync(term);
        }

        public async Task<Term> AddNewTermAutoNamedAsync()
        {
            var lastTerm = await _db.Table<Term>().OrderByDescending(t => t.TermId).FirstOrDefaultAsync();
            int nextId = lastTerm?.TermId + 1 ?? 1;
            var name = $"Term {nextId}";

            var newTerm = new Term
            {
                TermName = name,
                Start = DateTime.Now,
                End = DateTime.Now.AddMonths(2)
            };

            await _db.InsertAsync(newTerm);
            return newTerm;
        }

        public async Task DeleteTermCascadeAsync(int termId)
        {
            var courses = await _db.Table<Course>().Where(c => c.TermId == termId).ToListAsync();

            foreach (var course in courses)
            {
                var notes = await _db.Table<Note>().Where(n => n.CourseId == course.CourseId).ToListAsync();
                var assessments = await _db.Table<Assessment>().Where(a => a.CourseId == course.CourseId).ToListAsync();

                foreach (var note in notes)
                    await _db.DeleteAsync(note);

                foreach (var assessment in assessments)
                    await _db.DeleteAsync(assessment);

                await _db.DeleteAsync(course);
            }

            var term = await _db.Table<Term>().FirstOrDefaultAsync(t => t.TermId == termId);
            if (term != null)
                await _db.DeleteAsync(term);
        }
    }
}
