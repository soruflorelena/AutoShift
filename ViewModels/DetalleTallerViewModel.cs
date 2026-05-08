using AutoShift.Models;
using AutoShift.Services;
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

        public DetalleTallerViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        // Se ejecuta cuando se recibe el taller por navegación
        partial void OnTallerSeleccionadoChanged(Taller? value)
        {
            if (value != null)
            {
                _ = CargarServicios();
                _ = CargarVehiculos();
            }
        }

        private async Task CargarServicios()
        {
            if (TallerSeleccionado == null) return;

            IsBusy = true;
            var servicios = await _firebaseService.GetServiciosAsync(TallerSeleccionado.Id);

            MainThread.BeginInvokeOnMainThread(() => {
                ServiciosDisponibles.Clear();
                foreach (var s in servicios)
                {
                    var item = new ServicioSeleccionable { DatosServicio = s };
                    item.OnSelectionChanged = ActualizarSubtotal;
                    ServiciosDisponibles.Add(item);
                }
            });
            IsBusy = false;
        }

        private async Task CargarVehiculos()
        {
            var clienteId = Preferences.Get("UsuarioId", "");
            if (string.IsNullOrWhiteSpace(clienteId)) return;

            var lista = await _firebaseService.GetVehiculosClienteAsync(clienteId);
            MainThread.BeginInvokeOnMainThread(() => {
                Vehiculos.Clear();
                foreach (var v in lista) Vehiculos.Add(v);
                VehiculoSeleccionado = Vehiculos.FirstOrDefault();
            });
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
                await Shell.Current.DisplayAlert("Aviso", "Selecciona al menos un servicio", "OK");
                return;
            }

            if (Vehiculos.Count > 0 && VehiculoSeleccionado == null)
            {
                await Shell.Current.DisplayAlert("Aviso", "Selecciona un vehículo o ve al administrador de vehículos.", "OK");
                return;
            }

            if (TallerSeleccionado == null)
            {
                await Shell.Current.DisplayAlert("Error", "No se encontró el taller seleccionado.", "OK");
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
                DiagnosticoCliente = string.IsNullOrWhiteSpace(DescripcionCliente) ? string.Empty : DescripcionCliente.Trim()
            };

            var clientId = Preferences.Get("UsuarioId", "");
            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(TallerSeleccionado.Id, nuevaSolicitud);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(clientId, nuevaSolicitud);

            if (guardadoTaller && guardadoCliente)
            {
                await Shell.Current.DisplayAlert("Éxito", "Solicitud enviada al taller. Espera su respuesta.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo enviar la solicitud. Revisa tu conexión e inténtalo de nuevo.", "OK");
            }
        }

        [RelayCommand]
        private async Task VerVehiculos()
        {
            await Shell.Current.GoToAsync("VehiculosPage");
        }
    }

    // Clase auxiliar para la lista con Checkbox
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