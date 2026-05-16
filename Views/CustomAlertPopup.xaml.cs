using CommunityToolkit.Maui.Views;

namespace AutoShift.Views;

public partial class CustomAlertPopup : Popup
{
    public CustomAlertPopup(string titulo, string mensaje)
    {
        InitializeComponent();

        // Asignamos los textos que nos mande el ViewModel
        LblTitulo.Text = titulo;
        LblMensaje.Text = mensaje;
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        Close();
    }
}