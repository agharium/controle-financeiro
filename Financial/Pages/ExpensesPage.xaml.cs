using Financial.Models;
using Financial.Pages.Popups;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty("Filter", "filter")]
    public partial class ExpensesPage : ContentPage
    {
        private string _filter;
        public string Filter
        {
            get => _filter;
            set => _filter = Uri.UnescapeDataString(value);
        }

        ExpensesPageViewModel ViewModel = new ExpensesPageViewModel();
        public ExpensesPage()
        {
            InitializeComponent();

            BindingContext = ViewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            if (SearchBar.IsVisible == true)
            {
                SearchBar.IsVisible = false;
                MonthYearPicker.IsVisible = ValuesOverview.IsVisible = true;
                SearchBar.Text = "";
                Shell.SetNavBarIsVisible(this, true);
                return true;
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }

        private void OnSearchTapped(object sender, EventArgs e)
        {
            Shell.SetNavBarIsVisible(this, false);
            SearchBar.IsVisible = true;
            MonthYearPicker.IsVisible = ValuesOverview.IsVisible = false;
            SearchBar.Focus();
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e) => ViewModel.Search();

        private void OnSearchBarUnfocused(object sender, FocusEventArgs e) => OnBackButtonPressed();

        private void OnMonthYearPickerSelectedIndexChanged(object sender, EventArgs e) => ViewModel.UpdateCollection();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(Filter) && ViewModel.MonthYearPickerItemsSource.Contains(Filter))
                ViewModel.MonthYearPickerSelectedItem = Filter;
        }
    }

    class ExpensesPageViewModel : ViewModelBase
    {
        private ObservableCollection<Movement> ExpensesBackup { get; set; }
        private ObservableCollection<Movement> _expenses;
        public ObservableCollection<Movement> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                Notify("Expenses");
            }
        }

        public string SearchParameter { get; set; }

        public ICommand OpenAddMovementPopupSaveExpenseCommand { get; set; }
        public ICommand OpenMovementDetailsPopupExpenseCommand { get; set; }
        public ICommand OpenMoreOptionsActionSheetCommand { get; set; }
        public ICommand HandAllTithesCommand { get; set; }

        /// UI
        private bool _tipIsVisible;
        public bool TipIsVisible
        {
            get => _tipIsVisible;
            set
            {
                _tipIsVisible = value;
                ExpensesIsVisible = !value;
                Notify("TipIsVisible");
            }
        }

        private bool _expensesIsVisible;
        public bool ExpensesIsVisible
        {
            get => _expensesIsVisible;
            set
            {
                _expensesIsVisible = value;
                Notify("ExpensesIsVisible");
            }
        }

        private string _totalExpenses;
        public string TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                _totalExpenses = value;
                Notify("TotalExpenses");
            }
        }
        /// UI

        /// MONTH/YEAR PICKER
        private List<string> _monthYearPickerItemsSource;
        public List<string> MonthYearPickerItemsSource
        {
            get => _monthYearPickerItemsSource;
            set
            {
                _monthYearPickerItemsSource = value;
                Notify("MonthYearPickerItemsSource");
            }
        }
        private string _monthYearPickerSelectedItem;
        public string MonthYearPickerSelectedItem
        {
            get => _monthYearPickerSelectedItem;
            set
            {
                _monthYearPickerSelectedItem = value;
                Notify("MonthYearPickerSelectedItem");
            }
        }
        /// MONTH/YEAR PICKER
        public ExpensesPageViewModel()
        {
            OpenAddMovementPopupSaveExpenseCommand = new Command(OpenAddMovementPopupSaveExpense);
            OpenMovementDetailsPopupExpenseCommand = new Command<Movement>(OpenMovementDetailsPopupExpense);
            OpenMoreOptionsActionSheetCommand = new Command<Movement>(OpenMoreOptionsActionSheet);
            
            Expenses = new ObservableCollection<Movement>();

            UpdateCollection(true);
        }

        private void PopulateMonthYearPicker(bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            string filterToSelect = null;
            if (selectLastItemFilter)
                filterToSelect = App.Realm.All<Movement>().OrderByDescending(i => i.Id).First().Date_Display_Filter;

            var expenses = App.Realm.All<Movement>().Where(i => i.Type == App.EXPENSE).OrderByDescending(i => i.Date).ToList();
            MonthYearPickerItemsSource = expenses.Select(e => e.Date_Display_Filter).Distinct().ToList();

            string whereItIs = MonthYearPickerSelectedItem;

            if (MonthYearPickerItemsSource.Count() > 0)
            {
                if (filterToSelect != null && MonthYearPickerItemsSource.Contains(filterToSelect))
                    MonthYearPickerSelectedItem = filterToSelect;
                else if (tryToStayWhereItIs && whereItIs != null && MonthYearPickerItemsSource.Contains(whereItIs))
                    MonthYearPickerSelectedItem = whereItIs;
                else
                    MonthYearPickerSelectedItem = MonthYearPickerItemsSource[0];
            }
            else
                MonthYearPickerSelectedItem = null;
        }

        public void UpdateCollection(bool populatePicker = false, bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            if (populatePicker)
                PopulateMonthYearPicker(selectLastItemFilter, tryToStayWhereItIs);

            if (!string.IsNullOrWhiteSpace(MonthYearPickerSelectedItem))
            {
                var expenses = App.Realm.All<Movement>().Where(i => i.Type == App.EXPENSE).OrderByDescending(i => i.Date).ToList();
                expenses = expenses.Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem).ToList();
                ExpensesBackup = Expenses = new ObservableCollection<Movement>(expenses);
            }
            else
                ExpensesBackup = Expenses = new ObservableCollection<Movement>();

            var totalExpenses = Expenses.Sum(i => i.Value);
            TotalExpenses = totalExpenses.ToString("C", CultureInfo.CurrentCulture);

            TipIsVisible = Expenses.Count() == 0 ? true : false;
        }

        private void OpenAddMovementPopupSaveExpense()
        {
            var popupPage = new HandleMovementPopup(App.EXPENSE, App.OP_SAVE);
            popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, true);
            PopupNavigation.Instance.PushAsync(popupPage);
        }

        private async void OpenMovementDetailsPopupExpense(Movement Expense) => await PopupNavigation.Instance.PushAsync(new MovementDetailsPopup(Expense));

        public async void OpenMoreOptionsActionSheet(Movement Expense)
        {
            string[] options = { "Editar", "Excluir" };
            var actionSheet = await Shell.Current.DisplayActionSheet("Opções", "Cancelar", null, options);

            switch (actionSheet)
            {
                case "Editar":
                    var popupPage = new HandleMovementPopup(App.EXPENSE, App.OP_UPDATE, Expense);
                    popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, false, true);
                    await PopupNavigation.Instance.PushAsync(popupPage);
                    break;
                case "Excluir":
                    if (await Shell.Current.DisplayAlert("Confirmação", "Tem certeza que deseja excluir esta despesa?", "Sim", "Não"))
                    {
                        using (var trans = App.Realm.BeginWrite())
                        {
                            App.Realm.Remove(Expense);
                            trans.Commit();
                            UpdateCollection(true, false, true);
                            App.Toast("Despesa excluída com sucesso.");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void Search()
        {
            if (string.IsNullOrEmpty(SearchParameter))
                Expenses = new ObservableCollection<Movement>(ExpensesBackup);
            else
            {
                var words = App.NormalizeCharacters(SearchParameter.ToLower()).Split(' ');
                Expenses = new ObservableCollection<Movement>(ExpensesBackup.Where(i => words.All(w => App.NormalizeCharacters(i.Description.ToLower()).Contains(w))));
            }
        }
    }
}