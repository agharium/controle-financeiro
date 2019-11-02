using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();

            BindingContext = new HomePageViewModel();
        }
    }

    class HomePageViewModel : ViewModelBase
    {
        public ICommand GoToIncomesPageCommand { get; set; }
        public ICommand GoToExpensesPageCommand { get; set; }

        public HomePageViewModel()
        {
            GoToIncomesPageCommand = new Command(async () => await Shell.Current.GoToAsync("//incomes"));
            GoToExpensesPageCommand = new Command(async () => await Shell.Current.GoToAsync("//expenses"));
        }
    }
}