using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.IO;

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
    }

    public class setFilterData
    {
        public List<SettingDataColumn> getFilterData(string TextBox_set_error_name, string TextBox_set_column_name, float TextBox_set_value_above, float TextBox_set_value_below, string TextBox_set_etc)
        {
            try
            {
                List<SettingDataColumn> setlist = new List<SettingDataColumn>();

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

        // csv 데이터 로드
        public void LoadDataFromCSV(DataGrid dataGrid, string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                dataGrid.AutoGenerateColumns = false;

                if (lines.Length > 0)
                {
                    string[] columns = lines[0].Split(',');

                    // 열 추가
                    foreach (var column in columns)
                    {
                        dataGrid.Columns.Add(new DataGridTextColumn { Header = column });
                    }

                    // 데이터 추가
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] rowData = lines[i].Split(',');
                        dataGrid.Items.Add(rowData[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV 파일을 불러오는 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        // csv 파일에 데이터 저장
        public void SaveDataToCSV(DataGrid dataGrid, string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // 열 헤더 작성
                List<string> columnNames = new List<string>();
                foreach (var column in dataGrid.Columns)
                {
                    columnNames.Add(column.Header.ToString());
                }
                sb.AppendLine(string.Join(",", columnNames));

                // 데이터 행 작성
                foreach (var item in dataGrid.Items)
                {
                    List<string> rowData = new List<string>();
                    foreach (var col in dataGrid.Columns)
                    {
                        var cellContent = col.GetCellContent(item);
                        if (cellContent is TextBlock)
                        {
                            rowData.Add(((TextBlock)cellContent).Text);
                        }
                    }
                    sb.AppendLine(string.Join(",", rowData));
                }

                File.WriteAllText(filePath, sb.ToString());
                MessageBox.Show("데이터가 CSV 파일로 저장되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV 파일을 저장하는 중 오류가 발생했습니다: " + ex.Message);
            }
        }
    }
}
