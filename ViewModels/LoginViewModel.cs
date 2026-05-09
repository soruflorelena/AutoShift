using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoShift.Services;
using AutoShift.Models;
using CommunityToolkit.Maui.Views;
using AutoShift.Views;


namespace AutoShift.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isBusy;

        public LoginViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        [RelayCommand]
        private async Task Ingresar()
        {
            // 1. Validación básica
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Atención", "Por favor ingresa tus credenciales."));
                return;
            }

            IsBusy = true;
            try
            {
                // 2. Intentar Login
                var usuario = await _firebaseService.Login(Email, Password);

                if (usuario != null)
                {
                    // 3. GUARDAR SESIÓN (Esto es lo que usamos en las otras pantallas)
                    Preferences.Set("UsuarioId", usuario.Id);
                    Preferences.Set("UsuarioNombre", usuario.NombreCompleto);
                    Preferences.Set("UsuarioRol", usuario.Rol);
                    Preferences.Set("UsuarioCiudad", usuario.Ciudad);

                    // 4. NAVEGACIÓN SEGÚN EL ROL
                    if (usuario.Rol == "Taller")
                    {
                        await Shell.Current.GoToAsync("//MainTallerPage");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("//MainClientePage");
                    }
                }
                else
                {
                    await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "Correo o contraseña incorrectos."));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error de Conexión", ex.Message));
            }
            finally
            {
                IsBusy = false;
            }

            Email = string.Empty;
            Password = string.Empty;
        }

        [RelayCommand]
        private async Task IrARegistro()
        {
            await Shell.Current.GoToAsync("RegistroPage");
        }
    }
}