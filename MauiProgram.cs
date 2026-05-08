using Microsoft.Extensions.Logging;
using AutoShift.Views;
using AutoShift.ViewModels;
using CommunityToolkit.Maui;

namespace AutoShift
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Login
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<LoginViewModel>();

            // Registro
            builder.Services.AddTransient<RegistroPage>();
            builder.Services.AddTransient<RegistroViewModel>();

            // Main Pages & ViewModels (Taller, Cliente y Cotizaciones)
            builder.Services.AddTransient<MainClientePage>();
            builder.Services.AddTransient<MainTallerPage>();
            builder.Services.AddTransient<MainTallerViewModel>();

            builder.Services.AddTransient<CotizacionPage>();
            builder.Services.AddTransient<CotizacionViewModel>();

            builder.Services.AddTransient<DiagnosticoPage>();
            builder.Services.AddTransient<DiagnosticoViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}