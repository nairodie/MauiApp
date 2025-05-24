using MauiApp2.Models;

namespace MauiApp2.Interfaces
{
    public interface INotificationService
    {
        Task ScheduleCourseNotificationsAsync(IEnumerable<Course> courses);
        Task ScheduleAssessmentNotificationsAsync(IEnumerable<Assessment> assessments);
        Task CancelNotificationAsync(int notificationId);
        Task<bool> EnsureNotificationPermissionAsync();
    }
}
