using Puch.Common;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Reflection;
using System.Windows;
using System.Collections.ObjectModel;

namespace Puch.JPK
{
    public enum TImportType
    {
        Zakup,
        Sprzedaz
    }

    public class ImportExcelViewmodel : ViewmodelBase
    {
        private readonly ObservableCollection<object> _importRows = new ObservableCollection<object>();
        private readonly JPKPodmiot1 _daneWlasne;
        public ImportExcelViewmodel(JPKPodmiot1 daneWlasne)
        {
            _daneWlasne = daneWlasne;
            CommandReadExcel = new UICommand { ExecuteDelegate = _readExcel };
            CommandOK = new UICommand { ExecuteDelegate = _ok, CanExecuteDelegate = _canOK };
            CommandCancel = new UICommand { ExecuteDelegate = _cancel };
            CommandEditWiersz = new UICommand { ExecuteDelegate = _editWiersz, CanExecuteDelegate = _canEditWiersz };
        }

        private void _editWiersz(object obj)
        {
            ViewmodelBase vm = _selectedRow is JPKSprzedazWiersz ? new SprzedazWierszViewmodel((JPKSprzedazWiersz)_selectedRow, _daneWlasne) as ViewmodelBase :
                _selectedRow is JPKZakupWiersz ? new ZakupWierszViewmodel((JPKZakupWiersz)_selectedRow) : null;
            if (vm != null)
                ItemEditViewmodel.ShowModal(vm, "Zmiana danych");
        }

        private bool _canEditWiersz(object obj)
        {
            return _selectedRow != null;
        }

        private void _cancel(object obj)
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
        }

        private void _ok(object obj)
        {
            OnOk?.Invoke(this, EventArgs.Empty);
        }

        private bool _canOK(object obj)
        {
            return _importRows.Any();
        }

        public ICommand CommandReadExcel { get; private set; }
        public ICommand CommandOK { get; private set; }
        public ICommand CommandCancel { get; private set; }
        public ICommand CommandEditWiersz { get; private set; }

        private void _readExcel(object obj)
        {
            Excel.Application app = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            Excel.Workbook book = app?.ActiveWorkbook;
            if (_readWorkbook(book))
                IsModified = true;
            else
                MessageBox.Show("Nie znaleziono danych do zaimportowania", "Błąd");
        }

        private TImportType _importType;
        public TImportType ImportType { get { return _importType; } set { SetField(ref _importType, value, nameof(ImportType)); } }

        public IEnumerable<object> ImportRows { get { return _importRows; } }

        private object _selectedRow;
        public object SelectedRow { get { return _selectedRow; } set { SetField(ref _selectedRow, value, nameof(SelectedRow)); } }

