using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AutoShift.ViewModels
{
    [QueryProperty(nameof(SolicitudSeleccionada), "Solicitud")]
    public partial class CotizacionViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private SolicitudServicio? solicitudSeleccionada;
        [ObservableProperty] private string nuevoDetalleNombre = string.Empty;
        [ObservableProperty] private string nuevoDetallePrecio = string.Empty;
        [ObservableProperty] private string nuevoDetalleCantidad = "1";
        [ObservableProperty] private string comentario = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private DateTime fechaCitaSeleccionada = DateTime.Today.AddDays(1);
        [ObservableProperty] private TimeSpan horaCitaSeleccionada = TimeSpan.FromHours(9);

        public ObservableCollection<Servicio> ServiciosDelTaller { get; } = new();

        [ObservableProperty]
        private Servicio? servicioSeleccionado;

        public ObservableCollection<DetalleServicio> Detalles { get; } = new();

        public CotizacionViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        public decimal TotalCotizacion => Detalles.Sum(d => d.Precio * d.Cantidad);

        public bool PuedeCotizar => SolicitudSeleccionada?.EsPendiente == true;

        partial void OnSolicitudSeleccionadaChanged(SolicitudServicio? value)
        {
            Detalles.Clear();
            if (value == null) return;

            if (value.Cotizacion?.Detalles != null)
            {
                foreach (var detalle in value.Cotizacion.Detalles) Detalles.Add(detalle);
            }

            if (value.FechaCita.HasValue)
            {
                FechaCitaSeleccionada = value.FechaCita.Value.Date;
                HoraCitaSeleccionada = value.FechaCita.Value.TimeOfDay;
            }

            OnPropertyChanged(nameof(TotalCotizacion));
            OnPropertyChanged(nameof(PuedeCotizar));

            if (!string.IsNullOrEmpty(value.TallerId))
            {
                _ = CargarCatalogoTaller(value.TallerId);
            }
        }

        private async Task CargarCatalogoTaller(string tallerId)
        {
            var servicios = await _firebaseService.GetServiciosAsync(tallerId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ServiciosDelTaller.Clear();
                foreach (var s in servicios) ServiciosDelTaller.Add(s);
            });
        }

        partial void OnServicioSeleccionadoChanged(Servicio? value)
        {
            if (value != null)
            {
                NuevoDetalleNombre = value.Nombre;
                NuevoDetallePrecio = value.PrecioBase.ToString();
            }
        }

        [RelayCommand]
        private async Task AgregarDetalle()
        {
            if (string.IsNullOrWhiteSpace(NuevoDetalleNombre) || !decimal.TryParse(NuevoDetallePrecio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || !int.TryParse(NuevoDetalleCantidad, out var cantidad) || precio <= 0 || cantidad <= 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "Llena los campos correctamente."));
                return;
            }

            Detalles.Add(new DetalleServicio { Nombre = NuevoDetalleNombre.Trim(), Precio = precio, Cantidad = cantidad });
            NuevoDetalleNombre = string.Empty; NuevoDetallePrecio = string.Empty; NuevoDetalleCantidad = "1";
            OnPropertyChanged(nameof(TotalCotizacion));
        }

        [RelayCommand]
        private void EliminarDetalle(DetalleServicio detalle)
        {
            if (detalle != null) { Detalles.Remove(detalle); OnPropertyChanged(nameof(TotalCotizacion)); }
        }

        private async Task<bool> GuardarSolicitudAsync()
        {
            if (SolicitudSeleccionada == null) return false;
            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(SolicitudSeleccionada.TallerId, SolicitudSeleccionada);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(SolicitudSeleccionada.ClienteId, SolicitudSeleccionada);
            return guardadoTaller && guardadoCliente;
        }

        [RelayCommand]
        private async Task EnviarCotizacion()
        {
            if (SolicitudSeleccionada == null || Detalles.Count == 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "Agrega al menos un servicio a la cotización."));
                return;
            }

            IsBusy = true;
            SolicitudSeleccionada.Cotizacion = new Cotizacion { Id = Guid.NewGuid().ToString(), SolicitudId = SolicitudSeleccionada.Id, Detalles = Detalles.ToList() };
            SolicitudSeleccionada.Estado = "COTIZADO";
            SolicitudSeleccionada.MensajeTaller = Comentario?.Trim() ?? string.Empty;

            if (await GuardarSolicitudAsync())
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Cotización enviada al cliente."));
                await Shell.Current.GoToAsync("..", true);
            }
            IsBusy = false;
        }

        [RelayCommand]
        private async Task AsignarCita()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsAceptado) return;

            SolicitudSeleccionada.FechaCita = FechaCitaSeleccionada.Date + HoraCitaSeleccionada;
            SolicitudSeleccionada.Estado = "CITA_ASIGNADA";
            SolicitudSeleccionada.MensajeTaller = Comentario?.Trim() ?? string.Empty;

            if (await GuardarSolicitudAsync())
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "La cita ha sido agendada."));
                await Shell.Current.GoToAsync("..", true);
            }
        }

        [RelayCommand]
        private async Task MarcarEnProceso()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsCitaAsignada) return;

            SolicitudSeleccionada.Estado = "EN_PROCESO";
            if (await GuardarSolicitudAsync())
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "La solicitud se encuentra en proceso."));
                await Shell.Current.GoToAsync("..", true);
            }
        }

        [RelayCommand]
        private async Task MarcarComoCompletado()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsEnProceso) return;

            SolicitudSeleccionada.Estado = "COMPLETADO";
            SolicitudSeleccionada.MensajeTaller = "Servicio terminado. Esperando confirmación del cliente.";
            if (await GuardarSolicitudAsync())
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "El cliente ahora debe confirmar la entrega."));
                await Shell.Current.GoToAsync("..", true);
            }
        }
    }
}