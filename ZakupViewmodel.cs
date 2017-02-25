using Puch.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Puch.JPK
{
    public class ZakupViewmodel: ViewmodelBase
    {
        readonly JPK _jpk;
        public ZakupViewmodel(JPK jpk): base()
        {
            _zakupy = new ObservableCollection<ZakupWierszViewmodel>(jpk.ZakupWiersz.Select(z => new ZakupWierszViewmodel(z.Clone())));
            _jpk = jpk;
            CommandOK = new UICommand { ExecuteDelegate = _ok, CanExecuteDelegate = _canOK };
            CommandCancel = new UICommand { ExecuteDelegate = _cancel };
            CommandAddWiersz = new UICommand { ExecuteDelegate = _addWiersz };
            CommandDeleteWiersz = new UICommand { ExecuteDelegate = _deleteWiersz, CanExecuteDelegate = _isWierszSelected };
            CommandEditWiersz = new UICommand { ExecuteDelegate = _editWiersz, CanExecuteDelegate = _isWierszSelected };
        }

        private bool _isWierszSelected(object obj)
        {
            return SelectedWiersz is ZakupWierszViewmodel;
        }

        private void _editWiersz(object obj)
        {
            if (ItemEditViewmodel.ShowModal(SelectedWiersz, "Zakup - edycja") == true)
                IsModified = true;
        }

        private void _deleteWiersz(object obj)
        {
            Zakupy.Remove(SelectedWiersz);
            IsModified = true;
        }

        private void _addWiersz(object obj)
        {
            var vm = new ZakupWierszViewmodel(new JPKZakupWiersz() { DataZakupu = DateTime.Today, DataWplywu = DateTime.Today });
            if ((ItemEditViewmodel.ShowModal(vm, "Zakup - dodawanie")) == true)
            {
                Zakupy.Add(vm);
                IsModified = true;
            }
        }

        private void _cancel(object obj)
        {
            if (!IsModified || MessageBox.Show("Zmienione dane nie zostały przeniesione do JPK.\nRezygnujesz?", "Ostrzeżenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                OnCancel?.Invoke(this, EventArgs.Empty);
        }

        private bool _canOK(object obj)
        {
            return IsModified || _zakupy.Any(z => z.IsModified);
        }

        private void _ok(object obj)
        {
            SaveToModel();
            OnOk?.Invoke(this, EventArgs.Empty);
        }

        public override void SaveToModel()
        {
            foreach (var zakup in _zakupy.Where(z => z.IsModified))
                zakup.SaveToModel();
            _jpk.ZakupWiersz = _zakupy.Select(vm => vm.Zakup).ToList();
            _jpk.UpdateZakupCtrl();
        }

        private readonly ObservableCollection<ZakupWierszViewmodel> _zakupy;
        private ZakupWierszViewmodel _selectedWiersz;

        public ObservableCollection<ZakupWierszViewmodel> Zakupy { get { return _zakupy; } }
        public ZakupWierszViewmodel SelectedWiersz
        {
            get { return _selectedWiersz; }
            set
            {
                if (_selectedWiersz != value)
                    _selectedWiersz = value;
                NotifyPropertyChanged(nameof(SelectedWiersz));
            }
        }
        
        public ICommand CommandOK { get; private set; }
        public ICommand CommandCancel { get; private set; }
        public ICommand CommandAddWiersz { get; private set; }
        public ICommand CommandDeleteWiersz { get; private set; }
        public ICommand CommandEditWiersz { get; private set; }

        public event EventHandler OnOk;
        public event EventHandler OnCancel;

    }
}
