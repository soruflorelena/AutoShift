namespace AutoShift.Views;

public partial class DetalleTallerPage : ContentPage
{
    public DetalleTallerPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.DetalleTallerViewModel();
    }
}