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
    public partial class IncomesPage : ContentPage
    {
        IncomesPageViewModel ViewModel = App.IncomesViewModel;
        public IncomesPage()
        {
            InitializeComponent();

            BindingContext = ViewModel;
        }

        private void OnMonthYearPickerSelectedIndexChanged(object sender, EventArgs e) => ViewModel.UpdateCollection();
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!string.IsNullOrEmpty(App.HomePageSelectedDateFilter) && ViewModel.MonthYearPickerItemsSource.Contains(App.HomePageSelectedDateFilter))
                ViewModel.MonthYearPickerSelectedItem = App.HomePageSelectedDateFilter;
        }

        protected override bool OnBackButtonPressed()
        {
            Shell.Current.GoToAsync($"//home");
            return true;
        }
    }

    public class IncomesPageViewModel : ViewModelBase
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

        public ICommand OpenHandleMovementPopupSaveIncomeCommand { get; set; }
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
                _monthYearPickerSelectedItem = App.HomePageSelectedDateFilter = value;
                Notify("MonthYearPickerSelectedItem");
            }
        }
        /// MONTH/YEAR PICKER

        public IncomesPageViewModel()
        {
            OpenHandleMovementPopupSaveIncomeCommand = new Command(OpenHandleMovementPopupSaveIncome);
            OpenMovementDetailsPopupIncomeCommand = new Command<Movement>(OpenMovementDetailsPopupIncome);
            OpenMoreOptionsActionSheetCommand = new Command<Movement>(OpenMoreOptionsActionSheet);
            HandAllTithesCommand = new Command(HandAllTithes);

            ValuesOverviewWithTithes = App.UserGivesTithes;
            ValuesOverviewWithRevenuesOnly = !ValuesOverviewWithTithes;

            Incomes = new ObservableCollection<Movement>();

            var incomes = App.Realm.All<Movement>().Where(i => i.Type == App.T_INCOME).OrderByDescending(i => i.Date).ToList();
            MonthYearPickerItemsSource = incomes.Select(i => i.Date_Display_Filter).Distinct().ToList();
            if (!string.IsNullOrEmpty(App.HomePageSelectedDateFilter) && MonthYearPickerItemsSource.Contains(App.HomePageSelectedDateFilter))
                MonthYearPickerSelectedItem = App.HomePageSelectedDateFilter;

            UpdateCollection();
        }

        private void PopulateMonthYearPicker(bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            string filterToSelect = null;
            if (selectLastItemFilter)
                filterToSelect = App.Realm.All<Movement>().OrderByDescending(i => i.Id).First().Date_Display_Filter;

            var incomes = App.Realm.All<Movement>().Where(i => i.Type == App.T_INCOME).OrderByDescending(i => i.Date).ToList();
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
                var incomes = App.Realm.All<Movement>().Where(i => i.Type == App.T_INCOME).OrderByDescending(i => i.Date).ToList();
                incomes = incomes.Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem).ToList();
                IncomesBackup = Incomes = new ObservableCollection<Movement>(incomes);
            }
            else
                IncomesBackup = Incomes = new ObservableCollection<Movement>();

            var deductable = App.Realm.All<Movement>()
                .Where(m => m.Type == App.T_EXPENSE)
                .Where(m => m.IsTitheable)
                .Where(m => !m.Handed)
                .ToList()
                //.Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem)
                .Sum(m => m.Value);

            var revenues = Incomes.Sum(i => i.Value);
            var remainingTithes = (Incomes.Where(i => i.IsTitheable == true).Where(i => i.Handed == false).Sum(i => i.Value) - deductable) * .1;
            var totalTithes = Incomes.Where(i => i.IsTitheable == true).Sum(i => i.Value) * .1;

            Revenues = revenues.ToString("C", CultureInfo.CurrentCulture);
            RemainingTithes = remainingTithes.ToString("C", CultureInfo.CurrentCulture);
            TotalTithes = totalTithes.ToString("C", CultureInfo.CurrentCulture);

            HandAllTithesIsEnabled = remainingTithes > 0;
            HandAllTithesButtonColor = HandAllTithesIsEnabled ? ((Color)Application.Current.Resources["PrimaryColor"]).ToHex() : Color.LightGray.ToHex();

            TipIsVisible = Incomes.Count() == 0;
        }

        private void OpenHandleMovementPopupSaveIncome() => PopupNavigation.Instance.PushAsync(new HandleMovementPopup(App.T_INCOME, App.OP_SAVE));

        private async void OpenMovementDetailsPopupIncome(Movement Income) => await PopupNavigation.Instance.PushAsync(new MovementDetailsPopup(Income));

        public async void OpenMoreOptionsActionSheet(Movement Income)
        {
            List<string> options = new List<string>();

            /*if (App.UserGivesTithes && Income.IsTitheable && Income.Handed == false)
                options.Add("Entregar");*/

            options.Add("Editar");
            options.Add("Excluir");

            var actionSheet = await Shell.Current.DisplayActionSheet("Opções", "Cancelar", null, options.ToArray());
            
            switch (actionSheet)
            {
                //case "Entregar":
                //    if (await Shell.Current.DisplayAlert(Income.Tithes_Display, "Entregar dízimos referentes a esta entrada?", "Sim", "Não"))
                //    {
                //        using (var trans = App.Realm.BeginWrite())
                //        {
                //            Income.Handed = true;
                //            trans.Commit();
                //        }
                //        UpdateCollection();
                //        App.Toast("Dízimo entregue com sucesso.");

                //        var expense = new Movement(App.T_EXPENSE, Convert.ToDouble(Income.Value * 0.1), "Dízimo de " + Income.Value_Display, DateTime.Now, false);
                //        App.Realm.Write(() => { App.Realm.Add(expense); });
                //    }
                //    break;
                case "Editar":
                    await PopupNavigation.Instance.PushAsync(new HandleMovementPopup(App.T_INCOME, App.OP_UPDATE, Income));
                    break;
                case "Excluir":
                    var extra = Income.Handed ? " Esta entrada também representa um dízimo já entregue e uma despesa foi registrada referente à sua entrega." : "";
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

        public void Filter(string _filterParameter)
        {
            if (string.IsNullOrEmpty(_filterParameter))
                Incomes = new ObservableCollection<Movement>(IncomesBackup);
            else
            {
                var words = App.NormalizeCharacters(_filterParameter.ToLower()).Split(' ');
                Incomes = new ObservableCollection<Movement>(IncomesBackup.Where(i => words.All(w => App.NormalizeCharacters(i.Description.ToLower()).Contains(w))));
            }
        }

        private async void HandAllTithes()
        {
            if (await Application.Current.MainPage.DisplayAlert(RemainingTithes, "Entregar dízimos neste valor?", "Sim", "Não"))
            {
                var incomes = Incomes
                    .Where(i => i.IsTitheable)
                    .Where(i => !i.Handed)
                    .ToList()
                    .Where(m => m.Date_Display_Filter == MonthYearPickerSelectedItem);

                foreach (var i in incomes)
                {
                    using (var trans = App.Realm.BeginWrite())
                    {
                        i.Handed = true;
                        trans.Commit();
                    }
                }

                var deductables = App.Realm.All<Movement>()
                    .Where(m => m.Type == App.T_EXPENSE)
                    .Where(m => m.IsTitheable)
                    .Where(i => !i.Handed)
                    .ToList();

                foreach (var d in deductables)
                {
                    using (var trans = App.Realm.BeginWrite())
                    {
                        d.Handed = true;
                        trans.Commit();
                    }
                }

                UpdateCollection();
                App.Toast("Dízimos entregues.");

                var total = incomes.Sum(i => i.Value) - deductables.Sum(d => d.Value);
                var expense = new Movement(App.T_EXPENSE, total * 0.1, "Dízimos de " + (total).ToString("C", CultureInfo.CurrentCulture), DateTime.Now, false, true);
                App.Realm.Write(() => { App.Realm.Add(expense); });
                App.ExpensesViewModel.UpdateCollection();
            }
        }
    }
}