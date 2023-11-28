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

            InitializeWebView();

        }

        private async void InitializeWebView()
        {
            // Ensure CoreWebView2 is initialized
            await webView2_tab1.EnsureCoreWebView2Async(null);
            await webView2_tab2.EnsureCoreWebView2Async(null);
            await webView2_tab3.EnsureCoreWebView2Async(null);
            await webView2_tab4.EnsureCoreWebView2Async(null);

            // Load a webpage
            webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
            webView2_tab2.Source = new Uri("http://127.0.0.1:8050/");
            webView2_tab3.Source = new Uri("http://127.0.0.1:8050/");
            webView2_tab4.Source = new Uri("http://127.0.0.1:8050/");
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
            //if(listBox_Notice.SelectedItem != null)
            //{
            //    string selectedTime = listBox_Notice.SelectedItem.ToString();

            //    // DataGridView에서 해당 시간에 대한 행으로 이동
            //    foreach (DataGrid row in test_list.)
            //    {
            //        if (row.Cells["TimeColumn"].Value.ToString() == selectedTime)
            //        {
            //            row.Selected = true;
            //            dataGridView.FirstDisplayedScrollingRowIndex = row.Index;
            //            break;
            //        }
            //    }
            //}

        }


        // 1번 탭 각 라디오 버튼
        private async void radiobutton_tab1_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                await webView2_tab1.EnsureCoreWebView2Async(null);

                webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab1_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                await webView2_tab1.EnsureCoreWebView2Async(null);

                webView2_tab1.Source = new Uri("https://www.naver.com");
            }
        }

        private async void radiobutton_tab1_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                await webView2_tab1.EnsureCoreWebView2Async(null);

                webView2_tab1.Source = new Uri("http://127.0.0.1:8050");
            }
        }

        // 2번 탭 각 버튼 
        private async void radiobutton_tab2_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2 != null)
            {
                await webView2_tab2.EnsureCoreWebView2Async(null);

                webView2_tab2.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab2_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2 != null)
            {
                await webView2_tab2.EnsureCoreWebView2Async(null);

                webView2_tab2.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab2_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2 != null)
            {
                await webView2_tab2.EnsureCoreWebView2Async(null);

                webView2_tab2.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        // 3번 탭 각 버튼
        private async void radiobutton_tab3_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3 != null)
            {
                await webView2_tab3.EnsureCoreWebView2Async(null);

                webView2_tab3.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab3_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3 != null)
            {
                await webView2_tab3.EnsureCoreWebView2Async(null);

                webView2_tab3.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab3_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3 != null)
            {
                await webView2_tab3.EnsureCoreWebView2Async(null);

                webView2_tab3.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        // 4번 탭 각 버튼
        private async void radiobutton_tab4_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4 != null)
            {
                // 페이지 로딩 될 때까지 대기
                await webView2_tab4.EnsureCoreWebView2Async(null);

                // 이동할 구역의 아이디
                // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
                string sectionId = "Graph_temp_1";
                // 해당 섹션ID를 기준삼아 화면 이동
                webView2_tab4.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");
                webView2_tab4.Source = new Uri("http://127.0.0.1:8050/");

            }
        }
    

        private async void radiobutton_tab4_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4 != null)
            {
                await webView2_tab4.EnsureCoreWebView2Async(null);

                webView2_tab4.Source = new Uri("http://127.0.0.1:8050/");
            }
        }

        private async void radiobutton_tab4_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4 != null)
            {
                await webView2_tab4.EnsureCoreWebView2Async(null);

                webView2_tab4.Source = new Uri("http://127.0.0.1:8050/");
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