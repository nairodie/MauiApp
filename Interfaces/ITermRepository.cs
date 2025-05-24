using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface ITermRepository
    {
        Task InsertAsync(Term term);
        Task UpdateTermAsync(Term term);
        Task<List<Term>> GetAllAsync();
        Task DeleteAllAsync();
        Task DeleteTermAsync(Term term);
        Task<Term> AddNewTermAutoNamedAsync();
        Task DeleteTermCascadeAsync(int termId);
    }
}
