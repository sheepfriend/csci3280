using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    [Serializable()]
    class WaveOutStream: ISerializable
    {
        public List<WaveOutBuffer> stream;
        public int position;
        public bool empty;

        public WaveOutStream()
        {
            stream = new List<WaveOutBuffer>();
            position = 0;
            empty = true;
        }

        public WaveOutStream(List<WaveOutBuffer> list)
        {
            stream = list;
            position = 0;
            empty = list.Count == 0 ? true : false;
        }

        public WaveOutBuffer read()
        {
            if (position < stream.Count)
            {
                position++;
                return stream[position - 1];
            }
            else
            {
                return null;
            }
        }

        public void addBuffer(WaveOutBuffer input)
        {
            stream.Add(input);
            empty = false;
        }

        public WaveOutStream(SerializationInfo info, StreamingContext ctxt)
        {
            List<byte[]> stream_byte = (List<byte[]>)info.GetValue("stream", typeof(List<byte[]>));

            position = (int)info.GetValue("position", typeof(int));

            stream = new List<WaveOutBuffer>();

            empty = false;

            for (int i = 0; i < stream_byte.Count; i++)
            {
                empty = true;
                MemoryStream mstream = new MemoryStream(stream_byte[i]);
                mstream.Position = 0;
                BinaryFormatter bformatter = new BinaryFormatter();
                WaveOutBuffer buffer = (WaveOutBuffer)bformatter.Deserialize(mstream);
                stream.Add(buffer);
            }
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            List<byte[]> stream_byte = new List<byte[]>();
            for (int i = 0; i < stream.Count; i++)
            {
                MemoryStream mstream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(mstream, stream[i]);
                byte[] data = mstream.ToArray();

                stream_byte.Add(data);
            }

            info.AddValue("stream",stream_byte);
            info.AddValue("position", position);
        }
    }
}
