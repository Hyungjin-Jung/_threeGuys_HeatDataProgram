using PLCSocketHandler;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Interfaces;
using FactoryDataReader;
using _threeGuys_HeatDataProgram.PageViewModel;

namespace _threeGuys_HeatDataProgram.Views.Pages
{





    /// <summary>
    /// Interaction logic for _1_DashBoardPage.xaml
    /// </summary>

    public partial class _1_DashBoardPage : Page
    {
        PLCSocketHandler.PLCSocketManager PLCSocket = new PLCSocketHandler.PLCSocketManager();
        private bool isPLCConnected = false;

        string PLCIPAddress = "192.168.1.2";
        string PLCPortNumber = "2004";

        char PLCMemoryLocation = 'M';
        char PLCMemoryAccessSize = 'X';
        long PLCMemoryByteOffset = 8000;
        long PLCMemoryBitOffset = 1;

        bool is_Machine1_connected = false;
        bool is_Machine2_connected = false;
        bool is_Machine3_connected = false;
        bool is_Machine4_connected = false;

        public PageViewModel.PageViewModel testPageViewModel { get; } = new PageViewModel.PageViewModel();

        public _1_DashBoardPage()
        {
            InitializeComponent();
            // 실행시 PLC 연결 실행
            PLCConnect();
            DataContext = testPageViewModel;

        }


        public void UpdateRandomNumber(List<DataColumn> testlist)
        {
            // SampleText TextBox에 1초마다 생성된 난수 출력
            Application.Current.Dispatcher.Invoke(() => label_Watt1.Text = testlist[0].GN02N_MAIN_POWER.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Watt2.Text = testlist[0].GN04M_MAIN_POWER.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Watt3.Text = testlist[0].GN05M_MAIN_POWER.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Watt4.Text = testlist[0].GN07N_MAIN_POWER.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Temp1.Text = testlist[0].GN02N_TEMP.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Temp2.Text = testlist[0].GN04M_TEMP.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Temp3.Text = testlist[0].GN05M_TEMP.ToString());
            Application.Current.Dispatcher.Invoke(() => label_Temp4.Text = testlist[0].GN07N_TEMP.ToString());


            //if(isPLCConnected == true)
            //{


            //// 이하 plc로 데이터 전송하는 부분. 안되면 포기해야함.
            //// 원하는 메모리 위치 
            //PLCMemoryLocation = 'M';
            //PLCMemoryAccessSize = 'W';
            //PLCMemoryByteOffset = 1016;
            //PLCMemoryBitOffset = 1;

            //// 1=실행 , 0 = 종료
            //WriteToPLC($"{testlist[0].GN02N_TEMP.ToString()}");

            //PLCMemoryByteOffset = 1136;
            //WriteToPLC($"{testlist[0].GN04M_TEMP.ToString()}");

            //PLCMemoryByteOffset = 1142;
            //WriteToPLC($"{testlist[0].GN05M_TEMP.ToString()}");
            
            //PLCMemoryByteOffset = 1260;
            //WriteToPLC($"{testlist[0].GN07N_TEMP.ToString()}");
            //}

        }


