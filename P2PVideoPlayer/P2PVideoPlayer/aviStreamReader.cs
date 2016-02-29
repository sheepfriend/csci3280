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

        public AVIStreamReader(String path)
        {
            this.path = path;
            reader = new VideoFileReader();
            reader.Open(path);
            writer = new VideoFileWriter();
            height = reader.Height;
            width = reader.Width;
        }

        ~AVIStreamReader()
        {
            reader.Close();
        }

        public void readStream(String name,int num)
        {
            writer.Open(name, width, height);
            for (int i = 0; i < num; i++)
            {
                writer.WriteVideoFrame(reader.ReadVideoFrame());
            }
            writer.Close();
        }

    }
}