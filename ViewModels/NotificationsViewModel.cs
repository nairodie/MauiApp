using MauiApp2.Interfaces;
using MauiApp2.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MauiApp2.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        private readonly INotificationService _notificationService;
        private readonly ICourseRepository _courseRepository;
        private readonly IAssessmentRepository _assessmentRepository;

        //commands
        public ICommand RefreshNotificationsCommand { get; }
        public ICommand ToggleCourseNotificationsCommand { get; }
        public ICommand ToggleAssessmentNotificationsCommand { get; }

        private ObservableCollection<Course> _courses;
        public ObservableCollection<Course> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        private ObservableCollection<Assessment> _assessments;
        public ObservableCollection<Assessment> Assessments
        {
            get => _assessments;
            set => SetProperty(ref _assessments, value);
        }

        //DI
        public NotificationsViewModel(INotificationService notification, ICourseRepository course, IAssessmentRepository assessment)
        {
            _notificationService = notification;
            _courseRepository = course;
            _assessmentRepository = assessment;

            RefreshNotificationsCommand = new Command(async () => await RefreshNotificationsAsync());

            ToggleCourseNotificationsCommand = new Command<Course>(async (course) => await ToggleCourseNotificationsAsync(course));
            ToggleAssessmentNotificationsCommand = new Command<Assessment>(async (assessment) => await ToggleAssessmentNotificationsAsync(assessment));
        }

        private async Task ToggleAssessmentNotificationsAsync(Assessment assessment)
        {
            if (assessment.StartNotification > 0)
            {
                await _notificationService.CancelNotificationAsync(assessment.AssessmentId + 3000);
                assessment.StartNotification = 0;
            }
            else
            {
                assessment.StartNotification = 1;
                await _notificationService.ScheduleAssessmentNotificationsAsync(new[] { assessment });
            }

            await _assessmentRepository.UpdateAssessment(assessment);
        }

        private async Task ToggleCourseNotificationsAsync(Course course)
        {
            if (course.startNotification > 0)
            {
                await _notificationService.CancelNotificationAsync(course.CourseId + 1000);
                course.startNotification = 0;
            }
            else
            {
                course.startNotification = 1;
                await _notificationService.ScheduleCourseNotificationsAsync(new[] { course });
            }

            await _courseRepository.UpdateCourseAsync(course);
        }

        public async Task InitializeAsync()
        {
            Courses = new ObservableCollection<Course> (await _courseRepository.GetAllAsync());
            Assessments = new ObservableCollection<Assessment> (await _assessmentRepository.GetAllAsync());
            await RefreshNotificationsAsync();
        }

        private async Task RefreshNotificationsAsync()
        {
            if (!await _notificationService.EnsureNotificationPermissionAsync())
            {
                Debug.WriteLine($"Permission denied");
                return;
            }

            await _notificationService.ScheduleCourseNotificationsAsync(Courses.Where(c => c.startNotification > 0 || c.endNotification > 0));
            await _notificationService.ScheduleAssessmentNotificationsAsync(Assessments.Where(a => a.StartNotification > 0 || a.EndNotification > 0));            
        }
    }
}
