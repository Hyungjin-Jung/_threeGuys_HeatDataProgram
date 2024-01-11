using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace _threeGuys_HeatDataProgram.Views.Pages
{
    /// <summary>
    /// Interaction logic for _2_DetailsPage.xaml
    /// </summary>
    public partial class _2_DetailsPage : Page
    {
        private bool isWebPageConnected = false;

        string WebIPAddress = "127.0.0.1";
        string WebPortNumber = "8050";
        public _2_DetailsPage()
        {
            InitializeComponent();
        }

        private void button_ChangeWebPageIP_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            {
                // 텍스트 박스에 입력된 텍스트를 가져옴
                string plcIP = textBox_WebIP.Text;
                // Split
                string[] plcIP_list = plcIP.Split(":");

                WebIPAddress = plcIP_list[0];
                WebPortNumber = plcIP_list[1];

                textBox_WebIP.PlaceholderText = $"현재 : {WebIPAddress}:{WebPortNumber}";
                textBox_WebIP.Text = "";

                webView2_tab1.Source = new Uri($"http://{WebIPAddress}:{WebPortNumber}");
            }
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                button_ChangeWebPageIP_Click(sender,e);
            }
        }
    }
}