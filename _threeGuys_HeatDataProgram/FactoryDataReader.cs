using System.IO;
using System.Windows;

namespace FactoryDataReader
{
    public class DataColumn
    {
        public DataColumn() { }

        public string Time { get; set; }
        public float GN07N_MAIN_POWER { get; set; }
        public float GN07N_SUB_POWER { get; set; }
        public float GN07N_TEMP { get; set; }
        public float GN07N_HIGH_TEMP { get; set; }
        public float GN07N_MID_TEMP { get; set; }
        public float GN07N_LOW_TEMP { get; set; }
        public float GN07N_OVER_TEMP { get; set; }
        public float GN07N_GAS_NRG { get; set; }
        public float GN07N_GAS_AMM { get; set; }
        public float GN07N_GAS_CDO { get; set; }
        public float GN07N_PPIT { get; set; }
        public float GN05N_MAIN_POWER { get; set; }
        public float GN05M_MAIN_POWER { get; set; }
        public float GN05M_TEMP { get; set; }
        public float GN05M_HIGH_TEMP { get; set; }
        public float GN05M_LOW_TEMP { get; set; }
        public float GN05M_OVER_TEMP { get; set; }
        public float GN05M_GAS_NRG { get; set; }
        public float GN05M_GAS_AMM { get; set; }
        public float GN05M_GAS_CDO { get; set; }
        public float GN04N_MAIN_POWER { get; set; }
        public float GN04M_MAIN_POWER { get; set; }
        public float GN04M_TEMP { get; set; }
        public float GN04M_HIGH_TEMP { get; set; }
        public float GN04M_MID_TEMP { get; set; }
        public float GN04M_LOW_TEMP { get; set; }
        public float GN04M_OVER_TEMP { get; set; }
        public float GN04M_GAS_NRG { get; set; }
        public float GN04M_GAS_AMM { get; set; }
        public float GN04M_GAS_CDO { get; set; }
        public float GN03N_MAIN_POWER { get; set; }
        public float GN02N_MAIN_POWER { get; set; }
        public float GN02N_TEMP { get; set; }
        public float GN02N_HIGH_TEMP { get; set; }
        public float GN02N_MID_TEMP { get; set; }
        public float GN02N_LOW_TEMP { get; set; }
        public float GN02N_OVER_TEMP { get; set; }
        public float GN02N_GAS_NRG { get; set; }
        public float GN02N_GAS_AMM { get; set; }
        public float GN02N_GAS_CDO { get; set; }
        public float GN02N_PPIT { get; set; }
    }

    class FactoryDataReader : System.Data.DataColumn
    {
        public FactoryDataReader() // 생성자
        {

        }

        public List<DataColumn> heatTreatingFactoryDataRead(string filePath)
        {
            try
            {
                // 파일을 bin -> Debug 안에 넣어 두면 별도의 경로를 입력하지 않아도 댐

                string[] lines = File.ReadAllLines(filePath);

                List<DataColumn> FactoryData = new List<DataColumn>();

                for (int i = 1; i < lines.Length; i++)  // 첫째 줄(스키마) 건너 뜀
                {
                    string line = lines[i];
                    string[] data = line.Split(',');
                    if (data.Length != 42)
                    {
                        throw new Exception("File Error");
                    }

                    DataColumn column = new DataColumn();
                    column.Time = data[0];
                    column.GN07N_MAIN_POWER = float.Parse(data[1]);
                    column.GN07N_SUB_POWER = float.Parse(data[2]);
                    column.GN07N_TEMP = float.Parse(data[3]);
                    column.GN07N_HIGH_TEMP = float.Parse(data[4]);
                    column.GN07N_MID_TEMP = float.Parse(data[5]);
                    column.GN07N_LOW_TEMP = float.Parse(data[6]);
                    column.GN07N_OVER_TEMP = float.Parse(data[7]);
                    column.GN07N_GAS_NRG = float.Parse(data[8]);
                    column.GN07N_GAS_AMM = float.Parse(data[9]);
                    column.GN07N_GAS_CDO = float.Parse(data[10]);
                    column.GN07N_PPIT = float.Parse(data[11]);
                    column.GN05N_MAIN_POWER = float.Parse(data[12]);
                    column.GN05M_MAIN_POWER = float.Parse(data[13]);
                    column.GN05M_TEMP = float.Parse(data[14]);
                    column.GN05M_HIGH_TEMP = float.Parse(data[15]);
                    column.GN05M_LOW_TEMP = float.Parse(data[16]);
                    column.GN05M_OVER_TEMP = float.Parse(data[17]);
                    column.GN05M_GAS_NRG = float.Parse(data[18]);
                    column.GN05M_GAS_AMM = float.Parse(data[19]);
                    column.GN05M_GAS_CDO = float.Parse(data[20]);
                    column.GN04N_MAIN_POWER = float.Parse(data[21]);
                    column.GN04M_MAIN_POWER = float.Parse(data[22]);
                    column.GN04M_TEMP = float.Parse(data[23]);
                    column.GN04M_HIGH_TEMP = float.Parse(data[24]);
                    column.GN04M_MID_TEMP = float.Parse(data[25]);
                    column.GN04M_LOW_TEMP = float.Parse(data[26]);
                    column.GN04M_OVER_TEMP = float.Parse(data[27]);
                    column.GN04M_GAS_NRG = float.Parse(data[28]);
                    column.GN04M_GAS_AMM = float.Parse(data[29]);
                    column.GN04M_GAS_CDO = float.Parse(data[30]);
                    column.GN03N_MAIN_POWER = float.Parse(data[31]);
                    column.GN02N_MAIN_POWER = float.Parse(data[32]);
                    column.GN02N_TEMP = float.Parse(data[33]);
                    column.GN02N_HIGH_TEMP = float.Parse(data[34]);
                    column.GN02N_MID_TEMP = float.Parse(data[35]);
                    column.GN02N_LOW_TEMP = float.Parse(data[36]);
                    column.GN02N_OVER_TEMP = float.Parse(data[37]);
                    column.GN02N_GAS_NRG = float.Parse(data[38]);
                    column.GN02N_GAS_AMM = float.Parse(data[39]);
                    column.GN02N_GAS_CDO = float.Parse(data[40]);
                    column.GN02N_PPIT = float.Parse(data[41]);

                    FactoryData.Add(column);

                }
                return FactoryData;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            return new List<DataColumn>();
        }


    }
}