        private bool _readWorkbook(Excel.Workbook book)
        {
            Excel.Worksheet sheet = book?.ActiveSheet;
            if (sheet == null)
                return false;
            Excel.Range range = sheet.UsedRange;
            Type importType = ImportType == TImportType.Sprzedaz ? typeof(JPKSprzedazWiersz) : typeof(JPKZakupWiersz);
            var properties = importType.GetProperties();
            _importRows.Clear();
            SelectedRow = null;
            for (int row = 2; row <= range.Rows.Count; row++)
            {
                if (string.IsNullOrWhiteSpace(range.Cells[row, 1].Text))
                    break;
                object wiersz = importType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                for (int column = 1; column <= range.Columns.Count; column++)
                    try
                    {
                        _importCell(range, wiersz, importType, row, column);
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"Błąd czytania komórki {range.Cells[row, column].Address}. Błąd:\n{e.Message}", e);
                    }
                if (ImportType == TImportType.Sprzedaz)
                {
                    if (((JPKSprzedazWiersz)wiersz).DataWystawienia == DateTime.MinValue)
                        ((JPKSprzedazWiersz)wiersz).DataWystawienia = DateTimeUtils.LastDayOfPreviousMonth;
                }
                else
                {
                    if (((JPKZakupWiersz)wiersz).DataZakupu == DateTime.MinValue)
                        ((JPKZakupWiersz)wiersz).DataZakupu = DateTimeUtils.LastDayOfPreviousMonth;
                }
                _importRows.Add(wiersz);
            }
            return true;
        }

        private void _importCell(Excel.Range range, object row, Type rowType, int rowIndex, int columnIndex)
        {
            string propertyName = string.Empty;
            object propertyValue = null;
            string strVal = range.Cells[rowIndex, columnIndex].Text as string;
            string columnTitle = (range.Cells[1, columnIndex].Value as string)?.ToLower();

            switch (columnTitle)
            {
                case "lp":
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.LpSprzedazy) : nameof(JPKZakupWiersz.LpZakupu);
                    uint lp;
                    if (uint.TryParse(strVal, out lp))
                        propertyValue = lp;
                    break;
                case "nip":
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.NrKontrahenta) : nameof(JPKZakupWiersz.NrDostawcy);
                    propertyValue = string.IsNullOrWhiteSpace(strVal) ? string.Empty : new string(strVal.Where(c => Char.IsDigit(c)).ToArray());
                    break;
                case "numer":
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.DowodSprzedazy) : nameof(JPKZakupWiersz.DowodZakupu);
                    propertyValue = strVal;
                    break;
                case "data otrzym.":
                case "data otrzymania":
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.DataSprzedazy) : nameof(JPKZakupWiersz.DataWplywu);
                    DateTime dataO;
                    if (DateTime.TryParse(strVal, out dataO))
                        propertyValue = dataO;
                    break;
                case "data wyst.":
                case "data wystawienia":
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.DataWystawienia) : nameof(JPKZakupWiersz.DataZakupu);
                    DateTime dataW;
                    if (DateTime.TryParse(strVal, out dataW))
                        propertyValue = dataW;
                    break;
                case "vat":
                    if (_importType == TImportType.Zakup)
                    {
                        propertyName = nameof(JPKZakupWiersz.K_46);
                        propertyValue = Convert.ToDecimal(range.Cells[rowIndex, columnIndex].Value2);
                    }
                    break;
            }
            if (propertyValue != null && !string.IsNullOrWhiteSpace(propertyName))
                rowType.GetProperty(propertyName)?.SetValue(row, propertyValue, null);

            if (columnTitle?.StartsWith("nazwa i adres") == true)
            {
                string[] nazwaIAdres = strVal.Split(new char[] { ',' }, 2);
                if (nazwaIAdres.Length > 0)
                {
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.NazwaKontrahenta) : nameof(JPKZakupWiersz.NazwaDostawcy);
                    rowType.GetProperty(propertyName)?.SetValue(row, nazwaIAdres[0].Trim(), null);
                }
                if (nazwaIAdres.Length == 2)
                {
                    propertyName = ImportType == TImportType.Sprzedaz ? nameof(JPKSprzedazWiersz.AdresKontrahenta) : nameof(JPKZakupWiersz.AdresDostawcy);
                    rowType.GetProperty(propertyName)?.SetValue(row, nazwaIAdres[1].Trim(), null);
                }
            }
            if (columnTitle?.StartsWith("kwota netto") == true)
            {
                if (ImportType == TImportType.Zakup)
                    ((JPKZakupWiersz)row).K_45 = Convert.ToDecimal(range.Cells[rowIndex, columnIndex].Value2) + ((JPKZakupWiersz)row).K_45;
                else
                {
                    int stawka;
                    decimal kwota = Convert.ToDecimal(range.Cells[rowIndex, columnIndex].Value2);
                    if (kwota != 0)
                    {
                        if (int.TryParse(new string(columnTitle.Where(c => Char.IsDigit(c)).ToArray()), out stawka))
                        {
                            switch (stawka)
                            {
                                case 23:
                                case 22:
                                    ((JPKSprzedazWiersz)row).K_19 = kwota;
                                    break;
                                case 7:
                                case 8:
                                    ((JPKSprzedazWiersz)row).K_17 = kwota;
                                    break;
                                case 5:
                                    ((JPKSprzedazWiersz)row).K_15 = kwota;
                                    break;
                                case 0:
                                    ((JPKSprzedazWiersz)row).K_13 = kwota;
                                    break;
                            }
                        }
                        else ((JPKSprzedazWiersz)row).K_10 = kwota;
                    }
                }
            }
            if (ImportType == TImportType.Sprzedaz && columnTitle?.StartsWith("vat") == true)
            {
                int stawka;
                decimal kwota = Convert.ToDecimal(range.Cells[rowIndex, columnIndex].Value2);
                if (kwota != 0)
                {
                    if (int.TryParse(new string(columnTitle.Where(c => Char.IsDigit(c)).ToArray()), out stawka))
                    {
                        switch (stawka)
                        {
                            case 23:
                            case 22:
                                ((JPKSprzedazWiersz)row).K_20 = kwota;
                                break;
                            case 7:
                            case 8:
                                ((JPKSprzedazWiersz)row).K_18 = kwota;
                                break;
                            case 5:
                                ((JPKSprzedazWiersz)row).K_16 = kwota;
                                break;
                        }
                    }
                }
            }
        }


        public event EventHandler OnOk;
        public event EventHandler OnCancel;
    }

}
