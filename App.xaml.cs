namespace AutoShift
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 1. Instanciamos el mapa de navegación
            var shell = new AppShell();

            // 2. Revisamos si el usuario ya se había logueado antes
            string usuarioId = Preferences.Get("UsuarioId", string.Empty);
            string rol = Preferences.Get("UsuarioRol", string.Empty);

            // 3. Si hay un ID guardado, lo mandamos directo a su pantalla principal
            if (!string.IsNullOrEmpty(usuarioId))
            {
                if (rol == "Taller")
                {
                    shell.GoToAsync("//MainTallerPage");
                }
                else
                {
                    shell.GoToAsync("//MainClientePage");
                }
            }

            // 4. Creamos la ventana con la ruta ya decidida
            return new Window(shell);
        }
    }
}