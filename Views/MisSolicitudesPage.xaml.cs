using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class MisSolicitudesPage : ContentPage
{
    public MisSolicitudesPage()
    {
        InitializeComponent();
        BindingContext = new MisSolicitudesViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Dispara la carga de datos cada vez que la vista aparece en pantalla
        if (BindingContext is ViewModels.MisSolicitudesViewModel vm)
        {
            vm.RefrescarSolicitudesCommand.Execute(null);
        }
    }

}
