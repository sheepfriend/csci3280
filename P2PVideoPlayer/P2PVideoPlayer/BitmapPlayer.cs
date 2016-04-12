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
        const int buffer_num = 4;
        public PictureBox image_control;
        public WindowsFormsHost form_container;

        public static BitmapReader reader;
        public static List<BitmapStream> bitmap_stream;
        public Bitmap bitmap;
        public System.Timers.Timer timer;
        public int bmpPerStream;
        public static String current_type;
        public Thread loadStream;
        public static String address;
        public static bool isLocal;
        public int isPaused;
        public static List<String> header;
        public static int countBitmap;
        public static int countFrame;
        public Client client;
        public static int finish;
        public static int bitmapPerSec;
        public static int start;
        public static WpfApplication1.MainWindow window;
        //需要改三个部件的大小
        public BitmapPlayer(ref PictureBox input, ref WindowsFormsHost form, ref Client client_, ref WpfApplication1.MainWindow wind)
        {
            image_control = input;
            form_container = form;
            bmpPerStream = 1;
            window = wind;
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



        public void setLocalInfo(String addr, String type)
        {
            address = addr;
            current_type = type;
            finish = 0;
            if (Local.exist(addr)) { isLocal = true; }
            else { isLocal = false; }
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
                window.Height = 710 > Int32.Parse(header[1]) + 300 ? 710 : Int32.Parse(header[1]) + 300;
                window.Width = 820 > Int32.Parse(header[2]) + 300 ? 820 : Int32.Parse(header[2]) + 300;

                // window.Height = form_container.Height + 50;
                // window.Width = form_container.Width + 50;

                //给第一个留时间缓冲,等待缓存
                start = 0;
                while (bitmap_stream.Count < 2) { Thread.Sleep(300); }
                start = 1;
            }
            else
            {
                Console.Out.WriteLine("not local video");
                isLocal = false;
                header = client.askVideoHeader(address);

                if (header != null && header.Count > 0)
                {
                    //设置同步
                    //reader.bmpPerStream = Int32.Parse(header[0]);

                    loadStream = new Thread(loadBitmapStreamP2P);
                    loadStream.Start();

                    //调整窗口大小
                    form_container.Height = Int32.Parse(header[1]);
                    form_container.Width = Int32.Parse(header[2]);
                    window.Height = 710 > Int32.Parse(header[1]) + 300 ? 710 : Int32.Parse(header[1]) + 300;
                    window.Width = 820 > Int32.Parse(header[2]) + 300 ? 820 : Int32.Parse(header[2]) + 300;

                    //给第一个留时间缓冲
                    start = 0;
                    while (bitmap_stream.Count < 2) { Thread.Sleep(200); }
                    start = 1;

                }
            }
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
            //重新开始
            /*
             * reader读取载入的文件 名 
             * 不是本地文件：header = null （后面没写完呢）
             * 是本地文件：header里有数据（长宽和播放速度，或者有需要还可以往里扔）
             */
            isPaused = 0;
            if (header != null)
            {
                //header (List<String>): 
                // [0] frameRate
                // [1] Height
                // [2] Width
                timer = new System.Timers.Timer(810 / Int32.Parse(header[0]));
                bitmapPerSec = Int32.Parse(header[0]);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(playNextBitmap);
                timer.Enabled = true;
            }

        }




        public void playNextBitmap(object source, System.Timers.ElapsedEventArgs e)
        {

        //try
        //{
        //bitmap_stream is a list
        //contains streams of bitmap
        //List<BitmapStream>
        start:
            while (bitmap_stream.Count <= 0) { Thread.Sleep(300); }

            bitmap = bitmap_stream[0].read();

            //播完了
            if (bitmap != null) { }
            else if (reader.finish == 1 && bitmap == null)
            {
                BitmapPlayer.finish = 1;
                Console.Out.WriteLine("over");
                timer.Enabled = false;
                timer.Dispose();
                return;
            }
            else
            {
                //只是bitmap_stream[0]读完了
                //load到bitmap_stream[1]了
                //把bitmap_stream[0]扔了
                //bitmap_stream[1]自动变成了第一个
                Console.Out.WriteLine("first stream over");
                bitmap_stream.RemoveAt(0);
                goto start;
            }
            InvokeHelper.Set(image_control, "Image", bitmap);
            countFrame++;


        }

        public void loadBitmapStream()
        {
            //“后台”线程，如果bitmap_stream的长度小于2，就读新的BitmapStream
            //两个BitmapStream的时候这个function就卡住了
            while (true)
            {
                //load 4 buffers
                while (bitmap_stream.Count >= buffer_num)
                {
                    Thread.Sleep(200);
                    if (reader.finish == 1) { return; }
                }
                Console.Out.WriteLine("loading buffer...");
                BitmapStream stream_tmp = new BitmapStream();
                //reader的loadBitmapStream是读取bitmapstream
                //本地不用考虑异步、大家read的不对齐的情况，参数直接设-1
                stream_tmp = reader.loadBitmapStream_count(-1);
                bitmap_stream.Add(stream_tmp);
                countBitmap++;
            }
        }

        public void loadBitmapStreamP2P()
        {
            while (true)
            {
                while (bitmap_stream.Count >= buffer_num)
                {
                    Thread.Sleep(200);
                }
                Console.Out.WriteLine("loading buffer...");
                BitmapStream stream_tmp = new BitmapStream();
                stream_tmp = client.askBitmapStream(countBitmap);
                for (int i = 0; i < stream_tmp.stream.Count; i++)
                {
                    if (stream_tmp.stream[i] == null)
                    {
                        reader.finish = 1;
                        countBitmap++;
                        bitmap_stream.Add(stream_tmp);
                        return;
                    }
                }
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
            countFrame = 0;
            start = 0;
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
