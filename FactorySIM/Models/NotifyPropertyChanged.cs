using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FactorySIM.Models
{
    /// <summary>
    /// Base class that implements INotifyPropertyChanged for WPF data binding.
    /// Any class that inherits from this will automatically update the UI when properties change.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event. The [CallerMemberName] attribute 
        /// automatically provides the property name, so you just call OnPropertyChanged().
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper method that sets a field and raises PropertyChanged if the value actually changed.
        /// Usage: SetField(ref _myField, value);
        /// </summary>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}