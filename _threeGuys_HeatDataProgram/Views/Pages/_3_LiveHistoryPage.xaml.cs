using System;
using System.IO;
using System.Windows.Controls;



namespace _threeGuys_HeatDataProgram.Views.Pages
{
    /// <summary>
    /// Interaction logic for _3_LiveHistoryPage.xaml
    /// </summary>
    public partial class _3_LiveHistoryPage : Page
    {
        List<FactoryDataReader.DataColumn> factoryData_list = default;

        string filePath = Directory.GetCurrentDirectory() + "/heatTreatingFactoryData.csv";
        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";


        FactoryDataReader.FactoryDataReader factoryDataReader = new FactoryDataReader.FactoryDataReader();
        public _3_LiveHistoryPage()
        {
            InitializeComponent();
            // CSV 파일 열어서 DataGrid에 저장
            dataGrid_History.ItemsSource = factoryDataReader.heatTreatingFactoryDataRead(filePath);

            // CSV 파일 열어서 리스트에 저장
            factoryData_list = factoryDataReader.heatTreatingFactoryDataRead(filePath);

            dataGrid_History.Items.Refresh();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // CSV 파일 열어서 DataGrid에 저장
            dataGrid_History.ItemsSource = factoryDataReader.heatTreatingFactoryDataRead(filePath);

            // CSV 파일 열어서 리스트에 저장
            factoryData_list = factoryDataReader.heatTreatingFactoryDataRead(filePath);

            dataGrid_History.Items.Refresh();
        }

        private void button_DataGridSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string dataGrid_MachineName = comboBox_DataGridMachineName.Text;
            string dataGrid_Option = comboBox_DataGridOption.Text;
            
            DateTime dataGrid_StartDateTime = datePicker_startDate.SelectedDate.GetValueOrDefault();
           
            string[] dataGrid_SearchOption = comboBox_DataGrid_SearchOption.Text.Split(" ");
            
            int timespan = int.Parse(dataGrid_SearchOption[0]);    // 몇초
            string timespanHMS = dataGrid_SearchOption[1];
            int timespanInput = 0;

            switch (timespanHMS)
            {
                case "초":
                    timespanInput = timespan * 1;
                    break;
                case "분":
                    timespanInput = timespan * 60;
                    break;
                case "시간":
                    timespanInput = timespan * 3600;
                    break;
            }

            // 검색 시작 날짜
            DateTime date = dataGrid_StartDateTime;
            // 어디까지 검색할 건지
            TimeSpan time = new TimeSpan(0,0,timespanInput);
            // 검색 종료 날짜
            DateTime combined = date.Add(time);
            
            string startdate = date.ToString("yyyy-MM-dd_HH:mm:ss");
            string enddate = combined.ToString("yyyy-MM-dd_HH:mm:ss");


            string dataGrid_MachineOption = DataGrid_MachineOption(dataGrid_MachineName, dataGrid_Option);

