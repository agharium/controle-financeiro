using Android.App;
using Android.Widget;
using Financial.Droid.Services;
using Financial.Services;

[assembly: Xamarin.Forms.Dependency(typeof(ToastMessage_Android))]
namespace Financial.Droid.Services
{
    public class ToastMessage_Android : IToastMessage
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}