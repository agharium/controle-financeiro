using Realms;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Financial.Models
{
    public class Movement : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        /// 1 = Income
        /// 0 = Expense
        public int Type { get; set; }
        public double Value { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool IsTitheable { get; set; }
        public bool Handed { get; set; }

        [Ignored]
        public string Value_Display
        {
            get => Value.ToString("C", CultureInfo.CurrentCulture);
        }
        /*[Ignored]
        public string Description_ToString
        {
            get => string.IsNullOrEmpty(Description) ? "" : Description;
        }*/
        [Ignored]
        public string Description_Display
        {
            get
            {
                if (Description.Length > 20)
                    return Description.Substring(0, Math.Min(Description.Length, 20)) + "...";
                else
                    return Description;
            }
        }
        [Ignored]
        public string Date_Display
        {
            get => Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }
        [Ignored]
        public string Date_Display_Filter
        {
            get => Date.ToString("MMMM/yyyy", CultureInfo.CurrentCulture);
        }
        [Ignored]
        public string Tithes_Display
        {
            get => IsTitheable && App.UserGivesTithes ? "(10%) " + (Value * 0.1).ToString("C", CultureInfo.CurrentCulture) : "";
        }
        [Ignored]
        public string Tithes_Display_Color
        {
            get => Handed ? ((Color)Application.Current.Resources["IncomesColor"]).ToHex() : ((Color)Application.Current.Resources["PrimaryColor"]).ToHex();
        }
        [Ignored]
        public TextDecorations Tithes_Display_Decoration
        {
            get => Handed ? TextDecorations.Strikethrough : TextDecorations.None;
        }

        public Movement() { }

        public Movement(int _type, double _value, string _description, DateTime _date, bool _isTitheable)
        {
            Id = App.MOVEMENT_ID++;
            Type = _type;
            Value = _value;
            Description = _description;
            IsTitheable = _isTitheable;
            Date = _date;
            Handed = false;
        }
    }
}