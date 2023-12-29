using SetFilterData;
using System.IO;
using System.Windows.Controls;

namespace _threeGuys_HeatDataProgram.Views.Pages
{

    /// <summary>
    /// Interaction logic for _4_SetFilterPage.xaml
    /// </summary>
    public partial class _4_SetFilterPage : Page
    {
        SetFilterData.SetFilterData setFilterData = new SetFilterData.SetFilterData();

        string setfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";
        public _4_SetFilterPage()
        {

            InitializeComponent();

            DataGrid_SetFilter.ItemsSource = setFilterData.ReadCSV(setfilePath);
        }

        private void button_SetFilterApply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // csv 가 한글을 못읽는다면 주석 해제하고 2번쨰 매개변수를 csvSetFilterOption으로 변경
            //string setFilterOption = comboBox_SetFilterOption.Text;
            //string csvSetFilterOption = "";
            //switch (setFilterOption)
            //{
            //    case "전력":
            //        csvSetFilterOption = "POWER";
            //        break;
            //    case "온도":
            //        csvSetFilterOption = "TEMP";
            //        break;
            //    case "가스":
            //        csvSetFilterOption = "GAS";
            //        break;
            //    default:
            //        csvSetFilterOption = "ERROR";
            //        break;
            //}

            try
            {
                setFilterData.setlist = setFilterData.getFilterData(comboBox_SetFilterMachineName.Text, comboBox_SetFilterOption.Text, comboBox_SetFilterMaxMin.Text, float.Parse(textBox_SetFilterValue.Text));
                //DataGrid_SetFilter.Items.Add(setData.setlist);

                // 데이터 세팅
                SetFilterData.SettingDataColumn setData2 = new SettingDataColumn
                {
                    set_machine_name = comboBox_SetFilterMachineName.Text,
                    set_option_name = comboBox_SetFilterOption.Text,
                    set_max_min = comboBox_SetFilterMaxMin.Text,
                    set_input_value = float.Parse(textBox_SetFilterValue.Text),
                    //set_etc = TextBox_set_etc.Text
                };

                // 데이터 읽어옴
                List<SettingDataColumn> existingData = setFilterData.ReadCSV(setfilePath);
                // 데이터 병합
                existingData.Add(setData2);
                // 데이터 새로 쓰기
                setFilterData.WriteToCsv(existingData, setfilePath);
                MainWindow.filter_list = setFilterData.ReadCSV(setfilePath);
                DataGrid_SetFilter.ItemsSource = setFilterData.ReadCSV(setfilePath);
            }
            catch
            {
                textBox_SetFilterValue.Text = "0";
                setFilterData.setlist = setFilterData.getFilterData(comboBox_SetFilterMachineName.Text, comboBox_SetFilterOption.Text, comboBox_SetFilterMaxMin.Text, float.Parse(textBox_SetFilterValue.Text));
                //DataGrid_SetFilter.Items.Add(setData.setlist);

                // 데이터 세팅
                SetFilterData.SettingDataColumn setData2 = new SettingDataColumn
                {
                    set_machine_name = comboBox_SetFilterMachineName.Text,
                    set_option_name = comboBox_SetFilterOption.Text,
                    set_max_min = comboBox_SetFilterMaxMin.Text,
                    set_input_value = float.Parse(textBox_SetFilterValue.Text),
                    //set_etc = TextBox_set_etc.Text
                };

                // 데이터 읽어옴
                List<SettingDataColumn> existingData = setFilterData.ReadCSV(setfilePath);
                // 데이터 병합
                existingData.Add(setData2);
                // 데이터 새로 쓰기
                setFilterData.WriteToCsv(existingData, setfilePath);
                MainWindow.filter_list = setFilterData.ReadCSV(setfilePath);
                DataGrid_SetFilter.ItemsSource = setFilterData.ReadCSV(setfilePath);
            }
        }

        //규칙 삭제
        private void button_SetFilterDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<SettingDataColumn> existingData = new List<SettingDataColumn>();
            // 데이터 새로 쓰기
            setFilterData.WriteToCsv(existingData, setfilePath);
            MainWindow.filter_list = setFilterData.ReadCSV(setfilePath);
            DataGrid_SetFilter.ItemsSource = setFilterData.ReadCSV(setfilePath);
        }
    }
}
