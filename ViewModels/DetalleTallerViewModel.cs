using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.ViewModels
{
    [QueryProperty(nameof(TallerSeleccionado), "Taller")]
    [QueryProperty(nameof(DescripcionCliente), "DescripcionCliente")]
    public partial class DetalleTallerViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private Taller? tallerSeleccionado;
        [ObservableProperty] private decimal subtotal;
        [ObservableProperty] private string descripcionCliente = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private Vehiculo? vehiculoSeleccionado;

        public ObservableCollection<ServicioSeleccionable> ServiciosDisponibles { get; } = new();
        public ObservableCollection<Vehiculo> Vehiculos { get; } = new();
        public ObservableCollection<Resena> Resenas { get; } = new();

        public DetalleTallerViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        partial void OnTallerSeleccionadoChanged(Taller? value)
        {
            if (value != null)
            {
                _ = CargarServicios();
                _ = CargarVehiculos();
                _ = CargarResenas();
            }
        }

        private async Task CargarResenas()
        {
            try
            {
                if (TallerSeleccionado == null) return;
                var lista = await _firebaseService.GetResenasTallerAsync(TallerSeleccionado.Id);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Resenas.Clear();
                    if (lista != null)
                    {
                        foreach (var r in lista.OrderByDescending(x => x.Fecha)) Resenas.Add(r);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reseñas: {ex.Message}");
            }
        }

        private async Task CargarServicios()
        {
            try
            {
                if (TallerSeleccionado == null) return;

                IsBusy = true;
                var servicios = await _firebaseService.GetServiciosAsync(TallerSeleccionado.Id);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ServiciosDisponibles.Clear();
                    if (servicios != null)
                    {
                        foreach (var s in servicios)
                        {
                            var item = new ServicioSeleccionable { DatosServicio = s };
                            item.OnSelectionChanged = ActualizarSubtotal;
                            ServiciosDisponibles.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error servicios: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        private async Task CargarVehiculos()
        {
            try
            {
                var clienteId = Preferences.Get("UsuarioId", "");
                if (string.IsNullOrWhiteSpace(clienteId)) return;

                var lista = await _firebaseService.GetVehiculosClienteAsync(clienteId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Vehiculos.Clear();
                    if (lista != null)
                    {
                        foreach (var v in lista) Vehiculos.Add(v);
                        VehiculoSeleccionado = Vehiculos.FirstOrDefault();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error vehiculos: {ex.Message}");
            }
        }

        private void ActualizarSubtotal()
        {
            Subtotal = ServiciosDisponibles.Where(s => s.IsSelected).Sum(s => s.DatosServicio.PrecioBase);
        }

        [RelayCommand]
        private async Task EnviarSolicitud()
        {
            if (Subtotal == 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Aviso", "Selecciona al menos un servicio"));
                return;
            }

            if (Vehiculos.Count > 0 && VehiculoSeleccionado == null)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Aviso", "Selecciona un vehículo o ve al administrador de vehículos."));
                return;
            }

            if (TallerSeleccionado == null)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se encontró el taller seleccionado."));
                return;
            }

            var serviciosSeleccionados = ServiciosDisponibles.Where(s => s.IsSelected).Select(s => s.DatosServicio.Nombre).ToList();
            var descripcion = string.IsNullOrWhiteSpace(DescripcionCliente)
                ? (serviciosSeleccionados.Any()
                    ? $"Solicitud de servicios: {string.Join(", ", serviciosSeleccionados)}"
                    : "Solicitud sin descripción adicional")
                : DescripcionCliente.Trim();

            var nuevaSolicitud = new SolicitudServicio
            {
                Id = Guid.NewGuid().ToString(),
                Fecha = DateTime.Now,
                Estado = "PENDIENTE",
                ClienteId = Preferences.Get("UsuarioId", ""),
                ClienteNombre = Preferences.Get("UsuarioNombre", "Cliente"),
                TallerId = TallerSeleccionado.Id,
                TallerNombre = TallerSeleccionado.Nombre,
                DescripcionProblema = descripcion,
                VehiculoId = VehiculoSeleccionado?.Id ?? string.Empty,
                VehiculoMarca = VehiculoSeleccionado?.Marca ?? string.Empty,
                VehiculoModelo = VehiculoSeleccionado?.Modelo ?? string.Empty,
                VehiculoAnio = VehiculoSeleccionado?.Anio ?? 0,
                VehiculoPlacas = VehiculoSeleccionado?.Placas ?? string.Empty,
                ServiciosSolicitados = serviciosSeleccionados,
                DiagnosticoCliente = string.IsNullOrWhiteSpace(DescripcionCliente) ? string.Empty : DescripcionCliente.Trim(),
                ClienteTelefono = Preferences.Get("UsuarioTelefono", string.Empty)
            };

            var clientId = Preferences.Get("UsuarioId", "");
            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(TallerSeleccionado.Id, nuevaSolicitud);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(clientId, nuevaSolicitud);

            if (guardadoTaller && guardadoCliente)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Solicitud enviada al taller. Espera su respuesta."));
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo enviar la solicitud. Revisa tu conexión e inténtalo de nuevo."));
            }
        }

        [RelayCommand]
        private async Task VerVehiculos()
        {
            await Shell.Current.GoToAsync("VehiculosPage");
        }
    }

    public class ServicioSeleccionable : ObservableObject
    {
        public Servicio DatosServicio { get; set; } = new Servicio();
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set { isSelected = value; OnSelectionChanged?.Invoke(); }
        }
        public Action? OnSelectionChanged { get; set; }
    }
}