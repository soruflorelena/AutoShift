using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoShift.Models
{
    public class Taller
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = "taller_placeholder.png";
        public List<Servicio> Servicios { get; set; } = new();
        public double CalificacionPromedio { get; set; } = 0.0;
        public int TotalResenas { get; set; } = 0;
    }

    public class SolicitudServicio : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string DescripcionProblema { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string TallerId { get; set; } = string.Empty;
        public string TallerNombre { get; set; } = string.Empty;
        public string VehiculoId { get; set; } = string.Empty;
        public string VehiculoMarca { get; set; } = string.Empty;
        public string VehiculoModelo { get; set; } = string.Empty;
        public int VehiculoAnio { get; set; }
        public string VehiculoPlacas { get; set; } = string.Empty;
        public List<string> ServiciosSolicitados { get; set; } = new();
        public string DiagnosticoCliente { get; set; } = string.Empty;
        public Cotizacion? Cotizacion { get; set; }
        public DateTime? FechaCita { get; set; }
        public string MensajeTaller { get; set; } = string.Empty;
        public DateTime? FechaPropuesta { get; set; }
        public List<DateTime> FechasAlternativas { get; set; } = new();
        public DateTime? FechaValidada { get; set; }
        public string MensajeCliente { get; set; } = string.Empty;

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

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
        public bool EsNecesitaInspeccion => Estado?.Equals("NECESITA_INSPECCION", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsInspeccionSolicitada => Estado?.Equals("INSPECCION_SOLICITADA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsInspeccionAceptada => Estado?.Equals("INSPECCION_ACEPTADA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsFechaPropuesta => Estado?.Equals("FECHA_PROPUESTA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsFechaRechazada => Estado?.Equals("FECHA_RECHAZADA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsFechasPropuestas => Estado?.Equals("FECHAS_PROPUESTAS", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsFechaValidada => Estado?.Equals("FECHA_VALIDADA", StringComparison.OrdinalIgnoreCase) == true;
        public bool EsInspeccionRealizada => Estado?.Equals("INSPECCION_REALIZADA", StringComparison.OrdinalIgnoreCase) == true;

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
                    "NECESITA_INSPECCION" => "Requiere inspección",
                    "INSPECCION_SOLICITADA" => "Inspección solicitada",
                    "INSPECCION_ACEPTADA" => "Inspección aceptada",
                    "FECHA_PROPUESTA" => "Fecha propuesta",
                    "FECHA_RECHAZADA" => "Fecha rechazada",
                    "FECHAS_PROPUESTAS" => "Fechas propuestas",
                    "FECHA_VALIDADA" => "Fecha validada",
                    "INSPECCION_REALIZADA" => "Inspección realizada",
                    _ => Estado
                };
            }
        }

        public bool TallerCalificado { get; set; } = false;
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