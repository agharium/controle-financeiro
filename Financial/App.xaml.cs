using Financial.Pages;
using Financial.Services;
using Realms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Financial
{
    /* TO-DO:
     * - Em HandleMovementPopup/MovementDetailsPopup ajustar os botões para ficarem na mesma linha, para não ter que rolar o popup para visualizar tudo, de forma completa;
     * - Possibilitar maneira de entregar os dízimos SEM descontar os deduzíveis (ActionSheet/DisplayAlert ao tocar no botão?);
     * - Mostrar somente os números do resumo até o dia atual, quando estiver selecionado o mês atual;
     * - Talvez fazer uma opção global que permita desabilitar a "dedutibilidade" de despesas no app inteiro, assim como funciona com os dízimos? Não sei.. Talvez. Vamos ver como segue;
     * - No cálculo de dízimos, deve ser levado em conta as despesas deduzíveis de TODO o histórico, não somente do mês atual (como está atualmente);
     * - Mostrar no MovementDetailsPopup dados acerca de dízimos e dedução; e
     * - Focus no primeiro input em HandleMovementPopup quando o tipo da operação for SAVE.
    */
    public partial class App : Application
    {
        public const int T_INCOME   = 0;
        public const int T_EXPENSE  = 1;
        public const int OP_SAVE    = 0;
        public const int OP_UPDATE  = 1;

        public static bool HOMEPAGE_NEEDS_UPDATE = false;

        public static string HomePageSelectedDateFilter = null;
        public static IncomesPageViewModel IncomesViewModel = new IncomesPageViewModel();
        public static ExpensesPageViewModel ExpensesViewModel = new ExpensesPageViewModel();

        //public static ulong RealmSchemaVersion = 3;
        public static Realm Realm => Realm.GetInstance(new RealmConfiguration() { SchemaVersion = 3 });

        public static int MOVEMENT_ID
        {
            get => Preferences.Get(nameof(MOVEMENT_ID), 1);
            set => Preferences.Set(nameof(MOVEMENT_ID), value);
        }

        public static bool UserGivesTithes
        {
            get => Preferences.Get(nameof(UserGivesTithes), false);
            set => Preferences.Set(nameof(UserGivesTithes), value);
        }

        private static bool FirstLaunch
        {
            get => Preferences.Get(nameof(FirstLaunch), true);
            set => Preferences.Set(nameof(FirstLaunch), value);
        }

        // type: 0 = short, 1 = long
        public static void Toast(string message, int type = 1)
        {
            if (type == 0)
                DependencyService.Get<IToastMessage>().ShortAlert(message);
            else
                DependencyService.Get<IToastMessage>().LongAlert(message);
        }

        public static string NormalizeCharacters(string text)
        {
            string accentedCharacters = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string normalCharacters = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < accentedCharacters.Length; i++)
                text = text.Replace(accentedCharacters[i].ToString(), normalCharacters[i].ToString());

            return text;
        }

        public App()
        {
            DependencyService.Register<IToastMessage>();

            InitializeComponent();

            MainPage = new AppShell();

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (FirstLaunch)
                {
                    FirstLaunch = false;
                    UserGivesTithes = await Shell.Current.DisplayAlert("Antes de começarmos...", "Este app dá suporte a controle pessoal de entrega de dízimos. Você gostaria de habilitar esta funcionalidade?", "Sim", "Não");
                }
            });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
