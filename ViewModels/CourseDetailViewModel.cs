using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp2.Interfaces;
using MauiApp2.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MauiApp2.ViewModels
{
    public partial class CourseDetailViewModel : BaseViewModel, IDisposable
    {
        private readonly INotificationService _notificationService;
        private readonly IDatabaseService _databaseService;
        private readonly Func<Task>? _refreshCallback;
        private readonly Func<Task>? _refreshAssessments;
        private Instructor? _previousInstructor;
        private bool _disposed;

        public Term Term { get; }
        public ObservableCollection<Assessment> Assessments { get; } = new();
        public ObservableCollection<Note> Notes { get; set; } = new();

        public List<int> NotificationValues { get; } = new() { 0, 1, 3, 5, 7, 14 };
        public static List<string> StatusValues { get; } = new() { "In Progress", "Completed", "Dropped", "Plan to Take" };

        [ObservableProperty]
        private Course selectedCourse;

        [ObservableProperty]
        private string courseDetails;

        [ObservableProperty]
        private string startNotificationPreview;

        [ObservableProperty]
        private string endNotificationPreview;

        [ObservableProperty]
        private string? assessmentDueDateError;

        [ObservableProperty]
        private string? assessmentEndDateError;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDateRangeValid))]
        private string? startDateError;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDateRangeValid))]
        private string? endDateError;
        public bool IsDateRangeValid => string.IsNullOrEmpty(StartDateError) && string.IsNullOrEmpty(EndDateError);

        [ObservableProperty]
        private bool isSavingNote;

        [ObservableProperty]
        private Note newNote = new();

        [ObservableProperty]
        private Instructor instructor;

        partial void OnInstructorChanged(Instructor value)
        {
            if (value != null)
            {
                value.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(instructor.Name) || e.PropertyName == nameof(instructor.Phone) || e.PropertyName == nameof(instructor.Email))
                    {
                        ValidateInstructorFields();
                    }
                };
            }
        }

        [ObservableProperty]
        private Assessment? performanceAssessment;

        [ObservableProperty]
        private Assessment? objectiveAssessment;

        [ObservableProperty]
        private string instructorNameError;

        [ObservableProperty]
        private string instructorPhoneError;

        [ObservableProperty]
        private string instructorEmailError;

        public bool IsInstructorValid => !string.IsNullOrWhiteSpace(Instructor?.Name) && !string.IsNullOrWhiteSpace(Instructor?.Phone) && !string.IsNullOrWhiteSpace(Instructor?.Email);

        [RelayCommand]
        private async Task AddNote()
        {
            await SaveNoteAsync();
        }

        [RelayCommand]
        private async Task SaveNote()
        {
            if (string.IsNullOrWhiteSpace(NewNote.Content))
                return;

            NewNote.CourseId = SelectedCourse.CourseId;
            await _databaseService.NoteRepository.InsertAsync(NewNote);
            Notes.Add(NewNote);
            NewNote = new Note();

            if (_refreshCallback != null)
                await _refreshCallback();
        }

        [RelayCommand]
        private async Task DeleteNote(Note note)
        {
            if (note == null) return;

            await _databaseService.NoteRepository.DeleteNoteAsync(note);
            Notes.Remove(note);
        }

        [RelayCommand]
        private async Task DeleteCourse()
        {
            await _databaseService.CourseRepository.DeleteCourseAsync(SelectedCourse);

            if (_refreshCallback != null)
                await _refreshCallback();
        }

        [RelayCommand(CanExecute = nameof(CanSaveCourse))]
        private async Task SaveCourse()
        {
            await _databaseService.CourseRepository.UpdateCourseAsync(SelectedCourse);
            if (_refreshCallback != null)
                await _refreshCallback();
        }

        private bool CanSaveCourse() => IsDateRangeValid && IsInstructorValid;

        [RelayCommand]
        private async Task SaveAssessment(Assessment assessment)
        {
            await _databaseService.AssessmentRepository.UpdateAssessment(assessment);

            if (_refreshAssessments != null)
                await _refreshAssessments();
        }

        [RelayCommand]
        private async Task DeleteAssessment(Assessment assessment)
        {
            await _databaseService.AssessmentRepository.DeleteAssessment(assessment);
            Assessments.Remove(assessment);
        }

        [RelayCommand(CanExecute = nameof(CanShareNotes))]
        private async Task ShareNotes()
        {
            var allNotes = string.Join("\n\n", Notes.Where(n => !string.IsNullOrWhiteSpace(n.Content)).Select(n => n.Content));
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Course Notes",
                Text = allNotes
            });
        }

        private bool CanShareNotes() => Notes.Any(n => !string.IsNullOrWhiteSpace(n.Content));

        [RelayCommand]
        private async Task ScheduleCourseAlerts()
        {
            await SetBusyAsync(async () =>
            {
                await _notificationService.ScheduleCourseNotificationsAsync(SelectedCourse, Instructor);
            });
        }

        [RelayCommand]
        private async Task ClearAllNotifications()
        {
            await SetBusyAsync(async () =>
            {
                await _notificationService.CancelNotificationAsync();
            });
        }

        public CourseDetailViewModel(Course course, Term term, Instructor instructor, IEnumerable<Assessment> assessments, IEnumerable<Note> notes, IDatabaseService databaseService, Assessment? performanceAssessment, 
            Assessment? objectiveAssessment, Interfaces.INotificationService notifications, Func<Task>? refreshCallback, Func<Task>? refreshCourses = null, Func<Task>? refreshAssessments = null)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _notificationService = notifications ?? throw new ArgumentNullException(nameof(notifications));
            _refreshCallback = refreshCallback;
            _refreshAssessments = refreshAssessments;

            selectedCourse = course;
            Term = term;
            instructor = instructor;
            

            courseDetails = course.courseDetails;

            Assessments = new ObservableCollection<Assessment>(assessments);
            Notes = new ObservableCollection<Note>(notes);
            newNote = new Note();

            performanceAssessment = performanceAssessment;
            objectiveAssessment = objectiveAssessment;

            performanceAssessment ??= new Assessment
            {
                Name = string.Empty,
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(2),
                StartNotification = 0,
                EndNotification = 0,
                DueDate = DateTime.Now.AddDays(2),
                Type = performanceAssessment.Type
            };

            objectiveAssessment ??= new Assessment
            {
                Name = string.Empty,
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(2),
                StartNotification = 0,
                EndNotification = 0,
                DueDate = DateTime.Now.AddDays(2),
                Type = objectiveAssessment.Type
            };


            InitializeAssessmentDefaults(performanceAssessment);
            InitializeAssessmentDefaults(objectiveAssessment);

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

            selectedCourse.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(selectedCourse.start) || e.PropertyName == nameof(selectedCourse.end))
                {
                    ValidateDateRange();
                }
            };

            ScheduleStartAlertCommand = new Command(async () =>
            {
                if (selectedCourse.start > DateTime.Now)
                {
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                }
            });

            ScheduleEndAlertCommand = new Command(async () =>
            {
                if (selectedCourse.end > DateTime.Now)
                {
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                }
            });

            ToggleStartNotificationCommand = new Command(async () =>
            {
                if (selectedCourse.startNotification > 0)
                {
                    await _notificationService.CancelNotificationAsync(selectedCourse.CourseId + 1000);
                    selectedCourse.startNotification = 0;
                }
                else
                {
                    selectedCourse.startNotification = 1;
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                }

                await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);
            });

            ToggleEndNotificationCommand = new Command(async () =>
            {
                if (selectedCourse.endNotification > 0)
                {
                    await _notificationService.CancelNotificationAsync(selectedCourse.CourseId + 2000);
                    selectedCourse.endNotification = 0;
                }
                else
                {
                    selectedCourse.endNotification = 1;
                    await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                }

                await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);
            });

            selectedCourse.PropertyChanged += async (s, e) =>
            {
                try
                {
                    if (e.PropertyName == nameof(selectedCourse.startNotification) &&
                        CanScheduleNotification(selectedCourse.start, selectedCourse.startNotification, selectedCourse.CourseId, "Start"))
                    {
                        await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                        await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);
                        await ShowNotificationToastAsync("Start notification scheduled.");
                    }

                    if (e.PropertyName == nameof(selectedCourse.endNotification) &&
                        CanScheduleNotification(selectedCourse.end, selectedCourse.endNotification, selectedCourse.CourseId, "End"))
                    {
                        await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                        await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);
                        await ShowNotificationToastAsync("End notification scheduled.");
                    }

                    if (e.PropertyName == nameof(selectedCourse.start) || e.PropertyName == nameof(selectedCourse.end))
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
            SubscribeToAssessmentNotifications(performanceAssessment, "Performance assessment");
            SubscribeToAssessmentNotifications(objectiveAssessment, "Objective assessment");
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
                var courseName = selectedCourse?.courseName ?? "Current Course";

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

            selectedCourse.courseDetails = courseDetails;
            await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);

            if (instructor != null)
                await _databaseService.InstructorRepository.UpdateInstructor(instructor);

            if (performanceAssessment != null)
                await _databaseService.AssessmentRepository.UpdateAssessment(performanceAssessment);

            if (objectiveAssessment != null)
                await _databaseService.AssessmentRepository.UpdateAssessment(objectiveAssessment);

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
            if (selectedCourse == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert( "Delete Course", $"Are you sure you want to delete '{selectedCourse.courseName}'?", "Delete", "Cancel");

            if (!confirm)
                return;

            // Delete assessments
            var assessments = await _databaseService.AssessmentRepository.GetAssessmentsByCourseAsync(selectedCourse.CourseId);
            foreach (var a in assessments)
                await _databaseService.AssessmentRepository.DeleteAssessment(a);

            // Delete notes
            var notes = await _databaseService.NoteRepository.GetNotesByCourseAsync(selectedCourse.CourseId);
            foreach (var n in notes)
                await _databaseService.NoteRepository.DeleteNoteAsync(n);

            // Delete course
            await _databaseService.CourseRepository.DeleteCourseAsync(selectedCourse);

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
                await _notificationService.ScheduleCourseNotificationsAsync(new[] { selectedCourse });
                await _notificationService.ScheduleAssessmentNotificationsAsync(Assessments);
            }
        }

        private async Task ClearAllNotificationsAsync()
        { 
            //clear notifications
            selectedCourse.startNotification = 0;
            selectedCourse.endNotification = 0;
            
            await _notificationService.CancelNotificationAsync(selectedCourse.CourseId + 1000);
            await _notificationService.CancelNotificationAsync(selectedCourse.CourseId + 2000);

            await _databaseService.CourseRepository.UpdateCourseAsync(selectedCourse);

            //clear perf assessments
            if (performanceAssessment != null)
            {
                performanceAssessment.StartNotification = 0;
                performanceAssessment.EndNotification = 0;

                await _notificationService.CancelNotificationAsync(performanceAssessment.AssessmentId + 3000);
                await _notificationService.CancelNotificationAsync(performanceAssessment.AssessmentId + 4000);

                await _databaseService.AssessmentRepository.UpdateAssessment(performanceAssessment);
            }

            //clear OBJ assessments
            if (objectiveAssessment != null)
            {
                objectiveAssessment.StartNotification = 0;
                objectiveAssessment.EndNotification = 0;

                await _notificationService.CancelNotificationAsync(objectiveAssessment.AssessmentId + 3000);
                await _notificationService.CancelNotificationAsync(objectiveAssessment.AssessmentId + 4000);

                await _databaseService.AssessmentRepository.UpdateAssessment(objectiveAssessment);
            }           
            // toast
            await ShowNotificationToastAsync("All notifications have been cleared.");
        }

        private async Task SaveNoteAsync()
        {
            if (isSavingNote || string.IsNullOrWhiteSpace(newNote?.Content))
                return;

            if (_databaseService?.NoteRepository == null || selectedCourse == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Missing course or database service.", "OK");
                return;
            }

            try
            {
                isSavingNote = true;
                string trimmedContent = newNote.Content.Trim();
                bool isDuplicate = Notes.Any(n => string.Equals(n.Content?.Trim(), trimmedContent, StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                {
                    await Application.Current.MainPage.DisplayAlert("Duplicate", "This note already exists.", "OK");
                    return;
                }

                var note = new Note
                {
                    CourseId = selectedCourse.CourseId,
                    Content = newNote.Content.Trim()
                };

                await _databaseService.NoteRepository.InsertAsync(note);

                var savedNotes = await _databaseService.NoteRepository.GetNotesByCourseAsync(selectedCourse.CourseId);
                Notes.Clear();
                foreach (var n in savedNotes)
                {
                    Notes.Add(n);
                }

                newNote = new Note();
                await Application.Current.MainPage.DisplayAlert("Success", "Note saved.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save note: {ex.Message}", "OK");
            }
            finally
            {
                isSavingNote = false;
            }
        }

        private void ValidateInstructorFields()
        {
            if (instructor == null)
                return;

            instructorNameError = string.IsNullOrWhiteSpace(instructor?.Name) ? "Name is required" : null;
            instructorPhoneError = string.IsNullOrWhiteSpace(instructor?.Phone) ? "Phone is required" : null;
            InstructorEmailError = string.IsNullOrWhiteSpace(instructor?.Email)
                ? "Email is required" : (!Regex.IsMatch(instructor.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? "Invalid email format" : null);

            OnPropertyChanged(nameof(instructorNameError));
            OnPropertyChanged(nameof(instructorPhoneError));
            OnPropertyChanged(nameof(InstructorEmailError));
            OnPropertyChanged(nameof(IsInstructorValid));

            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void UpdateStartNotificationPreview()
        {
            if (selectedCourse.start != default && selectedCourse.startNotification > 0)
            {
                var notifyTime = selectedCourse.start.AddDays(-selectedCourse.startNotification);
                startNotificationPreview = $"This will notify on {notifyTime:MMMM dd, yyyy} at {notifyTime:hh:mm tt}";
            }
            else
            {
                startNotificationPreview = string.Empty;
            }
        }
        private void UpdateEndNotificationPreview()
        {
            if (selectedCourse.end != default && selectedCourse.endNotification > 0)
            {
                var notifyTime = selectedCourse.end.AddDays(-selectedCourse.endNotification);
                endNotificationPreview = $"This will notify on {notifyTime:MMMM dd, yyyy} at {notifyTime:hh:mm tt}";
            }
            else
            {
                endNotificationPreview = string.Empty;
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
            var (startError, endError) = DateValidationHelper.GetValidationErrors(selectedCourse.start, selectedCourse.end);
            startDateError = startError;
            endDateError = endError;

            OnPropertyChanged(nameof(startDateError));
            OnPropertyChanged(nameof(endDateError));
            OnPropertyChanged(nameof(IsDateRangeValid));
            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void ValidateCourseDates()
        {
            var (startError, endError) = DateValidationHelper.GetValidationErrors(selectedCourse.start, selectedCourse.end);

            startDateError = startError;
            endDateError = endError;

            OnPropertyChanged(nameof(startDateError));
            OnPropertyChanged(nameof(endDateError));
            OnPropertyChanged(nameof(IsDateRangeValid));
            (SaveCourseCommand as Command)?.ChangeCanExecute();
        }

        private void ValidateAssessmentDates(Assessment assessment)
        {
            if (assessment.DueDate < assessment.Start)
                assessmentDueDateError = "Due date cannot be before the start date.";
            else
                assessmentDueDateError = null;

            if (assessment.End < assessment.Start)
                assessmentEndDateError = "End date cannot be before the start date.";
            else
                assessmentEndDateError = null;

            OnPropertyChanged(nameof(assessmentDueDateError));
            OnPropertyChanged(nameof(assessmentEndDateError));
        }

    }
}