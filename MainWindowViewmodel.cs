using Microsoft.Win32;
using Puch.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Puch.JPK
{
    public class MainWindowViewmodel : Common.ViewmodelBase
    {

        private const string DEFAULT_PODMIOT_FILE = "Podmiot.xml";
        private const string APP_DATA_PATH = "JPK";

        public MainWindowViewmodel()
        {
            _createCommands();
            IsModified = false;
            string podmiotFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_DATA_PATH, DEFAULT_PODMIOT_FILE);
            if (File.Exists(podmiotFileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(JPKPodmiot1));
                using (StreamReader reader = new StreamReader(podmiotFileName))
                    _defaultPodmiot = (JPKPodmiot1)serializer.Deserialize(reader);
            }
            else _defaultPodmiot = new JPKPodmiot1();
        }

        public void WindowClosing(object sender, CancelEventArgs e)
        {
            bool canClose = (Content?.IsModified != true || _queryUnsavedVM()) &&  (!IsModified || _queryUnsavedFile());
            e.Cancel = !canClose;
        }

        #region Commands
        public ICommand CommandNewJPK { get; private set; }
        public ICommand CommandOpenJPK { get; private set; }
        public ICommand CommandSaveJPK { get; private set; }
        public ICommand CommandSaveJPKAs { get; private set; }
        public ICommand CommandCloseApp { get; private set; }
        public ICommand CommandShowPodmiot { get; private set; }
        public ICommand CommandShowNaglowek { get; private set; }
        public ICommand CommandDefaultPodmiotEdit { get; private set; }
        public ICommand CommandSprzedaz { get; private set; }
        public ICommand CommandZakup { get; private set; }
        public ICommand CommandImportExcel { get; private set; }

        private void _createCommands()
        {
            CommandNewJPK = new UICommand { ExecuteDelegate = _newJPK };
            CommandOpenJPK = new UICommand { ExecuteDelegate = _openJPK };
            CommandSaveJPK = new UICommand {
                ExecuteDelegate = _saveJPK,
                //CanExecuteDelegate = o => IsModified && _jpk != null
            };
            CommandSaveJPKAs = new UICommand { ExecuteDelegate = _saveJPKAs, CanExecuteDelegate = o => _jpk != null };
            CommandCloseApp = new UICommand { ExecuteDelegate = _closeApp };
            // page show commands
            CommandDefaultPodmiotEdit = new UICommand { ExecuteDelegate = _defaultPodmiotEdit, CanExecuteDelegate = o => _content == null };
            CommandShowPodmiot = new UICommand { ExecuteDelegate = _showPodmiot, CanExecuteDelegate = _canShowPage };
            CommandShowNaglowek = new UICommand { ExecuteDelegate = _showNaglowek, CanExecuteDelegate = _canShowPage };
            CommandSprzedaz = new UICommand { ExecuteDelegate = _showSprzedaz, CanExecuteDelegate = _canShowPage };
            CommandZakup = new UICommand { ExecuteDelegate = _showZakup, CanExecuteDelegate = _canShowPage };
            CommandImportExcel = new UICommand { ExecuteDelegate = _importExcel, CanExecuteDelegate = _canShowPage };
        }


        private bool _canShowPage(object obj)
        {
            return Content == null && _jpk != null;
        }

        private void _closeApp(object obj)
        {
                System.Windows.Application.Current.MainWindow.Close();
        }

        private void _openJPK(object obj)
        {
            if (!IsModified || _queryUnsavedFile())
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Pliki JPK XML|*.xml|Wszystkie pliki|*.*";
                if (openDialog.ShowDialog() == true)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(JPK));
                    using (StreamReader reader = new StreamReader(openDialog.FileName))
                        _jpk = (JPK)serializer.Deserialize(reader);
                    IsModified = false;
                    _jpkTitle = openDialog.FileName;
                    NotifyPropertyChanged(nameof(JPKTitle));
                    NotifyPropertyChanged(nameof(ContainsJPK));
                    NotifyPropertyChanged(nameof(DisplaySummary));
                    NotifyPropertyChanged(nameof(Jpk));
                }
            }
        }

        private void _saveJPK(object obj)
        {
            if (string.IsNullOrWhiteSpace(_jpkTitle))
                _saveJPKAs(obj);
            else
                saveJPKToFile(_jpkTitle);
        }
        
        private void _saveJPKAs(object obj)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Pliki JPK XML|*.xml|Wszystkie pliki|*.*";
            if (saveDialog.ShowDialog() == true)
                saveJPKToFile(saveDialog.FileName);
        }

        private void saveJPKToFile(string fileName)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            XmlSerializer serializer = new XmlSerializer(typeof(JPK));
            using (StreamWriter writer = new StreamWriter(fileName))
                serializer.Serialize(writer, _jpk);
            _jpkTitle = fileName;
            NotifyPropertyChanged(nameof(JPKTitle));
            IsModified = false;
        }

        private void _newJPK(object obj)
        {
            if (!IsModified || _queryUnsavedFile())
            {
                _jpk = JPK.New();
                _jpk.Podmiot1 = _defaultPodmiot.Clone();
                _jpkTitle = null;
                IsModified = false;
                NotifyPropertyChanged(nameof(JPKTitle));
                NotifyPropertyChanged(nameof(ContainsJPK));
                NotifyPropertyChanged(nameof(DisplaySummary));
                NotifyPropertyChanged(nameof(Jpk));
            }
        }

        private bool _queryUnsavedFile()
        {
            var result = MessageBox.Show("Dane zostały zmienione. Zapisać je teraz?", "Ostrzeżenie", System.Windows.MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case System.Windows.MessageBoxResult.Yes:
                    _saveJPK(null);
                    return !IsModified;
                case System.Windows.MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }

        private bool _queryUnsavedVM()
        {
            var result = MessageBox.Show("Dane widoczne w oknie zostały zmienione. Zapisać je teraz?", "Ostrzeżenie", System.Windows.MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case System.Windows.MessageBoxResult.Yes:
                    Content.SaveToModel();
                    _saveJPK(null);
                    return !IsModified;
                case System.Windows.MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }
        
        #endregion // Commands

        #region DefaultPodmiot
        private void _defaultPodmiotEdit(object obj)
        {
            if (!IsModified || _queryUnsavedFile())
            {
                var defaultPodmiot = new PodmiotViewmodel(_defaultPodmiot);
                defaultPodmiot.OnOk += _defaultPodmiot_OnOk; ;
                defaultPodmiot.OnCancel += _defaultPodmiot_OnCancel; ;
                Content = defaultPodmiot;
            }
        }

        private void _defaultPodmiot_OnCancel(object sender, EventArgs e)
        {
            var podmiot = sender as PodmiotViewmodel;
            if (podmiot != null)
            {
                podmiot.OnOk -= _defaultPodmiot_OnOk;
                podmiot.OnCancel -= _defaultPodmiot_OnOk;
                Content = null;
            }
        }

        private void _defaultPodmiot_OnOk(object sender, EventArgs e)
        {
            var podmiot = sender as PodmiotViewmodel;
            if (podmiot != null)
            {
                podmiot.OnOk -= _defaultPodmiot_OnOk;
                podmiot.OnCancel -= _defaultPodmiot_OnCancel;
                string podmiotFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_DATA_PATH, DEFAULT_PODMIOT_FILE);
                if (!Directory.Exists(Path.GetDirectoryName(podmiotFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(podmiotFileName));
                podmiot.SaveModelToFile(podmiotFileName);
                Content = null;
            }
        }
        #endregion //DefaultPodmiot
                
        #region Naglowek
        private void _showNaglowek(object obj)
        {
            var naglowek = new NaglowekViewmodel(_jpk.Naglowek);
            naglowek.OnOk += _naglowek_OnOk;
            naglowek.OnCancel += _naglowek_OnCancel;
            Content = naglowek;
        }

        private void _naglowek_OnCancel(object sender, EventArgs e)
        {
            var naglowek = sender as NaglowekViewmodel;
            if (naglowek != null)
            {
                naglowek.OnOk -= _naglowek_OnOk;
                naglowek.OnCancel -= _naglowek_OnCancel;
                Content = null;
            }
        }

        private void _naglowek_OnOk(object sender, EventArgs e)
        {
            var naglowek = sender as NaglowekViewmodel;
            if (naglowek != null)
            {
                naglowek.OnOk -= _naglowek_OnOk;
                naglowek.OnCancel -= _naglowek_OnCancel;
                IsModified = true;
                Content = null;
            }
        }
        #endregion // Nagłowek

        #region Podmiot
        private void _showPodmiot(object obj)
        {
            var podmiot = new PodmiotViewmodel(_jpk.Podmiot1);
            podmiot.OnOk += _podmiot_OnOk;
            podmiot.OnCancel += _podmiot_OnCancel;
            Content = podmiot;
        }

        private void _podmiot_OnCancel(object sender, EventArgs e)
        {
            var podmiot = sender as PodmiotViewmodel;
            if (podmiot != null)
            {
                podmiot.OnOk -= _podmiot_OnOk;
                podmiot.OnCancel -= _podmiot_OnCancel;
                Content = null;
            }
        }

        private void _podmiot_OnOk(object sender, EventArgs e)
        {
            var podmiot = sender as PodmiotViewmodel;
            if (podmiot != null)
            {
                podmiot.OnOk -= _podmiot_OnOk;
                podmiot.OnCancel -= _podmiot_OnCancel;
                IsModified = true;
                Content = null;
            }
        }
        #endregion // Podmiot

        #region Zakup
        private void _showZakup(object obj)
        {
            var vm = new ZakupViewmodel(_jpk);
            vm.OnOk += _zakup_OnOk;
            vm.OnCancel += _zakup_OnCancel;
            Content = vm;
        }

        private void _zakup_OnCancel(object sender, EventArgs e)
        {
            var zakup = sender as ZakupViewmodel;
            if (zakup != null)
            {
                zakup.OnOk -= _zakup_OnOk;
                zakup.OnCancel -= _zakup_OnCancel;
                Content = null;
            }
        }

        private void _zakup_OnOk(object sender, EventArgs e)
        {
            _zakup_OnCancel(sender, e);
            NotifyPropertyChanged(nameof(Jpk));
            IsModified = true;
        }

        #endregion // Zakup

        #region Sprzedaz
        private void _showSprzedaz(object obj)
        {
            var vm = new SprzedazViewmodel(_jpk);
            vm.OnOk += _sprzedaz_OnOk;
            vm.OnCancel += _sprzedaz_OnCancel;
            Content = vm;
        }

        private void _sprzedaz_OnCancel(object sender, EventArgs e)
        {
            var sprzedazVM = sender as SprzedazViewmodel;
            if (sprzedazVM != null)
            {
                sprzedazVM.OnCancel -= _sprzedaz_OnCancel;
                sprzedazVM.OnOk -= _sprzedaz_OnOk;
                Content = null;
            }
        }

        private void _sprzedaz_OnOk(object sender, EventArgs e)
        {
            _sprzedaz_OnCancel(sender, e);
            NotifyPropertyChanged(nameof(Jpk));
            IsModified = true;
        }
        #endregion //Sprzedaz

        #region ImportExcel
        private void _importExcel(object obj)
        {
            var newVm = new ImportExcelViewmodel(_jpk.Podmiot1);
            newVm.OnOk += _importExcelOnOk;
            newVm.OnCancel += _importExcelOnCancel;
            Content = newVm;
        }

        private void _importExcelOnCancel(object sender, EventArgs e)
        {
            ImportExcelViewmodel vm = sender as ImportExcelViewmodel;
            if (vm != null)
            {
                vm.OnCancel -= _importExcelOnCancel;
                vm.OnOk -= _importExcelOnOk;
            }
            Content = null;
        }

        private void _importExcelOnOk(object sender, EventArgs e)
        {
            ImportExcelViewmodel vm = sender as ImportExcelViewmodel;
            if (vm != null)
            {
                foreach (var m in vm.ImportRows)
                {
                    if (m is JPKZakupWiersz)
                        _jpk.ZakupWiersz.Add((JPKZakupWiersz)m);
                    if (m is JPKSprzedazWiersz)
                        _jpk.SprzedazWiersz.Add((JPKSprzedazWiersz)m);
                }
                if (vm.ImportType == TImportType.Sprzedaz)
                    _jpk.UpdateSprzedazCtl();
                if (vm.ImportType == TImportType.Zakup)
                    _jpk.UpdateZakupCtrl();
                NotifyPropertyChanged(nameof(Jpk));
                IsModified = true;
            }
            _importExcelOnCancel(sender, e);
        }
        #endregion //ImportExcel


        private ViewmodelBase _content;
        public ViewmodelBase Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    NotifyPropertyChanged(nameof(Content));
                    NotifyPropertyChanged(nameof(DisplaySummary));
                }
            }
        }

        private JPK _jpk;

        public bool ContainsJPK { get { return _jpk != null; } }

        public bool DisplaySummary { get { return _jpk != null && _content == null; } }

        private string _jpkTitle;
        public string JPKTitle
        {
            get
            {
                return _jpk == null ? "Brak pliku" :
                    (string.IsNullOrWhiteSpace(_jpkTitle) ? "Nowy" : Path.GetFileName(_jpkTitle));
            }
        }

        public JPK Jpk { get { return _jpk; } }

        private JPKPodmiot1 _defaultPodmiot;
        
        protected override void OnDispose()
        {
            
        }
    }
}
