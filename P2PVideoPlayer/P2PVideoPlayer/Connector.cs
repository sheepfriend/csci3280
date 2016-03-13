using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace WpfApplication1
{
    class Connector
    {
        public UdpClient conn;
        public byte[] buffer;
        public Connector() {
            conn = new UdpClient();
            buffer = new byte[0];
        }
        public void send(String ip, int port, Package pack) {
            conn = new UdpClient();
            conn.Connect(ip, port);
            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, pack);
            byte[] b;
            using (BinaryReader br = new BinaryReader(stream))
            {
                b = br.ReadBytes((int)stream.Length);
            }
            byte[] a = BitConverter.GetBytes(b.Length);
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            conn.Send(c, c.Length);
            conn.Close();
        }
        public Package recv(int port) {
            byte[] len = new byte[4];
            int length = 0,sum = 0;
            conn = new UdpClient(port);
            MemoryStream stream = new MemoryStream();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (buffer.Length > 0)
            {
                if (length == 0 && buffer.Length == 0) { buffer = conn.Receive(ref RemoteIpEndPoint); }
                if (length == 0)
                {
                    System.Buffer.BlockCopy(buffer, 0, len, 0, 4);
                    length = BitConverter.ToInt32(len, 0);
                    byte[] buffer1 = new byte[buffer.Length - 4];
                    System.Buffer.BlockCopy(buffer, 4, buffer1, 0, buffer.Length - 4);
                    buffer = new byte[buffer1.Length];
                    System.Buffer.BlockCopy(buffer1, 0, buffer, 0, buffer.Length);
                }
                if (buffer.Length + sum < length)
                {
                    stream.Write(buffer,0,buffer.Length);
                    buffer = new byte[0];
                }
                else
                {
                    stream.Write(buffer,0,length - sum);
                    byte[] buffer1 = new byte[buffer.Length - (length - sum)];
                    System.Buffer.BlockCopy(buffer, length - sum, buffer1, 0, buffer1.Length);
                    buffer = new byte[buffer1.Length];
                    System.Buffer.BlockCopy(buffer1, 0, buffer, 0, buffer.Length);
                    BinaryFormatter bformatter = new BinaryFormatter();
                    conn.Close();
                    return (Package)bformatter.Deserialize(stream);
                }
            }
            conn.Close();
            return null;
        }
    }
}
