using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Drawing;
using AForge.Video.FFMPEG;


namespace WpfApplication1
{
    public class AVIStreamReader
    {
        private String path;
        private VideoFileReader reader;
        private VideoFileWriter writer;
        private int height;
        private int width;
        private int fps;
        private long totalFrameNum;
        private long currentFrame;

        public AVIStreamReader(String path)
        {
            this.path = path;
            reader = new VideoFileReader();
            reader.Open(path);
            writer = new VideoFileWriter();
            height = reader.Height;
            width = reader.Width;
            fps = reader.FrameRate;
            totalFrameNum = reader.FrameCount;
            currentFrame = 0;
        }

        ~AVIStreamReader()
        {
            reader.Close();
        }

        public int readStream(String name)
        {
            writer.Open(name, width, height, fps);
            int reachEnd = 0;
            for (long i = currentFrame; i < currentFrame + fps && i < totalFrameNum; i++, currentFrame++)
            {
                writer.WriteVideoFrame(reader.ReadVideoFrame());
                if (totalFrameNum == i + 1)
                {
                    reachEnd = 1;
                }
            }
            writer.Close();
            return reachEnd;
        }


    }
}