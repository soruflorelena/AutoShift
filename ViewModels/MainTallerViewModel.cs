using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.ViewModels
{
    public partial class MainTallerViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;
        private string _tallerId;
        private string? _idEnEdicion = null;
        private IDisposable? _suscripcionFirebase;

        [ObservableProperty] private string nuevoNombre = string.Empty;
        [ObservableProperty] private string nuevaDescripcion = string.Empty;
        [ObservableProperty] private decimal nuevoPrecio;
        [ObservableProperty] private string nuevasMarcas = string.Empty;
        [ObservableProperty] private string textoBotonGuardar = "GUARDAR EN CATÁLOGO";
        [ObservableProperty] private bool isEditing = false;

        [ObservableProperty] private int nuevasCount;
        [ObservableProperty] private int enCursoCount;
        [ObservableProperty] private decimal gananciasMes;

        [ObservableProperty] private bool isSolicitudesVisible = false;
        [ObservableProperty] private bool isServiciosVisible = true;

        [ObservableProperty] private Color tabSolicitudesColor = Color.FromArgb("#FFC107");
        [ObservableProperty] private Color tabServiciosColor = Color.FromArgb("#1A1A1D");

        public ObservableCollection<SolicitudServicio> SolicitudesActivas { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesNuevas { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesEnGestion { get; } = new();
        public ObservableCollection<SolicitudServicio> SolicitudesFinalizadas { get; } = new();

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
                string telefonoTaller = Preferences.Get("UsuarioTelefono", "No disponible");
                string calleTaller = Preferences.Get("UsuarioCalle", "");
                string coloniaTaller = Preferences.Get("UsuarioColonia", "");
                string cpTaller = Preferences.Get("UsuarioCP", "");
                string referenciasTaller = Preferences.Get("UsuarioReferencias", "");

                await CargarServiciosDesdeFirebase();
                await CargarSolicitudesRapido();

                IniciarEscuchaEnTiempoReal();

                await _firebaseService.ActualizarDatosTaller(new Taller
                {
                    Id = _tallerId,
                    Nombre = nombreTaller,
                    Ubicacion = ciudadTaller,
                    Telefono = telefonoTaller,
                    Calle = calleTaller,
                    Colonia = coloniaTaller,
                    CodigoPostal = cpTaller,
                    Referencias = referenciasTaller
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar: {ex.Message}");
            }
        }

        public async Task CargarSolicitudesRapido()
        {
            var lista = await _firebaseService.GetSolicitudesTallerAsync(_tallerId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SolicitudesActivas.Clear();
                SolicitudesNuevas.Clear();
                SolicitudesEnGestion.Clear();
                SolicitudesFinalizadas.Clear();

                foreach (var sol in lista.OrderByDescending(s => s.Fecha))
                {
                    sol.IsExpanded = false;
                    SolicitudesActivas.Add(sol);

                    if (sol.Estado?.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase) == true)
                        SolicitudesNuevas.Add(sol);
                    else if (sol.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                             sol.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                        SolicitudesFinalizadas.Add(sol);
                    else
                        SolicitudesEnGestion.Add(sol);
                }
                ActualizarMetricas();
            });
        }

        private void IniciarEscuchaEnTiempoReal()
        {
            _suscripcionFirebase?.Dispose();
            _suscripcionFirebase = _firebaseService.EscucharSolicitudesTaller(_tallerId)
                .Subscribe(evento =>
                {
                    if (evento.Object != null && evento.EventType != Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var sol = evento.Object;
                            sol.IsExpanded = false;

                            var existente = SolicitudesActivas.FirstOrDefault(s => s.Id == sol.Id);
                            if (existente != null)
                            {
                                if (existente.Estado == sol.Estado) return; 
                                RemoverDeListas(sol.Id);
                            }

                            SolicitudesActivas.Add(sol);

                            if (sol.Estado?.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase) == true)
                                SolicitudesNuevas.Insert(0, sol);
                            else if (sol.Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                                     sol.Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true)
                                SolicitudesFinalizadas.Insert(0, sol);
                            else
                                SolicitudesEnGestion.Insert(0, sol);

                            ActualizarMetricas();
                        });
                    }
                });
        }

        private void RemoverDeListas(string id)
        {
            var extActiva = SolicitudesActivas.FirstOrDefault(s => s.Id == id);
            if (extActiva != null) SolicitudesActivas.Remove(extActiva);

            var extNueva = SolicitudesNuevas.FirstOrDefault(s => s.Id == id);
            if (extNueva != null) SolicitudesNuevas.Remove(extNueva);

            var extGestion = SolicitudesEnGestion.FirstOrDefault(s => s.Id == id);
            if (extGestion != null) SolicitudesEnGestion.Remove(extGestion);

            var extFin = SolicitudesFinalizadas.FirstOrDefault(s => s.Id == id);
            if (extFin != null) SolicitudesFinalizadas.Remove(extFin);
        }

        public async Task CargarServiciosDesdeFirebase()
        {
            var lista = await _firebaseService.GetServiciosAsync(_tallerId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MisServicios.Clear();
                foreach (var s in lista) MisServicios.Add(s);
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
        public async Task MostrarSolicitudesNuevas()
        {
            if (SolicitudesNuevas.Count == 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Nuevas Solicitudes", "No hay solicitudes nuevas"));
                return;
            }

            var popup = new Views.SolicitudesListPopup(SolicitudesNuevas, "NUEVAS SOLICITUDES", false);
            await Shell.Current.CurrentPage.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        public async Task MostrarSolicitudesEnGestion()
        {
            if (SolicitudesEnGestion.Count == 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("En Gestión", "No hay trabajos en gestión"));
                return;
            }

            var popup = new Views.SolicitudesListPopup(SolicitudesEnGestion, "TRABAJOS EN GESTIÓN", false);
            await Shell.Current.CurrentPage.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        public async Task MostrarSolicitudesFinalizadas()
        {
            if (SolicitudesFinalizadas.Count == 0)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Finalizadas", "No hay solicitudes finalizadas"));
                return;
            }

            var popup = new Views.SolicitudesListPopup(SolicitudesFinalizadas, "FINALIZADAS", true);
            await Shell.Current.CurrentPage.Navigation.PushModalAsync(popup);
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
        }

        [RelayCommand]
        private async Task ResponderSolicitud(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            var parameters = new Dictionary<string, object> { { "Solicitud", solicitud } };
            await Shell.Current.GoToAsync("CotizacionPage", parameters);
        }

        [RelayCommand]
        private void ToggleDetalle(SolicitudServicio solicitud)
        {
            if (solicitud == null) return;
            solicitud.IsExpanded = !solicitud.IsExpanded;
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
            IsEditing = true;
            SwitchTab("Servicios");
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            LimpiarCampos();
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
            _suscripcionFirebase?.Dispose();
            Preferences.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private void LimpiarCampos()
        {
            _idEnEdicion = null;
            NuevoNombre = NuevaDescripcion = NuevasMarcas = string.Empty;
            NuevoPrecio = 0;
            TextoBotonGuardar = "GUARDAR EN CATÁLOGO";
            IsEditing = false;
        }
    }
}