using CommunityToolkit.Mvvm.ComponentModel;
using MauiApp2.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MauiApp2.ViewModels
{
    public partial class InstructorViewModel : BaseViewModel
    {
        [ObservableProperty] private string _name = string.Empty;

        [ObservableProperty] private string _phone = string.Empty;

        [ObservableProperty] private string _email = string.Empty;

        public ObservableCollection<string> Errors { get; } = new();

        public bool IsValid => Errors.Count == 0 && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Phone) && !string.IsNullOrWhiteSpace(Email);

        public InstructorViewModel(Instructor? instructor = null)
        {
            instructor ??= new Instructor();
            Name = instructor.Name;
            Phone = instructor.Phone;
            Email = instructor.Email;
            Validate();
        }

        partial void OnNameChanged(string value) => Validate();
        partial void OnPhoneChanged(string value) => Validate();
        partial void OnEmailChanged(string value) => Validate();

        private void Validate()
        {
            Errors.Clear();

            if (string.IsNullOrWhiteSpace(Name))
                Errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(Phone))
                Errors.Add("Phone is required");

            else if (!Regex.IsMatch(Phone, @"^\+?[\d\s-]{10,}$"))
                Errors.Add("Invalid phone format");

            if (string.IsNullOrWhiteSpace(Email))
                Errors.Add("Email is required");

            else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                Errors.Add("Invalid email format");
        }

        public Instructor ToModel(int instructorId = 0)
        {
            return new()
            {
                InstructorId = instructorId,
                Name = Name ?? string.Empty,
                Phone = Phone ?? string.Empty,
                Email = Email ?? string.Empty
            };
        }
    }
}

