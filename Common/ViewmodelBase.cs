//#undef DEBUG
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Dynamic;
using System.Xml.Serialization;
using System.IO;

namespace Puch.Common
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public class ViewmodelBase : DynamicObject, INotifyPropertyChanged, IDisposable
    {
        readonly protected object Model;
        readonly protected Type ModelType;
        #region Constructors / Destructor
        public ViewmodelBase()
        {
            _isModified = false;
        }
        public ViewmodelBase(object model)
        {
            _properties = new Dictionary<string, object>();
            Model = model;
            ModelType = model.GetType();
            foreach (var property in ModelType.GetProperties())
                if (property.CanRead)
                {
                    object value = property.GetValue(model, null);
                    Type propertyType = property.PropertyType;
                    if (propertyType.IsClass && !(propertyType == typeof(String)))
                    {
                        var childVM = new ViewmodelBase(value);
                        _properties[property.Name] = childVM;
                        childVM.Modified += _childVM_Modified;
                    }
                    else
                        _properties[property.Name] = value;
                }
            _isModified = false;
        }

        private void _childVM_Modified(object sender, EventArgs e)
        {
            IsModified = true;
        }

#if DEBUG
        /// <summary>
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~ViewmodelBase()
        {
            Debug.WriteLine(string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this, this.GetHashCode()));
        }
#endif

        #endregion // Constructors / Destructor

        #region Dynamic
        private readonly Dictionary<string, object> _properties;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object oldValue;
            Type propertyType = ModelType.GetProperty(binder.Name).PropertyType;
            object newValue = (value == null) ? Activator.CreateInstance(propertyType) : Convert.ChangeType(value, propertyType);
            _properties.TryGetValue(binder.Name, out oldValue);
            if (oldValue == newValue)
                return false;
            _properties[binder.Name] = newValue;
            IsModified = true;
            NotifyPropertyChanged(binder.Name);
            return true;
        }

        protected T GetProperty<T>(string propertyName)
        {
            object value;
            if (_properties.TryGetValue(propertyName, out value))
                return (T)value;
            else
                return default(T);                
        }

        protected void SetProperty<T>(string propertyName, T value)
        {
            _properties[propertyName] = value;
            NotifyPropertyChanged(propertyName);
        }


        #endregion // Dynamic


        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null
                && !_properties.ContainsKey(propertyName))
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
                this.VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        private bool _disposed = false; 
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                this.OnDispose();
            }
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
            foreach (var property in _properties.Values)
            {
                var vm = property as ViewmodelBase;
                if (vm != null)
                {
                    vm.Modified -= _childVM_Modified;
                    vm.Dispose();
                }
            }
        }

        #endregion // IDisposable Members

        bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                if (_isModified != value)
                    _isModified = value;
                if (value)
                    Modified?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged(nameof(IsModified));
            }
        }

        public event EventHandler Modified;

        protected virtual bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            IsModified = true;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected virtual void InvalidateRequerySuggested()
        {
            Application.Current?.Dispatcher.BeginInvoke((Action)(() => CommandManager.InvalidateRequerySuggested()));
        }

        public virtual void SaveToModel()
        {
            foreach(var property in _properties)
            {
                var propertyInfo = ModelType.GetProperty(property.Key);
                if (propertyInfo.CanWrite)
                {
                    object propertyValue = null;
                    ViewmodelBase propertyVM = property.Value as ViewmodelBase;
                    if (propertyVM != null)
                    {
                        propertyVM.SaveToModel();
                        propertyValue = propertyVM.Model;
                    }
                    else
                        propertyValue = property.Value;
                    propertyInfo.SetValue(Model, propertyValue, null);
                }
            }
        }

        public void SaveModelToFile(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(Model.GetType());
            using (StreamWriter writer = new StreamWriter(fileName))
                serializer.Serialize(writer, Model);
        }

    }
}