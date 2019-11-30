using Financial.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        HomePageViewModel ViewModel = new HomePageViewModel(); 
        public HomePage()
        {
            InitializeComponent();

            BindingContext = ViewModel;
        }

        private void OnMonthYearPickerSelectedIndexChanged(object sender, EventArgs e) => ViewModel.UpdateValues();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var backup = ViewModel.MonthYearPickerSelectedItem;
            ViewModel.PopulateMonthYearPicker();
            ViewModel.UpdateValues();
            if (backup != null && ViewModel.MonthYearPickerItemsSource.Contains(backup))
                ViewModel.MonthYearPickerSelectedItem = backup;
        }
    }

    class HomePageViewModel : ViewModelBase
    {
        public ICommand GoToIncomesPageCommand { get; set; }
        public ICommand GoToExpensesPageCommand { get; set; }

        private string _totalIncome;
        public string TotalIncome
        {
            get => _totalIncome;
            set
            {
                _totalIncome = value;
                Notify("TotalIncome");
            }
        }

        private string _totalExpense;
        public string TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                Notify("TotalExpense");
            }
        }

        private string _balance;
        public string Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                Notify("Balance");
            }
        }

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

        public HomePageViewModel()
        {
            GoToIncomesPageCommand = new Command(async () => await Shell.Current.GoToAsync($"incomes?dateFilter={MonthYearPickerSelectedItem}"));
            GoToExpensesPageCommand = new Command(async () => await Shell.Current.GoToAsync($"expenses?dateFilter={MonthYearPickerSelectedItem}"));

            PopulateMonthYearPicker();
            UpdateValues();
        }

        public void PopulateMonthYearPicker()
        {
            var movements = App.Realm.All<Movement>().OrderByDescending(i => i.Date).ToList();
            MonthYearPickerItemsSource = movements.Select(m => m.Date_Display_Filter).Distinct().ToList();

            if (MonthYearPickerItemsSource.Count() > 0)
                MonthYearPickerSelectedItem = MonthYearPickerItemsSource[0];
            else
                MonthYearPickerSelectedItem = null;
        }

        public void UpdateValues()
        {
            try
            {
                if (MonthYearPickerSelectedItem != null)
                {
                    var movements = App.Realm.All<Movement>().OrderByDescending(i => i.Date).ToList();

                    var totalIncome = movements.Where(m => m.Type == App.INCOME).Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem).Sum(m => m.Value);
                    var totalExpense = movements.Where(m => m.Type == App.EXPENSE).Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem).Sum(m => m.Value);
                    var balance = totalIncome - totalExpense;
                    TotalIncome = totalIncome.ToString("C", CultureInfo.CurrentCulture);
                    TotalExpense = totalExpense.ToString("C", CultureInfo.CurrentCulture);
                    Balance = balance.ToString("C", CultureInfo.CurrentCulture);
                }
                else
                    TotalIncome = TotalExpense = Balance = 0.ToString("C", CultureInfo.CurrentCulture);
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}