using MauiApp2.Interfaces;

namespace MauiApp2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = MauiProgram.ServiceProvider.GetRequiredService<AppShell>();
            Task.Run(async () =>
            {
                try
                {
                    var dbService = MauiProgram.ServiceProvider.GetRequiredService<IDatabaseService>();
                    await dbService.CreateTablesAsync();

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.GoToAsync("//LoginPage");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB Init Error] {ex.Message}");
                }
            });
        }
    }
}
