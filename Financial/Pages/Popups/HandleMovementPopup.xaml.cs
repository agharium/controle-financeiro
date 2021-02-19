using dotMorten.Xamarin.Forms;
using Financial.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HandleMovementPopup : PopupPage
    {
        HandleMovementPopupViewModel ViewModel;

        public HandleMovementPopup(int type, int operation, Movement movement = null)
        {
            InitializeComponent();

            BindingContext = ViewModel = new HandleMovementPopupViewModel(type, operation, movement);
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(sender.Text))
                    sender.ItemsSource = null;
                else
                {
                    ViewModel.Description = sender.Text;

                    var words = App.NormalizeCharacters(sender.Text.ToLower()).Split(' ');
                    var movements = App.Realm.All<Movement>().Where(m => m.Type == ViewModel.Type).ToList().Select(m => m.Description).ToList();

                    sender.ItemsSource = movements.Where(i => words.All(w => App.NormalizeCharacters(i.ToLower()).Contains(w))).Distinct().ToList();
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) => ViewModel.Description = (string)args.SelectedItem;
    }

    class HandleMovementPopupViewModel : ViewModelBase
    {
        public int Type { get; set; } // 0 = income, 1 = expense
        private int Operation { get; set; } // 0 = save, 1 = update
        private Movement Movement { get; set; }

        public string Value { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        private bool _isTitheable;
        public bool IsTitheable {
            get => _isTitheable;
            set
            {
                _isTitheable = value;
                Notify("IsTitheable");
            }
        }

        public string Title { get; set; }
        public DateTime MaxDate { get; set; }
        public string IsTitheableText { get; set; }
        public bool IsTitheableVisible { get; set; }
        public bool IsTitheableEnabled { get; set; }
        public string IsTitheableColor { get; set; }
        public ICommand ToggleIsTitheableCommand { get; set; }

        public ICommand SaveMovementCommand { get; set; }

        public HandleMovementPopupViewModel(int type, int operation, Movement movement)
        {
            Type = type;
            Operation = operation;
            Movement = movement;

            Date = DateTimeOffset.Now.LocalDateTime;
            MaxDate = Date.AddMonths(1);
            Description = "";

            IsTitheableText = Type == App.INCOME ? "Entregar dízimos desta entrada" : "Deduzir esta despesa no cálculo dos dízimos";
            IsTitheableColor = ((Color)Application.Current.Resources["PrimaryColor"]).ToHex();
            IsTitheableVisible = IsTitheableEnabled = App.UserGivesTithes;
            ToggleIsTitheableCommand = new Command(ToggleIsTitheable);

            if (Operation == App.OP_SAVE)
            {
                if (Type == App.INCOME)
                {
                    SaveMovementCommand = new Command(SaveIncome);
                    IsTitheable = IsTitheableVisible;
                } else
                {
                    SaveMovementCommand = new Command(SaveExpense);
                    IsTitheable = !IsTitheableVisible;
                }

                Title = "Nova ";
            } else
            {
                Value = Convert.ToString(Movement.Value);
                Description = Movement.Description;
                Date = Movement.Date.DateTime;
                IsTitheable = Movement.IsTitheable;
                IsTitheableEnabled = !Movement.Handed;
                if (!IsTitheableEnabled)
                    IsTitheableColor = "Gray";

                SaveMovementCommand = Type == App.INCOME ? new Command(UpdateIncome) : new Command(UpdateExpense);

                Title = "Editar ";
            }

            Title += Type == App.INCOME ? "entrada" : "despesa";

            //if (Type == App.INCOME && Operation == App.OP_SAVE)
            //{
            //    SaveMovementCommand = new Command(SaveIncome);
            //    IsTitheable = IsTitheableVisible;
            //} else if (Type == App.INCOME && Operation == App.OP_UPDATE)
            //    SaveMovementCommand = new Command(UpdateIncome);
            //else if (Type == App.EXPENSE && Operation == App.OP_SAVE)
            //{
            //    SaveMovementCommand = new Command(SaveExpense);
            //    IsTitheable = !IsTitheableVisible;
            //} else if (Type == App.EXPENSE && Operation == App.OP_UPDATE)
            //    SaveMovementCommand = new Command(UpdateExpense);
        }

        private async void SaveIncome()
        {
            if (await FieldsVerification())
            {
                var income = new Movement(App.INCOME, Convert.ToDouble(Value), Description, Date, IsTitheable);
                App.Realm.Write(() => { App.Realm.Add(income); });
                App.IncomesViewModel.UpdateCollection(true, true);
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada registrada com sucesso.");
            }
        }

        private async void UpdateIncome()
        {
            if (await FieldsVerification())
            {
                Date = DateTime.SpecifyKind(Date, DateTimeKind.Unspecified);

                using (var trans = App.Realm.BeginWrite())
                {
                    Movement.Value = Convert.ToDouble(Value);
                    Movement.Description = Description;
                    Movement.Date = new DateTimeOffset(Date, TimeSpan.Zero);
                    Movement.IsTitheable = IsTitheable;
                    trans.Commit();
                }

                App.IncomesViewModel.UpdateCollection(true, false, true);
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada atualizada com sucesso.");
            }
        }

        private async void SaveExpense()
        {
            if (await FieldsVerification())
            {
                var expense = new Movement(App.EXPENSE, Convert.ToDouble(Value), Description, Date, IsTitheable, false);
                App.Realm.Write(() => { App.Realm.Add(expense); });
                App.ExpensesViewModel.UpdateCollection(true, true);
                App.IncomesViewModel.UpdateCollection();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Despesa registrada com sucesso.");
            }
        }

        private async void UpdateExpense()
        {
            if (await FieldsVerification())
            {
                Date = DateTime.SpecifyKind(Date, DateTimeKind.Unspecified);

                using (var trans = App.Realm.BeginWrite())
                {
                    Movement.Value = Convert.ToDouble(Value);
                    Movement.Description = Description;
                    Movement.Date = new DateTimeOffset(Date, TimeSpan.Zero);
                    Movement.IsTitheable = IsTitheable;
                    trans.Commit();
                }

                App.ExpensesViewModel.UpdateCollection(true, false, true);
                App.IncomesViewModel.UpdateCollection();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Despesa atualizada com sucesso.");
            }
        }

        private async Task<bool> FieldsVerification()
        {
            if (Convert.ToDouble(Value) <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Alerta", "Valor inválido!", "OK");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Alerta", "Descrição não pode ser vazia!", "OK");
                return false;
            }
            else
                return true;
        }

        private void ToggleIsTitheable()
        {
            if (IsTitheableEnabled)
                IsTitheable = !IsTitheable;
        }

        //private void ToggleIsDeductable()
        //{
        //    if (IsDeductableEnabled)
        //        IsDeductable = !IsDeductable;
        //}
    }
}