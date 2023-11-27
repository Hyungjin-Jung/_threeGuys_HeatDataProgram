using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _threeGuys_HeatDataProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }


        // 1번 탭 활성화
        private void _DisplayFirstAreaTab(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }

        private void _DisplaySecondAreaTab(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
        }

        private void _DisplayThirdAreaTab(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
        }

        private void _DisplayFourthAreaTab(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 4;
        }
    }
}