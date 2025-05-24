using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface INoteRepository
    {
        Task InsertAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(Note note);
        Task DeleteAllAsync();
        Task<List<Note>> GetAllAsync();
        Task<List<Note>> GetNotesByCourseAsync(int courseId);
        Task<List<Note>> GetAsync(int noteId);
    }
}
