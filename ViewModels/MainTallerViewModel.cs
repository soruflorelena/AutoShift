using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoShift.Models;
using AutoShift.Services;
using CommunityToolkit.Maui.Views;
using AutoShift.Views;

namespace AutoShift.ViewModels
{
    public partial class MainTallerViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private string _tallerId;
        private string? _idEnEdicion = null;

        // Propiedades de formulario
        [ObservableProperty] private string nuevoNombre = string.Empty;
        [ObservableProperty] private string nuevaDescripcion = string.Empty;
        [ObservableProperty] private decimal nuevoPrecio;
        [ObservableProperty] private string nuevasMarcas = string.Empty;
        [ObservableProperty] private string textoBotonGuardar = "GUARDAR EN CATÁLOGO";

        // KPIs que se vinculan a la vista
        [ObservableProperty] private int nuevasCount;
        [ObservableProperty] private int enCursoCount;
        [ObservableProperty] private decimal gananciasMes;

        // Tabs
        [ObservableProperty] private bool isSolicitudesVisible = true;
        [ObservableProperty] private bool isServiciosVisible = false;

        [ObservableProperty] private Color tabSolicitudesColor = Color.FromArgb("#FFC107");
        [ObservableProperty] private Color tabServiciosColor = Color.FromArgb("#1A1A1D");

        // FILOSOFÍA PREMIUM: Simplificación de listas
        public ObservableCollection<SolicitudServicio> SolicitudesActivas { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesNuevas { get; } = new();     // PENDIENTE
        public ObservableCollection<SolicitudServicio> SolicitudesEnGestion { get; } = new();  // COTIZADO, ACEPTADO, EN_PROCESO, INSPECCIONES...
        public ObservableCollection<SolicitudServicio> SolicitudesFinalizadas { get; } = new(); // FINALIZADO, RECHAZADO

        public ObservableCollection<Servicio> MisServicios { get; } = new();

        public MainTallerViewModel()
        {
            _firebaseService = new FirebaseService();
            _tallerId = Preferences.Get("UsuarioId", "Anonimo");
        }

        public async Task InicializarDatosAsync()
        {
            try
            {
                string nombreTaller = Preferences.Get("UsuarioNombre", "Taller Central");
                string ciudadTaller = Preferences.Get("UsuarioCiudad", "Sin Ubicación");

                await CargarServiciosDesdeFirebase();
                await CargarSolicitudesDesdeFirebase();

                await _firebaseService.ActualizarDatosTaller(new Taller
                {
                    Id = _tallerId,
                    Nombre = nombreTaller,
                    Ubicacion = ciudadTaller
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar: {ex.Message}");
            }
        }

        public async Task CargarServiciosDesdeFirebase()
        {
            var lista = await _firebaseService.GetServiciosAsync(_tallerId);
            MainThread.BeginInvokeOnMainThread(() => {
                MisServicios.Clear();
                foreach (var s in lista) MisServicios.Add(s);
            });
        }

        public async Task CargarSolicitudesDesdeFirebase()
        {
            var lista = await _firebaseService.GetSolicitudesTallerAsync(_tallerId);
            MainThread.BeginInvokeOnMainThread(() => {
                SolicitudesActivas.Clear();
                SolicitudesNuevas.Clear();
                SolicitudesEnGestion.Clear();
                SolicitudesFinalizadas.Clear();

                foreach (var s in lista.OrderByDescending(s => s.Fecha))
                {
                    s.IsExpanded = false;
                    SolicitudesActivas.Add(s);

                    if (s.Estado?.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        SolicitudesNuevas.Add(s);
                    }
                    else if (s.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                             s.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        SolicitudesFinalizadas.Add(s);
                    }
                    else
                    {
                        // Cualquier otra cosa está en gestión
                        SolicitudesEnGestion.Add(s);
                    }
                }
                ActualizarMetricas();
            });
        }

        private void ActualizarMetricas()
        {
            NuevasCount = SolicitudesNuevas.Count;
            EnCursoCount = SolicitudesEnGestion.Count;

            GananciasMes = SolicitudesActivas
                .Where(s => s.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true && s.Cotizacion != null)
                .Sum(s => s.Cotizacion.CostoTotal);
        }

        [RelayCommand]
        public void MostrarSolicitudes() => SwitchTab("Solicitudes");

        [RelayCommand]
        public void MostrarServicios() => SwitchTab("Servicios");

        public void SwitchTab(string tabName)
        {
            IsSolicitudesVisible = tabName == "Solicitudes";
            IsServiciosVisible = !IsSolicitudesVisible;
            TabSolicitudesColor = IsSolicitudesVisible ? Color.FromArgb("#FFC107") : Color.FromArgb("#1A1A1D");
            TabServiciosColor = IsServiciosVisible ? Color.FromArgb("#FFC107") : Color.FromArgb("#1A1A1D");

            if (IsSolicitudesVisible) _ = CargarSolicitudesDesdeFirebase();
        }

        [RelayCommand]
        private async Task ResponderSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud } };
            await Shell.Current.GoToAsync("CotizacionPage", parameters);
        }

        // --- NUEVOS COMANDOS DE ACCIÓN DIRECTA ---

        [RelayCommand]
        private async Task IrACotizar(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud }, { "AccionDirecta", "Cotizar" } };
            await Shell.Current.GoToAsync("CotizacionPage", parameters);
        }

        [RelayCommand]
        private async Task IrAProgramarCita(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud }, { "AccionDirecta", "Agendar" } };
            await Shell.Current.GoToAsync("CotizacionPage", parameters);
        }

        [RelayCommand]
        private async Task GuardarServicio()
        {
            if (string.IsNullOrWhiteSpace(NuevoNombre)) return;
            var servicio = new Servicio
            {
                Id = _idEnEdicion ?? Guid.NewGuid().ToString(),
                Nombre = NuevoNombre,
                Descripcion = NuevaDescripcion,
                PrecioBase = NuevoPrecio,
                MarcasCompatibles = NuevasMarcas.Split(',').Select(m => m.Trim()).ToList()
            };
            if (await _firebaseService.GuardarServicioTaller(_tallerId, servicio))
            {
                await CargarServiciosDesdeFirebase();
                LimpiarCampos();
            }
        }

        [RelayCommand]
        private void PrepararEdicion(Servicio s)
        {
            _idEnEdicion = s.Id;
            NuevoNombre = s.Nombre;
            NuevaDescripcion = s.Descripcion;
            NuevoPrecio = s.PrecioBase;
            NuevasMarcas = string.Join(", ", s.MarcasCompatibles);
            TextoBotonGuardar = "ACTUALIZAR SERVICIO";
            SwitchTab("Servicios");
        }

        [RelayCommand]
        private async Task EliminarServicio(Servicio s)
        {
            var resultat = await Application.Current.MainPage.ShowPopupAsync(new CustomConfirmPopup("Eliminar", "¿Borrar?"));
            bool confirm = resultat is bool val && val;
            if (confirm && await _firebaseService.EliminarServicioAsync(_tallerId, s.Id))
                MisServicios.Remove(s);
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            Preferences.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private void LimpiarCampos()
        {
            _idEnEdicion = null;
            NuevoNombre = NuevaDescripcion = NuevasMarcas = string.Empty;
            NuevoPrecio = 0;
            TextoBotonGuardar = "GUARDAR EN CATÁLOGO";
        }
    }
}