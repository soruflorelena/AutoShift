using CommunityToolkit.Maui.Views;

namespace AutoShift.Views;

public partial class CustomConfirmPopup : Popup
{
    public CustomConfirmPopup(string titulo, string mensaje)
    {
        InitializeComponent();

        LblTitulo.Text = titulo;
        LblMensaje.Text = mensaje;
    }

    private void OnYesClicked(object sender, EventArgs e)
    {
        Close(true);
    }

    private void OnNoClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}