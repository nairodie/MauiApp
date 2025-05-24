namespace MauiApp2.Interfaces
{
    public interface IDatabaseService
    {
        ITermRepository TermRepository { get; }
        ICourseRepository CourseRepository { get; }
        IAssessmentRepository AssessmentRepository { get; }
        IInstructorRepository InstructorRepository { get; }
        INoteRepository NoteRepository { get; }
        IUserRepository UserRepository { get; }
        

        Task CreateTablesAsync();
        Task ResetTermsAutoIncrementAsync();
    }
}