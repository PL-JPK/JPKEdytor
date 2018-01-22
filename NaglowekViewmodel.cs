using System;
using Puch.Common;
using System.Windows.Input;
using System.Collections.Generic;

namespace Puch.JPK
{
    public class NaglowekViewmodel: ViewmodelBase
    {
        public NaglowekViewmodel(TNaglowek naglowek) : base(naglowek)
        {
            CommandOK = new UICommand { ExecuteDelegate = _ok, CanExecuteDelegate = _canOK };
            CommandCancel = new UICommand { ExecuteDelegate = _cancel };
        }

        private void _cancel(object obj)
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
        }

        private bool _canOK(object obj)
        {
            return IsModified;
        }

        private void _ok(object obj)
        {
            SaveToModel();
            OnOk?.Invoke(this, EventArgs.Empty);
        }
        
        public ICommand CommandOK { get; private set; }
        public ICommand CommandCancel { get; private set; }

        public event EventHandler OnOk;
        public event EventHandler OnCancel;


        protected override bool SetField<T>(ref T field, T value, string propertyName)
        {
            InvalidateRequerySuggested();
            return base.SetField<T>(ref field, value, propertyName);
        }

    }
}