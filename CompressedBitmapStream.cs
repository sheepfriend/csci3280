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
                MemoryStream ms = new MemoryStream();
                bitmapStream.stream[i].Save(ms, ImageFormat.Jpeg);
                stream.Add(ms.ToArray());
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
            stream = (List<byte[]>)info.GetValue("stream", typeof(List<byte[]>));

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("stream", stream);
        }

        public BitmapStream toBitmapStream() {
            BitmapStream result = new BitmapStream();
            converter = new ImageConverter();
            for (int i = 0; i < stream.Count; i++)
            {
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
