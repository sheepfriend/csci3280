using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private media_info mediaInfo;
        public MainWindow()
        {
            InitializeComponent();
            mediaInfo = new media_info("N:/yxing2.V2/Downloads/WpfApplication1/csci3280/WpfApplication1/list.txt");
            textBlock.Text = mediaInfo.print();
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            media.LoadedBehavior = MediaState.Manual;
            media.Source = new Uri(mediaInfo.currentPlay, UriKind.RelativeOrAbsolute);
            media.ScrubbingEnabled = true;
            media.Play();
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            media.Pause();
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            mediaInfo.pre();
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            mediaInfo.next();
        }
        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            string path;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog(); 
            if (result == true)
            {
                path = dlg.FileName;
                mediaInfo.add(path);
            }
            textBlock.Text = mediaInfo.print();
        }
    }
}