        // 필터 CSV 체크
        public void showAlertOnDangerousLevels()
        {
            // Filter CSV 파일 열어서

            try
            {
                // CSV 파일과 현재 데이터를 비교
                for (int i = 0; i < MainWindow.filter_list.Count; i++)
                {
                    //// 필터 CSV 파일의 변수명, 옵션(전력,온도,가스), 최댓값, 최솟값, 비고
                    string filterName = MainWindow.filter_list[i].set_machine_name;
                    string filterOption = MainWindow.filter_list[i].set_option_name;
                    string filterMaxMin = MainWindow.filter_list[i].set_max_min;
                    float filterValue = MainWindow.filter_list[i].set_input_value;

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
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN02N_MAIN_POWER);
                            break;
                        case "GN04M_MAIN_POWER":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN04M_MAIN_POWER);
                            break;
                        case "GN05M_MAIN_POWER":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN05M_MAIN_POWER);
                            break;
                        case "GN07N_MAIN_POWER":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN07N_MAIN_POWER);
                            break;
                        case "GN02N_TEMP":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN02N_TEMP);
                            break;
                        case "GN04M_TEMP":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN04M_TEMP);
                            break;
                        case "GN05M_TEMP":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN05M_TEMP);
                            break;
                        case "GN07N_TEMP":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN07N_TEMP);
                            break;
                        case "GN02N_GAS_NRG":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 1, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN02N_GAS_NRG);
                            break;
                        case "GN04M_GAS_NRG":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 2, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN04M_GAS_NRG);
                            break;
                        case "GN05M_GAS_NRG":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 3, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN05M_GAS_NRG);
                            break;
                        case "GN07N_GAS_NRG":
                            appendListBoxNotice(MainWindow.factoryData_list[MainWindow.columNum].Time, 4, filterName, filterOption, filterMaxMin, filterValue, MainWindow.factoryData_list[MainWindow.columNum].GN07N_GAS_NRG);
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
                        if (!MainWindow.filter_alaram_list_string.Any(item => item == ($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과")))
                        {
                            // 중복이 아니라면 추가
                            MainWindow.filter_alaram_list_string.Add($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과");
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
                        if (MainWindow.filter_alaram_list_string.Any(item => item == ($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과")))
                        {
                            // 중복이라면 삭제
                            MainWindow.filter_alaram_list_string.Remove($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과");
                        }
                    }

                    break;

                case "이하":
                    if ((_max_min == "이하") && (between_value >= _input_value))
                    {
                        // 중복 체크
                        if (!MainWindow.filter_alaram_list_string.Any(item => item == ($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과")))
                        {
                            // 중복이 아니라면 추가
                            MainWindow.filter_alaram_list_string.Add($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과");
                        }
                    }
                    else
                    {
                        if (MainWindow.filter_alaram_list_string.Any(item => item == ($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과")))
                        {
                            // 중복이라면 삭제
                            MainWindow.filter_alaram_list_string.Remove($"{filterAreaNum} 번 기계 {_machine_name} 에서 {_option_name} 수치가 설정한 {between_value} {_max_min} 를 초과");
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
                    listView_Notice.Text = string.Join(Environment.NewLine, MainWindow.filter_alaram_list_string);
                });

            }
            catch
            {
                MainWindow.filter_alaram_list_string = new List<string>(); // 빈 컬렉션을 할당
            }
        }




        public string ReadToPLC()
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
                return PLCRead;
            }
            else
            {
                // 실패한 경우 오류 메시지 표시
                MessageBox.Show($"PLC 쓰기 실패!! 에러 코드: {PLCSocket.getResultCodeString(PLCReadResult)}");
                return "-1";
            }
        }


        private void WriteToPLC(string inputPLCValue)
        {
            Thread writeToPLCAsyncThread = new Thread(() =>
            {
                
                
            
            // 이미 PLC에 연결되어 있는 경우
            if (isPLCConnected == true)
            {
                //MessageBox.Show($"PLC와 이미 연결상태 입니다...");
                //return;
            }
            else
            {
                //MessageBox.Show($"PLC와 연결되어 있지 않습니다.");
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

            });
            writeToPLCAsyncThread.Start();
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


        // 기계 1 동작 시작
        private void button_Machine1_Green_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 8018;
            PLCMemoryBitOffset = 1;

            // 1=실행 , 0 = 종료



            if (!is_Machine1_connected)
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine1_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                button_Machine1_Green.Content = "작동 중...";
                is_Machine1_connected = true;
            }
            else
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine1_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Success;
                button_Machine1_Green.Content = "가동 버튼";
                is_Machine1_connected = false;
            }



        }
        // 기계 2 동작 시작
        private void button_Machine2_Green_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 9000;
            PLCMemoryBitOffset = 1;

            // 1=실행 , 0 = 종료


            if (!is_Machine2_connected)
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine2_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                button_Machine2_Green.Content = "작동 중...";
                is_Machine2_connected = true;
            }
            else
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine2_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Success;
                button_Machine2_Green.Content = "가동 버튼";
                is_Machine2_connected = false;
            }
        }

        // 기계 3 동작 시작
        private void button_Machine3_Green_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 10018;
            PLCMemoryBitOffset = 1;

            // 1=실행 , 0 = 종료


                if (!is_Machine3_connected)
                {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine3_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                    button_Machine3_Green.Content = "작동 중...";
                    is_Machine3_connected = true;
                }
                else
                {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine3_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Success;
                    button_Machine3_Green.Content = "가동 버튼";
                    is_Machine3_connected = false;
                }

        }
        // 기계 4 동작 시작
        private void button_Machine4_Green_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 11000;
            PLCMemoryBitOffset = 1;

            // 1=실행 , 0 = 종료

            if (!is_Machine4_connected)
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine4_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Light;
                button_Machine4_Green.Content = "작동 중...";
                is_Machine4_connected = true;
            }
            else
            {
                WriteToPLC("1");
                WriteToPLC("0");

                button_Machine4_Green.Appearance = Wpf.Ui.Common.ControlAppearance.Success;
                button_Machine4_Green.Content = "가동 버튼";
                is_Machine4_connected = false;
            }
        }

        private void button_Machine1_Yellow_Click(object sender, RoutedEventArgs e)
        {

                // UI 스레드에서 컬렉션 수정 작업 수행
                MainWindow.filter_alaram_list_string.RemoveAll(item => item.Contains("1 번"));
                listView_Notice.Text = string.Join(Environment.NewLine, MainWindow.filter_alaram_list_string);

        }

        private void button_Machine2_Yellow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.filter_alaram_list_string.RemoveAll(item => item.Contains("2 번"));
            listView_Notice.Text = string.Join(Environment.NewLine, MainWindow.filter_alaram_list_string);
        }

        private void button_Machine3_Yellow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.filter_alaram_list_string.RemoveAll(item => item.Contains("3 번"));
            listView_Notice.Text = string.Join(Environment.NewLine, MainWindow.filter_alaram_list_string);
        }

        private void button_Machine4_Yellow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.filter_alaram_list_string.RemoveAll(item => item.Contains("4 번"));
            listView_Notice.Text = string.Join(Environment.NewLine, MainWindow.filter_alaram_list_string);
        }

        // 중지 버튼이 해야할것 들
        private void button_Machine1_Red_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 8020;
            PLCMemoryBitOffset = 1;


            WriteToPLC("1");
            WriteToPLC("0");
        }

        private void button_Machine2_Red_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 9020;
            PLCMemoryBitOffset = 1;


            WriteToPLC("1");
            WriteToPLC("0");
        }

        private void button_Machine3_Red_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 10020;
            PLCMemoryBitOffset = 1;


            WriteToPLC("1");
            WriteToPLC("0");

        }

        private void button_Machine4_Red_Click(object sender, RoutedEventArgs e)
        {
            // 원하는 메모리 위치 
            PLCMemoryLocation = 'M';
            PLCMemoryAccessSize = 'X';
            PLCMemoryByteOffset = 11020;
            PLCMemoryBitOffset = 1;


            WriteToPLC("1");
            WriteToPLC("0");

        }


        // 의도적안 이상현상 발생

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // 가스 장치 이상 발생, numpad1
                case Key.NumPad1:
                    PLCMemoryLocation = 'M';
                    PLCMemoryAccessSize = 'X';
                    PLCMemoryByteOffset = 8061;
                    PLCMemoryBitOffset = 1;
                    break;
                // 냉각 장치 이상 발생, numpad2
                case Key.NumPad2:
                    PLCMemoryLocation = 'M';
                    PLCMemoryAccessSize = 'X';
                    PLCMemoryByteOffset = 8062;
                    PLCMemoryBitOffset = 1;
                    break;
                // 열선 이상 발생, numpad3
                case Key.NumPad3:
                    PLCMemoryLocation = 'M';
                    PLCMemoryAccessSize = 'X';
                    PLCMemoryByteOffset = 8063;
                    PLCMemoryBitOffset = 1;
                    break;
                // 문 이상 발생, numpad4
                case Key.NumPad4:
                    PLCMemoryLocation = 'M';
                    PLCMemoryAccessSize = 'X';
                    PLCMemoryByteOffset = 8064;
                    PLCMemoryBitOffset = 1;
                    break;
                case Key.NumPad5:
                    PLCMemoryLocation = 'M';
                    PLCMemoryAccessSize = 'X';
                    PLCMemoryByteOffset = 8000;
                    PLCMemoryBitOffset = 1;
                    break;
                default:
                    break;
            }

            // 1=실행 , 0 = 종료
            if((e.Key == Key.NumPad1) || (e.Key == Key.NumPad2) || (e.Key == Key.NumPad3) || (e.Key == Key.NumPad4) || (e.Key == Key.NumPad5) )
            WriteToPLC("1");

        }

        private void button_ChangePLCIP_Click(object sender, RoutedEventArgs e)
        {
            // 텍스트 박스에 입력된 텍스트를 가져옴
            string plcIP = textBox_PLCIP.Text;
            // Split
            string[] plcIP_list = plcIP.Split(":");
            
            PLCIPAddress =  plcIP_list[0];
            PLCPortNumber = plcIP_list[1];

            MessageBox.Show($"{PLCIPAddress}:{PLCPortNumber}");

            textBox_PLCIP.PlaceholderText = $"현재 : {PLCIPAddress}:{PLCPortNumber}";
            textBox_PLCIP.Text = "";
        }
    }


}
