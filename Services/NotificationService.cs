using MauiApp2.Models;
using Plugin.LocalNotification;
using System.Diagnostics;

namespace MauiApp2.Services
{
    public class NotificationService : Interfaces.INotificationService
    {
        public async Task CancelNotificationAsync(int notificationId)
        {
            var pending = await LocalNotificationCenter.Current.GetPendingNotificationList();
            var toCancel = pending.FirstOrDefault(n => n.NotificationId == notificationId);
            toCancel?.Cancel();
        }

        public async Task<bool> EnsureNotificationPermissionAsync()
        {
            if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            {
                return await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            return true;
        }

        public async Task ScheduleAssessmentNotificationsAsync(IEnumerable<Assessment> assessments)
        {
            var requests = assessments
                .SelectMany(CreateAssessmentNotifications)
                .Where(r => r != null)
                .ToList();

            await ProcessNotificationRequestsAsync(requests!);
        }

        public async Task ScheduleCourseNotificationsAsync(IEnumerable<Course> courses)
        {
            var requests = courses
                .SelectMany(CreateCourseNotifications)
                .Where(r => r != null)
                .ToList();

            await ProcessNotificationRequestsAsync(requests!);
        }

        private NotificationRequest CreateCourseStartNotification(Course course)
        {
            return new NotificationRequest
            {
                NotificationId = course.CourseId + 1000,
                Title = "Course Starting Reminder",
                Description = $"{course.courseName} Starting soon",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = CalculateNotificationTime(course.start, course.startNotification),
                    RepeatType = NotificationRepeat.Daily
                }
            };
        }

        private NotificationRequest CreateCourseEndNotification(Course course)
        {
            return new NotificationRequest
            {
                NotificationId = course.CourseId + 2000,
                Title = "Course Ending Reminder",
                Description = $"{course.courseName} Ending soon",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = CalculateNotificationTime(course.end, course.endNotification),
                    RepeatType = NotificationRepeat.Daily
                }
            };
        }

        private IEnumerable<NotificationRequest?> CreateCourseNotifications(Course course)
        {
            yield return CreateNotification(
                id: course.CourseId + 1000,
                title: "Course Starting Reminder",
                description: $"{course.courseName} is starting soon",
                eventDate: course.start,
                daysBefore: course.startNotification
            );

            yield return CreateNotification(
                id: course.CourseId + 2000,
                title: "Course Ending Reminder",
                description: $"{course.courseName} is ending soon",
                eventDate: course.end,
                daysBefore: course.endNotification
            );
        }

        private IEnumerable<NotificationRequest?> CreateAssessmentNotifications(Assessment assessment)
        {
            yield return CreateNotification(
                id: assessment.AssessmentId + 3000,
                title: "Assessment Starting Reminder",
                description: $"{assessment.Name} is starting soon",
                eventDate: assessment.Start,
                daysBefore: assessment.StartNotification
            );

            yield return CreateNotification(
                id: assessment.AssessmentId + 4000,
                title: "Assessment Ending Reminder",
                description: $"{assessment.Name} is ending soon",
                eventDate: assessment.End,
                daysBefore: assessment.EndNotification
            );
        }

        private NotificationRequest? CreateNotification(int id, string title, string description, DateTime eventDate, int daysBefore)
        {
            var notifyTime = CalculateNotificationTime(eventDate, daysBefore);
            if (notifyTime == null || notifyTime <= DateTime.Now)
                return null;

            return new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = description,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime,
                    RepeatType = NotificationRepeat.Daily
                }
            };
        }

        private DateTime? CalculateNotificationTime(DateTime eventDate, int daysBefore)
        {
            return eventDate.AddDays(-daysBefore).Date.AddHours(9);
        }

        private async Task ProcessNotificationRequestsAsync(List<NotificationRequest> requests)
        {
            foreach (var request in requests)
            {
                try
                {
                    Debug.WriteLine($"Scheduling Notification: {request.Title} at {request.Schedule?.NotifyTime}");
                    await LocalNotificationCenter.Current.Show(request);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to schedule notification (ID: {request.NotificationId}): {ex.Message}");
                }
            }
        }

        public async Task ScheduleCourseNotificationsAsync(Course course, Instructor instructor)
        {
            if (!await EnsureNotificationPermissionAsync()) return;

            var requests = CreateCourseNotifications(course, instructor)
                .Where(r => r != null)
                .ToList();

            await ProcessNotificationRequestsAsync(requests!);
        }

        private IEnumerable<NotificationRequest> CreateCourseNotifications(Course course, Instructor instructor)
        {
            var notifications = new List<NotificationRequest>();

            if (course.startNotification && course.StartDate > DateTime.Now)
            {
                notifications.Add(new NotificationRequest
                {
                    NotificationId = course.Id * 10 + 1,
                    Title = "Course Start",
                    Description = $"{course.Title} starts on {course.StartDate:d}. Contact: {instructor.Name}",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = course.StartDate,
                        RepeatType = NotificationRepeat.No
                    }
                });
            }

            if (course.NotifyEnd && course.EndDate > DateTime.Now)
            {
                notifications.Add(new NotificationRequest
                {
                    NotificationId = course.Id * 10 + 2,
                    Title = "Course End",
                    Description = $"{course.Title} ends on {course.EndDate:d}. Contact: {instructor.Name}",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = course.EndDate,
                        RepeatType = NotificationRepeat.No
                    }
                });
            }

            return notifications;
        }


    }
}
