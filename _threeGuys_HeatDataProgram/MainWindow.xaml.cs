
using _threeGuys_HeatDataProgram.Views.Pages;
using PLCSocketHandler;
using SetFilterData;
using System;
using System.ComponentModel;
using System.IO; // Directory(현재 주소위치 파악) 사용을 위해 필요.
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        // 소켓통신
        PLCSocketHandler.PLCSocketManager PLCSocket = new PLCSocketHandler.PLCSocketManager();
        private bool isPLCConnected = true;

        string PLCIPAddress = "192.168.1.2";
        string PLCPortNumber = "2004";

        char PLCMemoryLocation = 'M';
        char PLCMemoryAccessSize = 'X';
        long PLCMemoryByteOffset = 8000;
        long PLCMemoryBitOffset = 16;


        // 파일 절대경로 (실시간 데이터 생성용, 필터용)
        string filePath = Directory.GetCurrentDirectory() + "/heatTreatingFactoryData.csv";
        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";

        // 소켓 통신용 선언
        PySocketHandler.PySocketHandler PySocket = new PySocketHandler.PySocketHandler();
        SetFilterData.SetFilterData setFilterData = new SetFilterData.SetFilterData();
        // FactoryDataCSV 파일 열어서 저장할 리스트 선언
        static public List<FactoryDataReader.DataColumn> factoryData_list = default;
        // 이상탐지 필터 리스트
        static public List<SettingDataColumn> filter_list = default;
        // 이상탐지 발생 사건 리스트
        static public List<SetFilterAlarmColumn> filter_alaram_list = new List<SetFilterAlarmColumn>();

        static public List<string> filter_alaram_list_string = new List<string>();
        // 1초마다 작업을 하기위한 Timer 이용하기 위해 선언
        private DispatcherTimer timer = new DispatcherTimer();
        // 실시간 데이터 생성을 위해 1씩 증가하는 상수
        static public int columNum = 2000;
        

        private bool isPythonConnected = true;
        private bool isFalling = false;
        public MainWindow()
        {
           
            // 소켓 통신 대기
            Socket();
            

            InitializeComponent();

            FactoryDataReader.FactoryDataReader factoryDataReader = new FactoryDataReader.FactoryDataReader();

            // CSV 파일 열어서 리스트에 저장
            factoryData_list = factoryDataReader.heatTreatingFactoryDataRead(filePath);
            filter_list = setFilterData.ReadCSV(setfilePath);

            LiveHistoryPage.dataGrid_History.Items.Refresh();

            // 1초마다 TimerTick 메서드 호출 -> 1초마다 CSV 알림 받아오는 용도로 설정
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateLiveData;

            // 타이머 시작
            timer.Start();

            // 테마 깔맞춤 용
            Wpf.Ui.Appearance.Accent.ApplySystemAccent();
        }

        private bool Socket()
        {
            string isSuccess = PySocket.prepareSocket();

            if (isSuccess == "Success")
            {
                Thread pySocketThread = new Thread(() =>
                {
                    while (isPythonConnected == true)
                    {
                        string receivedString = PySocket.receivedSocketString();
                        Console.WriteLine(receivedString);
                        switch (receivedString)
                        {

                            case "Falling":
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (RootFrame.Content is _1_DashBoardPage dashboardPage)
                                    {
                                        dashboardPage.testPageViewModel.PythonAlertText = "작업자 넘어짐";
                                        if (isFalling)
                                        {
                                            MessageBox.Show(dashboardPage.testPageViewModel.PythonAlertText);
                                            isFalling = false;
                                        }
                                    }
                                });
                                break;
                            default:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (RootFrame.Content is _1_DashBoardPage dashboardPage)
                                    {
                                        dashboardPage.testPageViewModel.PythonAlertText = "데이터 값 오류";

                                        MessageBox.Show(dashboardPage.testPageViewModel.PythonAlertText);

                                    }
                                });
                                break;

                        }
                    }

                    // 연결 유지 실패 시 메시지 표시 및 플래그 설정
                    isPythonConnected = false;
                });

                // 스레드 시작
                pySocketThread.IsBackground = true;
                pySocketThread.Start();

                return true;

            }
            else
            {
                return false;
            }
        }


        // 실시간 데이터 받아오는 함수
        private void UpdateLiveData(object sender, EventArgs e)
        {
            if (RootNavigation.SelectedPageIndex == 0)
            {
                List<FactoryDataReader.DataColumn> send2DashboardPagelist = new List<FactoryDataReader.DataColumn>();
                send2DashboardPagelist.Add(MainWindow.factoryData_list[MainWindow.columNum]);

                ((_1_DashBoardPage)RootFrame.Content)?.UpdateRandomNumber(send2DashboardPagelist);
                ((_1_DashBoardPage)RootFrame.Content)?.showAlertOnDangerousLevels();

                //RootFrame.Navigate(DashBoardPage);
                columNum++;
            }
            else
            {
                columNum++;
            }
        }

        // 키입력 이벤트. 시연용으로 이상현상 발생등 넣어 둘 예정
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // 가스 장치 이상 발생, numpad1
                case Key.NumPad9:
                    break;

                default:
                    break;
            }
        }

        // todo -> 메인화면 알림을 더블클릭/클릭 하면 해당하는 문제가된 지점의 데이터를 보여줌 

        // 밝은 <-> 어두운 테마 변경
        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            var newTheme = Wpf.Ui.Appearance.Theme.GetAppTheme() == Wpf.Ui.Appearance.ThemeType.Dark
                ? Wpf.Ui.Appearance.ThemeType.Light
                : Wpf.Ui.Appearance.ThemeType.Dark;

            Wpf.Ui.Appearance.Theme.Apply(
                themeType: newTheme,
                backgroundEffect: Wpf.Ui.Appearance.BackgroundType.Mica,
                updateAccent: true,
                forceBackground: true);
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

        //private void PLCConnectButton_Click()
        //{
        //    // 이미 PLC에 연결되어 있는 경우
        //    if (isPLCConnected == true)
        //    {
        //        MessageBox.Show($"PLC와 이미 연결상태 입니다...");
        //        return;
        //    }
        //    else
        //    {
        //        isPLCConnected = PLCConnect();
        //    }

        //    // PLC 연결 성공 여부에 따라 메시지 박스를 표시
        //    if (isPLCConnected == true)
        //    {
        //        System.Windows.MessageBox.Show($"PLC 연결 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
        //    }
        //    else
        //    {
        //        MessageBox.Show($"PLC 연결 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
        //    }
        //}


        //private void PLCDisconnectButton_Click()
        //{
        //    uint isPLCDisconnect = PLCSocket.Disconnect();

        //    // PLC 연결 끊기 성공 여부에 따라 메시지 박스를 표시
        //    if (isPLCDisconnect == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
        //    {
        //        MessageBox.Show($"PLC 연결 끊기 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
        //        isPLCConnected = false;
        //    }
        //    else
        //    {
        //        MessageBox.Show($"PLC 연결 끊기 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
        //    }
        //}

        //private void PLCWriteButton_Click()
        //{
        //    // PLC 쓰기 결과 변수 초기화
        //    uint PLCWriteResult;
        //    string PLCWrite = "";

        //    // PLC 데이터 쓰기
        //    PLCWriteResult = PLCSocket.WriteData(PLCMemoryLocation, PLCMemoryAccessSize, PLCMemoryByteOffset, PLCMemoryBitOffset, PLCWrite);

        //    // PLC 쓰기 결과에 따라 메시지 박스 표시
        //    if (PLCWriteResult == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
        //    {
        //        MessageBox.Show($"PLC 쓰기 성공...");
        //    }
        //    else
        //    {
        //        MessageBox.Show($"PLC 쓰기 실패!! 에러 코드: {PLCSocket.getResultCodeString(PLCWriteResult)}");
        //    }
        //}


        //private void PLCReadButton_Click()
        //{
        //    // PLC에서 읽은 데이터를 저장할 변수 및 초기화
        //    uint PLCReadResult = 0;
        //    string PLCReadText = string.Empty;
        //    UInt16[] bufRead = new UInt16[PLCMemoryBitOffset];
        //    string PLCRead;

        //    // PLC에서 데이터 읽기
        //    PLCReadResult = PLCSocket.ReadData(PLCMemoryLocation, PLCMemoryAccessSize, PLCMemoryByteOffset, PLCMemoryBitOffset, bufRead);

        //    if (PLCReadResult == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
        //    {
        //        // 읽은 데이터를 텍스트로 변환하여 표시
        //        for (int i = 0; i < PLCMemoryBitOffset; i++)
        //        {
        //            PLCReadText += PLCMemoryAccessSize switch
        //            {
        //                DEF_DATA_TYPE.DATA_TYPE_BIT => $" {bufRead[i]:X1}",
        //                DEF_DATA_TYPE.DATA_TYPE_BYTE => $" {bufRead[i]:X2}",
        //                DEF_DATA_TYPE.DATA_TYPE_WORD => $" {bufRead[i]:X4}",
        //                _ => string.Empty,
        //            };
        //        }
        //        PLCRead = PLCReadText.Trim();
        //    }
        //    else
        //    {
        //        // 실패한 경우 오류 메시지 표시
        //        MessageBox.Show($"PLC 쓰기 실패!! 에러 코드: {PLCSocket.getResultCodeString(PLCReadResult)}");
        //    }
        //}


        //private bool PLCConnect()
        //{
        //    // PLC 소켓 연결 시도
        //    uint PLCConnectResult = PLCSocket.Connect(PLCIPAddress, Convert.ToInt32(PLCPortNumber));

        //    // 연결 실패 시 메시지를 표시하고 함수 종료
        //    if (PLCConnectResult != (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
        //    {
        //        return false;
        //    }

        //    // 유지 스레드가 이미 실행 중이지 않고, 연결 유지 플래그가 false인 경우
        //    if (!isPLCConnected)
        //    {
        //        // 연결 유지를 확인하는 스레드 시작
        //        Thread updateKeepAliveThread = new Thread(() =>
        //        {
        //            // 연결 유지 스레드 루프
        //            while (PLCSocket.UpdateKeepAlive() == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
        //            {
        //                isPLCConnected = true;
        //                Thread.Sleep(1000);
        //            }

        //            // 연결 유지 실패 시 메시지 표시 및 플래그 설정
        //            MessageBox.Show($"PLC와 연결이 끊겼습니다!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
        //            isPLCConnected = false;
        //        });

        //        // 스레드 시작
        //        updateKeepAliveThread.IsBackground = true;
        //        updateKeepAliveThread.Start();
        //    }
        //    return true;
        //}


        /*****************************************************************************************
        ******************************** PLC 통신부 (임시) *****************************************
        *******************************************************************************************/

    }
}