using MauiApp2.ViewModels;
using System.Diagnostics;

namespace MauiApp2;

public partial class CourseDetailPage : ContentPage
{
    public CourseDetailPage(CourseDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }   
}
