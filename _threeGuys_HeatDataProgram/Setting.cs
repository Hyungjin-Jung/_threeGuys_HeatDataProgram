using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections;

namespace Setting
{
    public class SettingDataColumn
    {
        public SettingDataColumn() { }

        public string set_error_name { get; set; }
        public string set_column_name { get; set; }
        public float set_value_above { get; set; }
        public float set_value_below { get; set; }
        public string set_etc { get; set; }
      
    public class setFilterData
    {
        public List<SettingDataColumn> getFilterData(string TextBox_set_error_name, string TextBox_set_column_name, float TextBox_set_value_above, float TextBox_set_value_below, string TextBox_set_etc)
        {
            try
            {
                SettingDataColumn setcolumn = new SettingDataColumn();
                setcolumn.set_error_name = TextBox_set_error_name;
                setcolumn.set_column_name = TextBox_set_column_name;
                setcolumn.set_value_above = TextBox_set_value_above;
                setcolumn.set_value_below = TextBox_set_value_below;
                setcolumn.set_etc = TextBox_set_etc;
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
        public void LoadDataFromCSV(DataGrid dataGrid_Settings, string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length > 0)
                {
                    string[] columns = lines[0].Split(',');
                    // 데이터 추가, 첫 번째 줄 건너뜀
                    for (int i = 1; i < lines.Length; i++)
                    {
                        SettingDataColumn setcolumn = new SettingDataColumn();
                        string setLine = lines[i];
                        string[] rowData = setLine.Split(',');
                        setcolumn.set_error_name = rowData[0];
                        setcolumn.set_column_name = rowData[1];
                        setcolumn.set_value_above = float.Parse(rowData[2]);
                        setcolumn.set_value_below = float.Parse(rowData[3]);
                        setcolumn.set_etc = rowData[4];
                        setlist.Add(setcolumn);
                        dataGrid_Settings.Items.Add(setcolumn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV 파일을 불러오는 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        // csv 파일에 데이터 저장하는 함수
        public void SaveDataToCSV(DataGrid dataGrid_Settings, string filePath)
        {
            try
            {
                if (dataGrid_Settings.Items.Count == 0)
                {
                    MessageBox.Show("저장할 데이터가 없습니다.");
                    return;
                }

                // CSV 문자열을 저장할 리스트
                List<string> csvLines = new List<string>();

                // CSV 헤더 생성 (컬럼 이름)
                var columns = dataGrid_Settings.Columns.Select(column => column.Header.ToString());
                csvLines.Add(string.Join(",", columns));

                // 각 행의 데이터를 CSV 문자열에 추가
                foreach (var item in dataGrid_Settings.Items)
                {
                    var row = item as SettingDataColumn;

                    // 각 열의 데이터를 CSV 형식으로 변환하여 리스트에 추가
                    if (row != null)
                    {
                        string rowData = $"{row.set_error_name},{row.set_column_name},{row.set_value_above},{row.set_value_below},{row.set_etc}";
                        csvLines.Add(rowData);
                    }
                }

                // 파일에 CSV 문자열 작성
                File.WriteAllLines(filePath, csvLines);

                MessageBox.Show("DataGrid의 데이터가 CSV 파일로 저장되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV 파일을 저장하는 중 오류가 발생했습니다: " + ex.Message);
            }
        }
    }
}
