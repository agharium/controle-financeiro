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
            if (App.HOMEPAGE_NEEDS_UPDATE)
            {
                ViewModel.TitheableIsVisible = App.UserGivesTithes.ToString();
                ViewModel.BalanceFrameColSpan = App.UserGivesTithes ? 1 : 2;

                App.HOMEPAGE_NEEDS_UPDATE = false;
            }

            base.OnAppearing();

            var backup = ViewModel.MonthYearPickerSelectedItem;
            ViewModel.PopulateMonthYearPicker();
            ViewModel.UpdateValues();
            if (backup != null && ViewModel.MonthYearPickerItemsSource.Contains(backup))
                ViewModel.MonthYearPickerSelectedItem = backup;

            if (!string.IsNullOrEmpty(App.HomePageSelectedDateFilter) && ViewModel.MonthYearPickerItemsSource.Contains(App.HomePageSelectedDateFilter))
                ViewModel.MonthYearPickerSelectedItem = App.HomePageSelectedDateFilter;
        }

        public void OnForwardSwipe(object sender, SwipedEventArgs e)
        {
            if (MonthYearPicker.SelectedIndex < MonthYearPicker.Items.Count - 1)
                MonthYearPicker.SelectedIndex++;
            else
                MonthYearPicker.SelectedIndex = 0;
        }

        public void OnBackwardSwipe(object sender, SwipedEventArgs e)
        {
            if (MonthYearPicker.SelectedIndex > 0)
                MonthYearPicker.SelectedIndex--;
            else
                MonthYearPicker.SelectedIndex = MonthYearPicker.Items.Count - 1;
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

        private string _titheable;
        public string Titheable
        {
            get => _titheable;
            set
            {
                _titheable = value;
                Notify("Titheable");
            }
        }

        private string _titheableIsVisible;
        public string TitheableIsVisible
        {
            get => _titheableIsVisible;
            set
            {
                _titheableIsVisible = value;
                Notify("TitheableIsVisible");
            }
        }

        private int _balanceFrameColSpan;
        public int BalanceFrameColSpan
        {
            get => _balanceFrameColSpan;
            set
            {
                _balanceFrameColSpan = value;
                Notify("BalanceFrameColSpan");
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
                _monthYearPickerSelectedItem = App.HomePageSelectedDateFilter = value;
                Notify("MonthYearPickerSelectedItem");
            }
        }
        /// MONTH/YEAR PICKER

        public HomePageViewModel()
        {
            GoToIncomesPageCommand = new Command(async () => await Shell.Current.GoToAsync($"//incomes"));
            GoToExpensesPageCommand = new Command(async () => await Shell.Current.GoToAsync($"//expenses"));

            TitheableIsVisible = App.UserGivesTithes.ToString();
            BalanceFrameColSpan = App.UserGivesTithes ? 1 : 2;

            PopulateMonthYearPicker();

            if (!string.IsNullOrEmpty(App.HomePageSelectedDateFilter) && MonthYearPickerItemsSource.Contains(App.HomePageSelectedDateFilter))
                MonthYearPickerSelectedItem = App.HomePageSelectedDateFilter;

            UpdateValues();
        }

        public void PopulateMonthYearPicker()
        {
            var movements = App.Realm.All<Movement>().OrderByDescending(i => i.Date).ToList();
            var list = movements.Select(m => m.Date_Display_Filter).Distinct().ToList();
            list.Add("Todo o período");
            MonthYearPickerItemsSource = list;

            if (MonthYearPickerItemsSource.Count() > 0)
                MonthYearPickerSelectedItem = MonthYearPickerItemsSource[0];
            else
                MonthYearPickerSelectedItem = null;
        }

        public void UpdateValues()
        {
            try
            {
                List<Movement> incomes, expenses;

                if (MonthYearPickerSelectedItem == "Todo o período")
                {
                    incomes = App.Realm.All<Movement>()
                        .Where(m => m.Type == App.INCOME)
                        .ToList();

                    expenses = App.Realm.All<Movement>()
                        .Where(m => m.Type == App.EXPENSE)
                        .ToList();
                }
                else
                {
                    var movements = App.Realm.All<Movement>()
                        .ToList()
                        .Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem);

                    incomes = movements
                        .Where(m => m.Type == App.INCOME)
                        .ToList();

                    expenses = movements
                        .Where(m => m.Type == App.EXPENSE)
                        .ToList();
                }

                double totalIncome, totalExpense, totalTitheable, deductable;

                totalIncome = incomes.Sum(m => m.Value);
                totalExpense = expenses.Sum(m => m.Value);

                deductable = expenses
                    .Where(m => m.IsDeductable)
                    .Sum(m => m.Value);

                totalTitheable = incomes
                    .Where(m => m.IsTitheable)
                    .Sum(m => m.Value)
                    - deductable;

                TotalIncome = totalIncome.ToString("C", CultureInfo.CurrentCulture);
                TotalExpense = totalExpense.ToString("C", CultureInfo.CurrentCulture);
                Balance = (totalIncome - totalExpense).ToString("C", CultureInfo.CurrentCulture);
                Titheable = totalTitheable.ToString("C", CultureInfo.CurrentCulture);
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}