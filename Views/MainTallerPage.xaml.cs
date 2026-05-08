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

            // Ejecuta la carga una vez renderizada la interfaz para evitar congelar la App al inicio
            if (_viewModel != null)
            {
                await _viewModel.InicializarDatosAsync();
            }

            if (BindingContext is ViewModels.MainTallerViewModel vm)
            {
                // Carga las solicitudes actualizadas al volver a la pantalla principal del taller
                await vm.CargarSolicitudesDesdeFirebase();
            }
        }
    }
}