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
    public partial class IncomesPage : ContentPage
    {
        private string _filter;
        public string Filter
        {
            get => _filter;
            set => _filter = Uri.UnescapeDataString(value);
        }

        IncomesPageViewModel ViewModel = new IncomesPageViewModel();
        public IncomesPage()
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

    class IncomesPageViewModel : ViewModelBase
    {
        private ObservableCollection<Movement> IncomesBackup { get; set; }
        private ObservableCollection<Movement> _incomes;
        public ObservableCollection<Movement> Incomes
        {
            get => _incomes;
            set
            {
                _incomes = value;
                Notify("Incomes");
            }
        }

        public string SearchParameter { get; set; }

        public ICommand OpenAddMovementPopupSaveIncomeCommand { get; set; }
        public ICommand OpenMovementDetailsPopupIncomeCommand { get; set; }
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
                IncomesIsVisible = !value;
                Notify("TipIsVisible");
            }
        }

        private bool _incomesIsVisible;
        public bool IncomesIsVisible
        {
            get => _incomesIsVisible;
            set
            {
                _incomesIsVisible = value;
                Notify("IncomesIsVisible");
            }
        }

        private string _revenues;
        public string Revenues
        {
            get => _revenues;
            set
            {
                _revenues = value;
                Notify("Revenues");
            }
        }

        private string _remainingTithes;
        public string RemainingTithes
        {
            get => _remainingTithes;
            set
            {
                _remainingTithes = value;
                Notify("RemainingTithes");
            }
        }

        private string _totalTithes;
        public string TotalTithes
        {
            get => _totalTithes;
            set
            {
                _totalTithes = value;
                Notify("TotalTithes");
            }
        }

        private bool _handAllTithesIsEnabled;
        public bool HandAllTithesIsEnabled
        {
            get => _handAllTithesIsEnabled;
            set
            {
                _handAllTithesIsEnabled = value;
                Notify("HandAllTithesIsEnabled");
            }
        }

        private string _handAllTithesButtonColor;
        public string HandAllTithesButtonColor
        {
            get => _handAllTithesButtonColor;
            set
            {
                _handAllTithesButtonColor = value;
                Notify("HandAllTithesButtonColor");
            }
        }

        private bool _valuesOverviewWithTithes;
        public bool ValuesOverviewWithTithes
        {
            get => _valuesOverviewWithTithes;
            set
            {
                _valuesOverviewWithTithes = value;
                Notify("ValuesOverviewWithTithes");
            }
        }

        private bool _valuesOverviewWithRevenuesOnly;
        public bool ValuesOverviewWithRevenuesOnly
        {
            get => _valuesOverviewWithRevenuesOnly;
            set
            {
                _valuesOverviewWithRevenuesOnly = value;
                Notify("ValuesOverviewWithRevenuesOnly");
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

        public IncomesPageViewModel()
        {
            OpenAddMovementPopupSaveIncomeCommand = new Command(OpenAddMovementPopupSaveIncome);
            OpenMovementDetailsPopupIncomeCommand = new Command<Movement>(OpenMovementDetailsPopupIncome);
            OpenMoreOptionsActionSheetCommand = new Command<Movement>(OpenMoreOptionsActionSheet);
            HandAllTithesCommand = new Command(HandAllTithes);

            ValuesOverviewWithTithes = App.UserGivesTithes;
            ValuesOverviewWithRevenuesOnly = !ValuesOverviewWithTithes;

            Incomes = new ObservableCollection<Movement>();

            UpdateCollection(true);
        }

        private void PopulateMonthYearPicker(bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            string filterToSelect = null;
            if (selectLastItemFilter)
                filterToSelect = App.Realm.All<Movement>().OrderByDescending(i => i.Id).First().Date_Display_Filter;

            var incomes = App.Realm.All<Movement>().Where(i => i.Type == App.INCOME).OrderByDescending(i => i.Date).ToList();
            MonthYearPickerItemsSource = incomes.Select(i => i.Date_Display_Filter).Distinct().ToList();

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
                var incomes = App.Realm.All<Movement>().Where(i => i.Type == App.INCOME).OrderByDescending(i => i.Date).ToList();
                incomes = incomes.Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem).ToList();
                IncomesBackup = Incomes = new ObservableCollection<Movement>(incomes);
            }
            else
                IncomesBackup = Incomes = new ObservableCollection<Movement>();

            var revenues = Incomes.Sum(i => i.Value);
            var remainingTithes = Incomes.Where(i => i.IsTitheable == true).Where(i => i.Handed == false).Sum(i => i.Value) * 0.1;
            var totalTithes = revenues * 0.1;

            Revenues = revenues.ToString("C", CultureInfo.CurrentCulture);
            RemainingTithes = remainingTithes.ToString("C", CultureInfo.CurrentCulture);
            TotalTithes = totalTithes.ToString("C", CultureInfo.CurrentCulture);

            HandAllTithesIsEnabled = remainingTithes > 0 ? true : false;
            HandAllTithesButtonColor = HandAllTithesIsEnabled ? ((Color)Application.Current.Resources["PrimaryColor"]).ToHex() : Color.LightGray.ToHex();

            TipIsVisible = Incomes.Count() == 0 ? true : false;
        }

        private void OpenAddMovementPopupSaveIncome()
        {
            var popupPage = new HandleMovementPopup(App.INCOME, App.OP_SAVE);
            popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, true);
            PopupNavigation.Instance.PushAsync(popupPage);
        }

        private async void OpenMovementDetailsPopupIncome(Movement Income) => await PopupNavigation.Instance.PushAsync(new MovementDetailsPopup(Income));

        public async void OpenMoreOptionsActionSheet(Movement Income)
        {
            List<string> options = new List<string>();

            if (App.UserGivesTithes && Income.IsTitheable && Income.Handed == false)
                options.Add("Entregar");

            options.Add("Editar");
            options.Add("Excluir");

            var actionSheet = await Shell.Current.DisplayActionSheet("Opções", "Cancelar", null, options.ToArray());
            
            switch (actionSheet)
            {
                case "Entregar":
                    if (await Shell.Current.DisplayAlert(Income.Tithes_Display, "Entregar dízimos referentes a esta entrada?", "Sim", "Não"))
                    {
                        using (var trans = App.Realm.BeginWrite())
                        {
                            Income.Handed = true;
                            trans.Commit();
                        }
                        UpdateCollection();
                        App.Toast("Dízimo entregue com sucesso.");

                        var expense = new Movement(App.EXPENSE, Convert.ToDouble(Income.Value * 0.1), "Dízimo de " + Income.Value_Display, DateTime.Now, false);
                        App.Realm.Write(() => { App.Realm.Add(expense); });
                    }
                    break;
                case "Editar":
                    var popupPage = new HandleMovementPopup(App.INCOME, App.OP_UPDATE, Income);
                    popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, false, true);
                    await PopupNavigation.Instance.PushAsync(popupPage);
                    break;
                case "Excluir":
                    var extra = Income.Handed ? " Esta entrada também representa um dízimo já entregue e há uma despesa registrada referente à sua entrega." : "";
                    if (await Shell.Current.DisplayAlert("Confirmação", "Tem certeza que deseja excluir esta entrada?" + extra, "Sim", "Não"))
                    {
                        using (var trans = App.Realm.BeginWrite())
                        {
                            App.Realm.Remove(Income);
                            trans.Commit();
                            UpdateCollection(true, false, true);
                            App.Toast("Entrada excluída com sucesso.");
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
                Incomes = new ObservableCollection<Movement>(IncomesBackup);
            else
            {
                var words = App.NormalizeCharacters(SearchParameter.ToLower()).Split(' ');
                Incomes = new ObservableCollection<Movement>(IncomesBackup.Where(i => words.All(w => App.NormalizeCharacters(i.Description.ToLower()).Contains(w))));
            }
        }

        private async void HandAllTithes()
        {
            if (await Application.Current.MainPage.DisplayAlert(RemainingTithes, "Entregar dízimos neste valor?", "Sim", "Não"))
            {
                double total = 0;
                foreach (var i in Incomes)
                {
                    using (var trans = App.Realm.BeginWrite())
                    {
                        if (i.IsTitheable && i.Handed == false)
                        {
                            total += i.Value;
                            i.Handed = true;
                            trans.Commit();
                        }
                    }
                }
                UpdateCollection();
                App.Toast("Dízimos entregues.");

                var expense = new Movement(App.EXPENSE, total * 0.1, "Dízimos de " + (total).ToString("C", CultureInfo.CurrentCulture), DateTime.Now, false);
                App.Realm.Write(() => { App.Realm.Add(expense); });
            }
        }
    }
}