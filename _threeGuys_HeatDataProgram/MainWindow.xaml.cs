
using setFilterData;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using System.Collections.Generic;
using System;
using System.IO;
using System.Windows.Controls;
using _threeGuys_HeatDataProgram.Views.Pages;    // Directory(현재 주소위치 파악) 사용을 위해 필요.
using PLCSocketHandler;

namespace _threeGuys_HeatDataProgram
{
    public partial class MainWindow
    {
        _1_DashBoardPage DashBoardPage = new _1_DashBoardPage();
        _2_DetailsPage DetailsPage = new _2_DetailsPage();
        _3_LiveHistoryPage LiveHistoryPage = new _3_LiveHistoryPage();
        _4_SetFilterPage SetFilterPageTest = new _4_SetFilterPage();

        /*****************************************************************************************
        ******************************** PLC 통신부 (임시) *****************************************
        *******************************************************************************************/

        PLCSocketHandler.PLCSocketManager PLCSocket = new PLCSocketHandler.PLCSocketManager();
        private bool isPLCConnected = false;

        string PLCIPAddress = "192.168.1.20";
        string PLCPortNumber = "2004";

        char PLCMemoryLocation = 'M';
        char PLCMemoryAccessSize = 'X';
        long PLCMemoryByteOffset = 8000;
        long PLCMemoryBitOffset = 16;

        /*****************************************************************************************
        ******************************** PLC 통신부 (임시) *****************************************
        *******************************************************************************************/

        PySocketHandler.PySocketHandler PySocket = new PySocketHandler.PySocketHandler();
        setFilterData.SettingDataColumn filterList = new SettingDataColumn();
        // FactoryDataCSV 파일 열어서 저장할 리스트 선언
        List<FactoryDataReader.DataColumn> test_list = default;
        // 설정 DataGrid에 넣기위한 csv list
        List<setFilterData.SettingDataColumn> set_list = default;
        // 알림창에 문제가 생긴 부분만 모아놓은 List
        List<string[]> alaram_to_Datagrid_list = new List<string[]>();
        // 필터 모아놓는곳


        // 1초마다 작업을 하기위한 Timer 이용하기 위해 선언
        private DispatcherTimer timer = new DispatcherTimer();
        private int currentRow = 0;
        public MainWindow()
        {

            PySocket.prepareSocket();

            InitializeComponent();

            /*****************************************************************************************
            ******************************** PLC 통신부 (임시) *****************************************
            *******************************************************************************************/

            /*isPLCConnected = PLCConnect();

            // 만약 PLC와의 연결이 실패했을 경우에만 실행. (재연결 시도는 안함)
            if (isPLCConnected == false)
            {
                MessageBox.Show($"PLC 연결 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            }*/

            /*****************************************************************************************
            ******************************** PLC 통신부 (임시) *****************************************
            *******************************************************************************************/

            string filePath = Directory.GetCurrentDirectory() + "/heatTreatingFactoryData.csv";
            string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";

            setFilterData.setFilterData setData = new setFilterData.setFilterData();

            setData.LoadDataFromCSV(SetFilterPageTest.listBox_SetFilter, setfilePath);
            FactoryDataReader.FactoryDataReader test = new FactoryDataReader.FactoryDataReader();
            // CSV 파일 열어서 DataGrid에 저장
            LiveHistoryPage.dataGrid_History.ItemsSource = test.heatTreatingFactoryDataRead(filePath);
            // CSV 파일 열어서 리스트에 저장
            test_list = test.heatTreatingFactoryDataRead(filePath);

            // 로딩 끝나면 탭들을 1~4 탭을 한번씩 활성화 시켜서 안의 그래프가 그려지게 만듬

            InitializeAsync();
            //testUI.Loaded += ResetTab;

            // 1초마다 TimerTick 메서드 호출 -> 1초마다 CSV 알림 받아오는 용도로 설정
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += showTemperatureAndPower;
            timer.Start();
        }




