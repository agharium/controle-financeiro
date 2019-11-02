using Financial.Models;
using Financial.Pages.Popups;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IncomesPage : ContentPage
    {
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
            SearchBar.Focus();
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            //ViewModel.Search();
        }

        private void OnSearchBarUnfocused(object sender, FocusEventArgs e) => OnBackButtonPressed();

        private void OnMonthYearPickerSelectedIndexChanged(object sender, EventArgs e) => ViewModel.UpdateCollection();
    }

    class IncomesPageViewModel : ViewModelBase
    {
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
        public ICommand OpenViewMovementPopupIncomeCommand { get; set; }
        public ICommand OpenMoreOptionsActionSheetCommand { get; set; }

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
            OpenViewMovementPopupIncomeCommand = new Command<Movement>(OpenViewMovementPopupIncome);
            OpenMoreOptionsActionSheetCommand = new Command<Movement>(OpenMoreOptionsActionSheet);

            //MonthYearPickerItemsSource = new List<string>();
            Incomes = new ObservableCollection<Movement>();

            UpdateCollection(true);
        }

        private void PopulateMonthYearPicker(bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            string filterToSelect = null;
            if (selectLastItemFilter)
                filterToSelect = App.Realm.All<Movement>().OrderByDescending(i => i.Id).First().Date_Display_Filter;

            try
            {
                var incomes_aux = App.Realm.All<Movement>().Where(i => i.Type == App.INCOME).OrderByDescending(i => i.Date).ToList();

                /*var toRemove = new List<string>();
                foreach (var i in MonthYearPickerItemsSource)
                {
                    var contains = true;
                    foreach (var j in incomes_aux)
                    {
                        if (i != j.Date_Display_Filter)
                        {
                            contains = false;
                            break;
                        }
                    }
                    if (!contains)
                        toRemove.Add(i);
                }

                foreach (var item in toRemove)
                {
                    Debug.WriteLine("PRA REMOVER: " + item);
                    MonthYearPickerItemsSource.Remove(item);
                }*/

                //MonthYearPickerItemsSource = new List<string>();
                //MonthYearPickerSelectedItem = null;
                //MonthYearPickerSelectedIndex = -1;

                string whereItIs = MonthYearPickerSelectedItem;

                List<string> aux = new List<string>();
                foreach (var i in incomes_aux)
                {
                    if (!aux.Contains(i.Date_Display_Filter))
                        aux.Add(i.Date_Display_Filter);
                }
                MonthYearPickerItemsSource = new List<string>(aux);

                if (MonthYearPickerItemsSource.Count() > 0){
                    if (filterToSelect != null && MonthYearPickerItemsSource.Contains(filterToSelect))
                    {
                        MonthYearPickerSelectedItem = filterToSelect;
                    } else if (tryToStayWhereItIs && whereItIs != null && MonthYearPickerItemsSource.Contains(whereItIs))
                    {
                        MonthYearPickerSelectedItem = whereItIs;
                    } 
                    else
                    {
                        MonthYearPickerSelectedItem = MonthYearPickerItemsSource[0];
                    }
                }
                else
                {
                    MonthYearPickerSelectedItem = null;
                }

                /*Debug.WriteLine("ITEMS SOURCE: ");
                for (int i = 0; i < MonthYearPickerItemsSource.Count(); i++)
                {
                    Debug.WriteLine("INDEX: " + i + " - VALUE: " + MonthYearPickerItemsSource[i]);
                }

                Debug.WriteLine("SELECTED ITEM: " + MonthYearPickerSelectedItem);*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void UpdateCollection(bool populatePicker = false, bool selectLastItemFilter = false, bool tryToStayWhereItIs = false)
        {
            if (populatePicker)
                PopulateMonthYearPicker(selectLastItemFilter, tryToStayWhereItIs);
            try
            {
                var incomes = new List<Movement>();
                var incomes_aux = App.Realm.All<Movement>().Where(i => i.Type == App.INCOME).OrderByDescending(i => i.Date).ToList();

                if (!string.IsNullOrWhiteSpace(MonthYearPickerSelectedItem))
                {
                    //var selectedDate = DateTime.ParseExact(MonthYearPickerSelectedItem, "MMMM/yyyy", CultureInfo.CurrentCulture);

                    foreach (var item in incomes_aux)
                    {
                        if (item.Date_Display_Filter == MonthYearPickerSelectedItem)
                            incomes.Add(item);
                    }
                    Incomes = new ObservableCollection<Movement>(incomes);
                }
                else
                    Incomes = new ObservableCollection<Movement>();
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            TipIsVisible = Incomes.Count() == 0 ? true : false;
        }

        private void OpenAddMovementPopupSaveIncome()
        {
            var popupPage = new HandleMovementPopup(App.INCOME, App.OP_SAVE);
            popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, true);
            PopupNavigation.Instance.PushAsync(popupPage);
        }

        private async void OpenViewMovementPopupIncome(Movement Income) => await PopupNavigation.Instance.PushAsync(new MovementDetailsPopup(Income));

        public async void OpenMoreOptionsActionSheet(Movement Income)
        {
            List<string> options = new List<string>();

            if (App.UserGivesTithes && Income.IsTitheable)
                options.Add("Entregar");

            options.Add("Editar");
            options.Add("Excluir");

            var actionSheet = await Application.Current.MainPage.DisplayActionSheet("Opções", "Cancelar", null, options.ToArray());

            switch (actionSheet)
            {
                case "Entregar":
                    if (await Application.Current.MainPage.DisplayAlert(Income.Tithes_Display, "Entregar dízimos referentes a esta entrada?", "Sim", "Não"))
                    {
                        using (var trans = App.Realm.BeginWrite())
                        {
                            Income.Handed = true;
                            trans.Commit();
                        }
                        UpdateCollection();
                        App.Toast("Dízimo entregue com sucesso.");
                    }
                    break;
                case "Editar":
                    var popupPage = new HandleMovementPopup(App.INCOME, App.OP_UPDATE, Income);
                    popupPage.CallbackEvent += (object sender, object e) => UpdateCollection(true, false, true);
                    await PopupNavigation.Instance.PushAsync(popupPage);
                    break;
                case "Excluir":
                    if (await Application.Current.MainPage.DisplayAlert("Confirmação", "Tem certeza que deseja excluir essa entrada?", "Sim", "Não"))
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
    }
}