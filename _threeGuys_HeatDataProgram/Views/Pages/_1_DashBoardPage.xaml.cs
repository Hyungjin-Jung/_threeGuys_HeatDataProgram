using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _threeGuys_HeatDataProgram.Views.Pages
{
    /// <summary>
    /// Interaction logic for _1_DashBoardPage.xaml
    /// </summary>
    public partial class _1_DashBoardPage : Page
    {
        public _1_DashBoardPage()
        {
            InitializeComponent();
            listBox_Notice.Items.Add("test");
        }

        private void listBoxNoticeMouseDouble_Click(object sender, MouseButtonEventArgs e)
        {

        }

        // 공장 버튼 눌렀을떄 1번 탭 활성화
        private void DisplayFirstAreaTab_Click(object sender, RoutedEventArgs e)
        {
            //tabControl.SelectedIndex = 1;
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

    }
}
