using Financial.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovementDetailsPopup : PopupPage
    {
        public MovementDetailsPopup(Movement Income)
        {
            InitializeComponent();

            BindingContext = new MovementDetailsPopupViewModel(Income);
        }

        private void OnCloseButtonTapped(object sender, EventArgs e) => PopupNavigation.Instance.PopAsync();
    }

    class MovementDetailsPopupViewModel : ViewModelBase
    {
        private Movement Movement;

        public string Description { get; set; }
        public string Date { get; set; }
        public string Value { get; set; }
        public string Tithes { get; set; }
        public string HandedStatus { get; set; }

        public bool TithesIsVisible { get; set; }

        public ICommand OpenHandleMovementPopupEditCommand { get; set; }
        public ICommand DeleteMovementCommand { get; set; }

        public MovementDetailsPopupViewModel(Movement _movement)
        {
            Movement = _movement;

            Description = Movement.Description;
            Value = Movement.Value_Display;
            Date = Movement.Date_Display;

            TithesIsVisible = App.UserGivesTithes && Movement.Type == App.T_INCOME && Movement.IsTitheable;
            if (TithesIsVisible)
            {
                Tithes = Movement.Tithes_Display.Remove(0, 6);
                HandedStatus = Movement.Handed ? "Entregue" : "A entregar";
            }

            OpenHandleMovementPopupEditCommand = new Command(OpenHandleMovementPopupEdit);
            DeleteMovementCommand = new Command(DeleteMovement);
        }


        private async void OpenHandleMovementPopupEdit()
        {
            await PopupNavigation.Instance.PopAsync();
            await PopupNavigation.Instance.PushAsync(new HandleMovementPopup(Movement.Type, App.OP_UPDATE, Movement));
        }

        private async void DeleteMovement()
        {
            var strType = Movement.Type == App.T_INCOME ? "entrada" : "despesa";
            var strExtra = Movement.Type == App.T_INCOME && Movement.Handed ? " Esta entrada também representa um dízimo já entregue e uma despesa foi registrada referente à sua entrega." : "";
            if (await Shell.Current.DisplayAlert("Confirmação", $"Tem certeza que deseja excluir esta {strType}?{strExtra}", "Sim", "Não"))
            {
                using (var trans = App.Realm.BeginWrite())
                {
                    var MovementType = Movement.Type;

                    App.Realm.Remove(Movement);
                    trans.Commit();

                    if (MovementType == App.T_INCOME)
                        App.IncomesViewModel.UpdateCollection(true, false, true);
                    else
                        App.ExpensesViewModel.UpdateCollection(true, false, true);
                    
                    await PopupNavigation.Instance.PopAsync();
                    App.Toast(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(strType) + " excluída com sucesso.");
                }
            }
        }
    }
}