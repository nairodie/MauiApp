using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Assessments")]
    public class Assessment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public enum AssessmentType
        {
            Objective = 0,
            Performance = 1
        }

        [PrimaryKey, AutoIncrement]
        public int AssessmentId { get; set; }

        public AssessmentType Type { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
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
                    End = _start.AddMonths(3);
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

                    if (DueDate != _end)
                    {
                        DueDate = _end;
                    }
                }
            }
        }

        private int _startNotification;
        public int StartNotification
        {
            get => _startNotification;
            set
            {
                if (_startNotification != value)
                {
                    _startNotification = value;
                    OnPropertyChanged(nameof(StartNotification));
                }
            }
        }

        private int _endNotification;
        public int EndNotification
        {
            get => _endNotification;
            set
            {
                if (_endNotification != value)
                {
                    _endNotification = value;
                    OnPropertyChanged(nameof(EndNotification));
                }
            }
        }

        private string _details;
        public string Details
        {
            get => _details;
            set { _details = value; OnPropertyChanged(nameof(Details)); }
        }

        [Indexed]
        public int CourseId { get; set; }

        private DateTime _dueDate;
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                if (_dueDate != value)
                {
                    _dueDate = value;
                    OnPropertyChanged(nameof(DueDate));

                    if (End != _dueDate)
                    {
                        End = _dueDate;
                    }
                }
            }
        }
    }
}