        //// 로딩용 눈속임. Loaded 핸들러(생성자의 로딩이 다 끝났을때)에 담아서 실행.
        ///  tab -> navigation 으로 바꿔서 사용 안함
        //private async void ResetTab(object sender, RoutedEventArgs e)
        //{
        //    // 탭 5를 활성화. 확실하게 활성화 시키기 위해 딜레이
        //    for (int i = 5; i >= 0; i--)
        //    {
        //        tabControl.SelectedIndex = i;
        //        await Task.Delay(100);
        //    }
        //    // 다 끝나면 최전방의 사각형 제거
        //    rectangle_Rectangle.Visibility = Visibility.Collapsed;
        //}
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
            DashBoardPage.label_Watt1.Content = value_Watt1.ToString() + " [W]";
            DashBoardPage.label_Watt2.Content = value_Watt2.ToString() + " [W]";
            DashBoardPage.label_Watt3.Content = value_Watt3.ToString() + " [W]";
            DashBoardPage.label_Watt4.Content = value_Watt4.ToString() + " [W]";

            DashBoardPage.label_Temp1.Content = value_Temp1.ToString() + " [°C]";
            DashBoardPage.label_Temp2.Content = value_Temp2.ToString() + " [°C]";
            DashBoardPage.label_Temp3.Content = value_Temp3.ToString() + " [°C]";
            DashBoardPage.label_Temp4.Content = value_Temp4.ToString() + " [°C]";

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
            //await DetailsPage.webView2_tab4.EnsureCoreWebView2Async(null);
            //await DetailsPage.webView2_tab3.EnsureCoreWebView2Async(null);
            //await DetailsPage.webView2_tab2.EnsureCoreWebView2Async(null);
            await DetailsPage.webView2_tab1.EnsureCoreWebView2Async(null);
            DetailsPage.webView2_tab1.Visibility = Visibility.Hidden;
            //DetailsPage.webView2_tab2.Visibility = Visibility.Hidden;
            //DetailsPage.webView2_tab3.Visibility = Visibility.Hidden;
            //DetailsPage.webView2_tab4.Visibility = Visibility.Hidden;
        }


        // 필터 CSV 체크
        private void showAlertOnDangerousLevels(string timer, float valueWatt1, float valueWatt2, float valueWatt3, float valueWatt4, float valueTemp1, float valueTemp2, float valueTemp3, float valueTemp4)
        {
            setFilterData.SettingDataColumn setData2 = new SettingDataColumn();




            // Filter CSV 파일 열어서
            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv");

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
        private void appendListBoxNotice(string timer, int filterAreaNum, string filterName, string option, float inputValue, string filterMax, string filterMin)
        {
            string[] alarmInfo = new string[4];


            if (int.Parse(filterMax) <= inputValue)
            {
                DashBoardPage.listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMax} 값 보다 높습니다.");
                alarmInfo[0] = DashBoardPage.listBox_Notice.Items.Count.ToString();
                alarmInfo[1] = filterName;
                alarmInfo[2] = inputValue.ToString();
                alarmInfo[3] = timer;
                // 알람이 일어나면 에러메세지를 담는 list에 기록
                if (alarmInfo != null)
                    alaram_to_Datagrid_list.Add(alarmInfo);
            }

            if (int.Parse(filterMin) >= inputValue)
            {
                DashBoardPage.listBox_Notice.Items.Add($"{filterAreaNum}번 구역의 {option}이(가) {inputValue}로, 설정한 {filterMin} 값 보다 낮습니다.");
                alarmInfo[0] = DashBoardPage.listBox_Notice.Items.Count.ToString();
                alarmInfo[1] = filterName;
                alarmInfo[2] = inputValue.ToString();
                alarmInfo[3] = timer;
                if (alarmInfo != null)
                    alaram_to_Datagrid_list.Add(alarmInfo);
            }
        }

        // 리스트 더블 클릭하면 알림창 -> datagrid 
        // 구조적 오류. 수정 필요
        //private void listBoxNoticeMouseDouble_Click(object sender, MouseButtonEventArgs e)
        //{
        //    if (DashBoardPage.listBox_Notice.SelectedItem != null)
        //    {
        //        // 알림 박스에서 선택을 한 부분의 번호
        //        string selectedIndex = listBox_Notice.SelectedIndex.ToString();


