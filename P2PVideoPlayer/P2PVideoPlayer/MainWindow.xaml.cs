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
        public int reachEnd = 0; // 0: video stream not reach its end; 1: stream reach its end

        public MainWindow()
        {
            InitializeComponent();
            dmkCurt = new DanmakuCurtain();
            media.LoadedBehavior = MediaState.Manual;
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (selector.SelectedItem != null)
            {
                if (isPlaying == 0)
                {
                    media.LoadedBehavior = MediaState.Manual;

                    media.Source = new Uri(mediaInfo.currentPlay.path, UriKind.RelativeOrAbsolute);
                    media.ScrubbingEnabled = true;
                    AVIStreamReader test = new AVIStreamReader(mediaInfo.currentPlay.path);
                    reachEnd = test.readStream("./temp1.avi");
                    if (reachEnd == 1)
                    {
                        media.Source = new Uri("./temp1.avi", UriKind.RelativeOrAbsolute);
                        media.Play();
                        return;
                    }
                    reachEnd = test.readStream("./temp2.avi");
                    if (reachEnd == 1)
                    {
                        media.Source = new Uri("./temp1.avi", UriKind.RelativeOrAbsolute);
                        media.Play();
                        media.Source = new Uri("./temp2.avi", UriKind.RelativeOrAbsolute);
                        media.Play();
                        return;
                    }
                    media.Source = new Uri("./temp1.avi", UriKind.RelativeOrAbsolute);
                    media.Play();
                    int flag = 2;
                    while (reachEnd == 0)
                    {
                        if (flag == 2)
                        {
                            reachEnd = test.readStream("./temp1.avi");
                            media.Source = new Uri("./temp2.avi", UriKind.RelativeOrAbsolute);
                            media.Play();
                            flag = 1;
                        }
                        if (reachEnd == 1)
                        {
                            media.Source = new Uri("./temp1.avi", UriKind.RelativeOrAbsolute);
                            media.Play();
                            break;
                        }
                        if (flag == 1)
                        {
                            reachEnd = test.readStream("./temp2.avi");
                            media.Source = new Uri("./temp1.avi", UriKind.RelativeOrAbsolute);
                            media.Play();
                            flag = 2;
                        }
                        if (reachEnd == 1)
                        {
                            media.Source = new Uri("./temp2.avi", UriKind.RelativeOrAbsolute);
                            media.Play();
                            break;
                        }
                    }
                }
                else if (isPlaying == 1)
                {
                    media.Play();
                }
            }
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            media.Pause();
            isPlaying = 1; // paused
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            
            media.Stop();
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            if(selector.SelectedIndex-1 >= 0)
                selector.SelectedIndex -= 1;
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
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
        

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selector.SelectedItem == null) return;
            mediaInfo.select_play(selector.SelectedItem.ToString());
            media.Source = new Uri(mediaInfo.currentPlay.path, UriKind.RelativeOrAbsolute);
            media.ScrubbingEnabled = true;

            //AVIStreamReader test = new AVIStreamReader(mediaInfo.currentPlay.path);
            //int totalClips = (int)(test.totalFrameNum / test.fps + 1);
            //string temp1 = "./videoClips/";
            //string temp3 = ".avi";
            //string temp2;
            //string clipName;
            //for (int i = 0; i < totalClips; i++)
            //{
            //    temp2 = i.ToString();
            //    clipName = temp1 + temp2 + temp3;
            //    reachEnd = test.readStream(clipName);
            //}

            //Avi文件读取
            AviManager aviManager = new AviManager(mediaInfo.currentPlay.path, true);
            VideoStream aviStream = aviManager.GetVideoStream();

            //获取和保存音频流到文件
            AudioStream audioStream = aviManager.GetWaveStream();
            audioStream.ExportStream("./audio.wav");

            aviStream.GetFrameOpen();

            //获取视频总帧数
            int framecount = aviStream.CountFrames;
            //获取第5帧的图片
            //Bitmap bmp = aviStream.GetBitmap(5);
            //视频速度
            double fps = aviStream.FrameRate;
            //直接保存帧图片到文件
            //aviStream.ExportBitmap(26, "./videoClips/26.jpg");
            //aviStream.ExportBitmap(27, "./videoClips/27.jpg");
            //aviStream.ExportBitmap(28, "./videoClips/28.jpg");
            //aviStream.ExportBitmap(29, "./videoClips/29.jpg");

            //int totalClips = (int)(framecount / fps + 1);
            //string temp1 = "./videoClips/";
            //string temp3 = ".avi";
            //string temp2 = "0";
            //string clipName;
            ////temp2 = i.ToString();
            //clipName = temp1 + temp2 + temp3;
            ////write a clip
            //AviManager aviManagerWriter = new AviManager(clipName, false);
            //Bitmap bmp = aviStream.GetBitmap(0);
            //VideoStream aviStreamWriter = aviManagerWriter.AddVideoStream(true, fps, bmp);
            //for (int i = 1; i < fps; i++)
            //{
            //    bmp = aviStream.GetBitmap(i);
            //    aviStreamWriter.AddFrame(bmp);
            //}
            //aviManagerWriter.Close();

            aviStream.GetFrameClose();
            aviManager.Close();


            //isPlaying = 1;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem here = (MenuItem)sender;
            
            int current = selector.SelectedIndex;
            switch ( here.Header.ToString())
            {
                case "Delete":
                    media.Close();
                    mediaInfo.delete();
                    selector.Items.Remove(selector.SelectedItem);

                    break;
                case "Property":
                    MessageBox.Show(mediaInfo.currentPlay.print());
                    break;
            }
            
        }
    }
}
