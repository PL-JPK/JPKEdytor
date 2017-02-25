using Puch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Puch.JPK
{
    public class ZakupWierszViewmodel: ViewmodelBase
    {
        public ZakupWierszViewmodel(JPKZakupWiersz zakup): base(zakup)
        {

        }

        public decimal KwotaNetto
        {
            get
            {
                return
                      GetProperty<decimal>("K_43")
                    + GetProperty<decimal>("K_45");
            }
        }
        public decimal KwotaVat
        {
            get
            {
                return
                      GetProperty<decimal>("K_44")
                    + GetProperty<decimal>("K_46")
                    + GetProperty<decimal>("K_47")
                    + GetProperty<decimal>("K_48")
                    + GetProperty<decimal>("K_49")
                    + GetProperty<decimal>("K_50");
            }
        }

        public JPKZakupWiersz Zakup { get { return Model as JPKZakupWiersz; } }

        public bool DataWplywuSpecified
        {
            get { return GetProperty<DateTime>(nameof(JPKZakupWiersz.DataWplywu)) != DateTime.MinValue; }
            set
            {
                DateTime now = DateTime.Now;
                if (value)
                    SetProperty(nameof(JPKZakupWiersz.DataWplywu), DateTimeUtils.LastDayOfPreviousMonth);
                else
                    SetProperty(nameof(JPKZakupWiersz.DataWplywu), DateTime.MinValue);
            }
        }

        protected override void NotifyPropertyChanged(string propertyName)
        {
            base.NotifyPropertyChanged(propertyName);
            if (propertyName.StartsWith("K_"))
            {
                NotifyPropertyChanged(nameof(KwotaNetto));
                NotifyPropertyChanged(nameof(KwotaVat));
            }
        }
    }
}
