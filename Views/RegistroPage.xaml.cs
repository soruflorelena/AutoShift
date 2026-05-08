using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class RegistroPage : ContentPage
{
    public RegistroPage()
    {
        InitializeComponent();
        BindingContext = new AutoShift.ViewModels.RegistroViewModel();
    }
}