        //        foreach (string[] text in alaram_to_Datagrid_list)
        //        {
        //            // 더블 클릭 했을때 리스트 항목 번호 = 메세지를 담아둔 리스트 번호가 같으면 작동
        //            if (text[0] == selectedIndex)
        //            {
        //                tabControl.SelectedIndex = 5;
        //                if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN02N_MAIN_POWER")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN02N_MAIN_POWER == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN04N_MAIN_POWER")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN04N_MAIN_POWER == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN05N_MAIN_POWER")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN05N_MAIN_POWER == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN07N_MAIN_POWER")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN07N_MAIN_POWER == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN02N_TEMP")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN02N_TEMP == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN04M_TEMP")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN04M_TEMP == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN05M_TEMP")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN05M_TEMP == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //                else if (alaram_to_Datagrid_list[int.Parse(selectedIndex)][1] == "GN07N_TEMP")
        //                {
        //                    var filteredItemsIndex = test_list.FindIndex(item => item.GN07N_TEMP == float.Parse(alaram_to_Datagrid_list[int.Parse(selectedIndex)][2])
        //                    && item.Time == alaram_to_Datagrid_list[int.Parse(selectedIndex)][3]);

        //                    dataGrid_History.SelectedIndex = filteredItemsIndex;

        //                    DataGridRow row = (DataGridRow)dataGrid_History.ItemContainerGenerator.ContainerFromIndex(filteredItemsIndex);
        //                    row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        //                }
        //            }
        //        }
        //    }
        //}


        // 1번 탭 각 라디오 버튼 -> 구조변경으로 사용안함. 추후 수정
        //private async void firstTabRadioButtonPower_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (webView2_tab1.Visibility == Visibility.Hidden)
        //    {
        //        webView2_tab1.Visibility = Visibility.Visible;
        //        string script = "document.documentElement.style.overflow = 'hidden';";
        //        await webView2_tab1.ExecuteScriptAsync(script);
        //    }
        //    // 페이지 로딩 될 때까지 대기
        //    await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);");
        //}

        //private async void firstTabRadioButtonTemp_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (webView2_tab1.Visibility == Visibility.Hidden)
        //    {
        //        webView2_tab1.Visibility = Visibility.Visible;
        //        string script = "document.documentElement.style.overflow = 'hidden';";
        //        await webView2_tab1.ExecuteScriptAsync(script);
        //    }
        //    // 페이지 로딩 될 때까지 대기
        //    await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 430);");
        //}

        //private async void firstTabRadioButtonGas_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (webView2_tab1.Visibility == Visibility.Hidden)
        //    {
        //        webView2_tab1.Visibility = Visibility.Visible;
        //        string script = "document.documentElement.style.overflow = 'hidden';";
        //        await webView2_tab1.ExecuteScriptAsync(script);
        //    }
        //    // 페이지 로딩 될 때까지 대기
        //    await webView2_tab1.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 1000);");
        //}


        // 필터조건 추가. 추후 수정 필요
        //private void appendSetFilterDataButton_Click(object sender, RoutedEventArgs e)
        //{
        //    setFilterData.setFilterData setData = new setFilterData.setFilterData();

        //    //string selectedComboBoxItem = ((ComboBoxItem)comboBox_test.SelectedItem)?.Content.ToString();
        //    try
        //    {
        //        //setData.setlist = setData.getFilterData(comboBox_test.Text, TextBox_set_column_name.Text, float.Parse(TextBox_set_value_above.Text), float.Parse(TextBox_set_value_below.Text), TextBox_set_etc.Text);

        //        //dataGrid_Settings.Items.Add(setData.setlist);
        //        //string setfilePath = "HeatDataAlarmFilter.csv";
        //        // 저장 오류 수정 필요
        //        //setData.SaveDataToCSV(dataGrid_Settings, setfilePath);

