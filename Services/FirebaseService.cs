using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using AutoShift.Models;

namespace AutoShift.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient firebase = new("https://autoshift-7a011-default-rtdb.firebaseio.com/");

        // --- USUARIOS ---
        public async Task<bool> RegistrarUsuario(Usuario usuario)
        {
            try { await firebase.Child("Usuarios").PostAsync(usuario); return true; }
            catch { return false; }
        }

        public async Task<Usuario?> Login(string email, string password)
        {
            try
            {
                var result = await firebase
                    .Child("Usuarios")
                    .OnceAsync<Usuario>();
                var userAccount = result.FirstOrDefault(u =>
                    u.Object.Email.ToLower().Trim() == email.ToLower().Trim() &&
                    u.Object.Password == password);

                if (userAccount != null)
                {
                    var usuarioFinal = userAccount.Object;
                    usuarioFinal.Id = userAccount.Key;
                    return usuarioFinal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // --- TALLERES ---
        public async Task<bool> ActualizarDatosTaller(Taller taller)
        {
            try
            {
                // Registra los datos del taller (nombre y ubicación) bajo su ID único
                await firebase.Child("Talleres").Child(taller.Id).PatchAsync(new
                {
                    Nombre = taller.Nombre,
                    Ubicacion = taller.Ubicacion
                });
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> GuardarServicioTaller(string tallerId, Servicio servicio)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Servicios").Child(servicio.Id).PutAsync(servicio);
                return true;
            }
            catch { return false; }
        }

        public async Task<List<Servicio>> GetServiciosAsync(string tallerId)
        {
            try
            {
                var data = await firebase.Child("Talleres").Child(tallerId).Child("Servicios").OnceAsync<Servicio>();
                return data.Select(x => x.Object).ToList();
            }
            catch { return new List<Servicio>(); }
        }

        public async Task<bool> EliminarServicioAsync(string tallerId, string servicioId)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Servicios").Child(servicioId).DeleteAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> GuardarSolicitudTaller(string tallerId, SolicitudServicio solicitud)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Solicitudes").Child(solicitud.Id).PutAsync(solicitud);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> GuardarSolicitudCliente(string clienteId, SolicitudServicio solicitud)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Solicitudes").Child(solicitud.Id).PutAsync(solicitud);
                return true;
            }
            catch { return false; }
        }

        public async Task<List<SolicitudServicio>> GetSolicitudesTallerAsync(string tallerId)
        {
            try
            {
                var datos = await firebase.Child("Talleres").Child(tallerId).Child("Solicitudes").OnceAsync<SolicitudServicio>();
                return datos.Select(x => x.Object).ToList();
            }
            catch { return new List<SolicitudServicio>(); }
        }

        public async Task<List<SolicitudServicio>> GetSolicitudesClienteAsync(string clienteId)
        {
            try
            {
                var datos = await firebase.Child("Usuarios").Child(clienteId).Child("Solicitudes").OnceAsync<SolicitudServicio>();
                return datos.Select(x => x.Object).ToList();
            }
            catch { return new List<SolicitudServicio>(); }
        }

        public async Task<bool> GuardarVehiculoCliente(string clienteId, Vehiculo vehiculo)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Vehiculos").Child(vehiculo.Id).PutAsync(vehiculo);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> EliminarVehiculoCliente(string clienteId, string vehiculoId)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Vehiculos").Child(vehiculoId).DeleteAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<List<Vehiculo>> GetVehiculosClienteAsync(string clienteId)
        {
            try
            {
                var datos = await firebase.Child("Usuarios").Child(clienteId).Child("Vehiculos").OnceAsync<Vehiculo>();
                return datos.Select(x => x.Object).ToList();
            }
            catch { return new List<Vehiculo>(); }
        }

        public async Task<bool> GuardarCotizacion(string tallerId, string solicitudId, Cotizacion cotizacion)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Solicitudes").Child(solicitudId).Child("Cotizacion").PutAsync(cotizacion);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> GuardarCotizacionCliente(string clienteId, string solicitudId, Cotizacion cotizacion)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Solicitudes").Child(solicitudId).Child("Cotizacion").PutAsync(cotizacion);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ActualizarSolicitudEstado(string tallerId, string solicitudId, string estado)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Solicitudes").Child(solicitudId).PatchAsync(new { Estado = estado });
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ActualizarSolicitudEstadoCliente(string clienteId, string solicitudId, string estado)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Solicitudes").Child(solicitudId).PatchAsync(new { Estado = estado });
                return true;
            }
            catch { return false; }
        }

        // --- CLIENTE ---
        public async Task<List<Taller>> GetAllTalleresAsync()
        {
            try
            {
                // Traemos el nodo completo de Talleres
                var result = await firebase.Child("Talleres").OnceAsync<dynamic>();
                var lista = new List<Taller>();

                foreach (var item in result)
                {
                    // Firebase nos devuelve un objeto dinámico, extraemos los datos básicos
                    lista.Add(new Taller
                    {
                        Id = item.Key,
                        Nombre = item.Object?.Nombre?.ToString() ?? "Taller Desconocido",
                        Ubicacion = item.Object?.Ubicacion?.ToString() ?? "Ubicación no disponible"
                    });
                }
                return lista;
            }
            catch { return new List<Taller>(); }
        }

        // --- RESEÑAS ---
        public async Task<bool> GuardarResenaTaller(string tallerId, Resena resena)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).Child("Resenas").Child(resena.Id).PutAsync(resena);
                return true;
            }
            catch { return false; }
        }

        public async Task<List<Resena>> GetResenasTallerAsync(string tallerId)
        {
            try
            {
                var datos = await firebase.Child("Talleres").Child(tallerId).Child("Resenas").OnceAsync<Resena>();
                return datos.Select(x => x.Object).ToList();
            }
            catch { return new List<Resena>(); }
        }

        public async Task<bool> ActualizarCalificacionTaller(string tallerId, double promedio, int total)
        {
            try
            {
                await firebase.Child("Talleres").Child(tallerId).PatchAsync(new
                {
                    CalificacionPromedio = promedio,
                    TotalResenas = total
                });
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> MarcarSolicitudComoCalificada(string clienteId, string solicitudId, int calificacion)
        {
            try
            {
                await firebase.Child("Usuarios").Child(clienteId).Child("Solicitudes").Child(solicitudId)
                    .PatchAsync(new
                    {
                        TallerCalificado = true,
                        MiCalificacion = calificacion
                    });
                return true;
            }
            catch { return false; }
        }
    }
}