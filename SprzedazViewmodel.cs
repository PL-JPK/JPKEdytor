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
    public class SprzedazViewmodel: ViewmodelBase
    {
        readonly JPK _jpk;
        public SprzedazViewmodel(JPK jpk): base()
        {
            _jpk = jpk;
            _sprzedaz = new ObservableCollection<SprzedazWierszViewmodel>(jpk.SprzedazWiersz.Select(z => new SprzedazWierszViewmodel(z.Clone(), jpk.Podmiot1)));
            CommandOK = new UICommand { ExecuteDelegate = _ok, CanExecuteDelegate = _canOK };
            CommandCancel = new UICommand { ExecuteDelegate = _cancel };
            CommandAddWiersz = new UICommand { ExecuteDelegate = _addWiersz };
            CommandDeleteWiersz = new UICommand { ExecuteDelegate = _deleteWiersz, CanExecuteDelegate = _isWierszSelected };
            CommandEditWiersz = new UICommand { ExecuteDelegate = _editWiersz, CanExecuteDelegate = _isWierszSelected };
        }

        private bool _isWierszSelected(object obj)
        {
            return SelectedWiersz is SprzedazWierszViewmodel;
        }

        private void _editWiersz(object obj)
        {
            if (ItemEditViewmodel.ShowModal(SelectedWiersz, "Sprzedaż - edycja") == true)
                IsModified = true;
        }

        private void _deleteWiersz(object obj)
        {
            Sprzedaz.Remove(SelectedWiersz);
            IsModified = true;
        }

        private void _addWiersz(object obj)
        {
            var vm = new SprzedazWierszViewmodel(new JPKSprzedazWiersz() { DataWystawienia = DateTimeUtils.LastDayOfPreviousMonth, DataSprzedazy = DateTime.MinValue }, _jpk.Podmiot1);
            if ((ItemEditViewmodel.ShowModal(vm, "Sprzedaż - dodawanie")) == true)
            {
                Sprzedaz.Add(vm);
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
            return IsModified || _sprzedaz.Any(z => z.IsModified);
        }

        private void _ok(object obj)
        {
            SaveToModel();
            OnOk?.Invoke(this, EventArgs.Empty);
        }

        public override void SaveToModel()
        {
            foreach (var sprzedaz in _sprzedaz.Where(z => z.IsModified))
                sprzedaz.SaveToModel();
            _jpk.SprzedazWiersz = _sprzedaz.Select(vm => vm.Sprzedaz).ToList();
            _jpk.UpdateSprzedazCtl();
        }

        private readonly ObservableCollection<SprzedazWierszViewmodel> _sprzedaz;
        private SprzedazWierszViewmodel _selectedWiersz;

        public ObservableCollection<SprzedazWierszViewmodel> Sprzedaz { get { return _sprzedaz; } }
        public SprzedazWierszViewmodel SelectedWiersz
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
