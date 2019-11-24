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
        public event EventHandler<object> CallbackEvent;
        public void InvokeCallback() => CallbackEvent?.Invoke(this, EventArgs.Empty);

        HandleMovementPopupViewModel ViewModel;

        public HandleMovementPopup(int type, int operation, Movement movement = null)
        {
            InitializeComponent();

            BindingContext = ViewModel = new HandleMovementPopupViewModel(this, type, operation, movement);
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.Description = sender.Text;

                if (string.IsNullOrWhiteSpace(sender.Text))
                    sender.ItemsSource = null;
                else
                {
                    var movements = App.Realm.All<Movement>().Where(m => m.Type == ViewModel.Type).ToList();
                    sender.ItemsSource = movements.Where(m => m.Description.ToLower().StartsWith(sender.Text.ToLower())).Select(m => m.Description).Distinct().ToList();
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) => ViewModel.Description = (string)args.SelectedItem;
    }

    class HandleMovementPopupViewModel : ViewModelBase
    {
        private HandleMovementPopup Parent { get; set; }
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
        public DateTime TodayDate { get; set; }
        public bool IsTitheableVisible { get; set; }
        public bool IsTitheableEnabled { get; set; }
        public string IsTitheableColor { get; set; }
        public ICommand ToggleIsTitheableCommand { get; set; }

        public ICommand SaveMovementCommand { get; set; }

        public HandleMovementPopupViewModel(HandleMovementPopup parent, int type, int operation, Movement movement)
        {
            Parent = parent;
            Type = type;
            Operation = operation;
            Movement = movement;

            Date = TodayDate = DateTime.Now;
            IsTitheableVisible = IsTitheableEnabled = App.UserGivesTithes && (Type == App.INCOME);
            IsTitheableColor = ((Color)Application.Current.Resources["PrimaryColor"]).ToHex();
            ToggleIsTitheableCommand = new Command(ToggleIsTitheable);

            if (Type == App.INCOME && Operation == App.OP_SAVE)
            {
                SaveMovementCommand = new Command(SaveIncome);
                Title = "Nova entrada";
                Description = "";
                IsTitheable = IsTitheableVisible;
            } else if (Type == App.INCOME && Operation == App.OP_UPDATE)
            {
                SaveMovementCommand = new Command(UpdateIncome);
                Title = "Editar entrada";
                Value = Convert.ToString(Movement.Value);
                Description = Movement.Description;
                Date = Movement.Date.DateTime;
                IsTitheable = Movement.IsTitheable;
                IsTitheableEnabled = !Movement.Handed;
                if (!IsTitheableEnabled)
                    IsTitheableColor = "Gray";
            } else if (Type == App.EXPENSE && Operation == App.OP_SAVE)
            {
                SaveMovementCommand = new Command(SaveExpense);
                Title = "Nova despesa";
                Description = "";
            } else if (Type == App.EXPENSE && Operation == App.OP_UPDATE)
            {
                SaveMovementCommand = new Command(UpdateExpense);
                Title = "Editar despesa";
                Value = Convert.ToString(Movement.Value);
                Description = Movement.Description;
            }
        }

        private async void SaveIncome()
        {
            if (await FieldsVerification())
            {
                var income = new Movement(App.INCOME, Convert.ToDouble(Value), Description, Date, IsTitheable);
                App.Realm.Write(() => { App.Realm.Add(income); });
                Parent.InvokeCallback();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada adicionada com sucesso.");
            }
        }

        private async void UpdateIncome()
        {
            if (await FieldsVerification())
            {
                using (var trans = App.Realm.BeginWrite())
                {
                    Movement.Value = Convert.ToDouble(Value);
                    Movement.Description = Description;
                    Movement.Date = Date;
                    Movement.IsTitheable = IsTitheable;
                    trans.Commit();
                }
                Parent.InvokeCallback();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada atualizada com sucesso.");
            }
        }

        private async void SaveExpense()
        {
            if (await FieldsVerification())
            {
                var expense = new Movement(App.EXPENSE, Convert.ToDouble(Value), Description, Date, false);
                App.Realm.Write(() => { App.Realm.Add(expense); });
                Parent.InvokeCallback();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Despesa adicionada com sucesso.");
            }
        }

        private async void UpdateExpense()
        {
            if (await FieldsVerification())
            {
                using (var trans = App.Realm.BeginWrite())
                {
                    Movement.Value = Convert.ToDouble(Value);
                    Movement.Description = Description;
                    Movement.Date = Date;
                    trans.Commit();
                }
                Parent.InvokeCallback();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada atualizada com sucesso.");
            }
        }

        private async Task<bool> FieldsVerification()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Alerta", "Descrição não pode ser vazia!", "OK");
                return false;
            }
            else if (Convert.ToDouble(Value) <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Alerta", "Valor inválido!", "OK");
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
    }
}