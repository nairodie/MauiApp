using SQLite;
using System.ComponentModel;

namespace MauiApp2.Models
{
    [Table("Instructors")]
    public class Instructor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [PrimaryKey, AutoIncrement]
        public int InstructorId { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(_name)); }
        }

        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
