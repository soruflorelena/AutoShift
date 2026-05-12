using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoShift.Models;
using AutoShift.Services;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;
using AutoShift.Views;

namespace AutoShift.ViewModels
{
    public partial class MainClienteViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private string _clienteId;
        private IDisposable? _suscripcionFirebase;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool tieneSolicitudesActivas;
        [ObservableProperty] private string nombreUsuario = "Usuario";

        public ObservableCollection<Taller> Talleres { get; } = new();
        public ObservableCollection<Vehiculo> Vehiculos { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesActivas { get; } = new();

        public MainClienteViewModel()
        {
            _firebaseService = new FirebaseService();
            _clienteId = Preferences.Get("UsuarioId", "");
            NombreUsuario = Preferences.Get("UsuarioNombre", "Usuario");
        }

        public async Task InicializarDatosAsync()
        {
            IsBusy = true;
            await CargarTalleres();
            await CargarVehiculos();
            await CargarSolicitudesActivasRapido();
            IniciarEscuchaSolicitudes();
            IsBusy = false;
        }

        private async Task CargarTalleres()
        {
            var lista = await _firebaseService.GetAllTalleresAsync();
            MainThread.BeginInvokeOnMainThread(() => {
                Talleres.Clear();
                foreach (var t in lista) Talleres.Add(t);
            });
        }

        private async Task CargarVehiculos()
        {
            var lista = await _firebaseService.GetVehiculosClienteAsync(_clienteId);
            MainThread.BeginInvokeOnMainThread(() => {
                Vehiculos.Clear();
                foreach (var v in lista) Vehiculos.Add(v);
            });
        }

        private async Task CargarSolicitudesActivasRapido()
        {
            if (string.IsNullOrWhiteSpace(_clienteId)) return;
            var lista = await _firebaseService.GetSolicitudesClienteAsync(_clienteId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                SolicitudesActivas.Clear();
                var activas = lista.Where(s => s.Estado != "FINALIZADO" && s.Estado != "RECHAZADO").OrderByDescending(s => s.Fecha);
                foreach (var s in activas) SolicitudesActivas.Add(s);
                TieneSolicitudesActivas = SolicitudesActivas.Any();
            });
        }

        private void IniciarEscuchaSolicitudes()
        {
            _suscripcionFirebase?.Dispose();
            _suscripcionFirebase = _firebaseService.EscucharSolicitudesCliente(_clienteId)
                .Subscribe(evento =>
                {
                    if (evento.Object != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var sol = evento.Object;
                            var existente = SolicitudesActivas.FirstOrDefault(s => s.Id == sol.Id);

                            if (sol.Estado == "FINALIZADO" || sol.Estado == "RECHAZADO")
                            {
                                if (existente != null) SolicitudesActivas.Remove(existente);
                            }
                            else
                            {
                                if (existente != null)
                                {
                                    int index = SolicitudesActivas.IndexOf(existente);
                                    SolicitudesActivas[index] = sol;
                                }
                                else
                                {
                                    SolicitudesActivas.Insert(0, sol);
                                }
                            }
                            TieneSolicitudesActivas = SolicitudesActivas.Any();
                        });
                    }
                });
        }

        [RelayCommand]
        private async Task AceptarSolicitud(SolicitudServicio solicitud)
        {
            var resultado = await Application.Current.MainPage.ShowPopupAsync(new CustomConfirmPopup("Aceptar Cotización", "¿Estás de acuerdo con los precios y deseas aceptar el servicio?"));
            if (resultado is bool val && val)
            {
                solicitud.Estado = "ACEPTADO";
                await _firebaseService.GuardarSolicitudTaller(solicitud.TallerId, solicitud);
                await _firebaseService.GuardarSolicitudCliente(_clienteId, solicitud);
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Cotización aceptada. El taller agendará tu cita."));
            }
        }

        [RelayCommand]
        private async Task RechazarSolicitud(SolicitudServicio solicitud)
        {
            var resultado = await Application.Current.MainPage.ShowPopupAsync(new CustomConfirmPopup("Rechazar", "¿Seguro que quieres rechazar esta cotización?"));
            if (resultado is bool val && val)
            {
                solicitud.Estado = "RECHAZADO";
                await _firebaseService.GuardarSolicitudTaller(solicitud.TallerId, solicitud);
                await _firebaseService.GuardarSolicitudCliente(_clienteId, solicitud);
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Rechazado", "La solicitud ha sido cancelada."));
            }
        }

        [RelayCommand]
        private async Task ConfirmarEntrega(SolicitudServicio solicitud)
        {
            var resultado = await Application.Current.MainPage.ShowPopupAsync(new CustomConfirmPopup("Confirmar Entrega", "¿Confirmas que recibiste tu vehículo y el trabajo es correcto?"));
            if (resultado is bool val && val)
            {
                solicitud.Estado = "FINALIZADO";
                await _firebaseService.GuardarSolicitudTaller(solicitud.TallerId, solicitud);
                await _firebaseService.GuardarSolicitudCliente(_clienteId, solicitud);
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("¡Finalizado!", "Servicio cerrado con éxito."));
            }
        }

        [RelayCommand] private async Task VerDetalleTaller(Taller taller) => await Shell.Current.GoToAsync("DetalleTallerPage", new Dictionary<string, object> { { "Taller", taller } });
        [RelayCommand] private async Task IrAMisSolicitudes() => await Shell.Current.GoToAsync("MisSolicitudesPage");
        [RelayCommand] private async Task IrAVehiculos() => await Shell.Current.GoToAsync("VehiculosPage");
        [RelayCommand] private async Task IrADiagnostico() => await Shell.Current.GoToAsync("DiagnosticoPage");

        [RelayCommand]
        private async Task CerrarSesion()
        {
            _suscripcionFirebase?.Dispose();
            Preferences.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}