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

using ThreadRunner;
using PySocketHandler;

namespace _threeGuys_HeatDataProgram
{
    public partial class MainWindow : Window
    {
        PySocketHandler.PySocketHandler psh = new PySocketHandler.PySocketHandler();
        ThreadRunner.ThreadRunner thr = new ThreadRunner.ThreadRunner();

        public MainWindow()
        {
            InitializeComponent();
            psh.prepareSocket();
        }

        /*
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