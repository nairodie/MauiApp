using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Notes")]
    public class Note : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int NoteId { get; set; }

        [Indexed]
        public int CourseId { get; set; }

        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
