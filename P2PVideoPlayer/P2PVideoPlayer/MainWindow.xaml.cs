﻿
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
using System.IO;
using System.Drawing;
using Microsoft.VisualBasic;


namespace WpfApplication1
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private media_info mediaInfo;
        public DanmakuCurtain dmkCurt;
        public int isPlaying = 0; // 0: no pause 1: pause
      

        public System.Windows.Forms.PictureBox image_control;
        BitmapPlayer player;
        Client client;

        public MainWindow()
        {
            InitializeComponent();
            dmkCurt = new DanmakuCurtain();
        }


        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (player != null) { }
            else { player = new BitmapPlayer(ref image_control, ref form_container, ref client); }
            if (selector.SelectedItem != null)
            {
                if (image_control != null) { }
                else
                {
                    image_control = new System.Windows.Forms.PictureBox();
                    form_container.Child = image_control;

                }

                if (player != null) { }
                else { player = new BitmapPlayer(ref image_control, ref form_container, ref client); }

                //AviManager aviManager = new AviManager(@"C:\Users\yxing2\Downloads\SHE_uncompressed.avi", true);
                //AudioStream audioStream = aviManager.GetWaveStream();
                //audioStream.ExportStream("./audio.wav");
                //aviManager.Close();

                player.setLocalInfo(selector.SelectedItem.ToString(), "wmv");

                //player.setLocalInfo(@"C:\Users\yxing2\Downloads\SHE_uncompressed.avi", "wmv");

                //player.setLocalInfo(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv", "wmv");
                player.play();
            }
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            player.pause();
            isPlaying = 1; // paused
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {

            player.stop();
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            if(selector.SelectedIndex-1 >= 0)
                selector.SelectedIndex -= 1;
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            if(selector.SelectedIndex + 1 < selector.Items.Count)
                selector.SelectedIndex += 1;
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
             if (mediaInfo == null)
             {
                 MessageBox.Show("Please load the list first");
                 return;
             }
            string path;
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = "VIDEO File|*";
            Nullable<bool> result = dlg.ShowDialog(Window.GetWindow(this));
            if (result == true)
            {
                //first add file and its name
                foreach (String filename in dlg.FileNames)
                {
                    path = dlg.FileName;
                    mediaInfo.add(path);
                }
            }

            
            List<String> plat_list = mediaInfo.print();
            selector.Items.Clear();
            for (int i = 0; i < plat_list.Count; i++)
            {
                selector.Items.Add(plat_list[i]);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Database File |*.xml";
            Nullable<bool> result = file.ShowDialog();
            if (result==true)
            {
                path = file.FileName;
                mediaInfo = new media_info(path);
                List<String> plat_list = mediaInfo.print();
                selector.Items.Clear();
                for (int i = 0; i < plat_list.Count; i++)
                {
                    selector.Items.Add(plat_list[i]);
                }
                selector.SelectedIndex = -1;
               
                ((Button)sender).Visibility = Visibility.Hidden;
                    
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
         {
             Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
             {
                 dmkCurt.Shoot(curtain,message.Text);
             }));
         }

        
        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Database File|*.xml";
            sfd.Title = "Create database file";
            sfd.FileName = "mylist";
            sfd.DefaultExt = ".xml";
            
            if(sfd.ShowDialog() == true)
            {
                String xmltext = "<?xml version=\"1.0\"?>\n"
                            + "<Karaoke xmlns:kara=\"list\">\n"
                            + "</Karaoke>";
                using (Stream output = sfd.OpenFile())
                {
                    if(output != null)
                    {
                        output.Write(Encoding.ASCII.GetBytes(xmltext), 0, xmltext.Length);
                    }
                }
            }

        }
        
        //add new play source
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selector.SelectedItem == null) return;
            mediaInfo.select_play(selector.SelectedItem.ToString());
            //media.Source = new Uri(mediaInfo.currentPlay.path, UriKind.RelativeOrAbsolute);
            //media.ScrubbingEnabled = true;

            ////cut the avi file into small clips
            //AVIStreamReader test = new AVIStreamReader(mediaInfo.currentPlay.path);
            //string temp1 = "./videoClips/";
            //string temp3 = ".avi";
            //string temp2;
            //string clipName;
            //int reachEnd = 0;
            //totalClip = 0;
            //for (int i = 0; reachEnd != 1; i++)
            //{
            //    temp2 = i.ToString();
            //    clipName = temp1 + temp2 + temp3;
            //    reachEnd = test.readStream(clipName);
            //    totalClip++;
            //}
            

            ////the following code can safely extract audio file
            ////Avi文件读取
            //AviManager aviManager = new AviManager(mediaInfo.currentPlay.path, true);
            ////获取和保存音频流到文件
            //AudioStream audioStream = aviManager.GetWaveStream();
            //audioStream.ExportStream("./audio.wav");
            //aviManager.Close();

            //isPlaying = 1;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem here = (MenuItem)sender;
            
            int current = selector.SelectedIndex;
            switch ( here.Header.ToString())
            {
                case "Delete":
                    player.stop();
                    mediaInfo.delete();
                    selector.Items.Remove(selector.SelectedItem);

                    break;
                case "Property":
                    MessageBox.Show(mediaInfo.currentPlay.print());
                    break;
            }
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            client = new ClientServer();
            client.run();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //title = InputBox("What is the title of the music?",fileName+": Title","N/A");
            string ser_ip = Interaction.InputBox("Input Server IP Address:","IP Address:","N/A");
            client = new ClientOnly(ser_ip);
            client.run();
        }
    }
}
