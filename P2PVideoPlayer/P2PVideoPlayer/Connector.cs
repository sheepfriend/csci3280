using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;


namespace WpfApplication1
{
    class Connector
    {
        public static int MAX_LEN = 5000;
        public bool connected,listenning;

        public int port_send,port_listen;
        public String ip_send;

        public Socket s;
        public Socket tmp_s;

        public Connector(String ip,int num1,int num2)
        {
            port_send = num1;
            port_listen = num2;
            ip_send = ip;
            connected = false;
            listenning = false;
        }

        public Connector()
        {
            connected = false;
            listenning = false;
        }

        public void connect(String ip, int num1, int num2)
        {
            ip_send = ip;
            port_send = num1;
            port_listen = num2;
            connect();
            connected = true;
        }

        public void connect()
        {

            IPAddress ip = IPAddress.Parse(ip_send);

            //IPAddress ip = IPAddress.Parse("192.168.110.30");
            IPEndPoint ipe = new IPEndPoint(ip, port_send);

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(ipe);
            connected = true;
        }

        public void listen()
        {
            IPAddress ip = IPAddress.Parse(Client.self_ip);
            IPEndPoint ipe = new IPEndPoint(ip, port_listen);

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(0);

            tmp_s = s.Accept();

            listenning = true;
        }

        public void disconnect() {
            s.Disconnect(false);
        }

        public void send(Package pack)
        {
            if (!connected) {
                connect();
            } 
            if (!s.Connected) {
                connect();
            }

            pack.from = Client.self_ip;

            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, pack);
            stream.Position = 0;
            BinaryReader br = new BinaryReader(stream);
            byte[] b = br.ReadBytes((int)stream.Length);
            byte[] a = BitConverter.GetBytes(b.Length);
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);

            s.Send(c, c.Length, 0);

            Console.Out.Write("Pacakge send: " + pack.to + "   type: " + pack.type+" port:"+port_send+" \n\tremote:"+s.RemoteEndPoint.ToString()+"\tlocal:"+s.LocalEndPoint.ToString()+"\n");
        }

        public Package recv(int port)
        {
            port_listen = port;
            return recv();
        }

        public Package recv_from_sender() {
            byte[] len = new byte[4];
            int length = 0;
            Console.Out.WriteLine("listening port {0}", port_listen);

            MemoryStream stream = new MemoryStream();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                s.Receive(len, 4, 0);
                length = BitConverter.ToInt32(len, 0);

               while (true)
                {
                   int tmp_len = length>MAX_LEN?MAX_LEN:length;
                   byte[] buffer = new byte[tmp_len];
                   s.Receive(buffer,tmp_len,0);
                   stream.Write(buffer, 0, buffer.Length);

                   if(stream.Length == length){
                       //都收完了
                        BinaryFormatter bformatter = new BinaryFormatter();
                        stream.Position = 0;
                       Package pack = (Package)bformatter.Deserialize(stream);
                       Console.Out.Write("Pacakge recv: " + pack.from + "   type: " + pack.type + "\n");
                       return pack;
                   }
                }

            
        }

        public Package recv()
        {
            if (!listenning) listen();
            
            byte[] len = new byte[4];
            int length = 0;
            Console.Out.WriteLine("listening port {0}", port_listen);

            MemoryStream stream = new MemoryStream();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                tmp_s.Receive(len, 4, 0);
            }
            catch
            {
                bool received = false;
                while (!received)
                {
                    s.Listen(0);
                    tmp_s = s.Accept();
                    tmp_s.Receive(len, 4, 0);
                    received = true;
                }
            }
                length = BitConverter.ToInt32(len, 0);
                Console.Out.WriteLine("size of received package: {0}", length);
               while (true)
                {
                   byte[] buffer = new byte[50000];
                   int tmp_len=tmp_s.Receive(buffer);

                   stream.Write(buffer, 0, tmp_len);

                   //Console.Out.WriteLine("buffering stream size: {0}", stream.Length);

                   if(stream.Length == length){
                       //都收完了
                        BinaryFormatter bformatter = new BinaryFormatter();
                        stream.Position = 0;
                       Package pack = (Package)bformatter.Deserialize(stream);
                       Console.Out.Write("Pacakge recv: " + pack.from + "   type: " + pack.type + "\n");
                       return pack;
                   }
                }


            }
    }
}
