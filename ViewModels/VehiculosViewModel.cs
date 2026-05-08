using AutoShift.Models;
using AutoShift.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.ViewModels
{
    public partial class VehiculosViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private string marca = string.Empty;
        [ObservableProperty] private string modelo = string.Empty;
        [ObservableProperty] private string anio = string.Empty;
        [ObservableProperty] private string placas = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private Vehiculo? vehiculoSeleccionado;
        [ObservableProperty] private bool isEditing;

        public ObservableCollection<Vehiculo> Vehiculos { get; } = new();

        partial void OnVehiculoSeleccionadoChanged(Vehiculo? value)
        {
            if (value != null)
            {
                Marca = value.Marca;
                Modelo = value.Modelo;
                Anio = value.Anio.ToString();
                Placas = value.Placas;
                IsEditing = true;
            }
            else
            {
                IsEditing = false;
                LimpiarCampos();
            }
        }

        public VehiculosViewModel()
        {
            _firebaseService = new FirebaseService();
            _ = CargarVehiculos();
        }

        private async Task CargarVehiculos()
        {
            IsBusy = true;
            var clienteId = Preferences.Get("UsuarioId", "");
            if (!string.IsNullOrWhiteSpace(clienteId))
            {
                var lista = await _firebaseService.GetVehiculosClienteAsync(clienteId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Vehiculos.Clear();
                    foreach (var v in lista)
                        Vehiculos.Add(v);
                });
            }
            IsBusy = false;
        }

        [RelayCommand]
        private async Task AgregarVehiculo()
        {
            if (string.IsNullOrWhiteSpace(Marca) || string.IsNullOrWhiteSpace(Modelo) ||
                string.IsNullOrWhiteSpace(Anio) || string.IsNullOrWhiteSpace(Placas))
            {
                await Shell.Current.DisplayAlert("Error", "Por favor completa todos los campos del vehículo.", "OK");
                return;
            }

            if (!int.TryParse(Anio, out var anioValue) || anioValue < 1900)
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa un año válido para el vehículo.", "OK");
                return;
            }

            var clienteId = Preferences.Get("UsuarioId", "");
            var clienteNombre = Preferences.Get("UsuarioNombre", "Cliente");
            if (string.IsNullOrWhiteSpace(clienteId))
            {
                await Shell.Current.DisplayAlert("Error", "No se encontró la sesión del cliente.", "OK");
                return;
            }

            var vehiculo = new Vehiculo
            {
                Id = Guid.NewGuid().ToString(),
                ClienteId = clienteId,
                ClienteNombre = clienteNombre,
                Marca = Marca.Trim(),
                Modelo = Modelo.Trim(),
                Anio = anioValue,
                Placas = Placas.Trim().ToUpper()
            };

            if (await _firebaseService.GuardarVehiculoCliente(clienteId, vehiculo))
            {
                await CargarVehiculos();
                LimpiarCampos();
                await Shell.Current.DisplayAlert("Éxito", "Vehículo registrado correctamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el vehículo. Revisa tu conexión.", "OK");
            }
        }

        [RelayCommand]
        private async Task EliminarVehiculo(Vehiculo? vehiculo = null)
        {
            var elegido = vehiculo ?? VehiculoSeleccionado;
            if (elegido == null)
            {
                await Shell.Current.DisplayAlert("Aviso", "Selecciona un vehículo para eliminar.", "OK");
                return;
            }

            var confirm = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar {elegido.Descripcion}?", "Sí", "No");
            if (!confirm) return;

            var clienteId = Preferences.Get("UsuarioId", "");
            if (string.IsNullOrWhiteSpace(clienteId)) return;

            if (await _firebaseService.EliminarVehiculoCliente(clienteId, elegido.Id))
            {
                await CargarVehiculos();
                if (VehiculoSeleccionado?.Id == elegido.Id)
                {
                    VehiculoSeleccionado = null;
                }
                await Shell.Current.DisplayAlert("Éxito", "Vehículo eliminado.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el vehículo. Revisa tu conexión.", "OK");
            }
        }

        [RelayCommand]
        private async Task GuardarVehiculo()
        {
            if (VehiculoSeleccionado == null)
            {
                await Shell.Current.DisplayAlert("Error", "Selecciona el vehículo que deseas editar.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Marca) || string.IsNullOrWhiteSpace(Modelo) ||
                string.IsNullOrWhiteSpace(Anio) || string.IsNullOrWhiteSpace(Placas))
            {
                await Shell.Current.DisplayAlert("Error", "Por favor completa todos los campos del vehículo.", "OK");
                return;
            }

            if (!int.TryParse(Anio, out var anioValue) || anioValue < 1900)
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa un año válido para el vehículo.", "OK");
                return;
            }

            VehiculoSeleccionado.Marca = Marca.Trim();
            VehiculoSeleccionado.Modelo = Modelo.Trim();
            VehiculoSeleccionado.Anio = anioValue;
            VehiculoSeleccionado.Placas = Placas.Trim().ToUpper();

            var clienteId = Preferences.Get("UsuarioId", "");
            if (string.IsNullOrWhiteSpace(clienteId))
            {
                await Shell.Current.DisplayAlert("Error", "No se encontró la sesión del cliente.", "OK");
                return;
            }

            if (await _firebaseService.GuardarVehiculoCliente(clienteId, VehiculoSeleccionado))
            {
                await CargarVehiculos();
                VehiculoSeleccionado = null;
                await Shell.Current.DisplayAlert("Éxito", "Vehículo actualizado correctamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo actualizar el vehículo. Revisa tu conexión.", "OK");
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            VehiculoSeleccionado = null;
        }

        [RelayCommand]
        private void EditarVehiculo(Vehiculo vehiculo)
        {
            VehiculoSeleccionado = vehiculo;
        }

        private void LimpiarCampos()
        {
            Marca = string.Empty;
            Modelo = string.Empty;
            Anio = string.Empty;
            Placas = string.Empty;
        }
    }
}
