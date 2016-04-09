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
        public override void run()
        {
            //从server监听消息更新clients
            ask_client();
        }


        public override DanmuList askDanmuList(string name)
        {
            //向server发送请求然后等。。。
            Connector conn = find_conn(conn_danmu_server, server_ip);
            Package pack = new Package("danmu_ask");
            pack.header.Add(name);
            conn.send(pack);
            danmuList = null;

            while (!(danmuList != null)) { }

            return danmuList;
        }

        public override void addDamnu(int num, string content)
        {
            //直接扔过去就行，不用等
            Connector conn = find_conn(conn_danmu_server, server_ip);
            Package pack = new Package("danmu_add");
            pack.header.Add( BitmapPlayer.address);
            pack.header.Add(num+"");
            pack.header.Add(content);
            conn.send(pack);
        }

        public static String addr;

        public ClientOnly(string input_ip)
        {
            conn_user_exit = new List<Connector>();
            conn_danmu_client = new List<Connector>();
            conn_danmu_server = new List<Connector>();
            thread_check_record = new List<ThreadWithPort>();
            video_stream_fractions = new List<byte[]>(NUM_FRACTION);
            clients = new List<List<String>>();
            conn_server = new Connector(server_ip, 9999, 10000);
            conn_server_listen = new Connector(self_ip, -1, 9999);
            conn_audio_data = new List<Connector>();
            conn_audio_header = new List<Connector>();
            conn_video_data = new List<Connector>();
            conn_video_header = new List<Connector>();
            video_client = new List<string>();

            server_ip = input_ip;

            addr = @"H:\csci3280-master\client\P2PVideoPlayer\";
            //receive 新的list之后才true
            ip_list = new List<string>();
            Enabled = false;
            thread_check = new List<bool>();

            self_ip = "";

            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    self_ip = ip.ToString();
                    break;
                }
            }
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
            pack = conn_server.recv_from_sender();
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
