using MauiApp2.Interfaces;
using MauiApp2.Models;
using MauiApp2.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MauiApp2.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly Interfaces.INotificationService _notifications;
        private readonly DummyDataService _dummyDataService;
        private bool _isInitialized = false;

        public bool ShowAddCourseButton => SelectedTerm != null && Courses.Count(c => c.TermId == SelectedTerm.TermId) < 6;
        public bool CanAddCourse => SelectedTerm != null && Courses.Count(c => c.TermId == SelectedTerm.TermId) < 6;
        public bool CanDeleteSelectedTerm => SelectedTerm != null && Terms.Count > 1;
        public bool CanSaveTerm => IsTermDirty && IsTermDateRangeValid;
        public ObservableCollection<Term> Terms { get; } = new();
        public ObservableCollection<Course> Courses { get; } = new();
        public ObservableCollection<Assessment> Assessments { get; } = new();
        public ObservableCollection<Instructor> Instructors { get; } = new();
        public ObservableCollection<Note> Notes { get; } = new();
        public Func<Task>? RefreshTermData { get; set; }

        private Term _selectedTerm;
        public Term? SelectedTerm
        {
            get => _selectedTerm;
            set
            {
                if (SetProperty(ref _selectedTerm, value))
                {
                    IsTermDirty = false;
                    ManageTermEventHandlers();

                    if (_selectedTerm != null && Terms.Any(t => t.TermId == _selectedTerm.TermId))
                    {
                        Task.Run(() => LoadTermData(_selectedTerm.TermId));
                    }

                    else
                    {
                        Courses.Clear();
                        Assessments.Clear();
                        Notes.Clear();
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTermDirty));
                    (DeleteTermCommand as Command)?.ChangeCanExecute();
                    UpdateCommandStates();
                }
            }
        }

        private string _termStartDateError;
        public string TermStartDateError
        {
            get => _termStartDateError;
            set => SetProperty(ref _termStartDateError, value);
        }

        private string _termEndDateError;
        public string TermEndDateError
        {
            get => _termEndDateError;
            set => SetProperty(ref _termEndDateError, value);
        }

        private string _termDateError;
        public string? TermDateError
        {
            get => _termDateError;
            set => SetProperty(ref _termDateError, value);
        }

        public bool IsTermDateRangeValid => string.IsNullOrWhiteSpace(TermStartDateError) && string.IsNullOrWhiteSpace(TermEndDateError);

        private void ManageTermEventHandlers()
        {
            // Remove handler from previous term
            if (_selectedTerm != null)
            {
                _selectedTerm.PropertyChanged -= OnSelectedTermChanged;
            }

            // Add handler to new term
            if (_selectedTerm != null)
            {
                _selectedTerm.PropertyChanged += OnSelectedTermChanged;
            }
        }

        private Course _selectedCourse;
        public Course? SelectedCourse
        {
            get => _selectedCourse;
            set => SetProperty(ref _selectedCourse, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isTermDirty;
        public bool IsTermDirty
        {
            get => _isTermDirty;
            set
            {
                if (SetProperty(ref _isTermDirty, value))
                {
                    UpdateCommandStates();
                    //(SaveTermCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        //commands
        public ICommand RefreshCommand { get; }
        public ICommand AddTermCommand { get; }
        public ICommand AddCourseCommand { get; }
        public ICommand SaveTermCommand { get; }
        public ICommand GenerateDummyDataCommand { get; }
        public ICommand SelectTermCommand { get; }
        public ICommand OpenCourseDetailCommand { get; }
        public ICommand DeleteTermCommand { get; }
        public ICommand ResetAppCommand { get; }
        public ICommand CreateTermCommand { get; }

        public static List<string> StatusValues { get; } = new List<string>
        {
            "In Progress", "Completed", "Dropped", "Plan to Take"
        };

        public static List<int> NotificationValues { get; } = new List<int>
        {
            0, 1, 2, 3, 5, 7, 14
        };

        public MainViewModel(IDatabaseService databaseService, DummyDataService dummyDataService, Interfaces.INotificationService notificationService)
        {
            _databaseService = databaseService;
            _dummyDataService = dummyDataService;
            _notifications = notificationService;

            //commands
            RefreshCommand = new Command(async () => await InitializeAsync());
            AddTermCommand = new Command(async () => await AddNewTermAsync());
            GenerateDummyDataCommand = new Command(async () => await GenerateDummyDataAsync());
            OpenCourseDetailCommand = new Command<Course>(async (course) => await OnOpenCourseDetailAsync(course));
            DeleteTermCommand = new Command(async () => await DeleteSelectedTermAsync(), () => CanDeleteSelectedTerm);
            SelectTermCommand = new Command<Term>(term => SelectedTerm = term);
            SaveTermCommand = new Command(execute: SaveSelectedTerm, canExecute: () => IsTermDirty);
            AddCourseCommand = new Command(execute: async () => await AddCourse(), canExecute: () => CanAddCourse);
            ResetAppCommand = new Command(async () => await ResetAppAsync());
            RefreshTermData = async () => await LoadTermData(SelectedTerm.TermId);
            CreateTermCommand = new Command(async () => await CreateNewTermAsync());
        }      

        private async Task ResetAppAsync()
        {
            try
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("⚠️ Reset Application", "This will PERMANENTLY DELETE ALL your data" + "This action cannot be undone!", "DELETE EVERYTHING", "CANCEL");

                if (!confirm)
                    return;

                IsBusy = true;

                // Clear all data repositories
                var deleteTasks = new[]
                {
                    _databaseService.NoteRepository.DeleteAllAsync(),
                    _databaseService.AssessmentRepository.DeleteAllAsync(),
                    _databaseService.CourseRepository.DeleteAllAsync(),
                    _databaseService.InstructorRepository.DeleteAllAsync(),
                    _databaseService.TermRepository.DeleteAllAsync()
                };

                await Task.WhenAll(deleteTasks);

                //clear all on main thread
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Terms.Clear();
                    Courses.Clear();
                    Assessments.Clear();
                    Notes.Clear();
                    Instructors.Clear();

                    SelectedTerm = null;
                    SelectedCourse = null;
                });

                //reset terms
                 await _databaseService.ResetTermsAutoIncrementAsync();

                // load dummy data again
                await _dummyDataService.GenerateAllDummyDataAsync();
                await LoadTerms();

                if (Terms.Any())
                {
                    SelectedTerm = Terms.First();
                }

                // Update UI
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Reset Failed", $"An error occurred while resetting the app: {ex.Message}","OK");
                Debug.WriteLine($"ResetAppAsync error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddCourse()
        {
            if (SelectedTerm == null)
                return;

            int count = Courses.Count(c => c.TermId == SelectedTerm.TermId);
            if (count >= 6)
            {
                await Application.Current.MainPage.DisplayAlert("Course limit reached", "You can only add up to 6 courses per term.", "OK");
                return;
            }

            string name = await Application.Current.MainPage.DisplayPromptAsync("New Course", "Enter the course name", "Save", "Cancel", "e.g. Biology 101");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var instructorId = await CreateDefaultInstructorAsync();

            var newCourse = new Course
            {
                TermId = SelectedTerm.TermId,
                InstructorId = instructorId,
                CourseName = name.Trim(),
                Start = SelectedTerm.Start,
                End = SelectedTerm.Start.AddMonths(3),
                Status = "Plan to Take",
                CourseDetails = "Enter course details...",
                PerformanceAssessmentId = 0,
                ObjectiveAssessmentId = 0
            };

            await _databaseService.CourseRepository.AddCourseAsync(newCourse);
            var courseId = await _databaseService.CourseRepository.GetLastInsertedCourseIdAsync();
            var insertedCourse = await _databaseService.CourseRepository.GetCourseAsync(courseId);

            if (insertedCourse == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to add course.", "OK");
                return;
            }           

            var performance = new Assessment
            {
                CourseId = insertedCourse.CourseId,
                Name = "Performance Assessment",
                Details = "Details here...",
                Type = Assessment.AssessmentType.Performance,
                Start = DateTime.Now
            };

            await _databaseService.AssessmentRepository.InsertAsync(performance);
            performance.AssessmentId = await _databaseService.AssessmentRepository.GetLastInsertedAssessmentIdAsync();

            var objective = new Assessment
            {
                CourseId = insertedCourse.CourseId,
                Name = "Objective Assessment",
                Details = "Details here...",
                Type = Assessment.AssessmentType.Objective,
                Start = DateTime.Now
            };

            await _databaseService.AssessmentRepository.InsertAsync(objective);
            objective.AssessmentId = await _databaseService.AssessmentRepository.GetLastInsertedAssessmentIdAsync();

            insertedCourse.PerformanceAssessmentId = performance.AssessmentId;
            insertedCourse.ObjectiveAssessmentId = objective.AssessmentId;

            await _databaseService.CourseRepository.UpdateCourseAsync(insertedCourse);

            Courses.Add(insertedCourse);
            OnPropertyChanged(nameof(Courses)); 
            OnPropertyChanged(nameof(ShowAddCourseButton));
            OnPropertyChanged(nameof(CanAddCourse));
            (AddCourseCommand as Command)?.ChangeCanExecute();
        }

        private async Task<int> CreateDefaultInstructorAsync()
        {
            var newInstructor = new Instructor
            {
                Name = "Enter Name",
                Phone = "555-000-0000",
                Email = "instructor@example.com"
            };

            int instructorId = await _databaseService.InstructorRepository.InsertAsync(newInstructor);
            return instructorId;
        }

        private async void SaveSelectedTerm()
        {
            if (SelectedTerm != null)
            {
                return;
            }

            if (SelectedTerm.End <= SelectedTerm.Start)
            {
                await Application.Current.MainPage.DisplayAlert("Invalid Dates", "End date must be after start date.", "OK");
                return;
            }

            _databaseService.TermRepository.UpdateTermAsync(SelectedTerm);

            await Application.Current.MainPage.DisplayAlert("Saved", "Term dates updated successfully", "OK");

            await LoadTerms();

            SelectedTerm = Terms.FirstOrDefault(t => t.TermId == SelectedTerm.TermId);
        }

        private async Task DeleteSelectedTermAsync()
        {
            if (SelectedTerm == null) 
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Delete Term?", $"Are you sure you want to delete {SelectedTerm.TermName} and all related data?", "Delete", "Cancel");
            
            if (!confirm) 
                return;

            await _databaseService.TermRepository.DeleteTermCascadeAsync(SelectedTerm.TermId);

            Terms.Remove(SelectedTerm);
            SelectedTerm = Terms.FirstOrDefault();

            if (SelectedTerm != null)
            {
                await LoadTermData(SelectedTerm.TermId);
            }
            else
            {
                await AddNewTermAsync();
            }

            UpdateCommandStates();            
        }

        private async void SyncCoursesToTermDates()
        {
            var courses = await _databaseService.CourseRepository.GetCoursesByTermAsync(SelectedTerm.TermId);

            foreach (var course in courses)
            {
                course.Start = SelectedTerm.Start;
                course.End = SelectedTerm.End;
                await _databaseService.CourseRepository.UpdateCourseAsync(course);
            }
            await LoadTermData(SelectedTerm.TermId).ConfigureAwait(false);
        }

        private async void SyncAssessmentsToTermDates()
        {
            if (SelectedTerm == null)
                return;

            var assessments = await _databaseService.AssessmentRepository.GetAssessmentsByTermAsync(SelectedTerm.TermId);

            foreach (var item in assessments)
            {
                item.Start = SelectedTerm.Start;
                item.End = SelectedTerm.End;
                item.DueDate = SelectedTerm.End;
                await _databaseService.AssessmentRepository.UpdateAssessment(item);
            }
            await LoadTermData(SelectedTerm.TermId);
        }

        private async Task CreateNewTermAsync()
        {
            string termName = await Application.Current.MainPage.DisplayPromptAsync("New Term", "Enter term name:", "Save", "Cancel", "e.g. Spring 2025");

            if (string.IsNullOrWhiteSpace(termName))
                return;

            // Check for duplicate name
            if (Terms.Any(t => t.TermName.Equals(termName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await Application.Current.MainPage.DisplayAlert("Duplicate Term", "A term with that name already exists.", "OK");
                return;
            }

            var newTerm = new Term
            {
                TermName = termName.Trim(),
                Start = DateTime.Today,
                End = DateTime.Today.AddMonths(6)
            };

            await _databaseService.TermRepository.InsertAsync(newTerm);
            await LoadTerms();

            SelectedTerm = Terms.FirstOrDefault(t => t.TermName == newTerm.TermName);

            await Application.Current.MainPage.DisplayAlert("Success", "Term created. You can now add courses.", "OK");
        }

        private async Task AddNewTermAsync()
        {
            try
            {
                IsBusy = true;

                string name = await GetTermNameFromUserAsync();

                if (string.IsNullOrWhiteSpace(name))
                    return;

                if (await IsDuplicateTermNameAsync(name))
                    return;

                if (Terms.Any(t => t.TermName.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    await Application.Current.MainPage.DisplayAlert("Duplicate Term", $"A term named '{name.Trim()}' already exists. Please choose a different name.", "OK");
                    return;
                }

                var nextId = Terms.Count > 0 ? Terms.Max(t => t.TermId) + 1 : 1;

                var startDate = DateTime.Today;
                var endDate = startDate.AddMonths(6);
                var newTerm = new Term($"Term {nextId}", startDate, endDate);

                await _databaseService.TermRepository.InsertAsync(newTerm);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Terms.Add(newTerm);
                    SelectedTerm = newTerm;
                    UpdateCommandStates();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding new term: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "Could not create new term", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<string> GetTermNameFromUserAsync()
        {
            return await Application.Current.MainPage.DisplayPromptAsync("New Term", "Enter a unique term name", "Save", "Cancel", "e.g. Spring 2025");
        }

        private async Task GenerateDummyDataAsync()
        {
            var success = await _dummyDataService.GenerateAllDummyDataAsync();
            if (success)
            {
                await InitializeAsync();
            }
        }

        private void OnSelectedTermChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Term.Start) || e.PropertyName == nameof(Term.End))
            {
                if (SelectedTerm.End < SelectedTerm.Start)
                {
                    SelectedTerm.End = SelectedTerm.Start;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Invalid Dates", "End date cannot be before start date. The dates have been synced.", "OK");
                    });
                }

                IsTermDirty = true;
                ValidateTermDateRange();
                (SaveTermCommand as Command)?.ChangeCanExecute();
                OnPropertyChanged(nameof(IsTermDirty));

                SyncCoursesToTermDates();
                SyncAssessmentsToTermDates();
            }
        }

        private void UpdateCommandStates()
        {
            try
            {
                // update all prop changes
                OnPropertyChanged(nameof(CanAddCourse));
                OnPropertyChanged(nameof(ShowAddCourseButton));
                OnPropertyChanged(nameof(CanDeleteSelectedTerm));

                ChangeCommandCanExecute(AddCourseCommand);
                ChangeCommandCanExecute(DeleteTermCommand);
                ChangeCommandCanExecute(SaveTermCommand);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCommandStates] Error: {ex.Message}");
            }
        }

        private void ChangeCommandCanExecute(ICommand saveTermCommand)
        {
            if (saveTermCommand is Command cmd)
                cmd.ChangeCanExecute();
        }        

        //refactor this - too much going on
        private async Task OnOpenCourseDetailAsync(Course course)
        {
            if (course == null || IsBusy)
                return;

            IsBusy = true;
            SelectedCourse = course;

            try
            {
                var freshCourse = await _databaseService.CourseRepository.GetCourseAsync(course.CourseId);

                if (freshCourse is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Course Not Found", "This course no longer exists.", "OK");
                    return;
                }

                var instructorTask = _databaseService.InstructorRepository.GetByIdAsync(freshCourse.InstructorId);
                var assessmentsTask = _databaseService.AssessmentRepository.GetAssessmentsByCourseAsync(freshCourse.CourseId);
                var notesTask = _databaseService.NoteRepository.GetNotesByCourseAsync(freshCourse.CourseId);

                await Task.WhenAll(instructorTask, assessmentsTask, notesTask);

                var instructor = instructorTask.Result;
                var assessments = assessmentsTask.Result;
                var notes = notesTask.Result;
                var performance = assessments.FirstOrDefault(a => a.AssessmentId == freshCourse.PerformanceAssessmentId);
                var objective = assessments.FirstOrDefault(a => a.AssessmentId == freshCourse.ObjectiveAssessmentId);

                var viewModel = new CourseDetailViewModel(course: freshCourse, term: SelectedTerm, instructor: instructor, assessments: assessments, notes: notes, performanceAssessment: performance, objectiveAssessment: objective,
                    databaseService: _databaseService, notifications: _notifications, refreshCallback: async () => await LoadTermData(SelectedTerm.TermId), refreshAssessments: RefreshAssessmentsAsync);

                var page = new CourseDetailPage(viewModel);

                if (Application.Current.MainPage is NavigationPage navPage)
                    await navPage.PushAsync(page);
                else
                    await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object> { ["ViewModel"] = viewModel });
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading course detail: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to load course details.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshAssessmentsAsync()
        {
            if (SelectedTerm == null) return;

            var courses = await _databaseService.CourseRepository.GetCoursesByTermAsync(SelectedTerm.TermId);
            Assessments.Clear();
            foreach (var course in courses)
            {
                var assessments = await _databaseService.AssessmentRepository.GetAssessmentsByCourseAsync(course.CourseId);
                foreach (var a in assessments)
                    Assessments.Add(a);
            }
        }

        public async Task InitializeAsync()
        {
            if (!await _notifications.EnsureNotificationPermissionAsync())
            {
                await Application.Current.MainPage.DisplayAlert("Notification Disabled", "Enable notifications in settings to receive course reminders.", "OK");
            }

            if (_isInitialized) 
                return;

            _isInitialized = true;

            //load data
            await LoadTerms();

            //reset term
            SelectedTerm = null;

            //get dummy data
            if (!Terms.Any())
            {
                await _dummyDataService.GenerateAllDummyDataAsync();
                //data refresh
                await LoadTerms();
            }

            if (Terms.Any())
            {
                SelectedTerm = Terms.First();
            }
        }

        private async Task LoadTerms()
        {
            var terms = (await _databaseService.TermRepository.GetAllAsync()).OrderBy(term => term.TermName).ToList();
            Terms.Clear();

            foreach (var term in terms)
            {
                Terms.Add(term);
            }

            if (Terms.Count == 0)
            {
                await AddNewTermAsync();
            }
        }

        private async Task LoadTermData(int termId)
        {
            if (termId <= 0) 
                return;

            try
            {
                IsBusy = true;

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Courses.Clear();
                    Assessments.Clear();
                    Notes.Clear();
                    Instructors.Clear();
                });

                //load all data
                var loadCoursesTask = _databaseService.CourseRepository.GetCoursesByTermAsync(termId);
                var loadInstructorsTask = _databaseService.InstructorRepository.GetAllAsync();

                var courses = await loadCoursesTask;
                var allInstructors = await loadInstructorsTask;

                // Process courses
                foreach (var course in courses)
                {
                    Application.Current.Dispatcher.Dispatch(() => Courses.Add(course));

                    // Load assessments and notes
                    var loadAssessmentsTask = _databaseService.AssessmentRepository.GetAssessmentsByCourseAsync(course.CourseId);
                    var loadNotesTask = _databaseService.NoteRepository.GetNotesByCourseAsync(course.CourseId);

                    var courseAssessments = await loadAssessmentsTask;
                    var courseNotes = await loadNotesTask;

                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        foreach (var assessment in courseAssessments)
                        {
                            Assessments.Add(assessment);
                        }

                        foreach (var note in courseNotes)
                        {
                            Notes.Add(note);
                        }
                    });
                }

                // Add instructors
                Application.Current.Dispatcher.Dispatch(() =>
                {
                   foreach (var instructor in allInstructors)
                   {
                       Instructors.Add(instructor);
                   }

                   SelectedCourse = null;
                   OnPropertyChanged(nameof(SelectedCourse));
                   UpdateCommandStates();
                   OnPropertyChanged(nameof(ShowAddCourseButton));
                   OnPropertyChanged(nameof(CanAddCourse));

               });
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading term data: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load term data", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ValidateTermDateRange()
        {
            var (startError, endError) = DateValidationHelper.GetValidationErrors(SelectedTerm.Start, SelectedTerm.End);

            TermStartDateError = startError;
            TermEndDateError = endError;

            OnPropertyChanged(nameof(IsTermDateRangeValid));
        }

        private async Task<bool> IsDuplicateTermNameAsync(string name)
        {
            if (Terms.Any(t => t.TermName.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Duplicate Term",
                    $"A term named '{name.Trim()}' already exists. Please choose a different name.",
                    "OK");
                return true;
            }
            return false;
        }
    }
}
