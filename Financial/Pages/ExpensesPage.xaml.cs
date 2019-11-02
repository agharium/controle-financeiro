using Financial.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExpensesPage : ContentPage
    {
        public ExpensesPage()
        {
            InitializeComponent();

            BindingContext = new ExpensesPageViewModel();
        }
    }

    class ExpensesPageViewModel : ViewModelBase
    {
        public ObservableCollection<Movement> Expenses { get; set; }
        public ExpensesPageViewModel()
        {
            Expenses = new ObservableCollection<Movement>();
        }
    }
}