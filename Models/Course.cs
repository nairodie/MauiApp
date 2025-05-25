using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Courses")]
    public partial class Course : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int CourseId { get; set; }

        private int termId;
        [Indexed]
        public int TermId
        {
            get => termId;
            set => SetProperty(ref termId, value);
        }

        private int instructorId;
        [Indexed]
        public int InstructorId
        {
            get => instructorId;
            set => SetProperty(ref instructorId, value);
        }

        [ObservableProperty] private string? courseName;
        [ObservableProperty] private DateTime start;
        [ObservableProperty] private DateTime end;
        [ObservableProperty] private string? status;
        [ObservableProperty] private string? courseDetails;
        [ObservableProperty] private int performanceAssessmentId;
        [ObservableProperty] private int objectiveAssessmentId;
        [ObservableProperty] private int startNotification;
        [ObservableProperty] private int endNotification;
    }
}
