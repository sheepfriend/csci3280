using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WpfApplication1
{
    [Serializable()]
    class SocketPackage : ISerializable
    {
        public String ip;
        public String type;
        public List<String> ipTable;
        public String video;
        public String audio;
        public String danmu;
    }
}
