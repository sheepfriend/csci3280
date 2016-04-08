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
using System.Windows.Forms.Integration;
using System.Windows.Controls;
using AForge.Video;
using AForge;
using AForge.Video.FFMPEG;
using AForge.Video.VFW;

namespace WpfApplication1
{

    class BitmapPlayer
    {
        public PictureBox image_control;
        public WindowsFormsHost form_container;

        //需要改三个部件的大小
        public BitmapPlayer(ref PictureBox input, ref WindowsFormsHost form,ref Client client_)
        {
            image_control = input;
            form_container = form;
            bmpPerStream = 1;
            Console.Out.WriteLine("init...?");
            reader = new BitmapReader();
            Console.Out.WriteLine("init...");
            bitmap_stream = new List<BitmapStream>();
            current_type = "";
            isPaused = 0;
            header = new List<string>();
            isLocal = true;
            client = client_;
            //client.run();
        }

        public static BitmapReader reader;
        public static List<BitmapStream> bitmap_stream;
        public Bitmap bitmap;
        public System.Timers.Timer timer;
        public int bmpPerStream;
        public String current_type;
        public Thread loadStream;
        public String address;
        public static bool isLocal;
        public int isPaused;
        public static List<String> header;
        public static int countBitmap;
        public Client client;

        public void setLocalInfo(String addr, String type)
        {
            address = addr;
            current_type = type;
        }

        /*
         * 播放视频主程序
         * 要先setLocalInfo
         * 用了两个额外的线程
         * playNextBitmap:在timer里面，定时换一次UI里面的图片
         * loadBitmapStream：不断监视bitmap_stream(List<BitmapStream>)的线程，空了就往里扔BitmapStream
         */
        public void loadVideo()
        {

            if (isPaused == 0)
            {
                //重新开始
                /*
                 * reader读取载入的文件 名 
                 * 不是本地文件：header = null （后面没写完呢）
                 * 是本地文件：header里有数据（长宽和播放速度，或者有需要还可以往里扔）
                 */
                header = reader.loadFile(address, current_type);
                if (header != null)
                {
                    Console.Out.WriteLine("local video");
                    loadStream = new Thread(loadBitmapStream);
                    loadStream.Start();

                    isLocal = true;

                    //本地文件                    
                    //调整窗口大小
                    form_container.Height = Int32.Parse(header[1]);
                    form_container.Width = Int32.Parse(header[2]);

                    // window.Height = form_container.Height + 50;
                    // window.Width = form_container.Width + 50;

                    //给第一个留时间缓冲
                    while (bitmap_stream.Count < 1) { }

                    //header (List<String>): 
                    // [0] frameRate
                    // [1] Height
                    // [2] Width
                    timer = new System.Timers.Timer(1000 / Int32.Parse(header[0]));
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(playNextBitmap);
                    timer.Enabled = true;
                }
                else
                {
                    Console.Out.WriteLine("not local video");
                    isLocal = false;
                    header = client.askVideoHeader(address);

                    
                    //header非空：有这个视频
                    //这个function有延迟，要等所有人回复
                    if (header != null)
                    {
                        loadStream = new Thread(loadBitmapStreamP2P);
                        loadStream.Start();

                        //调整窗口大小
                        form_container.Height = Int32.Parse(header[1]);
                        form_container.Width = Int32.Parse(header[2]);

                        //给第一个留时间缓冲
                        while (bitmap_stream.Count == 0) { }

                        //header (List<String>): 
                        // [0] frameRate
                        // [1] Height
                        // [2] Width
                        timer = new System.Timers.Timer(1000 / Int32.Parse(header[0]));
                        timer.Elapsed += new System.Timers.ElapsedEventHandler(playNextBitmap);
                        timer.Enabled = true;

                    }
                }
            }
            else
            {
                //暂停继续
                isPaused = 0;

                //while (bitmap_stream.Count == 0) { }
                timer = new System.Timers.Timer(1000 / Int32.Parse(header[0]));
                timer.Elapsed += new System.Timers.ElapsedEventHandler(playNextBitmap);
                timer.Enabled = true;
            }

        }


        public void playNextBitmap(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //bitmap_stream is a list
                //contains streams of bitmap
                //List<BitmapStream>
                bitmap = bitmap_stream[0].read();

                //播完了
                if (bitmap != null){}
                else if (reader.finish == 1 && bitmap_stream.Count == 1)
                {
                    Console.Out.WriteLine("over");
                    timer.Enabled = false;
                    timer.Dispose();
                    return;
                }
                else if (bitmap_stream.Count >= 2)
                {
                    //只是bitmap_stream[0]读完了
                    //load到bitmap_stream[1]了
                    //把bitmap_stream[0]扔了
                    //bitmap_stream[1]自动变成了第一个
                    Console.Out.WriteLine("first stream over");
                    bitmap_stream.RemoveAt(0);
                    bitmap = bitmap_stream[0].read();
                }
                InvokeHelper.Set(image_control, "Image", bitmap);
            }
            catch
            {
                //timer.Dispose();
            }
        }

        public void loadBitmapStream()
        {
            //“后台”线程，如果bitmap_stream的长度小于2，就读新的BitmapStream
            //两个BitmapStream的时候这个function就卡住了
            while (true)
            {
                while (bitmap_stream.Count > 3)
                {
                    if (reader.finish == 1) { return; }
                }
                Console.Out.WriteLine("loading buffer...");
                BitmapStream stream_tmp = new BitmapStream();
                //reader的loadBitmapStream是读取bitmapstream
                //本地不用考虑异步、大家read的不对齐的情况，参数直接设-1
                stream_tmp = reader.loadBitmapStream_count(-1);
                bitmap_stream.Add(stream_tmp);
            }
        }

        public void loadBitmapStreamP2P()
        {
            while (true)
            {
                while (bitmap_stream.Count > 3)
                {
                    if (reader.finish == 1) { return; }
                }
                Console.Out.WriteLine("loading buffer...");
                BitmapStream stream_tmp = new BitmapStream();
                stream_tmp = client.askBitmapStream(countBitmap);
                countBitmap++;
                bitmap_stream.Add(stream_tmp);
            }
        }

        //给UI事件用的那些
        public void stop()
        {
            timer.Dispose();
            loadStream.Abort();
            reader.flush();
            bitmap_stream = new List<BitmapStream>();
            bitmap = null;
            current_type = "";
            countBitmap = 0;
        }

        public void pause()
        {
            timer.Dispose();
            isPaused = 1;
        }

        public void play()
        {
            loadVideo();
        }

    }
}
