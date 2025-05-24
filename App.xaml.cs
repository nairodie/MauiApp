using MauiApp2.Interfaces;

namespace MauiApp2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var loginPage = MauiProgram.ServiceProvider.GetRequiredService<LoginPage>();
            MainPage = new NavigationPage(loginPage);

            Task.Run(async () =>
            {
                try
                {
                    var dbService = MauiProgram.ServiceProvider.GetRequiredService<IDatabaseService>();
                    await dbService.CreateTablesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB Init Error] {ex.Message}");
                }
            });
        }
    }
}
