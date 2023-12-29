
using _threeGuys_HeatDataProgram.Views.Pages;
using PLCSocketHandler;
using SetFilterData;
using System.IO; // Directory(현재 주소위치 파악) 사용을 위해 필요.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace _threeGuys_HeatDataProgram
{
    public class SetFilterAlarmColumn
    {
        public SetFilterAlarmColumn() { }

        public string time { get; set; }
        public int areaNum { get; set; }
        public string machine_name { get; set; }
        public string option_name { get; set; }
        public string max_min { get; set; }
        public float input_value { get; set; }
    }
    public partial class MainWindow
    {
        _1_DashBoardPage DashBoardPage = new _1_DashBoardPage();
        _2_DetailsPage DetailsPage = new _2_DetailsPage();
        _3_LiveHistoryPage LiveHistoryPage = new _3_LiveHistoryPage();
        _4_SetFilterPage SetFilterPage = new _4_SetFilterPage();

        // 테마 변경용 선언
        IThemeService ThemeService = new ThemeService();

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
        string filePath = Directory.GetCurrentDirectory() + "/heatTreatingFactoryData.csv";
        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";

        PySocketHandler.PySocketHandler PySocket = new PySocketHandler.PySocketHandler();
        SetFilterData.SetFilterData setFilterData = new SetFilterData.SetFilterData();
        // FactoryDataCSV 파일 열어서 저장할 리스트 선언
        static public List<FactoryDataReader.DataColumn> factoryData_list = default;
        static public List<SettingDataColumn> filter_list = default;
        static public List<SetFilterAlarmColumn> filter_alaram_list = new List<SetFilterAlarmColumn>();
        static public List<string> filter_alaram_list_string = new List<string>();
        // 1초마다 작업을 하기위한 Timer 이용하기 위해 선언
        private DispatcherTimer timer = new DispatcherTimer();
        int columNum = 0;
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





            FactoryDataReader.FactoryDataReader factoryDataReader = new FactoryDataReader.FactoryDataReader();

            // CSV 파일 열어서 리스트에 저장
            factoryData_list = factoryDataReader.heatTreatingFactoryDataRead(filePath);
            filter_list = setFilterData.ReadCSV(setfilePath);

            LiveHistoryPage.dataGrid_History.Items.Refresh();

            // 1초마다 TimerTick 메서드 호출 -> 1초마다 CSV 알림 받아오는 용도로 설정
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateLiveData;
            timer.Start();
        }


        // 실시간 데이터 받아오는 함수
        private void UpdateLiveData(object sender, EventArgs e)
        {
            if (RootNavigation.SelectedPageIndex == 0)
            {
                DashBoardPage.label_Watt1.Text = factoryData_list[columNum].GN02N_MAIN_POWER.ToString();
                DashBoardPage.label_Watt2.Text = factoryData_list[columNum].GN04M_MAIN_POWER.ToString();
                DashBoardPage.label_Watt3.Text = factoryData_list[columNum].GN05M_MAIN_POWER.ToString();
                DashBoardPage.label_Watt4.Text = factoryData_list[columNum].GN07N_MAIN_POWER.ToString();
                DashBoardPage.label_Temp1.Text = factoryData_list[columNum].GN02N_TEMP.ToString();
                DashBoardPage.label_Temp2.Text = factoryData_list[columNum].GN04M_TEMP.ToString();
                DashBoardPage.label_Temp3.Text = factoryData_list[columNum].GN05M_TEMP.ToString();
                DashBoardPage.label_Temp4.Text = factoryData_list[columNum].GN07N_TEMP.ToString();
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.None;
                RootFrame.Navigate(DashBoardPage);
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.FadeInWithSlide;
                columNum++;
            }
            else
            {
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.FadeInWithSlide;
                columNum++;
            }
            showAlertOnDangerousLevels();
        }

        // xaml 상에서 loaded 같은데서 사용
        private void UpdateLiveData(object sender, RoutedEventArgs e)
        {
            if (RootNavigation.SelectedPageIndex == 0)
            {
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.None;
                RootFrame.Navigate(DashBoardPage);
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.FadeInWithSlide;
            }
            else
            {
                RootNavigation.TransitionType = Wpf.Ui.Animations.TransitionType.FadeInWithSlide;
            }
        }


        // 필터 CSV 체크
        private void showAlertOnDangerousLevels()
        {
            // Filter CSV 파일 열어서

            try
            {
                // CSV 파일과 현재 데이터를 비교
                for (int i = 0; i < filter_list.Count; i++)
                {
                    //// 필터 CSV 파일의 변수명, 옵션(전력,온도,가스), 최댓값, 최솟값, 비고
                    string filterName = filter_list[i].set_machine_name;
                    string filterOption = filter_list[i].set_option_name;
                    string filterMaxMin = filter_list[i].set_max_min;
                    float filterValue = filter_list[i].set_input_value;

                    string machineFilterOption = "";

                    switch (filterOption)
                    {
                        case "전력":
                            machineFilterOption = "MAIN_POWER";
                            break;
                        case "온도":
                            machineFilterOption = "TEMP";
                            break;
                        case "가스":
                            machineFilterOption = "GAS_NRG";
                            break;
                        default:
                            break;
                    }

                    string machineName = filterName + "_" + machineFilterOption;

                    switch (machineName)
                    {
                        case "GN02N_MAIN_POWER":
                            appendListBoxNotice(factoryData_list[columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN02N_MAIN_POWER);
                            break;
                        case "GN04M_MAIN_POWER":
                            appendListBoxNotice(factoryData_list[columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN04M_MAIN_POWER);
                            break;
                        case "GN05M_MAIN_POWER":
                            appendListBoxNotice(factoryData_list[columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN05M_MAIN_POWER);
                            break;
                        case "GN07N_MAIN_POWER":
                            appendListBoxNotice(factoryData_list[columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN07N_MAIN_POWER);
                            break;
                        case "GN02N_TEMP":
                            appendListBoxNotice(factoryData_list[columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN02N_TEMP);
                            break;
                        case "GN04M_TEMP":
                            appendListBoxNotice(factoryData_list[columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN04M_TEMP);
                            break;
                        case "GN05M_TEMP":
                            appendListBoxNotice(factoryData_list[columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN05M_TEMP);
                            break;
                        case "GN07N_TEMP":
                            appendListBoxNotice(factoryData_list[columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN07N_TEMP);
                            break;
                        case "GN02N_GAS_NRG":
                            appendListBoxNotice(factoryData_list[columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN02N_GAS_NRG);
                            break;
                        case "GN04M_GAS_NRG":
                            appendListBoxNotice(factoryData_list[columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN04M_GAS_NRG);
                            break;
                        case "GN05M_GAS_NRG":
                            appendListBoxNotice(factoryData_list[columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN05M_GAS_NRG);
                            break;
                        case "GN07N_GAS_NRG":
                            appendListBoxNotice(factoryData_list[columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, factoryData_list[columNum].GN07N_GAS_NRG);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {

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
        /// 
        //public SetFilterAlarmColumn() { }

        //public string time { get; set; }
        //public string machine_name { get; set; }
        //public string option_name { get; set; }
        //public string max_min { get; set; }
        //public float input_value { get; set; }
        private void appendListBoxNotice(string _time, int filterAreaNum, string _machine_name, string _option_name, string _max_min, float between_value, float _input_value)
        {

            SetFilterAlarmColumn setFilterAlarmColumn = new SetFilterAlarmColumn
            {
                time = _time,
                areaNum = filterAreaNum,
                machine_name = _machine_name,
                option_name = _option_name,
                max_min = _max_min,
                input_value = _input_value
            };


            switch (_max_min)
            {
                case "이상":

                    if ((_max_min == "이상") && (between_value <= _input_value))
                    {
                        //filter_alaram_list.Add(setFilterAlarmColumn);

                        // 중복 체크
                        if (!filter_alaram_list_string.Any(item => item == (filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ")))
                        {
                            // 중복이 아니라면 추가
                            filter_alaram_list_string.Add(filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ");
                        }

                        //for (int i = 0; i < filter_alaram_list.Count; i++)
                        //{
                        //    if (filter_alaram_list[i].areaNum == filterAreaNum)
                        //    {
                        //        filter_alaram_list.Remove(filter_alaram_list.Last());
                        //        break;
                        //    }
                        //}

                    }
                    else
                    {
                        if (filter_alaram_list_string.Any(item => item == (filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ")))
                        {
                            // 중복이라면 삭제
                            filter_alaram_list_string.Remove(filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ");
                        }



                    }

                    break;

                case "이하":
                    if ((_max_min == "이하") && (between_value >= _input_value))
                    {
                        //filter_alaram_list.Add(setFilterAlarmColumn);

                        // 중복 체크
                        if (!filter_alaram_list_string.Any(item => item == (filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ")))
                        {
                            // 중복이 아니라면 추가
                            filter_alaram_list_string.Add(filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ");
                        }

                        //for (int i = 0; i < filter_alaram_list.Count; i++)
                        //{
                        //    if (filter_alaram_list[i].areaNum == filterAreaNum)
                        //    {
                        //        filter_alaram_list.Remove(filter_alaram_list.Last());
                        //        break;
                        //    }
                        //}
                    }
                    else
                    {
                        if (filter_alaram_list_string.Any(item => item == (filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ")))
                        {
                            // 중복이라면 삭제
                            filter_alaram_list_string.Remove(filterAreaNum + "번 기계 " + _machine_name + "에서 " + _option_name + " 수치가 설정한 " + between_value + _max_min + "를 초과 ");
                        }

                    }
                    break;
                default:
                    break;
            }

            // 중복 체크



            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // UI 스레드에서 컬렉션 수정 작업 수행
                    DashBoardPage.listView_Notice.Text = string.Join(Environment.NewLine, filter_alaram_list_string);
                });

            }
            catch
            {

                filter_alaram_list_string = new List<string>(); // 빈 컬렉션을 할당

            }
        }
        // todo -> 메인화면 알림을 더블클릭/클릭 하면 해당하는 문제가된 지점의 데이터를 보여줌 

        // 밝은 <-> 어두운 테마 변경
        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            ThemeService.SetTheme(
                     ThemeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark
                );
            //if (Theme.Equals(Wpf.Ui.Appearance.Theme.GetAppTheme(), Wpf.Ui.Appearance.ThemeType.Dark))
            //    Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
            //else
            //    Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
        }

        //트레이 아이콘 클릭시 작동
        private void NavigationPage_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem input = (MenuItem)sender;

            switch (input.Header)
            {
                case "DashBoard":
                    RootNavigation.Navigate(0);
                    break;
                case "Details":
                    RootNavigation.Navigate(1);
                    break;
                case "LiveHistory":
                    RootNavigation.Navigate(2);
                    break;
                case "SetFilter":
                    RootNavigation.Navigate(3);
                    break;
                case "Exit":
                    this.Close();
                    break;
                default:
                    break;
            }
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
                System.Windows.MessageBox.Show($"PLC 연결 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
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