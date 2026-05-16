using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;

namespace AutoShift.ViewModels
{
    public partial class RegistroViewModel : ObservableObject
    {
        [ObservableProperty] string nombre;
        [ObservableProperty] string apellidoPaterno;
        [ObservableProperty] string apellidoMaterno;
        [ObservableProperty] string email;
        [ObservableProperty] string telefono;
        [ObservableProperty] string password;
        [ObservableProperty] string confirmPassword;
        [ObservableProperty] bool isPasswordVisible = true; 

        // Campos de Dirección Separados
        [ObservableProperty] string codigoPostal;
        [ObservableProperty] string estado;
        [ObservableProperty] string ciudad;
        [ObservableProperty] string colonia;
        [ObservableProperty] string calle;
        [ObservableProperty] string referencias;

        [ObservableProperty] string rolSeleccionado;
        public List<string> Roles { get; } = new List<string> { "Cliente", "Taller" };

        private readonly FirebaseService _firebaseService = new FirebaseService();

        [RelayCommand]
        void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        partial void OnCodigoPostalChanged(string value)
        {
            if (value?.Length == 5) _ = ConsultarCP(value);
        }

        private async Task ConsultarCP(string cp)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"https://api.zippopotam.us/mx/{cp}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var place = doc.RootElement.GetProperty("places")[0];

                    Colonia = place.GetProperty("place name").GetString();
                    Estado = place.GetProperty("state").GetString();
                    Ciudad = "México"; 
                }
            }
            catch { /* Error de red */ }
        }

        [RelayCommand]
        async Task Registrar()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(ApellidoPaterno) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(RolSeleccionado) || string.IsNullOrWhiteSpace(Calle))
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "Faltan campos obligatorios"));
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "Las contraseñas no coinciden"));
                return;
            }

            var user = new Usuario
            {
                Nombre = Nombre,
                ApellidoPaterno = ApellidoPaterno,
                ApellidoMaterno = ApellidoMaterno,
                Email = Email,
                Password = Password,
                Rol = RolSeleccionado,
                Telefono = Telefono,
                CodigoPostal = CodigoPostal,
                Estado = Estado,
                Ciudad = Ciudad,
                Colonia = Colonia,
                Calle = Calle,
                Referencias = Referencias
            };

            if (await _firebaseService.RegistrarUsuario(user))
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Usuario registrado"));
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        [RelayCommand]
        async Task VolverLogin() => await Shell.Current.GoToAsync("//LoginPage");
    }
}