        //        setData.setlist = setData.getFilterData(comboBox_test.Text, TextBox_set_column_name.Text, float.Parse(TextBox_set_value_above.Text), float.Parse(TextBox_set_value_below.Text), TextBox_set_etc.Text);
        //        dataGrid_Settings.Items.Add(setData.setlist);

        //        // 데이터 세팅
        //        setFilterData.SettingDataColumn setData2 = new SettingDataColumn
        //        {
        //            set_error_name = comboBox_test.Text,
        //            set_column_name = TextBox_set_column_name.Text,
        //            set_value_above = float.Parse(TextBox_set_value_above.Text),
        //            set_value_below = float.Parse(TextBox_set_value_below.Text),
        //            set_etc = TextBox_set_etc.Text
        //        };
        //        // 파일 경로 설정
        //        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";
        //        // 데이터 읽어옴
        //        List<SettingDataColumn> existingData = setData.ReadCSV(setfilePath);
        //        // 데이터 병합
        //        existingData.Add(setData2);
        //        // 데이터 새로 쓰기
        //        setData.WriteToCsv(existingData, setfilePath);



        //    }
        //    catch
        //    {
        //        TextBox_set_value_above.Text = "0";
        //        TextBox_set_value_below.Text = "0";
        //        setData.setlist = setData.getFilterData(comboBox_test.Text, TextBox_set_column_name.Text, float.Parse(TextBox_set_value_above.Text), float.Parse(TextBox_set_value_below.Text), TextBox_set_etc.Text);
        //        dataGrid_Settings.Items.Add(setData.setlist);

        //        // 데이터 세팅
        //        setFilterData.SettingDataColumn setData2 = new SettingDataColumn
        //        {
        //            set_error_name = comboBox_test.Text,
        //            set_column_name = TextBox_set_column_name.Text,
        //            set_value_above = float.Parse(TextBox_set_value_above.Text),
        //            set_value_below = float.Parse(TextBox_set_value_below.Text),
        //            set_etc = TextBox_set_etc.Text
        //        };
        //        // 파일 경로 설정
        //        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";
        //        // 데이터 읽어옴
        //        List<SettingDataColumn> existingData = setData.ReadCSV(setfilePath);
        //        // 데이터 병합
        //        existingData.Add(setData2);
        //        // 데이터 새로 쓰기
        //        setData.WriteToCsv(existingData, setfilePath);

        //        // 저장 오류 수정 필요
        //        //setData.SaveDataToCSV(dataGrid_Settings, setfilePath);

        //    }
        //}



