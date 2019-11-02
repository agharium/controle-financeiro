using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Financial.Droid.Renderers;

[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarRenderer_Android))]
namespace Financial.Droid.Renderers
{
    #pragma warning disable CS0618 // O tipo ou membro é obsoleto
    public class SearchBarRenderer_Android : SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var color = Color.White;
                var searchView = Control as SearchView;

                // search icon
                var searchIconId = searchView.Resources.GetIdentifier("android:id/search_mag_icon", null, null);
                if (searchIconId > 0)
                {
                    var searchPlateIcon = searchView.FindViewById(searchIconId);
                    (searchPlateIcon as ImageView).SetColorFilter(color.ToAndroid(), Android.Graphics.PorterDuff.Mode.SrcIn);
                }

                // bottom line
                var searchPlateId = searchView.Context.Resources.GetIdentifier("android:id/search_plate", null, null);
                if (searchPlateId > 0)
                {
                    Android.Views.View searchPlateView = searchView.FindViewById(searchPlateId);
                    searchPlateView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                }

                // x button
                var closeIconId = searchView.Resources.GetIdentifier("android:id/search_close_btn", null, null);
                if (closeIconId > 0)
                {
                    var closeIcon = searchView.FindViewById(closeIconId);
                    (closeIcon as ImageView).SetColorFilter(color.ToAndroid(), Android.Graphics.PorterDuff.Mode.SrcIn);
                }
            }
        }
    }
    #pragma warning restore CS0618 // O tipo ou membro é obsoleto
}