using Financial.Models;
using Realms.Exceptions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
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

        public HandleMovementPopup(int type, int operation, Movement movement = null)
        {
            InitializeComponent();

            BindingContext = new HandleMovementPopupViewModel(this, type, operation, movement);
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }

    class HandleMovementPopupViewModel : ViewModelBase
    {
        private HandleMovementPopup Parent { get; set; }
        private int Type { get; set; } // 0 = income, 1 = expense
        private int Operation { get; set; } // 0 = save, 1 = update
        private Movement Movement { get; set; }

        public string Value { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsTitheable { get; set; }

        public string Title { get; set; }
        public DateTime TodayDate { get; set; }
        public bool IsTitheableVisible { get; set; }

        public ICommand SaveMovementCommand { get; set; }

        public HandleMovementPopupViewModel(HandleMovementPopup parent, int type, int operation, Movement movement)
        {
            Parent = parent;
            Type = type;
            Operation = operation;
            Movement = movement;

            Date = TodayDate = DateTime.Now;
            IsTitheableVisible = App.UserGivesTithes && (Type == App.INCOME);

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
            } else if (Type == App.EXPENSE && Operation == App.OP_SAVE)
            {
                SaveMovementCommand = new Command(SaveExpense);
                Title = "Novo gasto";
                Description = "";
            } else if (Type == App.EXPENSE && Operation == App.OP_UPDATE)
            {
                SaveMovementCommand = new Command(UpdateExpense);
                Title = "Editar gasto";
                Value = Convert.ToString(Movement.Value);
                Description = Movement.Description;
            }
        }

        private async void SaveIncome()
        {
            if (string.IsNullOrWhiteSpace(Value))
                await Application.Current.MainPage.DisplayAlert("Alerta", "Descrição não pode ser vazia!", "OK");
            else if (Convert.ToDouble(Value) <= 0)
                await Application.Current.MainPage.DisplayAlert("Alerta", "Valor inválido!", "OK");
            else
            {
                var income = new Movement(App.INCOME, Convert.ToDouble(Value), Description, Date, IsTitheable);
                try
                {
                    App.Realm.Write(() => { App.Realm.Add(income); });
                } catch (RealmDuplicatePrimaryKeyValueException ex)
                {
                    income.Id++;
                    App.Realm.Write(() => { App.Realm.Add(income); });
                } 
                Parent.InvokeCallback();
                await PopupNavigation.Instance.PopAsync();
                App.Toast("Entrada adicionada com sucesso.");
            }
        }

        private async void UpdateIncome()
        {
            if (string.IsNullOrWhiteSpace(Value))
                await Application.Current.MainPage.DisplayAlert("Alerta", "Descrição não pode ser vazia!", "OK");
            else if (Convert.ToDouble(Value) <= 0)
                await Application.Current.MainPage.DisplayAlert("Alerta", "Valor inválido!", "OK");
            else
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

        }

        private async void UpdateExpense()
        {

        }
    }
}