using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    class ThreadWithPort
    {
        public Connector conn;
        public int port_send,port_listen;
        public String ip_send;

        public String type;
        /* 
         * 每个ThreadWithPort实例有很多种可能的工作：
         * [0] 传递 视频/音频 的header，检查有没有这个视频，同时初始化BitmapReader
         * [1] 发送或接受 视频/音频，接受的时候check是不是被别人更新了（Client的static变量）
         *     没有更新的话就更新，然后改flag
         * [2] 弹幕什么的还没有考虑好。。。
         * 
         * 现有的type，和上面对应：
         * [0] video_header / audio_header
         * [1] video_data / audio_data
         * [2] danmu
         * 
         */

        public Thread a;

        public BitmapReader reader;
        /* 每一个线程只会操作一个BitmapReader */

        public BitmapStream bitmap_resp;

        public ThreadWithPort(String ip,int num1,int num2, String str)
        {
            reader = new BitmapReader();
            bitmap_resp = new BitmapStream();

            port_send = num1;
            port_listen = num2;
            ip_send = ip;
            conn = new Connector(ip_send, port_send, port_listen);

            type = str;
        }

        public void Start()
        {
            switch (type)
            {
                case "audio_data":
                    a = new Thread(listen_audio_data);
                    break;
                case "video_data":
                    a = new Thread(listen_video_data);
                    break;
                case "audio_header":
                    a = new Thread(listen_audio_header);
                    break;
                case "video_header":
                    a = new Thread(listen_video_header);
                    break;
                case "danmu":
                    a = new Thread(listen_danmu);
                    break;
                default:
                    a = new Thread(def);
                    break;
            }
            a.Start();
        }

        public void def()
        {
            Console.Out.WriteLine("Error: wrong type of thread: {0}",type);
            return;
        }

        public void listen_video_header()
        {
            while (true)
            {
                Package pack =conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "ask_video")
                {
                    /*
                     * header for request:
                     * [0] Name
                     */
                    List<String> header = new List<String>(pack.header);

                    if (Local.exist(header[0]))
                    {
                        //该文件存在，载入到本线程的reader里面
                        //接受的时候不需要reader，只有传的时候需要，所以一个reader就够了
                        /*
                         * type of resp: has_video
                         * 
                         * reader the info from the video file and response a header
                         * 
                         * header of resp:
                         * [0] FrameRate(给player设置interval)
                         * [1] Height
                         * [2] Width
                         * 
                         * 那个wmv是历史遗留问题反正avi也是这个参数。。。
                         * 
                         */
                        header = reader.loadFile(header[0], "wmv");
                        for (int i = 0; i < header.Count; i++)
                        {
                            Console.Out.WriteLine("{0}: {1}", i, header[i]);
                        }
                        Package pack_resp = new Package("has_video");
                        pack_resp.header = new List<string>(header);
                        Connector conn_resp = Client.find_conn(Client.conn_video_data, pack.from);
                        conn_resp.send(pack_resp);
                    }
                }
                else if (pack.type == "request_video")
                {
                    /*
                     * header for request:
                     * [0] 第几个bitmapStream
                     */
                    List<String> header = new List<String>(pack.header);

                    if (reader.finish == 1)
                    {
                        //空了，给一个end_video
                        Package pack_resp = new Package("end_video");
                        Connector conn_resp = Client.find_conn(Client.conn_video_data, pack.from);
                        conn_resp.send(pack_resp);
                    }
                    else
                    {
                        BitmapStream bitmapStream = reader.loadBitmapStream_count(Int32.Parse(header[0]));

                        //把object serialize成memoryStream,再到byte[]
                        MemoryStream stream = new MemoryStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, bitmapStream);

                        byte[] data = stream.ToArray();

                        //发回去
                        Package pack_resp = new Package("video_resp");
                        pack_resp.data = data;
                        Connector conn_resp = Client.find_conn(Client.conn_video_data, pack.from);
                        conn_resp.send(pack_resp);
                    }
                }
            }
        }

        public void listen_video_data()
        {
            while (true)
            {
                Package pack = conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "video_resp")
                {
                    if (Client.video == 0)
                    {
                        Client.video = 1;
                        MemoryStream stream = new MemoryStream(pack.data);
                        BinaryFormatter formatter = new BinaryFormatter();
                        bitmap_resp = (BitmapStream)formatter.Deserialize(stream);
                        Client.video_stream = bitmap_resp;
                    }
                }
                else if (pack.type == "no_video")
                {
                    Client.video_no++;
                }
                else if (pack.type == "has_video")
                {
                    Client.video_header = new List<string>(pack.header);
                    Client.video_has++;
                    Client.video_client.Add(pack.from);
                }
                else if (pack.type == "end_video")
                {
                    if (Client.video == 0)
                    {
                        Client.video = 1;
                        bitmap_resp = null;
                        Client.video_end = 1;
                    }
                }
                
            }
        }

        public void listen_danmu() { }
        public void listen_audio_header() { }
        public void listen_audio_data() { }
    }
}
