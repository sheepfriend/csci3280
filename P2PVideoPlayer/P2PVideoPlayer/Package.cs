using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace WpfApplication1
{
    [Serializable()]
    class Package : ISerializable
        //package class used in client_multi and server_multi
    {
        public String from;
        public String to;
        public int from_port;
        public int to_port;
        //port number of the sender
        //expected port to receive resp
        public String type;
        public List<List<String> > connectList;
        public List<String> header;
        public byte[] data;

        public Package(String type_)
        {
            from = "";
            to = "";
            type = type_;
            from_port = 0;
            to_port = 0;
            connectList = new List<List<String>>();
            header = new List<String>();
            data = new byte[0];
        }

        public Package(SerializationInfo info, StreamingContext ctxt)
        {
            from = (String)info.GetValue("from", typeof(String));
            to = (String)info.GetValue("to", typeof(String));
            from_port = (int)info.GetValue("from_port", typeof(int));
            to_port = (int)info.GetValue("to_port", typeof(int));
            type = (String)info.GetValue("type", typeof(String));
            data = (byte[])info.GetValue("data", typeof(byte[]));
            connectList = (List<List<String>>)info.GetValue("connectList", typeof(List<List<String>>));
            header = (List<String>)info.GetValue("header", typeof(List<String>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("from", from);
            info.AddValue("to", to);
            info.AddValue("type", type);
            info.AddValue("data", data);
            info.AddValue("from_port", from_port);
            info.AddValue("to_port", to_port);
            info.AddValue("header", header);
            info.AddValue("connectList", connectList);
        }
    }
}
