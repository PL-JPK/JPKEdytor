using Puch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Puch.JPK
{
    public class SprzedazWierszViewmodel: ViewmodelBase
    {
        private readonly JPKPodmiot1 _daneWlasne;
        public SprzedazWierszViewmodel(JPKSprzedazWiersz wiersz, JPKPodmiot1 daneWlasne): base(wiersz)
        {
            _daneWlasne = daneWlasne;
            CommandWstawDaneWlasne = new UICommand { ExecuteDelegate = _wstawDaneWlasne };
        }

        private void _wstawDaneWlasne(object obj)
        {
            SetProperty(nameof(JPKSprzedazWiersz.NrKontrahenta), _daneWlasne.NIP);
            SetProperty(nameof(JPKSprzedazWiersz.NazwaKontrahenta), _daneWlasne.PelnaNazwa);
        }

        public ICommand CommandWstawDaneWlasne { get; private set; }

        public JPKSprzedazWiersz Sprzedaz { get { return Model as JPKSprzedazWiersz; } }

        public bool DataSprzedazySpecified
        {
            get { return GetProperty<DateTime>(nameof(JPKSprzedazWiersz.DataSprzedazy)) != DateTime.MinValue; }
            set
            {
                DateTime now = DateTime.Now;
                if (value)
                    SetProperty(nameof(JPKSprzedazWiersz.DataSprzedazy), DateTimeUtils.LastDayOfPreviousMonth);
                else
                    SetProperty(nameof(JPKSprzedazWiersz.DataSprzedazy), DateTime.MinValue);
            }
        }
    }
}
