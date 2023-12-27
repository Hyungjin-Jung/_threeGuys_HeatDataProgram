using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PLCSocketHandler;

namespace _threeGuys_HeatDataProgram.Views.Pages
{
    /// <summary>
    /// Interaction logic for _1_DashBoardPage.xaml
    /// </summary>
    public partial class _1_DashBoardPage : Page
    {

        PLCSocketHandler.PLCSocketManager PLCSocket = new PLCSocketHandler.PLCSocketManager();
        private bool isPLCConnected = false;

        string PLCIPAddress = "192.168.1.20";
        string PLCPortNumber = "2004";

        char PLCMemoryLocation = 'M';
        char PLCMemoryAccessSize = 'X';
        long PLCMemoryByteOffset = 8000;
        long PLCMemoryBitOffset = 1;

        bool is_Button1_Green = false;

        public _1_DashBoardPage()
        {
            InitializeComponent();
            listView_Notice.Items.Add("test");
        }

        private void listViewNoticeMouseDouble_Click(object sender, MouseButtonEventArgs e)
        {

        }

        // 공장 버튼 눌렀을떄 1번 탭 활성화
        private void DisplayFirstAreaTab_Click(object sender, RoutedEventArgs e)
        {
           
        }
        // 공장 버튼 눌렀을 때 2번 탭 활성화
        private void DisplaySecondAreaTab_Click(object sender, RoutedEventArgs e)
        {
            //tabControl.SelectedIndex = 2;
        }
        // 공장 버튼 눌렀을 때 3번 탭 활성화
        private void DisplayThirdAreaTab_Click(object sender, RoutedEventArgs e)
        {
            //tabControl.SelectedIndex = 3;
        }
        // 공장 버튼 눌렀을 때 4번 탭 활성화
        private void DisplayFourthAreaTab_Click(object sender, RoutedEventArgs e)
        {
            //tabControl.SelectedIndex = 4;
        }

        // 눌렀을떄 기계 1 작동되는 구간의 메모리를 변경.
        private void button_Machine1_Green_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 8000;
            PLCMemoryBitOffset = 1;

            // 1=실행 , 0 = 종료
            //WriteToPLC("1");
            if(is_Button1_Green == false)
            {
                button_Machine1_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Success;
                button_Machine1_Green.Content = "작동 중...";
                button_Machine1_Red.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                button_Machine1_Red.Content = "가동 중지";
                is_Button1_Green = true;
            }
            else
            {
                button_Machine1_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                button_Machine1_Green.Content = "작동 시작";
                button_Machine1_Red.Appearance = Wpf.Ui.Common.ControlAppearance.Danger;
                button_Machine1_Red.Content = "가동 중지중...";
                is_Button1_Green = false;
            }
        }

        private void WriteToPLC(string inputPLCValue)
        {

            // 이미 PLC에 연결되어 있는 경우
            if (isPLCConnected == true)
            {
                //MessageBox.Show($"PLC와 이미 연결상태 입니다...");
                //return;
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

            // PLC 쓰기 결과 변수 초기화
            uint PLCWriteResult;
            string PLCWrite = inputPLCValue;

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
            // 연결 해제
            //uint isPLCDisconnect = PLCSocket.Disconnect();

            //// PLC 연결 끊기 성공 여부에 따라 메시지 박스를 표시
            //if (isPLCDisconnect == (uint)XGCOMM_FUNC_RESULT.RT_XGCOMM_SUCCESS)
            //{
            //    MessageBox.Show($"PLC 연결 끊기 성공... (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            //    isPLCConnected = false;
            //}
            //else
            //{
            //    MessageBox.Show($"PLC 연결 끊기 실패!! (IP: {PLCIPAddress}, Port: {PLCPortNumber})");
            //}
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
    }
}
