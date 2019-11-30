using Xamarin.Forms;

namespace Financial.Handlers
{
    class ExpensesSearchHandler : SearchHandler
    {
        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            ItemsSource = null;
            App.ExpensesViewModel.Filter(newValue);
        }
    }
}
