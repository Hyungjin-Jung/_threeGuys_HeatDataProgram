using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _threeGuys_HeatDataProgram.PageViewModel
{
    public class PageViewModel : INotifyPropertyChanged
    {
        private string _pythonAlert;

        public string PythonAlertText
        {
            get { return _pythonAlert; }
            set
            {
                if (_pythonAlert != value)
                {
                    _pythonAlert = value;
                    OnPropertyChanged(nameof(PythonAlertText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
