using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace Shared.Models
{
    public class ObservableObject : INotifyPropertyChanged // This is the base class for handling instant data refresh
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //This event handler watches for changes to the values of a property inside a class.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
