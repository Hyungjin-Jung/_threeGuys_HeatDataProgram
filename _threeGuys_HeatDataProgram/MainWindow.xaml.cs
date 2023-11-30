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

using PySocketHandler;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;
using static Setting.SettingDataColumn;

namespace _threeGuys_HeatDataProgram
{
    public partial class MainWindow : Window
    {
        PySocketHandler.PySocketHandler psh = new PySocketHandler.PySocketHandler();
        // FactoryDataCSV 파일 열어서 저장할 리스트 선언
        List<FactoryDataReader.DataColumn> test_list = default;
        // 설정 DataGrid에 넣기위한 csv list
        List<Setting.SettingDataColumn> set_list = default;
        // 알림창에 문제가 생긴 부분만 모아놓은 List
        List<string[]> alaram_to_Datagrid_list = new List<string[]>();
        // 1초마다 작업을 하기위한 Timer 이용하기 위해 선언
        private DispatcherTimer timer = new DispatcherTimer();
        private int currentRow = 0;
        public MainWindow()
        {
            InitializeComponent();
            
            string filePath = "heatTreatingFactoryData.csv";
            string setfilePath = "HeatDataAlarmFilter.csv";

            Setting.setFilterData setData= new Setting.setFilterData();

            setData.LoadDataFromCSV(dataGrid_Settings, setfilePath);
            FactoryDataReader.FactoryDataReader test = new FactoryDataReader.FactoryDataReader();
            // CSV 파일 열어서 DataGrid에 저장
            dataGrid_History.ItemsSource = test.heatTreatingFactoryDataRead(filePath);
            // CSV 파일 열어서 리스트에 저장
            test_list = test.heatTreatingFactoryDataRead(filePath);

            // 로딩 끝나면 탭들을 1~4 탭을 한번씩 활성화 시켜서 안의 그래프가 그려지게 만듬
            
            Loaded += ResetTab;
            InitializeAsync();

            // 1초마다 TimerTick 메서드 호출 -> 1초마다 CSV 알림 받아오는 용도로 설정
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += showTemperatureAndPower;
            timer.Start();
        }

        // 로딩용 눈속임. Loaded 핸들러(생성자의 로딩이 다 끝났을때)에 담아서 실행.
        private async void ResetTab(object sender,RoutedEventArgs e)
        {
            // 탭 5를 활성화. 확실하게 활성화 시키기 위해 딜레이
            for (int i = 5; i >= 0; i--)
            {
                tabControl.SelectedIndex = i;
                await Task.Delay(100);
            }
            // 다 끝나면 최전방의 사각형 제거
            rectangle_Rectangle.Visibility = Visibility.Collapsed;
        }
        // 1초마다 작업 실행 - 필터 및 알림 용도
        private void showTemperatureAndPower(object sender, EventArgs e)
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

            string timer = test_list[currentRow].Time;

            // 가져온 값에 대한 작업 수행
            label_Watt1.Content = value_Watt1.ToString() + " [W]";
            label_Watt2.Content = value_Watt2.ToString() + " [W]";
            label_Watt3.Content = value_Watt3.ToString() + " [W]";
            label_Watt4.Content = value_Watt4.ToString() + " [W]";

            label_Temp1.Content = value_Temp1.ToString() + " [°C]";
            label_Temp2.Content = value_Temp2.ToString() +" [°C]";
            label_Temp3.Content = value_Temp3.ToString() +" [°C]";
            label_Temp4.Content = value_Temp4.ToString() + " [°C]";

            // if( 필터체크 통과 못했으면 ) 아래 함수 실행
            showAlertOnDangerousLevels(timer, value_Watt1, value_Watt2, value_Watt3, value_Watt4, value_Temp1, value_Temp2, value_Temp3, value_Temp4);

            // 다음 행으로 이동
            currentRow++;
            if (currentRow >= test_list.Count)
            {
                currentRow = 0;  // 리스트의 끝에 도달하면 처음으로 돌아감
            }
        }
        //port : 56790

    
        // 인터넷 실행 초기화
        private async void InitializeAsync()
        {
            await webView2_tab4.EnsureCoreWebView2Async(null);
            await webView2_tab3.EnsureCoreWebView2Async(null);
            await webView2_tab2.EnsureCoreWebView2Async(null);
            await webView2_tab1.EnsureCoreWebView2Async(null);
            webView2_tab1.Visibility = Visibility.Hidden;
            webView2_tab2.Visibility = Visibility.Hidden;
            webView2_tab3.Visibility = Visibility.Hidden;
            webView2_tab4.Visibility = Visibility.Hidden;
        }


