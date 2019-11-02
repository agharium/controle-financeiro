using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Financial.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();

            var now = DateTimeOffset.Now;
            Debug.WriteLine(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(11));
            Debug.WriteLine(now.ToString("MMMM", CultureInfo.CurrentCulture));
        }
    }
}