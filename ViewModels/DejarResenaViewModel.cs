using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoShift.ViewModels
{
    [QueryProperty(nameof(SolicitudSeleccionada), "Solicitud")]
    public partial class DejarResenaViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private SolicitudServicio? solicitudSeleccionada;
        [ObservableProperty] private int calificacion = 5;
        [ObservableProperty] private string comentario = string.Empty;
        [ObservableProperty] private bool isBusy;

        public DejarResenaViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        [RelayCommand]
        private async Task EnviarResena()
        {
            if (SolicitudSeleccionada == null) return;
            IsBusy = true;

            var nuevaResena = new Resena
            {
                SolicitudId = SolicitudSeleccionada.Id,
                ClienteId = Preferences.Get("UsuarioId", ""),
                ClienteNombre = Preferences.Get("UsuarioNombre", "Cliente"),
                Calificacion = Calificacion,
                Comentario = Comentario?.Trim() ?? string.Empty
            };

            bool resenaGuardada = await _firebaseService.GuardarResenaTaller(SolicitudSeleccionada.TallerId, nuevaResena);

            if (resenaGuardada)
            {
                var todasLasResenas = await _firebaseService.GetResenasTallerAsync(SolicitudSeleccionada.TallerId);
                double nuevoPromedio = todasLasResenas.Any() ? todasLasResenas.Average(r => r.Calificacion) : Calificacion;
                int totalResenas = todasLasResenas.Count;

                await _firebaseService.ActualizarCalificacionTaller(SolicitudSeleccionada.TallerId, Math.Round(nuevoPromedio, 1), totalResenas);

                SolicitudSeleccionada.TallerCalificado = true;
                SolicitudSeleccionada.MiCalificacion = Calificacion;

                await _firebaseService.MarcarSolicitudComoCalificada(
                    nuevaResena.ClienteId,
                    SolicitudSeleccionada.TallerId,
                    SolicitudSeleccionada.Id,
                    Calificacion,
                    nuevaResena.Comentario);

                IsBusy = false;
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("¡Gracias!", "Tu calificación ha sido enviada con éxito."));
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                IsBusy = false;
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error", "No se pudo enviar la calificación."));
            }
        }
    }
}