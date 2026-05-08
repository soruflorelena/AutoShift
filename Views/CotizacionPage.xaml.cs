using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class CotizacionPage : ContentPage
{
    public CotizacionPage()
    {
        InitializeComponent();
        BindingContext = new CotizacionViewModel();
    }
}
