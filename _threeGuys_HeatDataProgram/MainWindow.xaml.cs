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
using Setting;
using System.Data;
using System.Linq;

using ThreadRunner;
using PySocketHandler;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;

namespace _threeGuys_HeatDataProgram
{
    public partial class MainWindow : Window
    {
        PySocketHandler.PySocketHandler psh = new PySocketHandler.PySocketHandler();
        ThreadRunner.ThreadRunner thr = new ThreadRunner.ThreadRunner();
        // FactoryDataCSV 파일 열어서 저장할 리스트 선언
        List<FactoryDataReader.DataColumn> test_list = default;
        // DataGrid에 넣기위한 csv list
        List<Setting.SettingDataColumn> set_list = default;
        // 1초마다 작업을 하기위한 Timer 이용하기 위해 선언
        private DispatcherTimer timer = new DispatcherTimer();
        private int currentRow = 0;
        public MainWindow()
        {
            InitializeComponent();
            psh.prepareSocket();

            string filePath = "heatTreatingFactoryData.csv";
            string setfilePath = "HeatDataAlarmFilter.csv";

            Setting.SetData setData = new Setting.SetData();
            setData.LoadDataFromCSV(dataGrid_Settings, setfilePath);

            FactoryDataReader.FactoryDataReader test = new FactoryDataReader.FactoryDataReader();

            // CSV 파일 열어서 DataGrid에 저장
            dataGrid_History.ItemsSource = test.heatTreatingFactoryDataRead(filePath);
            // CSV 파일 열어서 리스트에 저장
            test_list = test.heatTreatingFactoryDataRead(filePath);

            InitializeWebView();
            PerformRadiobuttonTab1PowerChecked();
        }

        private void PerformRadiobuttonTab1PowerChecked()
        {
            // 라디오 버튼 체크용 - 추후 
            radiobutton_tab1_power_Checked(null, null);
            // 1초마다 TimerTick 메서드 호출
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            // 1초마다 수행할 작업을 여기에 구현
            float value_Watt1 = test_list[currentRow].GN02N_MAIN_POWER; 
            float value_Watt2 = test_list[currentRow].GN04N_MAIN_POWER; 
            float value_Watt3 = test_list[currentRow].GN05N_MAIN_POWER; 
            float value_Watt4 = test_list[currentRow].GN07N_MAIN_POWER; 

            float value_Temp1 = test_list[currentRow].GN02N_TEMP;
            float value_Temp2 = test_list[currentRow].GN04M_TEMP;
            float value_Temp3 = test_list[currentRow].GN05M_TEMP;
            float value_Temp4 = test_list[currentRow].GN07N_TEMP;

            // 가져온 값에 대한 작업 수행
            label_Watt1.Content = value_Watt1.ToString() + " (W)";
            label_Watt2.Content = value_Watt2.ToString() + " (W)";
            label_Watt3.Content = value_Watt3.ToString() + " (W)";
            label_Watt4.Content = value_Watt4.ToString() + " (W)";

            label_Temp1.Content = value_Temp1.ToString() + "( °C)";
            label_Temp2.Content = value_Temp2.ToString() +"( °C)";
            label_Temp3.Content = value_Temp3.ToString() +"( °C)";
            label_Temp4.Content = value_Temp4.ToString() +"( °C)";

            // if( 필터체크 통과 못했으면 ) 아래 함수 실행
            ShowAlertOnDangerousLevels(value_Watt1, value_Watt2, value_Watt3, value_Watt4, value_Temp1, value_Temp2, value_Temp3, value_Temp4, currentRow);

            // 다음 행으로 이동
            currentRow++;
            if (currentRow >= test_list.Count)
            {
                currentRow = 0;  // 리스트의 끝에 도달하면 처음으로 돌아감
            }
        }
        //port : 56790

    
        // 인터넷 실행
        private async void InitializeWebView()
        {
            // Ensure CoreWebView2 is initialized
            await webView2_tab1.EnsureCoreWebView2Async(null);
            await webView2_tab2.EnsureCoreWebView2Async(null);
            await webView2_tab3.EnsureCoreWebView2Async(null);
            await webView2_tab4.EnsureCoreWebView2Async(null);

            webView2_tab4.CoreWebView2.Settings.IsScriptEnabled = true;
            webView2_tab4.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

            // Load a webpage
            webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
            webView2_tab2.Source = new Uri("http://127.0.0.1:8050/");
            webView2_tab3.Source = new Uri("http://127.0.0.1:8050/");

            // 이동할 구역의 아이디
            // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
            string sectionId = "Graph_temp_1";
            // 해당 섹션ID를 기준삼아 화면 이동
            webView2_tab4.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");
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

        // 필터에 걸렸을때 
        private void ShowAlertOnDangerousLevels(float valueWatt1, float valueWatt2, float valueWatt3, float valueWatt4, float valueTemp1, float valueTemp2, float valueTemp3, float valueTemp4, int currentRow)
        {
            // csv 파일 열어서
            string[] lines = File.ReadAllLines("HeatDataAlarmFilter.csv");
            
            List<string[]> test = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)  // 첫째 줄(스키마) 건너 뜀
            {
                string line = lines[i];
                string[] data = line.Split(',');
                test.Add(data);
                if (data.Length != 5)
                {
                    throw new Exception("File Error");
                }
            }

            for (int i = 0; i < test.Count; i++)
            {
                string filterName = test[i][0];
                string filterOption = test[i][1];
                string filterMax = test[i][2];
                string filterMin = test[i][3];
                string filterString = test[i][4];

                switch (filterName)
                {
                    case "GN02N_MAIN_POWER":
                        Check_Filter_CSV(1, filterName, "전력", valueWatt1, filterMax, filterMin);
                        break;
                    case "GN04N_MAIN_POWER":
                        Check_Filter_CSV(2, filterName, "전력", valueWatt2, filterMax, filterMin);
                        break;
                    case "GN05N_MAIN_POWER":
                        Check_Filter_CSV(3, filterName, "전력", valueWatt3, filterMax, filterMin);
                        break;
                    case "GN07N_MAIN_POWER":
                        Check_Filter_CSV(4, filterName, "전력", valueWatt4, filterMax, filterMin);
                        break;
                    case "GN02N_TEMP":
                        Check_Filter_CSV(1, filterName, "온도", valueTemp1, filterMax, filterMin);
                        break;
                    case "GN04M_TEMP":
                        Check_Filter_CSV(2, filterName, "온도", valueTemp2, filterMax, filterMin);
                        break;
                    case "GN05M_TEMP":
                        Check_Filter_CSV(3, filterName, "온도", valueTemp3, filterMax, filterMin);
                        break;
                    case "GN07N_TEMP":
                        Check_Filter_CSV(4, filterName, "온도", valueTemp4, filterMax, filterMin);
                        break;
                }
            }
        }

