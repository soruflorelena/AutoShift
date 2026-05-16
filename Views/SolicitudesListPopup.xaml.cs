using AutoShift.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.Views
{
    public partial class SolicitudesListPopup : ContentPage
    {
        public SolicitudesListPopup(ObservableCollection<SolicitudServicio> solicitudes, string titulo, bool esFinalizadas)
        {
            InitializeComponent();

            var vm = new SolicitudesListPopupViewModel(solicitudes, titulo, esFinalizadas);
            BindingContext = vm;
        }
    }

    public partial class SolicitudesListPopupViewModel : ObservableObject
    {
        [ObservableProperty] private string tituloPopup;
        [ObservableProperty] private ObservableCollection<SolicitudServicio> solicitudesActuales;
        [ObservableProperty] private bool esFinalizadas;

        public SolicitudesListPopupViewModel(ObservableCollection<SolicitudServicio> solicitudes, string titulo, bool esFinalizadas)
        {
            TituloPopup = titulo;
            SolicitudesActuales = new ObservableCollection<SolicitudServicio>(solicitudes);
            EsFinalizadas = esFinalizadas;
        }

        [RelayCommand]
        public async Task Cerrar()
        {
            if (Shell.Current?.CurrentPage is ContentPage page)
            {
                await page.Navigation.PopModalAsync();
            }
        }

        [RelayCommand]
        public async Task ResponderSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;

            if (Shell.Current?.CurrentPage is ContentPage page)
            {
                await page.Navigation.PopModalAsync();
            }

            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud } };
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("CotizacionPage", parameters);
            }
        }
    }
}
