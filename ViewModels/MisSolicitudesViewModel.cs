using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.ViewModels
{
    public partial class MisSolicitudesViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private bool isBusy;

        // SE REDUJERON LAS LISTAS A SOLO 2 (EN CURSO Y FINALIZADAS)
        public ObservableCollection<SolicitudServicio> SolicitudesActivas { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesFinalizadas { get; } = new();

        public MisSolicitudesViewModel()
        {
            _firebaseService = new FirebaseService();
            _ = CargarSolicitudes();
        }

        private async Task CargarSolicitudes()
        {
            IsBusy = true;
            var clienteId = Preferences.Get("UsuarioId", "");
            if (!string.IsNullOrWhiteSpace(clienteId))
            {
                var lista = await _firebaseService.GetSolicitudesClienteAsync(clienteId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SolicitudesActivas.Clear();
                    SolicitudesFinalizadas.Clear();

                    foreach (var s in lista.OrderByDescending(s => s.Fecha))
                    {
                        s.IsExpanded = false;

                        // Separar finalizadas y rechazadas del resto "En Curso"
                        if (s.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                            s.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            SolicitudesFinalizadas.Add(s);
                        }
                        else
                        {
                            SolicitudesActivas.Add(s); // PENDIENTE, COTIZADO, EN PROCESO, etc...
                        }
                    }
                });
            }
            IsBusy = false;
        }

        [RelayCommand]
        private async Task RefrescarSolicitudes()
        {
            await CargarSolicitudes();
        }

        [RelayCommand]
        private void ExpandirSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            solicitud.IsExpanded = !solicitud.IsExpanded;
        }

        [RelayCommand]
        private async Task AceptarSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsCotizado) return;

            solicitud.Estado = "ACEPTADO";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has aceptado la cotización. El taller coordinará una cita."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task RechazarSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsCotizado) return;

            solicitud.Estado = "RECHAZADO";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has rechazado la cotización. La solicitud ha finalizado."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task MarcarCompletado(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsEnProceso) return;

            solicitud.Estado = "COMPLETADO";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has marcado la reparación como completada."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task AceptarInspeccion(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsInspeccionSolicitada) return;

            solicitud.Estado = "INSPECCION_ACEPTADA";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has aceptado la solicitud de inspección."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task RechazarInspeccion(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsInspeccionSolicitada) return;

            solicitud.Estado = "RECHAZADO";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has rechazado la solicitud de inspección. La solicitud ha finalizado."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task AceptarFecha(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsFechaPropuesta) return;

            solicitud.Estado = "FECHA_VALIDADA";
            solicitud.FechaValidada = solicitud.FechaPropuesta;
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has aceptado la fecha propuesta."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task RechazarFecha(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsFechaPropuesta) return;

            solicitud.Estado = "FECHA_RECHAZADA";
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has rechazado la fecha propuesta. Ahora puedes proponer fechas alternativas."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        [RelayCommand]
        private async Task ProponerFechas(SolicitudServicio solicitud)
        {
            if (solicitud == null || !solicitud.EsFechaRechazada) return;

            // Aquí necesitarías un diálogo para seleccionar fechas, pero por simplicidad, asumimos que se setean
            // Por ahora, solo cambiar estado
            solicitud.Estado = "FECHAS_PROPUESTAS";
            // solicitud.FechasAlternativas = ... ; // Setear en UI
            var ok = await GuardarSolicitudEnFirebase(solicitud);

            if (ok)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Éxito", "Has propuesto fechas alternativas."));
                await CargarSolicitudes();
            }
            else
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo actualizar el estado de la solicitud."));
            }
        }

        private async Task<bool> GuardarSolicitudEnFirebase(SolicitudServicio solicitud)
        {
            var clienteId = Preferences.Get("UsuarioId", "");
            var guardadoTaller = await _firebaseService.GuardarSolicitudTaller(solicitud.TallerId, solicitud);
            var guardadoCliente = await _firebaseService.GuardarSolicitudCliente(clienteId, solicitud);
            return guardadoTaller && guardadoCliente;
        }

        [RelayCommand]
        private async Task Volver()
        {
            // Cierra la pantalla modal o regresa a la anterior
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