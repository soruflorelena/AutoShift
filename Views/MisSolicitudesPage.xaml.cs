using AutoShift.ViewModels;

namespace AutoShift.Views;

public partial class MisSolicitudesPage : ContentPage
{
    private MisSolicitudesViewModel _viewModel;

    public MisSolicitudesPage()
    {
        InitializeComponent();
        _viewModel = new MisSolicitudesViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.InicializarDatosAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}