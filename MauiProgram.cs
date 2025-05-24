using MauiApp2.Converters;
using MauiApp2.Interfaces;
using MauiApp2.Repositories;
using MauiApp2.Services;
using MauiApp2.ViewModels;
using Microsoft.Extensions.Logging;
using SQLite;

namespace MauiApp2
{
    public static class MauiProgram
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // SQLite connection registration
            builder.Services.AddSingleton<SQLiteAsyncConnection>(_ =>
            {
                string databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyData.db");
                return new SQLiteAsyncConnection(databasePath);
            });

            // Repository Layer
            builder.Services.AddSingleton<ITermRepository, TermRepository>();
            builder.Services.AddSingleton<ICourseRepository, CourseRepository>();
            builder.Services.AddSingleton<IAssessmentRepository, AssessmentRepository>();
            builder.Services.AddSingleton<IInstructorRepository, InstructorRepository>();
            builder.Services.AddSingleton<INoteRepository, NoteRepository>();

            // Services
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<DummyDataService>();
            builder.Services.AddSingleton<ICourseRepository, CourseRepository>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<CourseDetailViewModel>();
            builder.Services.AddTransient<NotificationsViewModel>();

            // Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CourseDetailPage>();

            // Converters
            builder.Services.AddSingleton<NullToBoolConverter>();
            builder.Services.AddSingleton<BoolToColorConverter>();
            builder.Services.AddSingleton<InverseBoolConverter>();
            builder.Services.AddSingleton<GreaterThanZeroConverter>();
            builder.Services.AddSingleton<SelectedTermColorConverter>();

            // Build and assign provider
            var app = builder.Build();
            ServiceProvider = app.Services;           
            return app;
        }
    }
}
