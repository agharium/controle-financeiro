using Xamarin.Forms;

namespace Financial.Handlers
{
    class IncomesSearchHandler : SearchHandler
    {
        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            ItemsSource = null;
            App.IncomesViewModel.Filter(newValue);
        }
    }
}
