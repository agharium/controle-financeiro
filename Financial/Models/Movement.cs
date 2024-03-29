﻿using Realms;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Financial.Models
{
    public class Movement : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }
        /// 0 = Income
        /// 1 = Expense
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
        [Ignored]
        public string Description_Display
        {
            get => 
                Description.Length > 20
                    ? Description.Substring(0, Math.Min(Description.Length, 20)) + "..."
                    : Description;
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
            get => 
                IsTitheable && App.UserGivesTithes
                    ? Type == App.T_INCOME
                        ? "(10%) " + (Value * 0.1).ToString("C", CultureInfo.CurrentCulture)
                        : Handed
                            ? "Deduzido"
                            : "Deduzível"
                    : "";
        }
        [Ignored]
        public string Tithes_Display_Color
        {
            get => 
                Handed
                    ? ((Color)Application.Current.Resources["IncomesColor"]).ToHex()
                    : ((Color)Application.Current.Resources["PrimaryColor"]).ToHex();
        }
        [Ignored]
        public TextDecorations Tithes_Display_Decoration
        {
            get => 
                Handed
                    ? TextDecorations.Strikethrough
                    : TextDecorations.None;
        }

        public Movement() {}

        public Movement(int _type, double _value, string _description, DateTime _date, bool _isTitheable = false, bool _isHanded = false)
        {
            _date = DateTime.SpecifyKind(_date, DateTimeKind.Unspecified);

            Id = App.MOVEMENT_ID++;
            Type = _type;
            Value = _value;
            Description = _description;
            Date = new DateTimeOffset(_date, TimeSpan.Zero);
            IsTitheable = _isTitheable;
            Handed = _isHanded;
        }
    }
}