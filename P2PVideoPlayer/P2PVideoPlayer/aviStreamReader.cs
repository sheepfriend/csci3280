using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Drawing;
using AForge.Video.FFMPEG;
using AForge.Video.VFW;


namespace WpfApplication1
{
    public class AVIStreamReader
    {
        private String path;
        private VideoFileReader reader;
        private VideoFileWriter writer;
        private int height;
        private int width;
        public int fps;
        public int totalFrameNum;
        private int currentFrame;

        public AVIStreamReader(String path)
        {
            this.path = path;
            reader = new VideoFileReader();
            reader.Open(path);
            height = reader.Height;
            width = reader.Width;
            fps = reader.FrameRate;
            totalFrameNum = (int)reader.FrameCount;
            currentFrame = 0;
        }

        ~AVIStreamReader()
        {
            reader.Close();
        }

        public int readStream(String name)
        {
            //Console.WriteLine("begin: {0} \ntotalFrameNum: {1}\ncurrentFrame: {2}\n fps: {3}\n", name, totalFrameNum, currentFrame, fps);
            //Console.ReadKey();
            writer = new VideoFileWriter();
            writer.Open(name, width, height, fps);
            int reachEnd = 0;
            Bitmap temp;
            for (int i = currentFrame; i < currentFrame + fps && i < totalFrameNum; i++)
            {
                temp = reader.ReadVideoFrame();
                writer.WriteVideoFrame(temp);
                temp.Dispose();
                if (totalFrameNum == i + 1)
                {
                    reachEnd = 1;
                    currentFrame = i;
                    writer.Close();
                    return reachEnd;
                }
            }
            currentFrame = currentFrame + fps;
            writer.Close();
            //Console.WriteLine("after: {0} \ntotalFrameNum: {1}\ncurrentFrame: {2}\n", name, totalFrameNum, currentFrame);
            //Console.ReadKey();
            return reachEnd;
        }


    }
}