using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class MisSolicitudesPage : ContentPage
{
    public MisSolicitudesPage()
    {
        InitializeComponent();
        BindingContext = new MisSolicitudesViewModel();
    }
}