        private void RootNavigation_OnNavigated(INavigation sender, Wpf.Ui.Common.RoutedNavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}",
                "Wpf.Ui.Demo"
        );

            // This funky solution allows us to impose a negative
            // margin for Frame only for the Dashboard page, thanks
            // to which the banner will cover the entire page nicely.
            //RootFrame.Margin = new Thickness(
            //    left: 0,
            //    top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
            //    right: 0,
            //    bottom: 0
            //);
        }

        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            if (Theme.Equals(Wpf.Ui.Appearance.Theme.GetAppTheme(), Wpf.Ui.Appearance.ThemeType.Dark))
                Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
            else
                Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);

        }

        private void NavigationButtonDashboard_OnClick(object sender, RoutedEventArgs e)
        {
            if (Wpf.Ui.Controls.NavigationView.MouseLeftButtonDownEvent.ToString() != "0")
            {

            }


        }

        private void navigation_Loaded(INavigation sender, RoutedNavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}",
                "Wpf.Ui.Demo"
);
            RootFrame.Margin = new Thickness(
             left: 0,
              top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
             right: 0,
             bottom: 0
);
        }

        /*****************************************************************************************
        ******************************** PLC 통신부 (임시) *****************************************
        *******************************************************************************************/

        private void PLCConnectButton_Click()
        {
            // 이미 PLC에 연결되어 있는 경우
            if (isPLCConnected == true)
            {
                MessageBox.Show($"PLC와 이미 연결상태 입니다...");
                return;
            }
            else
            {
                isPLCConnected = PLCConnect();
            }

            // PLC 연결 성공 여부에 따라 메시지 박스를 표시
            if (isPLCConnected == true)
            {
                MessageBox.Show($"PLC 연결 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            }
            else
            {
                MessageBox.Show($"PLC 연결 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            }
        }


        private void PLCDisconnectButton_Click()
        {
            uint isPLCDisconnect = PLCSocket.Disconnect();

            // PLC 연결 끊기 성공 여부에 따라 메시지 박스를 표시
            if (isPLCDisconnect == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
            {
                MessageBox.Show($"PLC 연결 끊기 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
                isPLCConnected = false;
            }
            else
            {
                MessageBox.Show($"PLC 연결 끊기 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            }
        }

        private void PLCWriteButton_Click()
        {
            // PLC 쓰기 결과 변수 초기화
            uint PLCWriteResult;
            string PLCWrite = "";

            // PLC 데이터 쓰기
            PLCWriteResult = PLCSocket.WriteData(PLCMemoryLocation, PLCMemoryAccessSize, PLCMemoryByteOffset, PLCMemoryBitOffset, PLCWrite);

            // PLC 쓰기 결과에 따라 메시지 박스 표시
            if (PLCWriteResult == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
            {
                MessageBox.Show($"PLC 쓰기 성공...");
            }
            else
            {
                MessageBox.Show($"PLC 쓰기 실패!! 에러 코드: {PLCSocket.getResultCodeString(PLCWriteResult)}");
            }
        }


        private void PLCReadButton_Click()
        {
            // PLC에서 읽은 데이터를 저장할 변수 및 초기화
            uint PLCReadResult = 0;
            string PLCReadText = string.Empty;
            UInt16[] bufRead = new UInt16[PLCMemoryBitOffset];
            string PLCRead;

            // PLC에서 데이터 읽기
            PLCReadResult = PLCSocket.ReadData(PLCMemoryLocation, PLCMemoryAccessSize, PLCMemoryByteOffset, PLCMemoryBitOffset, bufRead);

            if (PLCReadResult == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
            {
                // 읽은 데이터를 텍스트로 변환하여 표시
                for (int i = 0; i < PLCMemoryBitOffset; i++)
                {
                    PLCReadText += PLCMemoryAccessSize switch
                    {
                        DEF_DATA_TYPE.DATA_TYPE_BIT => $" {bufRead[i]:X1}",
                        DEF_DATA_TYPE.DATA_TYPE_BYTE => $" {bufRead[i]:X2}",
                        DEF_DATA_TYPE.DATA_TYPE_WORD => $" {bufRead[i]:X4}",
                        _ => string.Empty,
                    };
                }
                PLCRead = PLCReadText.Trim();
            }
            else
            {
                // 실패한 경우 오류 메시지 표시
                MessageBox.Show($"PLC 쓰기 실패!! 에러 코드: {PLCSocket.getResultCodeString(PLCReadResult)}");
            }
        }


        private bool PLCConnect()
        {
            // PLC 소켓 연결 시도
            uint PLCConnectResult = PLCSocket.Connect(PLCIPAddress, Convert.ToInt32(PLCPortNumber));

            // 연결 실패 시 메시지를 표시하고 함수 종료
            if (PLCConnectResult != (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
            {
                return false;
            }

            // 유지 스레드가 이미 실행 중이지 않고, 연결 유지 플래그가 false인 경우
            if (!isPLCConnected)
            {
                // 연결 유지를 확인하는 스레드 시작
                Thread updateKeepAliveThread = new Thread(() =>
                {
                    // 연결 유지 스레드 루프
                    while (PLCSocket.UpdateKeepAlive() == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
                    {
                        isPLCConnected = true;
                        Thread.Sleep(1000);
                    }

                    // 연결 유지 실패 시 메시지 표시 및 플래그 설정
                    MessageBox.Show($"PLC와 연결이 끊겼습니다!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
                    isPLCConnected = false;
                });

                // 스레드 시작
                updateKeepAliveThread.IsBackground = true;
                updateKeepAliveThread.Start();
            }
            return true;
        }

        /*****************************************************************************************
        ******************************** PLC 통신부 (임시) *****************************************
        *******************************************************************************************/

    }
}