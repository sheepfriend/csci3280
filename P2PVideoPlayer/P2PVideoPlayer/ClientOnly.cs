using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WpfApplication1
{
    class ClientOnly : Client
    {

        public override List<String> askVideoHeader(String name)
        {
            /*向所有client询问有没有这段视频
             *video线程监听这些回复
             *之后while loop在这个线程等监听的回复
             *如果没有人有这个视频：null
             *有人有：返回header
             *监听的时候收到人有这个视频的消息后video_has就一个一个往上加
             *监听到没有这个视频后video_no就一个一个往上加
             *同时更新有这个video_has的人的list（每个人只能播放一个视频所以直接实现即可）
             *之后要视频的时候往里面加就可以
             */
            video_header = new List<string>();
            video_client = new List<string>();
            video_has = 0;
            video_no = 0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip)
                {
                    Connector conn = find_conn(conn_video_header, ip_list[i]);
                    Package pack = new Package("ask_video");
                    pack.header.Add(name);
                    conn.send(pack);
                }
            }
            while (true)
            {
                if (video_no == ip_list.Count)
                {
                    //没有人有这个视频
                    return null;
                }
                else if (video_has + video_no == ip_list.Count)
                {
                    //所有人都有回复了
                    //这时video_list里面已经有所有有这个视频的client的ip了
                    return video_header;
                }
                //Console.Out.WriteLine("Collecting resp video_no: {0}\t video_has: {1}\t", video_no, video_has);
            }
            /*
             * steps:
             * [1] ask_video: "no_video" => does not has this video
             *                "has_video" => return the header data of the file (frameRate, width, height)
             *                               set bitmapReader
             * [2] request_video:   send the bitmapReader.Count
             *                      "video_resp" return bitmapStream package
             *                      
             */
        }

        public override BitmapStream askBitmapStream(int streamCount)
        {
            video = 0;
            video_stream = new BitmapStream();

            //“有这个视频”名单空的，返回null
            if (video_client.Count == 0) { return null; }

            //给所有名单里的人发请求
            for (int i = 0; i < video_client.Count; i++)
            {
                Connector conn = find_conn(conn_video_header, video_client[i]);
                Package pack = new Package("request_video");
                pack.header.Add("" + streamCount);
                conn.send(pack);
            }
            while (true)
            {
                //等待回复
                //如果某个线程知道有回复了，就把video改成1，然后读data写到video_stream（buffer）里面
                if (video == 1)
                {
                    return video_stream;
                }
            }
        }

        public override void run()
        {
            //从server监听消息更新clients
            ask_client();

        }

        
        public ClientOnly(string ser_ip)
        {
            server_ip = ser_ip;
            clients = new List<List<String>>();
            conn_server = new Connector(server_ip, 9999, 10000);
            conn_server_listen = new Connector(self_ip, -1, 9999);
            conn_audio_data = new List<Connector>();
            conn_audio_header = new List<Connector>();
            conn_video_data = new List<Connector>();
            conn_video_header = new List<Connector>();
            video_client = new List<string>();
            
            //receive 新的list之后才true
            ip_list = new List<string>();
            Enabled = false;
            thread_check = new List<bool>();

            self_ip = GetLocalIPAddress();

              
        }

        public Connector conn_server;
        public Connector conn_server_listen;
        /* 这个是ClientServer没有的 */

        public void listen_main()
        {
            while (true)
            {
                Package pack = conn_server_listen.recv();
                if (pack == null) { continue; }
                if (pack.type == "ip_list")
                {
                    update_client_list(pack);
                }
            }
        }



        public void ask_client()
        {
            Package pack = new Package("ip_list");
            conn_server.send(pack);
            pack=conn_server.recv_from_sender();
            if (pack.type == "ip_port_change")
            {
                conn_server_listen = new Connector(Client.server_ip, -1, Int32.Parse(pack.header[0]));
                Thread thread_server = new Thread(listen_main);
                thread_server.Start();

                conn_server.disconnect();
            }
        }
    }

}
