using AutoShift.Models;
using AutoShift.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

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

        public ObservableCollection<DetalleServicio> Detalles { get; } = new();

        public CotizacionViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        partial void OnSolicitudSeleccionadaChanged(SolicitudServicio? value)
        {
            Detalles.Clear();
            if (value == null)
                return;

            if (value.Cotizacion?.Detalles != null)
            {
                foreach (var detalle in value.Cotizacion.Detalles)
                {
                    Detalles.Add(detalle);
                }
            }

            if (value.FechaCita.HasValue)
            {
                FechaCitaSeleccionada = value.FechaCita.Value.Date;
                HoraCitaSeleccionada = value.FechaCita.Value.TimeOfDay;
            }

            OnPropertyChanged(nameof(TotalCotizacion));
        }

        public decimal TotalCotizacion => Detalles.Sum(d => d.Precio * d.Cantidad);

        [RelayCommand]
        private async Task AgregarDetalle()
        {
            if (string.IsNullOrWhiteSpace(NuevoDetalleNombre))
            {
                await Shell.Current.DisplayAlert("Error", "Proporciona un nombre para el servicio.", "OK");
                return;
            }

            if (!decimal.TryParse(NuevoDetallePrecio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa un precio válido.", "OK");
                return;
            }

            if (!int.TryParse(NuevoDetalleCantidad, out var cantidad) || cantidad <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa una cantidad válida.", "OK");
                return;
            }

            Detalles.Add(new DetalleServicio
            {
                Nombre = NuevoDetalleNombre.Trim(),
                Precio = precio,
                Cantidad = cantidad
            });

            NuevoDetalleNombre = string.Empty;
            NuevoDetallePrecio = string.Empty;
            NuevoDetalleCantidad = "1";
            OnPropertyChanged(nameof(TotalCotizacion));
        }

        [RelayCommand]
        private void EliminarDetalle(DetalleServicio detalle)
        {
            if (detalle == null)
                return;

            Detalles.Remove(detalle);
            OnPropertyChanged(nameof(TotalCotizacion));
        }

        private async Task<bool> GuardarSolicitudAsync()
        {
            if (SolicitudSeleccionada == null)
                return false;

            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(SolicitudSeleccionada.TallerId, SolicitudSeleccionada);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(SolicitudSeleccionada.ClienteId, SolicitudSeleccionada);
            return guardadoTaller && guardadoCliente;
        }

        [RelayCommand]
        private async Task EnviarCotizacion()
        {
            if (SolicitudSeleccionada == null)
            {
                await Shell.Current.DisplayAlert("Error", "No se encontró la solicitud.", "OK");
                return;
            }

            if (Detalles.Count == 0)
            {
                await Shell.Current.DisplayAlert("Error", "Agrega al menos un servicio a la cotización.", "OK");
                return;
            }

            IsBusy = true;

            SolicitudSeleccionada.Cotizacion = new Cotizacion
            {
                Id = Guid.NewGuid().ToString(),
                SolicitudId = SolicitudSeleccionada.Id,
                Detalles = Detalles.ToList()
            };
            SolicitudSeleccionada.Estado = "COTIZADO";
            SolicitudSeleccionada.MensajeTaller = Comentario?.Trim() ?? string.Empty;

            var guardado = await GuardarSolicitudAsync();
            IsBusy = false;

            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "Cotización enviada al cliente.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo enviar la cotización. Intenta de nuevo.", "OK");
            }
        }

        [RelayCommand]
        private async Task SolicitarInspeccion()
        {
            if (SolicitudSeleccionada == null)
                return;

            SolicitudSeleccionada.Estado = "INSPECCION_SOLICITADA";
            SolicitudSeleccionada.MensajeTaller = "El taller requiere inspección física para completar el diagnóstico.";

            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "Se registró que el taller necesita inspección física.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo actualizar la solicitud.", "OK");
            }
        }

        [RelayCommand]
        private async Task AsignarCita()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsAceptado)
                return;

            SolicitudSeleccionada.FechaCita = FechaCitaSeleccionada.Date + HoraCitaSeleccionada;
            SolicitudSeleccionada.Estado = "CITA_ASIGNADA";
            SolicitudSeleccionada.MensajeTaller = Comentario?.Trim() ?? string.Empty;

            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "La cita ha sido agendada.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo agendar la cita.", "OK");
            }
        }

        [RelayCommand]
        private async Task MarcarEnProceso()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsCitaAsignada)
                return;

            SolicitudSeleccionada.Estado = "EN_PROCESO";
            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "La solicitud se encuentra en proceso.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo actualizar el estado.", "OK");
            }
        }

        [RelayCommand]
        private async Task MarcarFinalizado()
        {
            if (SolicitudSeleccionada == null || !(SolicitudSeleccionada.EsEnProceso || SolicitudSeleccionada.EsCompletado))
                return;

            SolicitudSeleccionada.Estado = "FINALIZADO";
            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "La solicitud ha sido finalizada.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo finalizar la solicitud.", "OK");
            }
        }

        [RelayCommand]
        private async Task ProponerFecha()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsInspeccionAceptada)
                return;

            SolicitudSeleccionada.FechaPropuesta = FechaCitaSeleccionada.Date + HoraCitaSeleccionada;
            SolicitudSeleccionada.Estado = "FECHA_PROPUESTA";
            SolicitudSeleccionada.MensajeTaller = Comentario?.Trim() ?? string.Empty;

            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "Fecha propuesta al cliente.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo proponer la fecha.", "OK");
            }
        }

        [RelayCommand]
        private async Task ValidarFecha(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsFechasPropuestas || solicitud.FechasAlternativas.Count == 0)
                return;

            // Asumir que selecciona la primera, en UI se podría elegir
            solicitud.FechaValidada = solicitud.FechasAlternativas.First();
            solicitud.Estado = "FECHA_VALIDADA";

            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(solicitud.TallerId, solicitud);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(solicitud.ClienteId, solicitud);

            if (guardadoTaller && guardadoCliente)
            {
                await Shell.Current.DisplayAlert("Éxito", "Fecha validada.", "OK");
                // Recargar o notificar
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo validar la fecha.", "OK");
            }
        }

        [RelayCommand]
        private async Task MarcarInspeccionRealizada()
        {
            if (SolicitudSeleccionada == null || !SolicitudSeleccionada.EsFechaValidada)
                return;

            SolicitudSeleccionada.Estado = "INSPECCION_REALIZADA";
            var guardado = await GuardarSolicitudAsync();
            if (guardado)
            {
                await Shell.Current.DisplayAlert("Éxito", "Inspección marcada como realizada.", "OK");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo actualizar el estado.", "OK");
            }
        }
    }
}
