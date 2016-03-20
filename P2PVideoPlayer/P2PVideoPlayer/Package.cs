using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace WpfApplication1
{
    [Serializable()]
    class Package: ISerializable
    {
        public String from;
        public String to;
        public String type;
        public List<String> connectList;
        public byte[] video;
        public byte[] audio;
        public byte[] danmu;
        public List<Tuple<String, String> > header;

        public Package(String type_){
            from = "";
            to = "";
            type = type_;
            connectList = new List<String>();
            header = new List<Tuple<String, String>>();
            video = new byte[0];
            audio = new byte[0];
            danmu = new byte[0];
        }

        public Package(SerializationInfo info, StreamingContext ctxt)
        {
          from = (String)info.GetValue("from", typeof(String));
          to = (String)info.GetValue("to", typeof(String));
          type = (String)info.GetValue("type", typeof(String));
          video = (byte[])info.GetValue("video", typeof(byte[]));
          audio = (byte[])info.GetValue("audio", typeof(byte[]));
          danmu = (byte[])info.GetValue("danmu", typeof(byte[]));
          connectList = (List<String>)info.GetValue("connectList", typeof(List<String>));
          header = (List<Tuple<String, String>>)info.GetValue("header", typeof(List<Tuple<String, String> >));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("from", from);
            info.AddValue("to", to);
            info.AddValue("type", type);
            info.AddValue("video", video);
            info.AddValue("audio", audio);
            info.AddValue("danmu", danmu);
            info.AddValue("header", header);
            info.AddValue("connectList", connectList);
        }


    }
}
