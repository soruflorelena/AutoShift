using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoShift.Models;
using AutoShift.Services;

namespace AutoShift.ViewModels
{
    public partial class MainClienteViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private bool isBusy;
        public ObservableCollection<Taller> Talleres { get; } = new();

        public MainClienteViewModel()
        {
            _firebaseService = new FirebaseService();
            _ = CargarTalleres();
        }

        [RelayCommand]
        private async Task CargarTalleres()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var lista = await _firebaseService.GetAllTalleresAsync();
                MainThread.BeginInvokeOnMainThread(() => {
                    Talleres.Clear();
                    foreach (var t in lista) Talleres.Add(t);
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        // FUNCIONALIDAD DEL BOTÓN DIAGNÓSTICO
        [RelayCommand]
        private async Task IrADiagnostico()
        {
            await Shell.Current.GoToAsync("DiagnosticoPage");
        }

        [RelayCommand]
        private async Task VerVehiculos()
        {
            await Shell.Current.GoToAsync("VehiculosPage");
        }

        [RelayCommand]
        private async Task VerMisSolicitudes()
        {
            await Shell.Current.GoToAsync("MisSolicitudesPage");
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            Preferences.Remove("UsuarioId");
            Preferences.Remove("UsuarioRol");
            Preferences.Remove("UsuarioNombre");
            Preferences.Remove("UsuarioCiudad");
            await Shell.Current.GoToAsync("//LoginPage");
        }

        // FUNCIONALIDAD AL TOCAR UN TALLER DIRECTAMENTE
        [RelayCommand]
        private async Task VerDetalleTaller(Taller taller)
        {
            if (taller == null) return;
            var parameters = new Dictionary<string, object> { { "Taller", taller } };
            await Shell.Current.GoToAsync("DetalleTallerPage", parameters);
        }
    }
}