using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Windows;

namespace SetFilterData
{
    public class SettingDataColumn
    {
        public SettingDataColumn() { }

        public string set_machine_name { get; set; }
        public string set_option_name { get; set; }
        public string set_max_min { get; set; }
        public float set_input_value { get; set; }
        public string set_etc { get; set; }
    }

    public class SetFilterData
    {
        public List<SettingDataColumn> setlist = new List<SettingDataColumn>();

        public List<SettingDataColumn> ReadCSV(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                try
                {
                    return csv.GetRecords<SettingDataColumn>().ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading csv file");
                    return new List<SettingDataColumn>();
                }
        }

        public void WriteToCsv(List<SettingDataColumn> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                try
                {

                    csv.WriteRecords(data);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing csv file : (ex.Message)");
                }
        }




        public List<SettingDataColumn> getFilterData(string machineName, string option, string maxMin, float inputValue)    // Min 이하 = 1, Max 이상 = 0
        {
            try
            {
                SettingDataColumn setcolumn = new SettingDataColumn();
                setcolumn.set_machine_name = machineName;
                setcolumn.set_option_name = option;
                setcolumn.set_max_min = maxMin;
                setcolumn.set_input_value = inputValue;
                //setcolumn.set_etc = TextBox_set_etc;
                setlist.Add(setcolumn);
                return setlist;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return new List<SettingDataColumn>();
        }

        // csv 데이터 가져오는 함수
        //public void LoadDataFromCSV(DataGrid dataGrid_Settings, string filePath)
        //{
        //    try
        //    {
        //        string[] lines = File.ReadAllLines(filePath);
        //        if (lines.Length > 0)
        //        {
        //            string[] columns = lines[0].Split(',');
        //            // 데이터 추가, 첫 번째 줄 건너뜀
        //            for (int i = 1; i < lines.Length; i++)
        //            {
        //                SettingDataColumn setcolumn = new SettingDataColumn();
        //                string setLine = lines[i];
        //                string[] rowData = setLine.Split(',');
        //                setcolumn.set_error_name = rowData[0];
        //                setcolumn.set_column_name = rowData[1];
        //                setcolumn.set_value_above = float.Parse(rowData[2]);
        //                setcolumn.set_value_below = float.Parse(rowData[3]);
        //                setcolumn.set_etc = rowData[4];
        //                setlist.Add(setcolumn);
        //                dataGrid_Settings.Items.Add(setcolumn);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("CSV 파일을 불러오는 중 오류가 발생했습니다: " + ex.Message);
        //    }
        //}

        //// csv 파일에 데이터 저장하는 함수
        //public void SaveDataToCSV(DataGrid dataGrid_Settings, string filePath)
        //{
        //    try
        //    {
        //        if (dataGrid_Settings.Items.Count == 0)
        //        {
        //            MessageBox.Show("저장할 데이터가 없습니다.");
        //            return;
        //        }

        //        // CSV 문자열을 저장할 리스트
        //        List<string> csvLines = new List<string>();

        //        // CSV 헤더 생성 (컬럼 이름)
        //        var columns = dataGrid_Settings.Columns.Select(column => column.Header.ToString());
        //        csvLines.Add(string.Join(",", columns));

        //        // 각 행의 데이터를 CSV 문자열에 추가
        //        foreach (var item in dataGrid_Settings.Items)
        //        {
        //            var row = item as SettingDataColumn;

        //            // 각 열의 데이터를 CSV 형식으로 변환하여 리스트에 추가
        //            if (row != null)
        //            {
        //                string rowData = $"{row.set_error_name},{row.set_column_name},{row.set_value_above},{row.set_value_below},{row.set_etc}";
        //                csvLines.Add(rowData);
        //            }
        //        }

        //        // 파일에 CSV 문자열 작성
        //        File.WriteAllLines(filePath, csvLines);

        //        MessageBox.Show("DataGrid의 데이터가 CSV 파일로 저장되었습니다.");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("CSV 파일을 저장하는 중 오류가 발생했습니다: " + ex.Message);
        //    }

        //}
    }
}
