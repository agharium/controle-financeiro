using Financial.Services;
using Plugin.Settings;
using Realms;
using Xamarin.Forms;

namespace Financial
{
    public partial class App : Application
    {
        public const int INCOME = 0;
        public const int EXPENSE = 1;
        public const int OP_SAVE = 0;
        public const int OP_UPDATE = 1;

        public static ulong RealmSchemaVersion = 1;
        public static Realm Realm => Realm.GetInstance(new RealmConfiguration() { SchemaVersion = RealmSchemaVersion });

        public static int MOVEMENT_ID
        {
            get => CrossSettings.Current.GetValueOrDefault(nameof(MOVEMENT_ID), 1);
            set => CrossSettings.Current.AddOrUpdateValue(nameof(MOVEMENT_ID), value);
        }

        public static bool UserGivesTithes
        {
            get => CrossSettings.Current.GetValueOrDefault(nameof(UserGivesTithes), true);
            set => CrossSettings.Current.AddOrUpdateValue(nameof(UserGivesTithes), value);
        }

        // type: 0 = short, 1 = long
        public static void Toast(string message, int type = 1)
        {
            if (type == 0)
                DependencyService.Get<IToastMessage>().ShortAlert(message);
            else
                DependencyService.Get<IToastMessage>().LongAlert(message);
        }

        public App()
        {
            DependencyService.Register<IToastMessage>();

            InitializeComponent();

            MainPage = new AppShell();
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
