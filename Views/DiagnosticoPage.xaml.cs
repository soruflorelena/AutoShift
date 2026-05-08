using AutoShift.ViewModels;

namespace AutoShift.Views
{
    public partial class DiagnosticoPage : ContentPage
    {
        public DiagnosticoPage()
        {
            InitializeComponent();
            BindingContext = new DiagnosticoViewModel();
        }
    }
}