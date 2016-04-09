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
    class BitmapReader
    {
        //需要改三个部件的大小
        public BitmapReader()
        {
            bmpPerStream = 2;
            video_wmv = new VideoFileReader();
            //video_avi = new AVIReader();
            bitmap_stream = new List<BitmapStream>();
            current_type = "";
            streamCount = 0;
            finish = 0;
        }

        public VideoFileReader video_wmv;
        //public AVIReader video_avi;
        public List<BitmapStream> bitmap_stream;
        public int streamCount;
        public int finish;
        public int bmpPerStream;
        public String current_type;
        public String address;
        //public Clinet_multi client;

        public void flush()
        {
            if (current_type == "wmv")
            {
                video_wmv.Close();
            }
            else if (current_type == "avi")
            {
                //video_avi.Close();
            }
            bitmap_stream = new List<BitmapStream>();
            finish = 0;
            current_type = "";
            streamCount = 0;
        }

        //先check有没有这个本地文件,没有返回null
        //设置reader并返回[frameRate, height, width]的header List
        //读video的时候自动导出audio
        public List<String> loadFile(String name,String type)
        {
            flush();
            if (Local.exist( name))
            {
                List<String> header = new List<string>();
                switch (type)
                {
                    case "wmv":
                        current_type = "wmv";
                        address = Local.ref_addr + name;
                        video_wmv.Open(Local.ref_addr+name);
                        header.Add("" + video_wmv.FrameRate);
                        header.Add("" + video_wmv.Height);
                        header.Add("" + video_wmv.Width);
                        bmpPerStream = video_wmv.FrameRate;
                        break;
                    default:
                        return null;
                }
                AudioExtract.extract(name);
                return header;
            }
            else
            {
                return null;
            }
        }

        public BitmapStream loadBitmapStream_wmv()
        {
                BitmapStream stream_tmp = new BitmapStream();
                for (int i = 0; i < bmpPerStream; i++)
                {
                    try
                    {
                        //现在不会判断它停。。。
                        Bitmap bitmap_tmp = video_wmv.ReadVideoFrame();
                        stream_tmp.addFrame(bitmap_tmp);
                    }
                    catch
                    {
                        //buffer装不满了
                        finish = 1;
                        video_wmv.Close();
                        streamCount++;
                        return stream_tmp;
                    }
                }
                streamCount++;
                return stream_tmp;
        }

        //reader这里client直接用这个就好啦,自动调的
        public BitmapStream loadBitmapStream_count(int inputCount)
        {
            //load太多了
            //-1是不管直接读
            if (inputCount < streamCount && inputCount>=0)
            {
                //其实要不要重新来过的?
                return null;
            }
            else if (inputCount == streamCount || inputCount == -1)
            {
                switch(current_type){
                    case "wmv":
                        return loadBitmapStream_wmv();
                    case "avi":
                        return null;
                    default:
                        return null;
                }
            }
            else if (inputCount > streamCount)
            {
                while (inputCount > streamCount)
                {
                    switch (current_type)
                    {
                        case "wmv":
                            loadBitmapStream_wmv();
                            break;
                        case "avi":
                            break;
                        default:
                            break;
                    }
                }
                return loadBitmapStream_count(inputCount);
            }
            return null;
        }
    }
}
