using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;

namespace WpfApplication1
{
    [Serializable()]
    class BitmapStream : ISerializable
    {
        public List<Bitmap> stream;
        public int position;
        public bool empty;
        
        public BitmapStream()
        {
            stream = new List<Bitmap>();
            position = 0;
            empty = true;
        }

        public BitmapStream(List<Bitmap> input)
        {
            stream = new List<Bitmap>(input);
            position = 0;
            if (input.Count == 0) { empty = true; }
            else { empty = false; }
        }

        public Bitmap read()
        {
            if (position < stream.Count)
            {
                position++;
                return stream[position-1];
            }
            else
            {
                return null;
            }
        }

        public void addFrame(Bitmap input){
            stream.Add(input);
            empty = false;
        }


        public BitmapStream(SerializationInfo info, StreamingContext ctxt)
        {
            empty = (bool)info.GetValue("empty", typeof(bool));
            stream = (List<Bitmap>)info.GetValue("stream", typeof(List<Bitmap>));
            position = (int)info.GetValue("position", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("stream",stream);
            info.AddValue("position", position);
            info.AddValue("empty", empty);
        }
    }
}
