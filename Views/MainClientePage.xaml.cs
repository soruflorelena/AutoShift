using AutoShift.ViewModels;

namespace AutoShift.Views
{
    public partial class MainClientePage : ContentPage
    {
        public MainClientePage()
        {
            InitializeComponent();
            // Esto es lo que "llena" la página con los datos del ViewModel
            BindingContext = new MainClienteViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Forzar recarga cada vez que entramos a la página
            if (BindingContext is MainClienteViewModel vm)
            {
                vm.CargarTalleresCommand.Execute(null);
            }
        }
    }
}