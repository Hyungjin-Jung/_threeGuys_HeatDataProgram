using System.IO;
using System.Windows.Controls;
using System.Linq;
using System.Globalization;
using Wpf.Ui.Controls;
using System.Windows.Data;
using System.ComponentModel;

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
            string dataGrid_StartDate = textBox_DataGridStartDate.Text;
            string dataGrid_EndDate = textBox_DataGridEndDate.Text;

            string dataGrid_MachineOption = DataGrid_MachineOption(dataGrid_MachineName, dataGrid_Option);

            var filteredList = factoryData_list
                .Where(d => d.Time.CompareTo(dataGrid_StartDate) > 0 &&
                            d.Time.CompareTo(dataGrid_EndDate) < 1
                ).Select(d => new
                {
                    d.Time,
                    dataGrid_MachineOption = GetColumnValue(d, dataGrid_MachineOption),
                })
                .ToList();

            
            // DataGrid에 바인딩
            dataGrid_History.ItemsSource = filteredList;

        }
        private float GetColumnValue(FactoryDataReader.DataColumn data, string columnName)
        {
            // 선택한 열에 대한 값을 동적으로 가져오기
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
