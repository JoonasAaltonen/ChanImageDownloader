using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChanImageDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DownloaderClient client;
        public MainWindow()
        {
            InitializeComponent();
            client = new DownloaderClient(this);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            client.DownloadHtml(UrlBox.Text);
        }

        public void UpdateText(string text)
        {
            StatusBox.Text = text;
        }

        public void UpdateProgress(int status)
        {
            ProgressBar.Value = status;
        }
    }
}
