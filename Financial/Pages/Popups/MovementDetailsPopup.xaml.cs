using Financial.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
/*using System.Windows.Input;
using Xamarin.Forms;*/
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
        public string Description { get; set; }
        public string Date { get; set; }
        public string Value { get; set; }
        public string Tithes { get; set; }
        public string HandedStatus { get; set; }

        public bool TithesIsVisible { get; set; }

        /*private ICommand OpenHandleMovementPopupEditCommand { get; set; }
        private ICommand DeleteMovementCommand { get; set; }*/

        public MovementDetailsPopupViewModel(Movement Movement)
        {
            Description = Movement.Description;
            Value = Movement.Value_Display;
            Date = Movement.Date_Display;

            TithesIsVisible = App.UserGivesTithes && Movement.Type == App.INCOME && Movement.IsTitheable;
            if (TithesIsVisible)
            {
                Tithes = Movement.Tithes_Display.Remove(0, 6);
                HandedStatus = Movement.Handed ? "Entregue" : "A entregar";
            }

            /*OpenHandleMovementPopupEditCommand = new Command(OpenHandleMovementPopupEdit);
            DeleteMovementCommand = new Command(DeleteMovement);*/
        }


        /*private async void OpenHandleMovementPopupEdit()
        {

        }

        private async void DeleteMovement()
        {

        }*/
    }
}