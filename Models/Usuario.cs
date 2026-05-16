namespace AutoShift.Models
{
    public class Usuario
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; // "Taller" o "Cliente"
        public string Telefono { get; set; } = string.Empty;

        public string CodigoPostal { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Referencias { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombre} {ApellidoPaterno}";
    }
}