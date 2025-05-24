using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Courses")]
    public class Course : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [PrimaryKey, AutoIncrement]
        public int CourseId { get; set; }

        private int _termId;
        [Indexed]
        public int TermId
        {
            get => _termId;
            set { _termId = value; OnPropertyChanged(nameof(TermId)); }
        }

        private int _instructorId;
        [Indexed]
        public int InstructorId
        {
            get => _instructorId;
            set { _instructorId = value; OnPropertyChanged(nameof(InstructorId)); }
        }

        private string _courseName;
        public string CourseName
        {
            get => _courseName;
            set { _courseName = value; OnPropertyChanged(nameof(CourseName)); }
        }

        private DateTime _start;
        public DateTime Start
        {
            get => _start;
            set
            {
                if (_start != value)
                {
                    _start = value;
                    OnPropertyChanged(nameof(Start));
                }
            }
        }

        private DateTime _end;
        public DateTime End
        {
            get => _end;
            set
            {
                if (_end != value)
                {
                    _end = value;
                    OnPropertyChanged(nameof(End));
                }
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        private string _courseDetails;
        public string CourseDetails
        {
            get => _courseDetails;
            set { _courseDetails = value; OnPropertyChanged(nameof(CourseDetails)); }
        }

        // Assessment references
        private int _performanceAssessmentId;
        public int PerformanceAssessmentId
        {
            get => _performanceAssessmentId;
            set { _performanceAssessmentId = value; OnPropertyChanged(nameof(PerformanceAssessmentId)); }
        }

        private int _objectiveAssessmentId;
        public int ObjectiveAssessmentId
        {
            get => _objectiveAssessmentId;
            set { _objectiveAssessmentId = value; OnPropertyChanged(nameof(ObjectiveAssessmentId)); }
        }

        // Notification flags
        private int _startNotification;
        public int StartNotification
        {
            get => _startNotification;
            set { _startNotification = value; OnPropertyChanged(nameof(StartNotification)); }
        }

        private int _endNotification;
        public int EndNotification
        {
            get => _endNotification;
            set { _endNotification = value; OnPropertyChanged(nameof(EndNotification)); }
        }
    }
}
