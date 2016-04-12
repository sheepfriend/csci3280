
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
        public Thread waitToFinish;

        BitmapPlayer player;
        Client client;
        DanmuPlayer danmuPlayer;
        WaveOutPlayer audioPlayer;

        public System.Windows.Forms.PictureBox image_control;
        //public Grid curtain;


        public MainWindow()
        {

            InitializeComponent();
            image_control = new System.Windows.Forms.PictureBox();
            form_container.Child = image_control;
            image_control.SendToBack();
            Local.ref_addr = Directory.GetCurrentDirectory()+"\\";
            if (!Directory.Exists("danmu"))
            {
                Directory.CreateDirectory("danmu");
            }
            if (!Directory.Exists("audio"))
            {
                Directory.CreateDirectory("audio");
            }
            dmkCurt = new DanmakuCurtain();
            Console.Out.WriteLine("starting");

        }


        public void wait_to_finish()
        {
            while (true)
            {
                if (BitmapPlayer.finish == 1)
                {
                    player.stop();
                    try
                    {
                        audioPlayer.stop();

                    }
                    catch { }
                    try
                    {
                        danmuPlayer.stop();
                    }
                    catch { }
                    return;
                }
            }
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (selector.SelectedItem != null)
            {
                if (image_control != null) { }
                else
                {
                    image_control = new System.Windows.Forms.PictureBox();
                    form_container.Child = image_control;
                    
                    image_control.SendToBack();
                }

                if (player == null) { player = new BitmapPlayer(ref image_control, ref form_container, ref client,ref window); }
                if (audioPlayer == null)  { audioPlayer = new WaveOutPlayer(ref client); }
                if (danmuPlayer == null)
                {
                    danmuPlayer = new DanmuPlayer(ref dmkCurt, ref client, ref curtain);
                }
                //AviManager aviManager = new AviManager(@"C:\Users\yxing2\Downloads\SHE_uncompressed.avi", true);
                //AudioStream audioStream = aviManager.GetWaveStream();
                //audioStream.ExportStream("./audio.wav");
                //aviManager.Close();

                String filename = selector.SelectedItem.ToString();
                String filepath = mediaInfo.name_to_list[filename].path;

                waitToFinish = new Thread(wait_to_finish);
                bool hasAudio = true;

                if (player.isPaused == 0)
                {
                    player.setLocalInfo(filepath, "wmv");
                    if (BitmapPlayer.header == null || BitmapPlayer.header.Count == 0)
                    {
                        return;
                    }
                    try
                    {
                        audioPlayer.setLocalInfo("audio\\" + filename + ".wav");
                    }
                    catch
                    {
                        Console.Out.WriteLine("Fail to load audio");
                        hasAudio = false;
                    }
                    player.play();
                    if (hasAudio)
                    {
                        audioPlayer.play();
                    }
                    danmuPlayer.playDanmu();
                    waitToFinish.Start();
                }



                //player.setLocalInfo(@"C:\Users\yxing2\Downloads\SHE_uncompressed.avi", "wmv");

                //player.setLocalInfo(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv", "wmv");            
                if (player.isPaused == 1)
                {
                    player.play();

                }
                
                //danmuPlayer.playDanmu();

            }
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            player.pause();
            //audioPlayer.pause();
            isPlaying = 1; // paused
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {

            player.stop();
            audioPlayer.stop();
            if(waitToFinish.IsAlive){
                waitToFinish.Abort();
            }
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            audioPlayer.stop();
            danmuPlayer.stop();
            if(selector.SelectedIndex-1 >= 0)
                selector.SelectedIndex -= 1;
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            audioPlayer.stop();
            danmuPlayer.stop();
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

            Utils.general_add(mediaInfo.print(),selector);

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
                HashSet<String> plat_list = mediaInfo.print();
                Utils.general_add(plat_list, selector);
                selector.SelectedIndex = -1;
               
                ((Button)sender).Visibility = Visibility.Hidden;
                    
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
         {

             danmuPlayer.addDanmu(message.Text);
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
            client = new ClientServer(ref mediaInfo);
            client.run();
            if (player == null)
            {
                danmuPlayer = new DanmuPlayer(ref dmkCurt, ref client, ref curtain);
                player = new BitmapPlayer(ref image_control, ref form_container, ref client,ref window);
                audioPlayer = new WaveOutPlayer(ref client);
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //title = InputBox("What is the title of the music?",fileName+": Title","N/A");
            string ser_ip = Interaction.InputBox("Input Server IP Address:","IP Address:","N/A");
            client = new ClientOnly(ser_ip, ref mediaInfo);
            client.run();
            if (player == null)
            {
                danmuPlayer = new DanmuPlayer(ref dmkCurt, ref client, ref curtain);
                player = new BitmapPlayer(ref image_control, ref form_container, ref client, ref window);
                audioPlayer = new WaveOutPlayer(ref client);
            }
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
                if (player.loadStream!=null && player.loadStream.IsAlive) { player.loadStream.Abort(); }
                if (player.timer != null && player.timer.Enabled) { player.timer.Dispose(); }
                if (danmuPlayer != null && danmuPlayer.play != null && danmuPlayer.play.IsAlive) { danmuPlayer.play.Abort(); }
                if (audioPlayer != null && audioPlayer.load_waveoutstream != null && audioPlayer.load_waveoutstream.IsAlive) { audioPlayer.load_waveoutstream.Abort(); }
                if (audioPlayer != null && audioPlayer.load_audio != null && audioPlayer.load_audio.IsAlive) { audioPlayer.load_audio.Abort(); }
            }
            Environment.Exit(0);
        }

        private void ppm_test_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Please connect to server first!");
            }
            else
            {
                String filename = Interaction.InputBox("Input ppm file name (include .ppm)", "filename:", "N/A");
                client.askPMM(filename);
            }
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            String key = SearchBox.Text;
            List<video_info> result = Utils.search_list(key, mediaInfo);
            HashSet<String> result_name = new HashSet<string>();
            //List<String> plat_list = mediaInfo.print();
            //selector.Items.Clear();
            //for (int i = 0; i < plat_list.Count; i++)
            //{
            //    selector.Items.Add(plat_list[i]);
            //}
            //List<String> result_name = new List<string>(result.Count + 5); 
            if (result.Count > 0)
            {
                //本地就有
                //之后怎么搞？ --> 加入结果队列咯=。=
                foreach( var _ in result )
                {
                    result_name.Add(_.fileName);
                }
                Utils.general_add(result_name, selector);
            }
            //本地没有，向别人请求
            else
            {
                if (client == null)
                {
                    //没有和别人连接
                    MessageBox.Show("Please connect to server first!");
                }
                else
                {
                    List<List<video_info>> result_from_others = client.search_key(key);
                    //返回结果：list<某个client的搜索结果>
                    //之后怎么搞？
                    foreach (var list in result_from_others)
                    {
                        foreach (var video in list)
                        {
                            if (!result_name.Contains(video.fileName))
                            {
                                mediaInfo.name_to_list.Add(video.fileName, video);
                                result_name.Add(video.fileName);
                            }
                        }
                    }
                    Utils.general_add(result_name, selector);
                }

            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Utils.general_add(mediaInfo.print(), selector);
        }

    }
}
