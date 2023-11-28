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
using System.IO;

using FactoryDataReader;
using System.Data;
using System.Linq;

namespace _threeGuys_HeatDataProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<FactoryDataReader.DataColumn> test_list = default;
        public MainWindow()
        {
            InitializeComponent();



            string filePath = "heatTreatingFactoryData.csv";

            FactoryDataReader.FactoryDataReader test = new FactoryDataReader.FactoryDataReader();
            
            dataGrid_History.ItemsSource = test.heatTreatingFactoryDataRead(filePath);
            test_list = test.heatTreatingFactoryDataRead(filePath);
        }


        // 1번 탭 활성화
        private void DisplayFirstAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }

        private void DisplaySecondAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
        }

        private void DisplayThirdAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
        }

        private void DisplayFourthAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 4;
        }

        private void button_test_Click(object sender, RoutedEventArgs e)
        {
            listBox_Notice.Items.Add(test_list[0].Time);
        }

        private void listBox_Notice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            
        }
    }
}