using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _threeGuys_HeatDataProgram.PageModel
{
    class PageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // private로 선언
        private int num = 1;
        private int num2 = 1;
        private string string1 = "what";

        public int Num
        {
            get
            {
                return num;
            }
            set
            {
                num = value;
                Num2 = value * 2;
                OnPropertyChanged("Num");
            }

        }

        public int Num2
        {
            get
            {
                return num2;
            }
            set
            {
                num2 = value;
                OnPropertyChanged("Num2");
            }
        }

        public string _string1
        {
            get
            {
                return string1;
            }
            set
            {
                string1 = value;
                OnPropertyChanged("_string1");
            }
        }



        // 프로퍼티가 바뀌면 작동하는 함수
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
