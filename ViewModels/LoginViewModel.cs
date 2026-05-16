using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


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
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Atención", "Por favor ingresa tus credenciales."));
                return;
            }

            IsBusy = true;
            try
            {
                var usuario = await _firebaseService.Login(Email, Password);

                if (usuario != null)
                {
                    Preferences.Set("UsuarioId", usuario.Id);
                    Preferences.Set("UsuarioNombre", usuario.NombreCompleto);
                    Preferences.Set("UsuarioRol", usuario.Rol);
                    Preferences.Set("UsuarioCiudad", usuario.Ciudad);
                    Preferences.Set("UsuarioTelefono", usuario.Telefono);
                    Preferences.Set("UsuarioCalle", usuario.Calle);
                    Preferences.Set("UsuarioColonia", usuario.Colonia);
                    Preferences.Set("UsuarioCP", usuario.CodigoPostal);
                    Preferences.Set("UsuarioReferencias", usuario.Referencias);

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