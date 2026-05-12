namespace AutoShift.Views
{
    public partial class MainTallerPage : ContentPage
    {
        private readonly ViewModels.MainTallerViewModel _viewModel;

        public MainTallerPage(ViewModels.MainTallerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.InicializarDatosAsync();
            }
        }
    }
}