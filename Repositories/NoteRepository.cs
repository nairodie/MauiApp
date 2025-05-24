using MauiApp2.Interfaces;
using MauiApp2.Models;
using SQLite;

namespace MauiApp2.Repositories
{
    public class NoteRepository(SQLiteAsyncConnection db) : INoteRepository
    {
        private readonly SQLiteAsyncConnection _db = db;

        public Task InsertAsync(Note note)
        {
            return _db.InsertAsync(note);
        }

        public Task UpdateNoteAsync(Note note)
        {
            return _db.UpdateAsync(note);
        }

        public Task DeleteNoteAsync(Note note)
        {
            return _db.DeleteAsync(note);
        }

        public async Task<List<Note>> GetNotesByCourseAsync(int courseId)
        {
            return await _db.Table<Note>().Where(n => n.CourseId == courseId).ToListAsync();
        }

        public Task DeleteAllAsync()
        {
            return _db.DeleteAllAsync<Note>();
        }

        public Task<List<Note>> GetAllAsync()
        {
            return _db.Table<Note>().ToListAsync();
        }

        public async Task<List<Note>> GetAsync(int noteId)
        {
            return await _db.Table<Note>().Where(n => n.NoteId == noteId).ToListAsync();
        }
    }
}
