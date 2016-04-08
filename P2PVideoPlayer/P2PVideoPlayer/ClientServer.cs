using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace WpfApplication1
{
    class ClientServer:Client
    {
        public List< List<String>> playList;

        public ClientServer()
        {
            clients = new List<List <String> >();
            playList = new List<List<string>>();
            ip_list = new List<String >();
            ip_port = new List<int>();

            server_ip = GetLocalIPAddress();
            ip_list.Add(self_ip);
            ip_port.Add(10001);
            Enabled = false;

            self_ip = "";

            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    self_ip= ip.ToString();
                    break;
                }
            }  
        }


        public static String[] types_ = { "video_header", 
                                         "audio_header",
                                         "video_data",
                                         "audio_data",
                                         "danmu"};
        public static List<String> types = new List<String>(types_);

        public override void run()
        {
            Thread a = new Thread(listen);
            a.Start();
        }

        public override List<String> askVideoHeader(String name)
        {
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
                if (video_no == ip_list.Count - 1)
                {
                    return null;
                }
                else if (video_has + video_no == ip_list.Count - 1)
                {
                    return video_header;
                }
            }
        }

        public override BitmapStream askBitmapStream(int streamCount)
        {
            video = 0;
            video_stream = new BitmapStream();
            if (video_client.Count == 0) { return null; }

            for (int i = 0; i < video_client.Count; i++)
            {
                Connector conn = find_conn(conn_video_data, video_client[i]);
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


        public void load_play_list(String path)
        {
            //read play list from a file in hard disk
        }

        public void emit_list(Package pack)
        {
            if (ip_list.IndexOf(pack.from) == -1)//newly come client
            {
                int port_num = 10001;
                for (int j = 0; j < ip_list.Count; j++)
                {
                    int b_port_num = ip_port[j];
                    for (int i = 0; i < types.Count; i++)
                     {
                         List<String> a = new List<String>();
                         a.Add(pack.from);
                         a.Add(ip_list[j]);
                         a.Add(port_num+"");
                         a.Add(types[i]);
                         clients.Add(a);

                         List<String> b = new List<String>();
                         b.Add(ip_list[j]);
                         b.Add(pack.from);
                         b.Add(b_port_num+"");
                         b.Add(types[i]);
                         clients.Add(b);

                         port_num++;
                         b_port_num++;
                     }
                    ip_port[j] = b_port_num;
                }
                ip_list.Add(pack.from);
                ip_port.Add(port_num);
            }

            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] == self_ip) { continue; }
                Package pack_res = new Package("ip_list");
                pack_res.connectList = clients;
                Connector conn = new Connector(ip_list[i], 10000, 9999);
                conn.send(pack_res);
                conn.disconnect();
            }

            update_client_list();

        }

        public void search_video(Package pack)
        {
            for (int i = 0; i < playList.Count; i++)
            {
                String video_name = pack.header[0];
                if (video_name == playList[i][0])
                {
                    // find the target video
                    Package pack_res = new Package("");
                }
            }
            //target video not find
        }
        public void listen()
        {
            while (true)
            {
                Connector conn = new Connector("",10000,9999);
                Package pack = conn.recv();
                if (pack == null) { continue; }
                switch (pack.type)
                {
                    case "ip_list":
                        //broadcast clientList to all clients
                        emit_list(pack);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
