using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoShift.Models
{
    public partial class Taller : ObservableObject
    {
        [ObservableProperty] private string id = string.Empty;
        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string ubicacion = string.Empty; // Ciudad
        [ObservableProperty] private string telefono = string.Empty;
        [ObservableProperty] private string calle = string.Empty;
        [ObservableProperty] private string colonia = string.Empty;
        [ObservableProperty] private string codigoPostal = string.Empty;
        [ObservableProperty] private string referencias = string.Empty;
        [ObservableProperty] private double calificacionPromedio = 0.0;
        [ObservableProperty] private int totalResenas = 0;

        public string DireccionCompleta
        {
            get
            {
                var partes = new List<string>();
                if (!string.IsNullOrWhiteSpace(Calle)) partes.Add(Calle);
                if (!string.IsNullOrWhiteSpace(Colonia)) partes.Add(Colonia);
                if (!string.IsNullOrWhiteSpace(CodigoPostal)) partes.Add($"CP {CodigoPostal}");
                if (!string.IsNullOrWhiteSpace(Ubicacion)) partes.Add(Ubicacion);

                return partes.Count > 0 ? string.Join(", ", partes) : "Ubicación no disponible";
            }
        }
    }

    public partial class SolicitudServicio : ObservableObject
    {
        [ObservableProperty] private string id = string.Empty;
        [ObservableProperty] private DateTime fecha;
        [ObservableProperty] private string estado = string.Empty;
        [ObservableProperty] private string descripcionProblema = string.Empty;
        [ObservableProperty] private string clienteId = string.Empty;
        [ObservableProperty] private string clienteNombre = string.Empty;
        [ObservableProperty] private string clienteTelefono = string.Empty;
        [ObservableProperty] private string tallerId = string.Empty;
        [ObservableProperty] private string tallerNombre = string.Empty;
        [ObservableProperty] private string vehiculoId = string.Empty;
        [ObservableProperty] private string vehiculoMarca = string.Empty;
        [ObservableProperty] private string vehiculoModelo = string.Empty;
        [ObservableProperty] private int vehiculoAnio;
        [ObservableProperty] private string vehiculoPlacas = string.Empty;
        [ObservableProperty] private List<string> serviciosSolicitados = new();
        [ObservableProperty] private string diagnosticoCliente = string.Empty;
        [ObservableProperty] private Cotizacion? cotizacion;
        [ObservableProperty] private DateTime? fechaCita;
        [ObservableProperty] private string mensajeTaller = string.Empty;
        [ObservableProperty] private DateTime? fechaPropuesta;
        [ObservableProperty] private List<DateTime> fechasAlternativas = new();
        [ObservableProperty] private DateTime? fechaValidada;
        [ObservableProperty] private string mensajeCliente = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoVerDetalle))]
        private bool isExpanded;

        public string TextoVerDetalle => IsExpanded ? "OCULTAR DETALLES" : "VER DETALLES";
        public string TelefonoTexto => string.IsNullOrWhiteSpace(ClienteTelefono) ? "Teléfono: No disponible" : $"Teléfono: {ClienteTelefono}";
        public string FechaSolicitudTexto => $"Fecha solicitud: {Fecha:dd/MM/yyyy HH:mm}";
        public string ObservacionesTexto => string.IsNullOrWhiteSpace(DiagnosticoCliente) ? "Observaciones: Sin información adicional" : $"Observaciones: {DiagnosticoCliente}";
        public string CostoTotalTexto => Cotizacion != null ? $"Costo total: ${Cotizacion.CostoTotal:F2}" : string.Empty;

        public string VehiculoInfo => $"{VehiculoMarca} {VehiculoModelo} ({VehiculoAnio})";
        public string ServiciosSolicitadosTexto => ServiciosSolicitados != null && ServiciosSolicitados.Any()
            ? string.Join(", ", ServiciosSolicitados)
            : "Sin servicios seleccionados";
        public bool TieneCotizacion => Cotizacion != null;
        public bool TieneCita => FechaCita.HasValue;
        public string FechaCitaTexto => FechaCita.HasValue ? FechaCita.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
        public string FechaPropuestaTexto => FechaPropuesta.HasValue ? FechaPropuesta.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
        public string FechasAlternativasTexto => FechasAlternativas != null && FechasAlternativas.Any()
            ? string.Join(", ", FechasAlternativas.Select(f => f.ToString("dd/MM/yyyy HH:mm")))
            : string.Empty;
        public string FechaValidadaTexto => FechaValidada.HasValue ? FechaValidada.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;

        public bool EsPendiente => Estado?.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsCotizado => Estado?.Equals("COTIZADO", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsAceptado => Estado?.Equals("ACEPTADO", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsRechazado => Estado?.Equals("RECHAZADO", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsCitaAsignada => Estado?.Equals("CITA_ASIGNADA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsEnProceso => Estado?.Equals("EN_PROCESO", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsCompletado => Estado?.Equals("COMPLETADO", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsFinalizado => Estado?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true;

        public string EstadoTexto
        {
            get
            {
                return Estado?.ToUpperInvariant() switch
                {
                    "PENDIENTE" => "Pendiente",
                    "COTIZADO" => "Cotizado",
                    "ACEPTADO" => "Aceptado",
                    "RECHAZADO" => "Rechazado",
                    "CITA_ASIGNADA" => "Cita agendada",
                    "EN_PROCESO" => "En proceso",
                    "COMPLETADO" => "Completado",
                    "FINALIZADO" => "Finalizado",
                    _ => Estado
                };
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarBotonCalificar))]
        private bool tallerCalificado = false;

        [ObservableProperty] private int miCalificacion = 0;
        [ObservableProperty] private string miComentarioResena = string.Empty;

        public bool MostrarBotonCalificar => !TallerCalificado;
        partial void OnEstadoChanged(string value)
        {
            OnPropertyChanged(nameof(EsPendiente));
            OnPropertyChanged(nameof(EsCotizado));
            OnPropertyChanged(nameof(EsAceptado));
            OnPropertyChanged(nameof(EsRechazado));
            OnPropertyChanged(nameof(EsCitaAsignada));
            OnPropertyChanged(nameof(EsEnProceso));
            OnPropertyChanged(nameof(EsCompletado));
            OnPropertyChanged(nameof(EsFinalizado));
            OnPropertyChanged(nameof(EstadoTexto));
        }

        partial void OnCotizacionChanged(Cotizacion? value)
        {
            OnPropertyChanged(nameof(TieneCotizacion));
            OnPropertyChanged(nameof(CostoTotalTexto));
        }

        partial void OnFechaCitaChanged(DateTime? value)
        {
            OnPropertyChanged(nameof(TieneCita));
            OnPropertyChanged(nameof(FechaCitaTexto));
        }
    }


    public class Cotizacion
    {
        public string Id { get; set; } = string.Empty;
        public string SolicitudId { get; set; } = string.Empty;
        public List<DetalleServicio> Detalles { get; set; } = new();
        public decimal CostoTotal => Detalles != null ? Detalles.Sum(d => d.Precio * d.Cantidad) : 0;
    }

    public class DetalleServicio
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Precio * Cantidad;
    }

    public class Diagnostico
    {
        public string Id { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string SolicitudId { get; set; } = string.Empty;
    }

    public class Servicio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public List<string> MarcasCompatibles { get; set; } = new();
        public string MarcasTexto => MarcasCompatibles != null && MarcasCompatibles.Any()
            ? string.Join(", ", MarcasCompatibles)
            : "Todas las marcas";
    }

    public class Resena
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SolicitudId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public int Calificacion { get; set; } = 5;
        public string Comentario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
