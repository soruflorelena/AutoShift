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
            await _viewModel.InicializarDatosAsync();
        }
    }
}