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
        public List<Tuple<String,int>> connectList;
        public String video;
        public String audio;
        public String danmu;
        public List<Tuple<String, int>> header;

        public Package(String type_){
            from = "";
            to = "";
            type = type_;
            connectList = new List<Tuple<String, int>>();
            header = new List<Tuple<String, int>>();
            video = "";
            audio = "";
            danmu = "";
        }

        public Package(SerializationInfo info, StreamingContext ctxt)
        {
          from = (String)info.GetValue("from", typeof(String));
          to = (String)info.GetValue("to", typeof(String));
          type = (String)info.GetValue("type", typeof(String));
          video = (String)info.GetValue("video", typeof(String));
          audio = (String)info.GetValue("audio", typeof(String));
          danmu = (String)info.GetValue("danmu", typeof(String));
          connectList = (List<Tuple<String, int>>)info.GetValue("connectList", typeof(List<Tuple<String, int>>));
          header = (List<Tuple<String, int>>)info.GetValue("header", typeof(List<Tuple<String, int>>));
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
