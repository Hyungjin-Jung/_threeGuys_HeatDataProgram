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

            // 초기 웹사이트 로드 함수 호출
            LoadInitialWebsite();
        }

        // 실행하자 마자 사이트 불러옴. 
        private void LoadInitialWebsite()
        {
            if (radiobutton_tab1_power.IsChecked == true)
            {
                webBrowser_tab1.Navigate(new Uri("https://www.naver.com"));
            }
            if (radiobutton_tab2_power.IsChecked == true)
            {
                webBrowser_tab2.Navigate(new Uri("https://www.naver.com"));
            }
            if (radiobutton_tab3_power.IsChecked == true)
            {
                webBrowser_tab3.Navigate(new Uri("https://www.naver.com"));
            }
            if (radiobutton_tab4_power.IsChecked == true)
            {
                webBrowser_tab4.Navigate(new Uri("https://www.naver.com"));
            }
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

        // 1번 탭 각 라디오 버튼
        private void radiobutton_tab1_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab1 != null)
            {
                webBrowser_tab1.Navigate(new Uri("https://www.naver.com"));
            }
        }

        private void radiobutton_tab1_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab1 != null)
            {
                webBrowser_tab1.Navigate(new Uri("https://www.daum.net"));
            }
        }

        private void radiobutton_tab1_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab1 != null)
            {
                webBrowser_tab1.Navigate(new Uri("https://www.youtube.com"));
            }
        }

        // 2번 탭 각 버튼 
        private void radiobutton_tab2_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab2 != null)
            {
                webBrowser_tab2.Navigate(new Uri("https://www.naver.com"));
            }
        }

        private void radiobutton_tab2_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab2 != null)
            {
                webBrowser_tab2.Navigate(new Uri("https://www.daum.net"));
            }
        }

        private void radiobutton_tab2_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab2 != null)
            {
                webBrowser_tab2.Navigate(new Uri("https://www.youtube.com"));
            }
        }

        // 3번 탭 각 버튼
        private void radiobutton_tab3_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab3 != null)
            {
                webBrowser_tab3.Navigate(new Uri("https://www.naver.com"));
            }
        }

        private void radiobutton_tab3_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab3 != null)
            {
                webBrowser_tab3.Navigate(new Uri("https://www.daum.net"));
            }
        }

        private void radiobutton_tab3_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab3 != null)
            {
                webBrowser_tab3.Navigate(new Uri("https://www.youtube.com"));
            }
        }

        // 4번 탭 각 버튼
        private void radiobutton_tab4_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab4 != null)
            {
                webBrowser_tab3.Navigate(new Uri("https://www.naver.com"));
            }
        }

        private void radiobutton_tab4_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab4 != null)
            {
                webBrowser_tab4.Navigate(new Uri("https://www.daum.net"));
            }
        }

        private void radiobutton_tab4_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webBrowser_tab4 != null)
            {
                webBrowser_tab4.Navigate(new Uri("https://www.youtube.com"));
            }
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