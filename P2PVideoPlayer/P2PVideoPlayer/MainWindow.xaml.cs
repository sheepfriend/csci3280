
using Microsoft.Win32;
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
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private media_info mediaInfo;
        public DanmakuCurtain dmkCurt;

        public MainWindow()
        {
            InitializeComponent();
            dmkCurt = new DanmakuCurtain();
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
            // if (mediaInfo == null)
            // {
                MessageBox.Show("Please load the list first");
                // return;
            // }
            string path;
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "VIDEO File|*";
            Nullable<bool> result = dlg.ShowDialog(Window.GetWindow(this));
            if (result == true)
            {
                path = dlg.FileName;
                mediaInfo.add(path);
            }
            textBlock.Text = mediaInfo.print();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Database File |*.xml";
            Nullable<bool> result = file.ShowDialog();
            if (result==true)
            {

                    path = file.FileName;
                    mediaInfo = new media_info(path);
                    textBlock.Text = mediaInfo.print();
                    Button bt = (Button)sender;
                    bt.Visibility = Visibility.Hidden;
                    
            }
        }
        private void send_Click(object sender, RoutedEventArgs e)
         {
             Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
             {
                 dmkCurt.Shoot(curtain,message.Text);
             }));
         }
    }
}
