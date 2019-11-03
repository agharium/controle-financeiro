using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            BindingContext = new AboutPageViewModel();
        }
    }

    class AboutPageViewModel : ViewModelBase
    {
        public ICommand OpenPortfolioCommand { get; set; }
        public ICommand SendEmailCommand { get; set; }
        public ICommand SendWhatsappCommand { get; set; }
        public AboutPageViewModel()
        {
            OpenPortfolioCommand = new Command(OpenPortfolio);
            SendWhatsappCommand = new Command(SendWhatsapp);
            SendEmailCommand = new Command(SendEmail);
        }

        public async void OpenPortfolio()
        {
            await Browser.OpenAsync("https://ancorasistemas.com.br");
        }
        public async void SendWhatsapp()
        {
            await Browser.OpenAsync("https://api.whatsapp.com/send?phone=5551998937700&text=Olá%20José%21%20Tudo%20bem%3F");
        }

        public async void SendEmail()
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Contato via app \"Controle Financeiro\"",
                    Body = "Olá José! Tudo bem?",
                    To = { "ancorasistemasdev@gmail.com" }
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                App.Toast("O seu dispositivo não suporta e-mails.");
            }
            catch (Exception)
            {
                App.Toast("Não foi possível executar a ação.");
            }
        }
    }
}