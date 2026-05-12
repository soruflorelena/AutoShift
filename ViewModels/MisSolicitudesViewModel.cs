using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AutoShift.ViewModels
{
    public partial class MisSolicitudesViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private string _clienteId;
        private IDisposable? _suscripcionFirebase;

        [ObservableProperty] private bool isBusy;

        public ObservableCollection<SolicitudServicio> SolicitudesActivas { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesFinalizadas { get; } = new();

        public MisSolicitudesViewModel()
        {
            _firebaseService = new FirebaseService();
            _clienteId = Preferences.Get("UsuarioId", "");
        }

        public async Task InicializarDatosAsync()
        {
            IsBusy = true;
            await CargarSolicitudesRapido();
            IniciarEscuchaEnTiempoReal();
            IsBusy = false;
        }

        private async Task CargarSolicitudesRapido()
        {
            if (string.IsNullOrWhiteSpace(_clienteId)) return;
            var lista = await _firebaseService.GetSolicitudesClienteAsync(_clienteId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                SolicitudesActivas.Clear();
                SolicitudesFinalizadas.Clear();

                foreach (var sol in lista.OrderByDescending(s => s.Fecha))
                {
                    sol.IsExpanded = false;
                    if (sol.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                        sol.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                        SolicitudesFinalizadas.Add(sol);
                    else
                        SolicitudesActivas.Add(sol);
                }
            });
        }

        private void IniciarEscuchaEnTiempoReal()
        {
            _suscripcionFirebase?.Dispose();
            if (string.IsNullOrWhiteSpace(_clienteId)) return;

            _suscripcionFirebase = _firebaseService.EscucharSolicitudesCliente(_clienteId)
                .Subscribe(evento =>
                {
                    if (evento.Object != null && evento.EventType != Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var sol = evento.Object;
                            sol.IsExpanded = false;

                            var enActivas = SolicitudesActivas.FirstOrDefault(s => s.Id == sol.Id);
                            var enFin = SolicitudesFinalizadas.FirstOrDefault(s => s.Id == sol.Id);

                            if (enActivas != null && enActivas.Estado == sol.Estado) return;
                            if (enFin != null && enFin.Estado == sol.Estado) return;

                            if (enActivas != null) SolicitudesActivas.Remove(enActivas);
                            if (enFin != null) SolicitudesFinalizadas.Remove(enFin);

                            if (sol.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                                sol.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                                SolicitudesFinalizadas.Insert(0, sol);
                            else
                                SolicitudesActivas.Insert(0, sol);
                        });
                    }
                });
        }

        [RelayCommand] private void ExpandirSolicitud(SolicitudServicio solicitud) { if (solicitud != null) solicitud.IsExpanded = !solicitud.IsExpanded; }

        [RelayCommand]
        private async Task Volver()
        {
            _suscripcionFirebase?.Dispose();
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task NavegarAResena(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud } };
            await Shell.Current.GoToAsync("DejarResenaPage", parameters);
        }
    }
}