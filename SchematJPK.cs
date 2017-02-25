using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Puch.Common;
using System.ComponentModel;
using PropertyChanged;

namespace Puch.JPK
{
    [ImplementPropertyChanged]
    [XmlRoot(Namespace = "http://jpk.mf.gov.pl/wzor/2016/10/26/10261/")]
    public partial class JPK
    {
        internal const string etd = "http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2016/01/25/eD/DefinicjeTypy/";

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns
        {
            get
            {
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add("etd", etd);
                return xmlns;
            }
            set { }
        }

        public static JPK New()
        {
            JPK jpk = new JPK();
            jpk.Naglowek.WariantFormularza = 2;
            jpk.Naglowek.CelZlozenia = 1; // 1 - nowa deklaracja, 2 - korekta
            DateTime now = DateTime.UtcNow;
            jpk.Naglowek.DataWytworzeniaJPK = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
            jpk.Naglowek.DataDo = DateTimeUtils.LastDayOfPreviousMonth;
            jpk.Naglowek.DataOd = DateTimeUtils.FirstDayOfPreviousMonth;
            jpk.Naglowek.DomyslnyKodWaluty = currCode_Type.PLN;
            return jpk;
        }

        public void UpdateZakupCtrl()
        {
            ZakupCtrl.LiczbaWierszyZakupow = (uint)ZakupWiersz.Count;
            ZakupCtrl.PodatekNaliczony = ZakupWiersz.Sum(w => w.K_44 + w.K_46 + w.K_47 + w.K_48 + w.K_49 + w.K_50);
            uint lp = 1;
            ZakupWiersz.ForEach(w => w.LpZakupu = lp++);
        }

        public void UpdateSprzedazCtl()
        {
            SprzedazCtrl.LiczbaWierszySprzedazy = (uint)SprzedazWiersz.Count;
            //K_16, K_18, K_20, K_24, K_26, K_28, K_30, K_33, K_35, K_36 i K_37 pomniejszona o kwotę z elementów K_38 i K_39 <
            SprzedazCtrl.PodatekNalezny = SprzedazWiersz.Sum(w => w.K_16 + w.K_18 + w.K_20 + w.K_24 + w.K_26 + w.K_28 + w.K_30 + w.K_33 + w.K_35 + w.K_36 + w.K_37 - w.K_38 - w.K_39);
            uint lp = 1;
            SprzedazWiersz.ForEach(w => w.LpSprzedazy = lp++);
        }

    }
    [XmlRoot(Namespace = JPK.etd)]
    public partial class TIdentyfikatorOsobyNiefizycznej
    {
    }

    [ImplementPropertyChanged]
    public partial class JPKPodmiot1
    {
        public JPKPodmiot1 Clone()
        {
            return new JPKPodmiot1()
            {
                IdentyfikatorPodmiotu = new TIdentyfikatorOsobyNiefizycznej
                {
                    NIP = this.IdentyfikatorPodmiotu.NIP,
                    REGON = this.IdentyfikatorPodmiotu.REGON,
                    PelnaNazwa = this.IdentyfikatorPodmiotu.PelnaNazwa
                },
                AdresPodmiotu = new TAdresJPK
                {
                    Gmina = this.AdresPodmiotu.Gmina,
                    KodKraju = this.AdresPodmiotu.KodKraju,
                    KodPocztowy = this.AdresPodmiotu.KodPocztowy,
                    Miejscowosc = this.AdresPodmiotu.Miejscowosc,
                    NrDomu = this.AdresPodmiotu.NrDomu,
                    NrLokalu = this.AdresPodmiotu.NrLokalu,
                    Poczta = this.AdresPodmiotu.Poczta,
                    Powiat = this.AdresPodmiotu.Powiat,
                    Ulica = this.AdresPodmiotu.Ulica,
                    Wojewodztwo = this.AdresPodmiotu.Wojewodztwo
                }
            };
        }
    }

    [ImplementPropertyChanged]
    public partial class JPKZakupWiersz
    {
        public JPKZakupWiersz Clone()
        {
            var dest = new JPKZakupWiersz();
            foreach (PropertyInfo property in typeof(JPKZakupWiersz).GetProperties())
                property.SetValue(dest, property.GetValue(this, null), null);
            return dest;
        }
    }

    [ImplementPropertyChanged]
    public partial class JPKSprzedazWiersz
    {
        public JPKSprzedazWiersz Clone()
        {
            var dest = new JPKSprzedazWiersz();
            foreach (PropertyInfo property in typeof(JPKSprzedazWiersz).GetProperties())
                property.SetValue(dest, property.GetValue(this, null), null);
            return dest;
        }
    }

     public partial class TAdresJPK
    {
        public override string ToString()
        {
            return $"{KodPocztowy} {Miejscowosc}, {Ulica} {NrDomu}";
        }
    }


}
