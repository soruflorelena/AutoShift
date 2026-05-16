using AutoShift.ViewModels;

namespace AutoShift.Views
{
    public partial class MainClientePage : ContentPage
    {
        private MainClienteViewModel _viewModel;

        public MainClientePage()
        {
            InitializeComponent();
            _viewModel = new MainClienteViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _viewModel.InicializarDatosAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar cliente: {ex.Message}");
            }
        }
    }
}