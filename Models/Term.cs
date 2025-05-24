using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Terms")]
    public class Term : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsAddButton { get; set; }

        [PrimaryKey, AutoIncrement]
        public int TermId { get; set; }

        private string _termName;
        public string TermName
        {
            get => _termName;
            set
            {
                if (_termName != value)
                {
                    _termName = value;
                    OnPropertyChanged(nameof(TermName));
                }
            }
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

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Term() { }

        public Term(string termName, DateTime start, DateTime end)
        {
            TermName = termName;
            Start = start;
            End = end;
        }
    }
}
