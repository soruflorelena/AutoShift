using System;

namespace AutoShift.Models
{
    public class Vehiculo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Placas { get; set; } = string.Empty;
        public string Descripcion => $"{Marca} {Modelo} ({Anio})";
    }
}
