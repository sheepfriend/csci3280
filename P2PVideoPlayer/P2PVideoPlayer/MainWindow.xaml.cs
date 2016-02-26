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
            selector.SelectedIndex = mediaInfo.currentNum;
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
            mediaInfo.next();
            selector.SelectedIndex = mediaInfo.currentNum;
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
            selector.Items.MoveCurrentTo(mediaInfo.currentNum);
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
                    
            List<String> plat_list = mediaInfo.print();
            selector.Items.Clear();
            for (int i = 0; i < plat_list.Count; i++)
            {
                selector.Items.Add(plat_list[i]);
            }
            selector.Items.MoveCurrentTo(mediaInfo.currentNum);
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
            mediaInfo.select_play((String)selector.Items.CurrentItem);

            media.LoadedBehavior = MediaState.Manual;

            media.Source = new Uri(mediaInfo.currentPlay, UriKind.RelativeOrAbsolute);
            media.ScrubbingEnabled = true;
            media.Play();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem here = (MenuItem)sender;
            int current = mediaInfo.currentNum;
            switch ( here.Header.ToString())
            {
                case "Delete":

                    break;
                case "Property":
                    MessageBox.Show(mediaInfo.playList[current].print());
                    break;
            }
            
        }
    }
}
