using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private bool firstTime = true;
        public SettingsPage()
        {
            InitializeComponent();

            //BindingContext = new SettingsPageViewModel();
            TithesControlSwitch.IsToggled = App.UserGivesTithes;
        }

        private void TithesControlSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (!firstTime)
            {
                App.UserGivesTithes = e.Value;

                var extra = !e.Value ? "des" : "";
                App.Toast("Funcionalidade de controle de dízimos " + extra + "ativada.");
            }
            firstTime = false;
        }
    }

    /*class SettingsPageViewModel : ViewModelBase
    {
        private bool _isTithesControlEnabled;
        public bool IsTithesControlEnabled
        {
            get => _isTithesControlEnabled;
            set
            {
                _isTithesControlEnabled = value;
                Notify("IsTithesControlEnabled");
            }
        }

        public SettingsPageViewModel()
        {
            IsTithesControlEnabled = App.UserGivesTithes;
        }
    }*/
}