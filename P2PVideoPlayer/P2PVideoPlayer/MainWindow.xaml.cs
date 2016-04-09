using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Controls;
using AForge.Video;
using AForge;
using AForge.Controls;
using AForge.Video.FFMPEG;
using AForge.Video.VFW;
using AForge.Video.DirectShow;

namespace WpfApplication4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapPlayer player;
        Client client;
        DanmakuCurtain dmkCurt;
        DanmuPlayer danmuPlayer;
        WaveOutPlayer audioPlayer;

        public System.Windows.Forms.PictureBox image_control;
        public Grid curtain;

        public MainWindow()
        {
            InitializeComponent();
            //invokehelper用的picturebox要单独加上
            image_control = new System.Windows.Forms.PictureBox();
            form_container.Child = image_control;
            image_control.SendToBack();
            curtain = new Grid();
            curtain_contain.Children.Add(curtain);
            dmkCurt = new DanmakuCurtain();
            Console.Out.WriteLine("starting");
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //AviManager aviManager = new AviManager(@"C:\Users\yxing2\Downloads\SHE_uncompressed.avi", true);
            //AudioStream audioStream = aviManager.GetWaveStream();
            //audioStream.ExportStream("./audio.wav");
            //aviManager.Close();

            player.setLocalInfo(@"SHE_uncompressed.avi", "wmv");
            player.play();
            danmuPlayer.playDanmu();
            audioPlayer.setLocalInfo(@"SHE_uncompressed.avi");
            audioPlayer.play();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (player.isPaused == 1)
            {
                player.play();
                audioPlayer.play();
                ((System.Windows.Controls.Button)sender).Content = "pause";
            }
            else
            {
                player.pause();
                audioPlayer.pause();
                ((System.Windows.Controls.Button)sender).Content = "play";
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            audioPlayer.stop();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            client = new ClientServer();
            client.run();
            if (player == null)
            {
                danmuPlayer = new DanmuPlayer(ref dmkCurt, ref client, ref curtain);
                player = new BitmapPlayer(ref image_control, ref form_container, ref client);
                audioPlayer = new WaveOutPlayer(ref client);
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Connector a = new Connector("192.168.110.31", 9999, 10000);
            //a.connect();
            client = new ClientOnly();
            client.run();
            if (player == null)
            {
                danmuPlayer = new DanmuPlayer(ref dmkCurt, ref client, ref curtain);
                player = new BitmapPlayer(ref image_control, ref form_container, ref client);
                audioPlayer = new WaveOutPlayer(ref client);
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (!BitmapPlayer.isLocal) {
                danmuPlayer.addDanmu(message.Text);
            }
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                dmkCurt.Shoot(curtain, message.Text);
            }));
            
        }


        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //向所有人发送这个人已经退出的消息（如果设置了是server还是client的话，本地直接退出）
            if (client != null)
            {
                Client.leave();
            }

            //视屏神马的有播放
            if (player != null)
            {
                if (player.loadStream.IsAlive) { player.loadStream.Abort(); }
                if (player.timer.Enabled){player.timer.Dispose();}
                if (danmuPlayer.play.IsAlive) { danmuPlayer.play.Abort(); }
            }
            Environment.Exit(0);
        }
        
    }
}
