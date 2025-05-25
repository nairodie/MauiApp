using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiApp2.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? title;

        protected async Task SetBusyAsync(Func<Task> action, [System.Runtime.CompilerServices.CallerMemberName] string? caller = null)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Busy Error in {caller}] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}