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

using ThreadRunner;
using PySocketHandler;

namespace _threeGuys_HeatDataProgram
{
    public partial class MainWindow : Window
    {
        PySocketHandler.PySocketHandler psh = new PySocketHandler.PySocketHandler();
        ThreadRunner.ThreadRunner thr = new ThreadRunner.ThreadRunner();
        List<FactoryDataReader.DataColumn> test_list = default;
        public MainWindow()
        {
            InitializeComponent();
            psh.prepareSocket();

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

        /* 소켓 통신 임시 함수
        private async void button_tempSocket_Click(object sender, RoutedEventArgs e)
        {
            string sendString = textBox_tempSocketSend.Text;

            // 비동기로 데이터 보내기
            await Task.Run(() =>
            {
                psh.sendSocketString(sendString);
            });

            textBox_tempSocketSend.Text = "";

            // 비동기로 데이터 받기
            Task<string> receiveTask = Task.Run(() => psh.receivedSocketString());

            // UI 스레드 차단하지 않고 받은 데이터를 기다림
            while (!receiveTask.IsCompleted)
            {
                await Task.Delay(1); // 100ms 마다 체크
            }

            textBox_tempSocketReceived.Text = receiveTask.Result;
        }
        */
    }
}