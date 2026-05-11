using AutoShift.Models;
using AutoShift.Services;
using AutoShift.Views;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoShift.ViewModels
{
    public partial class DiagnosticoViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] private string sintomas = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool mostrarResultados;

        public ObservableCollection<ResultadoIA> ResultadosMultiples { get; } = new();

        public DiagnosticoViewModel()
        {
            _firebaseService = new FirebaseService();
        }

        [RelayCommand]
        private async Task AnalizarSintomas()
        {
            if (string.IsNullOrWhiteSpace(Sintomas) || Sintomas.Length < 10)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("IA AutoShift", "Por favor, describa con más tecnicismos o detalle su problema para un análisis profesional."));
                return;
            }

            IsBusy = true;
            MostrarResultados = false;
            ResultadosMultiples.Clear();

            // EJECUCIÓN DEL MOTOR DE INFERENCIA MULTI-FALLA PROFESIONAL
            var fallasDetectadas = MotorInferenciaProfesional(Sintomas.ToLower());

            try
            {
                var todosLosTalleres = await _firebaseService.GetAllTalleresAsync();

                foreach (var falla in fallasDetectadas)
                {
                    var resultado = new ResultadoIA
                    {
                        Categoria = falla.Categoria,
                        Explicacion = falla.Explicacion
                    };

                    foreach (var taller in todosLosTalleres)
                    {
                        var servicios = await _firebaseService.GetServiciosAsync(taller.Id);

                        // Búsqueda semántica cruzada entre etiquetas de la IA y el catálogo del taller
                        bool cubreFalla = servicios.Any(s =>
                            falla.Tags.Any(tag => s.Nombre.ToLower().Contains(tag) || s.Descripcion.ToLower().Contains(tag)));

                        if (cubreFalla)
                        {
                            MainThread.BeginInvokeOnMainThread(() => resultado.TalleresQueLoArreglan.Add(taller));
                        }
                    }
                    ResultadosMultiples.Add(resultado);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.ShowPopupAsync(new CustomAlertPopup("Error de Sistema", ex.Message));
            }
            finally
            {
                IsBusy = false;
                MostrarResultados = true;
            }
        }

        private List<InfoFalla> MotorInferenciaProfesional(string input)
        {
            var fallasIdentificadas = new List<InfoFalla>();

            // BASE DE CONOCIMIENTO TÉCNICA AMPLIADA
            var catalogoIA = new[]
            {
                new InfoFalla {
                    Categoria = "Sistema de Frenado y Seguridad Activa",
                    Keys = new[] { "fren", "balata", "disco", "pedal", "chilla", "raspa", "vibr", "abs", "liquido" },
                    Explicacion = "El análisis detecta una degradación en el material de fricción o fatiga térmica en los discos. Es crítico revisar el espesor de las balatas y la presión hidráulica del sistema para evitar pérdida de eficiencia.",
                    Tags = new[] { "frenos", "balatas", "seguridad", "mantenimiento" }
                },
                new InfoFalla {
                    Categoria = "Tren Motriz y Gestión de Combustión",
                    Keys = new[] { "motor", "aceite", "humo", "tira", "riega", "gotea", "fuerza", "cascabele", "bujia", "check engine", "vibracion" },
                    Explicacion = "Se identifican anomalías en el ciclo Otto de combustión o pérdida de estanqueidad en sellos. Los síntomas sugieren una revisión de compresión, inyectores o empaques críticos del bloque.",
                    Tags = new[] { "motor", "aceite", "fuga", "afinacion" }
                },
                new InfoFalla {
                    Categoria = "Suspensión, Amortiguación y Ejes",
                    Keys = new[] { "tronido", "golpe", "bache", "amortiguador", "buje", "suspens", "clanc", "suelto", "rebote" },
                    Explicacion = "La inestabilidad estructural detectada apunta a un desgaste severo en los componentes elásticos (bujes) o fatiga en las válvulas internas de los amortiguadores.",
                    Tags = new[] { "suspension", "amortiguadores", "alineacion" }
                },
                new InfoFalla {
                    Categoria = "Neumáticos y Geometría de Dirección",
                    Keys = new[] { "llanta", "rueda", "aire", "ponchado", "compañaron", "liso", "vibracion volante", "jala", "direccion" },
                    Explicacion = "Se identifica una anomalía en la huella de contacto o pérdida de presión neumática. Requiere inspección de integridad estructural del caucho y posiblemente alineación y balanceo.",
                    Tags = new[] { "llantas", "alineacion", "direccion" }
                },
                new InfoFalla {
                    Categoria = "Sistema Eléctrico y de Carga",
                    Keys = new[] { "bateria", "arranc", "marcha", "alternad", "luz", "tablero", "fusible", "corto", "chispa" },
                    Explicacion = "Fluctuaciones de voltaje detectadas. El sistema de carga (alternador) o el acumulador presentan fallos en el ciclo de retención de energía o entrega de corriente inicial.",
                    Tags = new[] { "electrico", "bateria", "marcha", "alternador" }
                },
                new InfoFalla {
                    Categoria = "Transmisión y Tren de Engranes",
                    Keys = new[] { "caja", "velocidad", "cambio", "patea", "clutch", "embrague", "zumbido", "transmision" },
                    Explicacion = "Se detecta un posible fallo en la sincronización de marchas o degradación del fluido ATF/Manual. El patinamiento del embrague sugiere desgaste en el plato de presión.",
                    Tags = new[] { "transmision", "clutch", "caja" }
                },
                new InfoFalla {
                    Categoria = "Sistema de Enfriamiento y HVAC",
                    Keys = new[] { "calienta", "vapor", "agua", "anticongelante", "radiador", "aire", "acondicionado", "enfria", "calor" },
                    Explicacion = "Fallo en el intercambio de calor. El motor presenta riesgo de choque térmico por obstrucción en el radiador o pérdida de gas refrigerante en el sistema de cabina.",
                    Tags = new[] { "radiador", "enfriamiento", "aire acondicionado" }
                }
            };

            foreach (var falla in catalogoIA)
            {
                // Si la descripción del usuario contiene al menos una palabra clave de esta categoría
                if (falla.Keys.Any(k => input.Contains(k)))
                {
                    fallasIdentificadas.Add(falla);
                }
            }

            if (fallasIdentificadas.Count == 0)
            {
                fallasIdentificadas.Add(new InfoFalla
                {
                    Categoria = "Revisión Preventiva General",
                    Explicacion = "El análisis no arrojó coincidencias críticas. Se recomienda un diagnóstico físico multipuntos para identificar ruidos parásitos o fallas intermitentes.",
                    Tags = new[] { "revision", "diagnostico", "mantenimiento" }
                });
            }

            return fallasIdentificadas;
        }

        [RelayCommand]
        private async Task SeleccionarTaller(Taller taller)
        {
            var parameters = new Dictionary<string, object>
            {
                { "Taller", taller },
                { "DescripcionCliente", Sintomas }
            };
            await Shell.Current.GoToAsync("DetalleTallerPage", parameters);

            // 1. LIMPIAR EL FORMULARIO DESPUÉS DE ENVIAR
            Sintomas = string.Empty;
            ResultadosMultiples.Clear();
            MostrarResultados = false;
        }
    }

    // Clases de soporte
    public class InfoFalla
    {
        public string Categoria { get; set; } = string.Empty;
        public string[] Keys { get; set; } = Array.Empty<string>();
        public string Explicacion { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class ResultadoIA
    {
        public string Categoria { get; set; } = string.Empty;
        public string Explicacion { get; set; } = string.Empty;
        public ObservableCollection<Taller> TalleresQueLoArreglan { get; } = new();
    }
}