        // 공장 버튼 눌렀을떄 1번 탭 활성화
        private void DisplayFirstAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }
        // 공장 버튼 눌렀을 때 2번 탭 활성화
        private void DisplaySecondAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
        }
        // 공장 버튼 눌렀을 때 3번 탭 활성화
        private void DisplayThirdAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
        }
        // 공장 버튼 눌렀을 때 4번 탭 활성화
        private void DisplayFourthAreaTab_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 4;
        }

        // 필터 CSV 체크
        private void showAlertOnDangerousLevels(string timer, float valueWatt1, float valueWatt2, float valueWatt3, float valueWatt4, float valueTemp1, float valueTemp2, float valueTemp3, float valueTemp4)
        {
            // Filter CSV 파일 열어서
            string[] lines = File.ReadAllLines("HeatDataAlarmFilter.csv");
            
            List<string[]> filter_list = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)  // 첫째 줄(스키마) 건너 뜀
            {
                string line = lines[i];
                string[] data = line.Split(',');
                filter_list.Add(data);
                if (data.Length != 5)
                {
                    throw new Exception("File Error");
                }
            }

            // CSV 파일과 현재 데이터를 비교
            for (int i = 0; i < filter_list.Count; i++)
            {
                // 필터 CSV 파일의 변수명, 옵션(전력,온도,가스), 최댓값, 최솟값, 비고
                string filterName = filter_list[i][0];
                string filterOption = filter_list[i][1];
                string filterMax = filter_list[i][2];
                string filterMin = filter_list[i][3];
                string filterString = filter_list[i][4];

                switch (filterName)
                {
                    case "GN02N_MAIN_POWER":
                        appendListBoxNotice(timer, 1, filterName, "전력", valueWatt1, filterMax, filterMin);
                        break;
                    case "GN04N_MAIN_POWER":
                        appendListBoxNotice(timer, 2, filterName, "전력", valueWatt2, filterMax, filterMin);
                        break;
                    case "GN05N_MAIN_POWER":
                        appendListBoxNotice(timer, 3, filterName, "전력", valueWatt3, filterMax, filterMin);
                        break;
                    case "GN07N_MAIN_POWER":
                        appendListBoxNotice(timer, 4, filterName, "전력", valueWatt4, filterMax, filterMin);
                        break;
                    case "GN02N_TEMP":
                        appendListBoxNotice(timer, 1, filterName, "온도", valueTemp1, filterMax, filterMin);
                        break;
                    case "GN04M_TEMP":
                        appendListBoxNotice(timer, 2, filterName, "온도", valueTemp2, filterMax, filterMin);
                        break;
                    case "GN05M_TEMP":
                        appendListBoxNotice(timer, 3, filterName, "온도", valueTemp3, filterMax, filterMin);
                        break;
                    case "GN07N_TEMP":
                        appendListBoxNotice(timer, 4, filterName, "온도", valueTemp4, filterMax, filterMin);
                        break;
                }
            }
        }

        /// <summary>
        /// 현재 필터에 들어간 값과 동일한지
        /// </summary>
        /// <param name="filterAreaNum">구역번호</param>
        /// <param name="filterName">구역명</param>
        /// <param name="option">옵션</param>
        /// <param name="inputValue">현재 값(CSV에서 읽은 값)</param>
        /// <param name="filterMax">필터에서 설정한 최댓값</param>
        /// <param name="filterMin">필터에서 설정한 최솟값</param>
        private void appendListBoxNotice(string timer, int filterAreaNum, string filterName, string option,float inputValue, string filterMax, string filterMin)
        {
            string[] alarmInfo = new string[4];
            
            
            if (int.Parse(filterMax) <= inputValue)
            {
                listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMax} 값 보다 높습니다.");
                alarmInfo[0] = listBox_Notice.Items.Count.ToString();
                alarmInfo[1] = filterName;
                alarmInfo[2] = inputValue.ToString();
                alarmInfo[3] = timer;
                // 알람이 일어나면 에러메세지를 담는 list에 기록
                if(alarmInfo!=null)
                    alaram_to_Datagrid_list.Add(alarmInfo);
            }

            if (int.Parse(filterMin) >= inputValue)
            {
                listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMin} 값 보다 낮습니다.");
                alarmInfo[0] = listBox_Notice.Items.Count.ToString();
                alarmInfo[1] = filterName;
                alarmInfo[2] = inputValue.ToString();
                alarmInfo[3] = timer;
                if (alarmInfo != null)
                    alaram_to_Datagrid_list.Add(alarmInfo);
            }
        }
        
        // 리스트 더블 클릭하면 알림창 -> datagrid 
        private void listBoxNoticeMouseDouble_Click(object sender, MouseButtonEventArgs e)
        {
            if (listBox_Notice.SelectedItem != null)
            {
                // 알림 박스에서 선택을 한 부분의 번호
                string selectedIndex = listBox_Notice.SelectedIndex.ToString();


                foreach (string[] text in alaram_to_Datagrid_list)
                {
                    // 더블 클릭 했을때 리스트 항목 번호 = 메세지를 담아둔 리스트 번호가 같으면 작동
                    if (text[0] == selectedIndex)
                    {
                        tabControl.SelectedIndex = 5;

                        var filteredItemsIndex = test_list.FindIndex(item => item.GN04N_MAIN_POWER == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
                        && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

                        dataGrid_History.SelectedIndex = filteredItemsIndex;

                        DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
                        row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                    }
                }
            }
        }

        // 1번 탭 각 라디오 버튼
        private async void radiobutton_tab1_power_Checked(object sender, RoutedEventArgs e)
        {
            if(webView2_tab1.Visibility == Visibility.Hidden)
            {
                webView2_tab1.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab1.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        }

        private async void radiobutton_tab1_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1.Visibility == Visibility.Hidden)
            {
                webView2_tab1.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab1.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 430);");
        }

        private async void radiobutton_tab1_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab1.Visibility == Visibility.Hidden)
            {
                webView2_tab1.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab1.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 1000);");
        }

        // 2번 탭 각 버튼 
        private async void radiobutton_tab2_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2.Visibility == Visibility.Hidden)
            {
                webView2_tab2.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab2.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab2.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        }

        private async void radiobutton_tab2_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2.Visibility == Visibility.Hidden)
            {
                webView2_tab2.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab2.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab2.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 430);");
        }

        private async void radiobutton_tab2_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab2.Visibility == Visibility.Hidden)
            {
                webView2_tab2.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab2.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab2.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 1000);");
        }

        // 3번 탭 각 버튼
        private async void radiobutton_tab3_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3.Visibility == Visibility.Hidden)
            {
                webView2_tab3.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab3.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab3.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        }

        private async void radiobutton_tab3_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3.Visibility == Visibility.Hidden)
            {
                webView2_tab3.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab3.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab3.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 430);");
        }

        private async void radiobutton_tab3_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab3.Visibility == Visibility.Hidden)
            {
                webView2_tab3.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab3.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab3.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 1000);");
        }

        // 4번 탭 각 버튼
        private async void radiobutton_tab4_power_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4.Visibility == Visibility.Hidden)
            {
                webView2_tab4.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab4.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab4.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        }

        private async void radiobutton_tab4_temp_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4.Visibility == Visibility.Hidden)
            {
                webView2_tab4.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab4.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기
            await webView2_tab4.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 430);");
        }

        private async void radiobutton_tab4_gas_Checked(object sender, RoutedEventArgs e)
        {
            if (webView2_tab4.Visibility == Visibility.Hidden)
            {
                webView2_tab4.Visibility = Visibility.Visible;
                string script = "document.documentElement.style.overflow = 'hidden';";
                await webView2_tab4.ExecuteScriptAsync(script);
            }
            // 페이지 로딩 될 때까지 대기

            await webView2_tab4.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 1000);");
        }

        private void button_set_add_Click(object sender, RoutedEventArgs e)
        {
            Setting.setFilterData setData = new Setting.setFilterData();

            set_list = setData.getFilterData(TextBox_set_error_name.Text, TextBox_set_column_name.Text, float.Parse(TextBox_set_value_above.Text), float.Parse(TextBox_set_value_below.Text), TextBox_set_etc.Text);

            dataGrid_Settings.Items.Add(set_list);
            string setfilePath = "HeatDataAlarmFilter.csv";
            // 저장 오류 수정 필요
            //setData.SaveDataToCSV(dataGrid_Settings, setfilePath);

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