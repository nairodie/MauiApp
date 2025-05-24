using MauiApp2.Interfaces;
using MauiApp2.Models;
using MauiApp2.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;


namespace MauiApp2.ViewModels
{
    public class CourseDetailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Func<Task>? _refreshCallback;
        private readonly Func<Task>? _refreshAssessments;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly Interfaces.INotificationService _notificationService;
        private readonly IDatabaseService _databaseService;
        private bool _isSavingNote;

        public Term Term { get; }

        private Course _selectedCourse;
        public Course SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    OnPropertyChanged(nameof(SelectedCourse));
                }
            }
        }

        private string _courseDetails;
        public string CourseDetails
        {
            get => _courseDetails;
            set
            {
                if (_courseDetails != value)
                {
                    _courseDetails = value;
                    OnPropertyChanged(nameof(CourseDetails));
                }
            }
        }

        private string _startNotificationPreview;
        public string StartNotificationPreview
        {
            get => _startNotificationPreview;
            set
            {
                if (value != _startNotificationPreview)
                {
                    _startNotificationPreview = value;
                    OnPropertyChanged(nameof(StartNotificationPreview));
                }
            }
        }

        private string _endNotificationPreview;
        public string EndNotificationPreview
        {
            get => _endNotificationPreview;
            set
            {
                if (value != _endNotificationPreview)
                {
                    _endNotificationPreview = value;
                    OnPropertyChanged(nameof(EndNotificationPreview));
                }
            }
        }

        private string? _assessmentDueDateError;
        public string? AssessmentDueDateError
        {
            get => _assessmentDueDateError;
            set
            {
                if (value != _assessmentDueDateError)
                {
                    _assessmentDueDateError = value;
                    OnPropertyChanged(nameof(AssessmentDueDateError));
                }
            }
        }

        private string? _assessmentEndDateError;
        public string? AssessmentEndDateError
        {
            get => _assessmentEndDateError;
            set
            {
                if (value != _assessmentEndDateError)
                {
                    _assessmentEndDateError = value;
                    OnPropertyChanged(nameof(AssessmentEndDateError));
                }
            }
        }


        private string _startDateError;
        public string? StartDateError
        {
            get => _startDateError;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _startDateError = value;
                    OnPropertyChanged(nameof(StartDateError));
                }
            }
        }

        private string _endDateError;
        public string? EndDateError
        {
            get => _endDateError;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _endDateError = value;
                    OnPropertyChanged(nameof(EndDateError));
                }
            }
        }
        public bool IsDateRangeValid => string.IsNullOrEmpty(StartDateError) && string.IsNullOrEmpty(EndDateError);
        public Instructor Instructor { get; }
        public Assessment? PerformanceAssessment { get; set; }
        public Assessment? ObjectiveAssessment { get; set; }
        public ObservableCollection<Assessment> Assessments { get; }

        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
        public List<int> NotificationValues { get; } = new() { 0, 1, 3, 5, 7, 14 };

        public List<string> StatusValues { get; } = new List<string>
        {
            "In Progress", "Completed", "Dropped", "Plan to Take"
        };
        public string InstructorNameError { get; private set; }
        public string InstructorPhoneError { get; private set; }
        public string InstructorEmailError { get; private set; }
        public bool IsInstructorValid => !string.IsNullOrWhiteSpace(Instructor?.Name) && !string.IsNullOrWhiteSpace(Instructor?.Phone) && !string.IsNullOrWhiteSpace(Instructor?.Email);
        public ICommand SaveNoteCommand { get; }
        public ICommand DeleteCourseCommand { get; }
        public ICommand DeleteNote { get; }
        public ICommand ToggleStartNotificationCommand { get; }
        public ICommand ToggleEndNotificationCommand { get; }
        public ICommand ScheduleStartAlertCommand { get; }
        public ICommand ScheduleEndAlertCommand { get; }
        public ICommand ShareNotesCommand { get; }
        public ICommand ScheduleCourseAlertsCommand { get; }
        public ICommand SaveCourseCommand { get; }
        public ICommand SaveAssessmentCommand { get; }
        public ICommand DeleteAssessmentCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand ClearAllNotificationsCommand { get; }
        public ICommand AddNoteCommand => new Command(async () => await SaveNoteAsync());

        private Note _newNote = new Note();
        public Note NewNote
        {
            get => _newNote;
            set
            {
                _newNote = value;
                OnPropertyChanged(nameof(NewNote));
            }
        }
        
        public CourseDetailViewModel(Course course, Term term, Instructor instructor, IEnumerable<Assessment> assessments, IEnumerable<Note> notes, IDatabaseService databaseService, Assessment? performanceAssessment, 
            Assessment? objectiveAssessment, Interfaces.INotificationService notifications, Func<Task>? refreshCallback, Func<Task>? refreshCourses = null, Func<Task>? refreshAssessments = null)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _notificationService = notifications ?? throw new ArgumentNullException(nameof(notifications));
            _refreshCallback = refreshCallback;
            _refreshAssessments = refreshAssessments;

            SelectedCourse = course;
            Term = term;
            Instructor = instructor;

            if (instructor != null)
            {
                Instructor.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Instructor.Name) || e.PropertyName == nameof(Instructor.Phone) || e.PropertyName == nameof(Instructor.Email))
                    {
                        ValidateInstructorFields();
                    }
                };
            }

            CourseDetails = course.CourseDetails;

            Assessments = new ObservableCollection<Assessment>(assessments);
            Notes = new ObservableCollection<Note>(notes);
            NewNote = new Note();

            PerformanceAssessment = performanceAssessment;
            ObjectiveAssessment = objectiveAssessment;

            PerformanceAssessment ??= new Assessment
            {
                Name = string.Empty,
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(2),
                StartNotification = 0,
                EndNotification = 0,
                DueDate = DateTime.Now.AddDays(2),
                Type = performanceAssessment.Type
            };

            ObjectiveAssessment ??= new Assessment
            {
                Name = string.Empty,
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(2),
                StartNotification = 0,
                EndNotification = 0,
                DueDate = DateTime.Now.AddDays(2),
                Type = objectiveAssessment.Type
            };


            InitializeAssessmentDefaults(PerformanceAssessment);
            InitializeAssessmentDefaults(ObjectiveAssessment);

            Notes.CollectionChanged += OnNotesCollectionChanged;

            //Commands
            SaveNoteCommand = new Command(async () => await SaveNoteAsync());
            DeleteNoteCommand = new Command<Note>(async (note) => await DeleteNoteAsync(note));
            DeleteCourseCommand = new Command(async () => await DeleteCourseAsync());
            SaveCourseCommand = new Command(async () => await SaveCourseAsync(), () => IsDateRangeValid && IsInstructorValid);
            SaveAssessmentCommand = new Command<Assessment>(async (a) => await SaveAssessmentAsync(a));
            DeleteAssessmentCommand = new Command<Assessment>(async (a) => await DeleteAssessmentAsync(a));
            ShareNotesCommand = new Command(async () => await ExecuteShareNotesCommand(), () => Notes.Any(n => !string.IsNullOrWhiteSpace(n.Content)));
            ScheduleCourseAlertsCommand = new Command(async () => await ScheduleCourseAlertsAsync());
            ClearAllNotificationsCommand = new Command(async () => await ClearAllNotificationsAsync());

            SelectedCourse.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCourse.Start) || e.PropertyName == nameof(SelectedCourse.End))
                {
                    ValidateDateRange();
                }
            };

            ScheduleStartAlertCommand = new Command(async () =>
            {
                if (SelectedCourse.Start > DateTime.Now)
                {
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                }
            });

            ScheduleEndAlertCommand = new Command(async () =>
            {
                if (SelectedCourse.End > DateTime.Now)
                {
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                }
            });

            ToggleStartNotificationCommand = new Command(async () =>
            {
                if (SelectedCourse.StartNotification > 0)
                {
                    await _notificationService.CancelNotificationAsync(SelectedCourse.CourseId + 1000);
                    SelectedCourse.StartNotification = 0;
                }
                else
                {
                    SelectedCourse.StartNotification = 1;
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                }

                await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);
            });

            ToggleEndNotificationCommand = new Command(async () =>
            {
                if (SelectedCourse.EndNotification > 0)
                {
                    await _notificationService.CancelNotificationAsync(SelectedCourse.CourseId + 2000);
                    SelectedCourse.EndNotification = 0;
                }
                else
                {
                    SelectedCourse.EndNotification = 1;
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                }

                await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);
            });

            SelectedCourse.PropertyChanged += async (s, e) =>
            {
                try
                {
                    if (e.PropertyName == nameof(SelectedCourse.StartNotification) &&
                        CanScheduleNotification(SelectedCourse.Start, SelectedCourse.StartNotification, SelectedCourse.CourseId, "Start"))
                    {
                        await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                        await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);
                        await ShowNotificationToastAsync("Start notification scheduled.");
                    }

                    if (e.PropertyName == nameof(SelectedCourse.EndNotification) &&
                        CanScheduleNotification(SelectedCourse.End, SelectedCourse.EndNotification, SelectedCourse.CourseId, "End"))
                    {
                        await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                        await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);
                        await ShowNotificationToastAsync("End notification scheduled.");
                    }

                    if (e.PropertyName == nameof(SelectedCourse.Start) || e.PropertyName == nameof(SelectedCourse.End))
                    {
                        OnPropertyChanged(nameof(IsDateRangeValid));
                        (SaveCourseCommand as Command)?.ChangeCanExecute();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Notification Error] {ex.Message}");
                }
            };
            SubscribeToAssessmentNotifications(PerformanceAssessment, "Performance assessment");
            SubscribeToAssessmentNotifications(ObjectiveAssessment, "Objective assessment");
            UpdateStartNotificationPreview();
            UpdateEndNotificationPreview();

            ValidateInstructorFields();
            ValidateDateRange();
            (SaveCourseCommand as Command)?.ChangeCanExecute();

        }

        private async Task ShowNotificationToastAsync(string message)
        {
#if ANDROID
            var context = Android.App.Application.Context;
            var toast = Android.Widget.Toast.MakeText(context, message, Android.Widget.ToastLength.Short);
            toast.Show();
#elif IOS
            await Application.Current.MainPage.DisplayAlert("Notification", message, "OK");
#else
            await Application.Current.MainPage.DisplayAlert("Notification", message, "OK");
#endif
        }

        private static void InitializeAssessmentDefaults(Assessment? assessment)
        {
            if (assessment == null)
                return;

            assessment.Start = assessment.Start;

            if (assessment.DueDate == default || assessment.DueDate == assessment.End)
                assessment.DueDate = assessment.End;
        }

        private async Task ExecuteShareNotesCommand()
        {
            try
            {
                // Filter and prepare notes content
                var validNotes = Notes.Where(n => !string.IsNullOrWhiteSpace(n.Content?.Trim())).Select(n => n.Content.Trim()).ToList();
                if (validNotes.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("No Notes", "There are no notes available to share.", "OK");
                    return;
                }

                var allNotes = string.Join("\n\n", validNotes);
                var courseName = SelectedCourse?.CourseName ?? "Current Course";

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = allNotes,
                    Title = $"Notes for {courseName}",
                    Subject = $"Course Notes: {courseName}"
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to share notes: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "Could not share notes. Please try again.", "OK");
            }
        }

        private async Task DeleteNoteAsync(Note note)
        {
            if (note == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Delete this note?", "Yes", "No");
            if (!confirm) return;

            await _databaseService.NoteRepository.DeleteNoteAsync(note);
            Notes.Remove(note);
        }

        private async Task SaveCourseAsync()
        {
            if (!IsInstructorValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "All instructor fields must be filled.", "OK");
                return;
            }

            SelectedCourse.CourseDetails = CourseDetails;
            await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);

            if (Instructor != null)
                await _databaseService.InstructorRepository.UpdateInstructor(Instructor);

            if (PerformanceAssessment != null)
                await _databaseService.AssessmentRepository.UpdateAssessment(PerformanceAssessment);

            if (ObjectiveAssessment != null)
                await _databaseService.AssessmentRepository.UpdateAssessment(ObjectiveAssessment);

            await Application.Current.MainPage.DisplayAlert("Saved", "Course details saved.", "OK");

            if (_refreshCallback is not null)
                await _refreshCallback.Invoke();

            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task SaveAssessmentAsync(Assessment a)
        {
            if (a == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Assessment not found.", "OK");
                return;
            }

            if (a.DueDate == default)
                a.DueDate = a.End;

            await _databaseService.AssessmentRepository.UpdateAssessment(a);
            await _notificationService.ScheduleAssessmentNotificationsAsync(new[] { a });
            await Application.Current.MainPage.DisplayAlert("Saved", "Assessment saved.", "OK");

            if (_refreshAssessments is not null)
                await _refreshAssessments.Invoke();
        }

        private async Task DeleteAssessmentAsync(Assessment a)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Delete Assessment?", $"Are you sure to delete {a.Name}?", "Delete", "Cancel");
            if (!confirm) return;

            await _databaseService.AssessmentRepository.DeleteAssessment(a);
            Assessments.Remove(a);

            if (_refreshAssessments is not null)
                await _refreshAssessments.Invoke();
        }

        private async Task DeleteCourseAsync()
        {
            if (SelectedCourse == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert( "Delete Course", $"Are you sure you want to delete '{SelectedCourse.CourseName}'?", "Delete", "Cancel");

            if (!confirm)
                return;

            // Delete assessments
            var assessments = await _databaseService.AssessmentRepository.GetAssessmentsByCourseAsync(SelectedCourse.CourseId);
            foreach (var a in assessments)
                await _databaseService.AssessmentRepository.DeleteAssessment(a);

            // Delete notes
            var notes = await _databaseService.NoteRepository.GetNotesByCourseAsync(SelectedCourse.CourseId);
            foreach (var n in notes)
                await _databaseService.NoteRepository.DeleteNoteAsync(n);

            // Delete course
            await _databaseService.CourseRepository.DeleteCourseAsync(SelectedCourse);

            // Callback to refresh (MainViewModel)
            if (_refreshCallback is not null)
                await _refreshCallback.Invoke();          

            // Navigate back
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task ScheduleCourseAlertsAsync()
        {
            if (await _notificationService.EnsureNotificationPermissionAsync())
            {
                await _notificationService.ScheduleCourseNotificationsAsync(new[] { SelectedCourse });
                await _notificationService.ScheduleAssessmentNotificationsAsync(Assessments);
            }
        }

        private async Task ClearAllNotificationsAsync()
        { 
            //clear notifications
            SelectedCourse.StartNotification = 0;
            SelectedCourse.EndNotification = 0;
            
            await _notificationService.CancelNotificationAsync(SelectedCourse.CourseId + 1000);
            await _notificationService.CancelNotificationAsync(SelectedCourse.CourseId + 2000);

            await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);

            //clear perf assessments
            if (PerformanceAssessment != null)
            {
                PerformanceAssessment.StartNotification = 0;
                PerformanceAssessment.EndNotification = 0;

                await _notificationService.CancelNotificationAsync(PerformanceAssessment.AssessmentId + 3000);
                await _notificationService.CancelNotificationAsync(PerformanceAssessment.AssessmentId + 4000);

                await _databaseService.AssessmentRepository.UpdateAssessment(PerformanceAssessment);
            }

            //clear OBJ assessments
            if (ObjectiveAssessment != null)
            {
                ObjectiveAssessment.StartNotification = 0;
                ObjectiveAssessment.EndNotification = 0;

                await _notificationService.CancelNotificationAsync(ObjectiveAssessment.AssessmentId + 3000);
                await _notificationService.CancelNotificationAsync(ObjectiveAssessment.AssessmentId + 4000);

                await _databaseService.AssessmentRepository.UpdateAssessment(ObjectiveAssessment);
            }           
            // toast
            await ShowNotificationToastAsync("All notifications have been cleared.");
        }

        private async Task SaveNoteAsync()
        {
            if (_isSavingNote || string.IsNullOrWhiteSpace(NewNote?.Content))
                return;

            if (_databaseService?.NoteRepository == null || SelectedCourse == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Missing course or database service.", "OK");
                return;
            }

            try
            {
                _isSavingNote = true;
                string trimmedContent = NewNote.Content.Trim();
                bool isDuplicate = Notes.Any(n => string.Equals(n.Content?.Trim(), trimmedContent, StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                {
                    await Application.Current.MainPage.DisplayAlert("Duplicate", "This note already exists.", "OK");
                    return;
                }

                var note = new Note
                {
                    CourseId = SelectedCourse.CourseId,
                    Content = NewNote.Content.Trim()
                };

                await _databaseService.NoteRepository.InsertAsync(note);

                var savedNotes = await _databaseService.NoteRepository.GetNotesByCourseAsync(SelectedCourse.CourseId);
                Notes.Clear();
                foreach (var n in savedNotes)
                {
                    Notes.Add(n);
                }

                NewNote = new Note();
                await Application.Current.MainPage.DisplayAlert("Success", "Note saved.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save note: {ex.Message}", "OK");
            }
            finally
            {
                _isSavingNote = false;
            }
        }

        private void ValidateInstructorFields()
        {
            if (Instructor == null)
                return;

            InstructorNameError = string.IsNullOrWhiteSpace(Instructor?.Name) ? "Name is required" : null;
            InstructorPhoneError = string.IsNullOrWhiteSpace(Instructor?.Phone) ? "Phone is required" : null;
            InstructorEmailError = string.IsNullOrWhiteSpace(Instructor?.Email)
                ? "Email is required" : (!Regex.IsMatch(Instructor.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? "Invalid email format" : null);

            OnPropertyChanged(nameof(InstructorNameError));
            OnPropertyChanged(nameof(InstructorPhoneError));
            OnPropertyChanged(nameof(InstructorEmailError));
            OnPropertyChanged(nameof(IsInstructorValid));

            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void UpdateStartNotificationPreview()
        {
            if (SelectedCourse.Start != default && SelectedCourse.StartNotification > 0)
            {
                var notifyTime = SelectedCourse.Start.AddDays(-SelectedCourse.StartNotification);
                StartNotificationPreview = $"This will notify on {notifyTime:MMMM dd, yyyy} at {notifyTime:hh:mm tt}";
            }
            else
            {
                StartNotificationPreview = string.Empty;
            }
        }
        private void UpdateEndNotificationPreview()
        {
            if (SelectedCourse.End != default && SelectedCourse.EndNotification > 0)
            {
                var notifyTime = SelectedCourse.End.AddDays(-SelectedCourse.EndNotification);
                EndNotificationPreview = $"This will notify on {notifyTime:MMMM dd, yyyy} at {notifyTime:hh:mm tt}";
            }
            else
            {
                EndNotificationPreview = string.Empty;
            }
        }

        //Helpers
        private void OnNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            (ShareNotesCommand as Command)?.ChangeCanExecute();
        }

        private bool CanScheduleNotification(DateTime date, int daysBefore, int courseId, string label)
        {
            if (date == default || courseId <= 0 || daysBefore <= 0)
            {
                Debug.WriteLine($"[Notification] {label} - Skipped: invalid date, course ID, or daysBefore");
                return false;
            }

            var notifyTime = date.AddDays(-daysBefore);
            if (notifyTime <= DateTime.Now)
            {
                Debug.WriteLine($"[Notification] {label} - Skipped: notify time is in the past");
                return false;
            }

            return true;
        }

        private static async Task HandleCourseNotificationAsync(DateTime date, int daysBefore, string label, Action resetAction, Func<Task> scheduleAction)
        {
            var notifyTime = date.AddDays(-daysBefore);
            Debug.WriteLine($"[Notification] {label} notifyTime: {notifyTime}");

            if (notifyTime <= DateTime.Now)
            {
                resetAction();
                await Application.Current.MainPage.DisplayAlert("Invalid Notification", $"{label} notification would occur in the past.", "OK");
            }
            else
            {
                await scheduleAction();
            }
        }

        private static async Task HandleAssessmentNotificationAsync(DateTime date, int daysBefore, string label, Action resetAction, Func<Task> scheduleAction)
        {
            var notifyTime = date.AddDays(-daysBefore);
            if (notifyTime <= DateTime.Now)
            {
                resetAction();
                await Application.Current.MainPage.DisplayAlert("Invalid Notification", $"{label} notification would occur in the past.", "OK");
            }
            else
            {
                await scheduleAction();
            }
        }

        private void SubscribeToAssessmentNotifications(Assessment? assessment, string label)
        {
            if (assessment == null)
                return;

            assessment.PropertyChanged += async (_, e) =>
            {
                try
                {
                    if (e.PropertyName == nameof(Assessment.StartNotification))
                    {
                        await HandleAssessmentNotificationAsync(assessment.Start, assessment.StartNotification, $"{label} start",
                            () => assessment.StartNotification = 0,
                            async () =>
                            {
                                await _notificationService.ScheduleAssessmentNotificationsAsync(new[] { assessment });
                                await _databaseService.AssessmentRepository.UpdateAssessment(assessment);
                            });
                    }

                    if (e.PropertyName == nameof(Assessment.EndNotification))
                    {
                        await HandleAssessmentNotificationAsync(assessment.End, assessment.EndNotification, $"{label} end",
                            () => assessment.EndNotification = 0,
                            async () =>
                            {
                                await _notificationService.ScheduleAssessmentNotificationsAsync(new[] { assessment });
                                await _databaseService.AssessmentRepository.UpdateAssessment(assessment);
                            });
                    }

                    if (e.PropertyName == nameof(Assessment.Start) || e.PropertyName == nameof(Assessment.End) || e.PropertyName == nameof(Assessment.DueDate))
                    {
                        ValidateAssessmentDates(assessment);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Assessment Notification Error] {ex.Message}");
                }
            };
        }

        private void ValidateDateRange()
        {
            var (startError, endError) = DateValidationHelper.GetValidationErrors(SelectedCourse.Start, SelectedCourse.End);
            StartDateError = startError;
            EndDateError = endError;

            OnPropertyChanged(nameof(StartDateError));
            OnPropertyChanged(nameof(EndDateError));
            OnPropertyChanged(nameof(IsDateRangeValid));
            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void ValidateCourseDates()
        {
            var (startError, endError) = DateValidationHelper.GetValidationErrors(SelectedCourse.Start, SelectedCourse.End);

            StartDateError = startError;
            EndDateError = endError;

            OnPropertyChanged(nameof(StartDateError));
            OnPropertyChanged(nameof(EndDateError));
            OnPropertyChanged(nameof(IsDateRangeValid));
            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void ValidateAssessmentDates(Assessment assessment)
        {
            if (assessment.DueDate < assessment.Start)
                AssessmentDueDateError = "Due date cannot be before the start date.";
            else
                AssessmentDueDateError = null;

            if (assessment.End < assessment.Start)
                AssessmentEndDateError = "End date cannot be before the start date.";
            else
                AssessmentEndDateError = null;

            OnPropertyChanged(nameof(AssessmentDueDateError));
            OnPropertyChanged(nameof(AssessmentEndDateError));
        }

    }
}