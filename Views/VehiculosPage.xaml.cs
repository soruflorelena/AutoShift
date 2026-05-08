using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class VehiculosPage : ContentPage
{
    public VehiculosPage()
    {
        InitializeComponent();
        BindingContext = new VehiculosViewModel();
    }
}
