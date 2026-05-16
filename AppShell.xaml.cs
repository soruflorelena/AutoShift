namespace AutoShift
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.RegistroPage), typeof(Views.RegistroPage));
            Routing.RegisterRoute("DiagnosticoPage", typeof(Views.DiagnosticoPage));
            Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
            Routing.RegisterRoute(nameof(Views.DetalleTallerPage), typeof(Views.DetalleTallerPage));
            Routing.RegisterRoute(nameof(Views.VehiculosPage), typeof(Views.VehiculosPage));
            Routing.RegisterRoute(nameof(Views.MisSolicitudesPage), typeof(Views.MisSolicitudesPage));
            Routing.RegisterRoute(nameof(Views.CotizacionPage), typeof(Views.CotizacionPage));
            Routing.RegisterRoute("DejarResenaPage", typeof(Views.DejarResenaPage));
        }
    }
}
