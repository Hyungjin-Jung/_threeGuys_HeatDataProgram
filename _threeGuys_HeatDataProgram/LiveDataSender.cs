using System.IO;


namespace _threeGuys_HeatDataProgram
{


    public class LiveDataSender
    {
        string heatTreatingDatafilePath = Directory.GetCurrentDirectory() + "/heatTreatingFactoryData.csv";
        string heatDataAlarmFilterfilePath = Directory.GetCurrentDirectory() + "/HeatDataAlarmFilter.csv";

        // todo
        //  event Action<int> DataUpdated: 데이터 업데이트 이벤트
        //  List<Page1>, List<Page2>, List<Page3>, List<Page4>: 각 페이지를 저장하는 리스트
        //  void AddPage(Page1 page), void AddPage(Page2 page), ... : 페이지를 리스트에 추가하는 메서드
        //  void StartDataUpdate(): 데이터 주기적 업데이트를 시작하는 메서드
        //  private void UpdateData(int newData): 데이터를 각 페이지에 전달하고 이벤트를 발생시키는 메서드
    }
}