        /* 구역 번호, 구역명, 옵션(전력,온도,etc...), 현재 값, 최대 값, 최소 값*/
        private void Check_Filter_CSV(int filterAreaNum, string filterName, string option,float inputValue, string filterMax, string filterMin)
        {
            if (int.Parse(filterMax) <= inputValue)
                listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMax} 값 보다 높습니다.");

            if (int.Parse(filterMin) >= inputValue)
                listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMin} 값 보다 낮습니다.");
        }
        
        // 리스트 더블 클릭하면 알림창 -> datagrid 
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
        
        // 선택한 라디오 버튼으로 이동시키는 함수
        private async Task MoveWebViewToCoordinates(int x, string y)
        {
            await webView2_tab1.EnsureCoreWebView2Async();

            string script = $"window.scrollTo({x}, {y});";
            await webView2_tab1.ExecuteScriptAsync(script);

            await HideScrollBars();
        }

        // 마우스 스크롤을 숨기는 함수 
        private async Task HideScrollBars()
        {
            await webView2_tab1.EnsureCoreWebView2Async(null);

            string script = "document.documentElement.style.overflow = 'hidden';";
            await webView2_tab1.ExecuteScriptAsync(script);
        }

        // 1번 탭 각 라디오 버튼
        private async void radiobutton_tab1_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                // 페이지 로딩 될 때까지 대기
                await webView2_tab1.EnsureCoreWebView2Async(null);

                // 이동할 구역의 아이디
                // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
                string sectionId = "live-update-graph-1";
                // 해당 섹션ID를 기준삼아 화면 이동
                webView2_tab1.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");
                webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
                await HideScrollBars();
            }
        }

        private async void radiobutton_tab1_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                // 페이지 로딩 될 때까지 대기
                await webView2_tab1.EnsureCoreWebView2Async(null);

                // 이동할 구역의 아이디
                // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
                string sectionId = "live-update-graph-2";
                // 해당 섹션ID를 기준삼아 화면 이동
                webView2_tab1.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");
                webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
                await HideScrollBars();
            }
        }

        private async void radiobutton_tab1_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1 != null)
            {
                // 페이지 로딩 될 때까지 대기
                await webView2_tab1.EnsureCoreWebView2Async(null);

                // 이동할 구역의 아이디
                // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
                string sectionId = "live-update-graph-3";
                // 해당 섹션ID를 기준삼아 화면 이동
                webView2_tab1.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");
                webView2_tab1.Source = new Uri("http://127.0.0.1:8050/");
                await HideScrollBars();
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
                await webView2_tab4.EnsureCoreWebView2Async(null);

                // 이동할 구역의 아이디
                // 파이썬 부에서 해당 구역이 설정 되어 있어야함.
                string sectionId = "live-update-graph-2";
                // 해당 섹션ID를 기준삼아 화면 이동
                webView2_tab4.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{sectionId}').scrollIntoView();");

                // 스크롤 막기 위한 JavaScript 코드
                webView2_tab4.CoreWebView2.ExecuteScriptAsync("document.addEventListener('scroll', function(e) { e.preventDefault(); }, { passive: false });");

                // 웹 페이지 로드
                webView2_tab4.Source = new Uri("http://127.0.0.1:8050/");



            }
        }

        private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ((Microsoft.Web.WebView2.Wpf.WebView2)sender).ExecuteScriptAsync("document.querySelector('body').style.overflow='hidden'");
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

        private void button_set_add_Click(object sender, RoutedEventArgs e)
        {
            Setting.SetData setData = new Setting.SetData();

            set_list = setData.settingData(TextBox_set_error_name.Text, TextBox_set_column_name.Text, float.Parse(TextBox_set_value_above.Text), float.Parse(TextBox_set_value_below.Text), TextBox_set_etc.Text);
            dataGrid_Settings.Items.Add(set_list);

            //setData.SaveDataToCSV(dataGrid_Settings, filePath);

        }

        private void button_set_delete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_Settings.SelectedItem != null)
            {
                dataGrid_Settings.Items.Remove(dataGrid_Settings.SelectedItem);
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