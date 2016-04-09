using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WpfApplication1
{
    [Serializable()]
    class CompressedBitmapStream: ISerializable
    {
        public List<byte[]> stream;
        public int position;
        public bool empty;
        ImageConverter converter;
        public int buffer_size=1000000;

        public CompressedBitmapStream(BitmapStream bitmapStream)
        {
            position = bitmapStream.position;
            empty = bitmapStream.empty;
            stream = new List<byte[]>();
            converter = new ImageConverter();
            for (int i = 0; i < bitmapStream.stream.Count; i++)
            {
                //stream.Add(Compress.compress((byte[])converter.ConvertTo(bitmapStream.stream[i], typeof(byte[]))));
                MemoryStream ms = new MemoryStream();
                bitmapStream.stream[i].Save(ms, ImageFormat.Jpeg);
                stream.Add(ms.ToArray());
                //stream.Add((byte[])converter.ConvertTo(bitmapStream.stream[i], typeof(byte[])));
            }
        }

        public CompressedBitmapStream()
        {
            position = 0;
            empty = true;
            stream = new List<byte[]>();
            converter = new ImageConverter();
        }

        public CompressedBitmapStream(SerializationInfo info, StreamingContext ctxt)
        {
            //empty = (bool)info.GetValue("empty", typeof(bool));
            stream = (List<byte[]>)info.GetValue("stream", typeof(List<byte[]>));
            //position = (int)info.GetValue("position", typeof(int));
            /*
            //data = Compress.decompress(data);
            int max = 0;

            for (int i = 0; i < data_len.Count; i++)
            {
                max = max > data_len[i] ? max : data_len[i];
            }
            List<byte[]> tmp_stream = new List<byte[]>();
            for (int i = 0; i < data_len.Count; i++)
            {
                tmp_stream.Add(new byte[max]);
            }

             int blocks=(max/buffer_size*buffer_size)==max?max/buffer_size-1:max/buffer_size;
            int last_size = max - blocks * buffer_size;

            for (int i = 0; i < data_len.Count; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    System.Buffer.BlockCopy(data, (j * tmp_stream.Count + i) * buffer_size, tmp_stream[i], j * buffer_size, buffer_size);
                }
                System.Buffer.BlockCopy(data, (blocks * tmp_stream.Count) * buffer_size + i * last_size, tmp_stream[i], blocks * buffer_size, last_size);
            }
            stream = new List<byte[]>();
            for (int i = 0; i < data_len.Count; i++)
            {
                stream.Add(new byte[data_len[i]]);
                System.Buffer.BlockCopy(tmp_stream[i], 0, stream[i], 0, data_len[i]);
            }
            */
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //find the max size of bitmap
            /*int max = 0;
            data_len = new List<int>();

            for (int i = 0; i < stream.Count; i++) {
                data_len.Add(stream[i].Length);
                if (max < stream[i].Length) { max = stream[i].Length; }
                }
            data = new byte[max * stream.Count];

            List<byte[]> tmp_stream = new List<byte[]>();

            for (int i = 0; i < stream.Count; i++)
            {
                tmp_stream.Add(new byte[max]);
                System.Buffer.BlockCopy(stream[i], 0, tmp_stream[i], 0, data_len[i]);
            }

            int blocks=(max/buffer_size*buffer_size)==max?max/buffer_size-1:max/buffer_size;
            int last_size = max - blocks * buffer_size;

            for (int i = 0; i < stream.Count; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    System.Buffer.BlockCopy(tmp_stream[i], j * buffer_size, data, (j * stream.Count + i) * buffer_size, buffer_size);
                }
                System.Buffer.BlockCopy(tmp_stream[i], blocks * buffer_size, data, (blocks * stream.Count) * buffer_size + i * last_size, last_size);
            }
            */
            //info.AddValue("data_len", data_len);
            //info.AddValue("data", data);
            //info.AddValue("data", Compress.compress(data));
            info.AddValue("stream", stream);
            //info.AddValue("position", position);
            //info.AddValue("empty", empty);
        }

        public BitmapStream toBitmapStream() {
            BitmapStream result = new BitmapStream();
            converter = new ImageConverter();
            for (int i = 0; i < stream.Count; i++)
            {
                //Image img = (Image)converter.ConvertFrom(Compress.decompress(stream[i]));
                Image img = (Image)converter.ConvertFrom(stream[i]);
                Bitmap bitmap = new Bitmap(img);

                result.addFrame(bitmap);
            }
            return result;
        }

        public void addFrame(Bitmap input)
        {
            stream.Add(Compress.compress((byte[])converter.ConvertTo(input,typeof(byte[]))));
            empty = false;
        }

    }
}