            // 이하 조건에 맞게 해당하는 컬럼만 반환
            if (dataGrid_MachineName == "전체 구역")
            {
                switch (dataGrid_Option)
                {
                    case "전력":
                        var filteredList_AllPower = factoryData_list
                            .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                        d.Time.CompareTo(enddate) < 1)
                            .Select(d => new
                            {
                                시간 = d.Time,
                                GN02N전력 = d.GN02N_MAIN_POWER,
                                GN04M전력 = d.GN04M_MAIN_POWER,
                                GN05M전력 = d.GN05M_MAIN_POWER,
                                GN07N전력 = d.GN07N_MAIN_POWER
                            })
                            .ToList();
                        dataGrid_History.ItemsSource = filteredList_AllPower;
                        break;
                    case "온도":
                        var filteredList_AllTemp = factoryData_list
                            .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                        d.Time.CompareTo(enddate) < 1)
                            .Select(d => new
                            {
                                시간 = d.Time,
                                GN02N온도 = d.GN02N_TEMP,
                                GN04M온도 = d.GN04M_TEMP,
                                GN05M온도 = d.GN05M_TEMP,
                                GN07N온도 = d.GN07N_TEMP
                            })
                            .ToList();
                        dataGrid_History.ItemsSource = filteredList_AllTemp;
                        break;
                    case "가스":
                        var filteredList_AllGas = factoryData_list
                            .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                        d.Time.CompareTo(enddate) < 1)
                            .Select(d => new
                            {
                                시간 = d.Time,
                                GN02N질소 = d.GN02N_GAS_NRG,
                                GN04M질소 = d.GN04M_GAS_NRG,
                                GN05M질소 = d.GN05M_GAS_NRG,
                                GN07N질소 = d.GN07N_GAS_NRG
                            })
                            .ToList();
                        dataGrid_History.ItemsSource = filteredList_AllGas;
                        break;
                    case "모든 수치":
                        var filteredList = factoryData_list
                            .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                        d.Time.CompareTo(enddate) < 1
                            )
                            .ToList();

                        // DataGrid에 바인딩
                        dataGrid_History.ItemsSource = filteredList;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (dataGrid_Option == "모든 수치")
                {

                    switch (dataGrid_MachineName)
                    {
                        case "GN02N":
                            var filteredList_AllDataMachine1 = factoryData_list
                                .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                            d.Time.CompareTo(enddate) < 1)
                                .Select(d => new
                                {
                                    시간 = d.Time,
                                    GN02N전력 = d.GN02N_MAIN_POWER,
                                    GN02N온도 = d.GN02N_TEMP,
                                    GN02N질소 = d.GN02N_GAS_NRG
                                })
                                .ToList();
                            dataGrid_History.ItemsSource = filteredList_AllDataMachine1;
                            break;
                        case "GN04M":
                            var filteredList_AllDataMachine2 = factoryData_list
                                .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                            d.Time.CompareTo(enddate) < 1)
                                .Select(d => new
                                {
                                    시간 = d.Time,
                                    GN04M전력 = d.GN04M_MAIN_POWER,
                                    GN04M온도 = d.GN04M_TEMP,
                                    GN04M질소 = d.GN04M_GAS_NRG
                                })
                                .ToList();
                            dataGrid_History.ItemsSource = filteredList_AllDataMachine2;
                            break;
                        case "GN05M":
                            var filteredList_AllDataMachine3 = factoryData_list
                                .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                            d.Time.CompareTo(enddate) < 1)
                                .Select(d => new
                                {
                                    시간 = d.Time,
                                    GN05M전력 = d.GN05M_MAIN_POWER,
                                    GN05M온도 = d.GN05M_TEMP,
                                    GN05M질소 = d.GN05M_GAS_NRG
                                })
                                .ToList();
                            dataGrid_History.ItemsSource = filteredList_AllDataMachine3;
                            break;
                        case "GN07N":
                            var filteredList_AllDataMachine4 = factoryData_list
                                .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                            d.Time.CompareTo(enddate) < 1)
                                .Select(d => new
                                {
                                    시간 = d.Time,
                                    GN07N전력 = d.GN07N_MAIN_POWER,
                                    GN07N온도 = d.GN07N_TEMP,
                                    GN07N질소 = d.GN07N_GAS_NRG
                                })
                                .ToList();
                            dataGrid_History.ItemsSource = filteredList_AllDataMachine4;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    var filteredList = factoryData_list
                        .Where(d => d.Time.CompareTo(startdate) > 0 &&
                                    d.Time.CompareTo(enddate) < 1
                        ).Select(d => new
                        {
                            d.Time,
                            dataGrid_MachineOption = GetColumnValue(d, dataGrid_MachineOption),
                        })
                        .ToList();

                    // DataGrid에 바인딩
                    dataGrid_History.ItemsSource = filteredList;
                }
            }
        }
        private float GetColumnValue(FactoryDataReader.DataColumn data, string columnName)
        {
            switch (columnName)
            {
                case "GN02N_MAIN_POWER":
                    return data.GN02N_MAIN_POWER;
                case "GN04M_MAIN_POWER":
                    return data.GN04N_MAIN_POWER;
                case "GN05M_MAIN_POWER":
                    return data.GN05N_MAIN_POWER;
                case "GN07N_MAIN_POWER":
                    return data.GN07N_MAIN_POWER;
                case "GN02N_TEMP":
                    return data.GN02N_TEMP;
                case "GN04M_TEMP":
                    return data.GN04M_TEMP;
                case "GN05M_TEMP":
                    return data.GN05M_TEMP;
                case "GN07N_TEMP":
                    return data.GN07N_TEMP;
                case "GN02N_GAS":
                    return data.GN02N_GAS_NRG;
                case "GN04M_GAS":
                    return data.GN04M_GAS_NRG;
                case "GN05M_GAS":
                    return data.GN05M_GAS_NRG;
                case "GN07N_GAS":
                    return data.GN07N_GAS_NRG;
                default:
                    return 0;
            }
        }


        // 컬럼 하나만 가져올 때 컬럼명 쉽게 변경하게 하려고 만든 함수
        private void dataGrid_History_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "dataGrid_MachineOption")
            {
                string dataGrid_MachineName = comboBox_DataGridMachineName.Text;
                string dataGrid_Option = comboBox_DataGridOption.Text;

                e.Column.Header = DataGrid_MachineOption(dataGrid_MachineName, dataGrid_Option);
            }
        }

        private string DataGrid_MachineOption(string dataGrid_MachineName, string dataGrid_Option)
        {
            switch (dataGrid_Option)
            {
                case "전력":
                    dataGrid_Option = "MAIN_POWER";
                    break;
                case "온도":
                    dataGrid_Option = "TEMP";
                    break;
                case "가스":
                    dataGrid_Option = "GAS_NRG";
                    break;
                default:
                    break;
            }

            string dataGrid_MachineOption = dataGrid_MachineName + "_" + dataGrid_Option;
            return dataGrid_MachineOption;
        }
    }
}
