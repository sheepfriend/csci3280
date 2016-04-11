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

        public Connector conn_server;

        public List<Connector> conn_ip_list;
        public int client_ip_list_port;

        public override DanmuList askDanmuList(string name)
        {
            //server直接读本地就行
            String[] tmp = name.Split('\\');
            DanmuList danmu = new DanmuList();
            danmu.readFromFile(tmp[tmp.Length-1]);
            danmuList = danmu;
            return danmu;
        }

        public override void addDamnu(int num, string content)
        {
            //server直接在本地add
            String[] tmp = BitmapPlayer.address.Split('\\');
            DanmuList.appendDanmu(num, content, Local.ref_addr+@"src\danmu\"+tmp[tmp.Length-1]);
        }

        public ClientServer()
        {
            thread_check_record = new List<ThreadWithPort>();
            conn_user_exit = new List<Connector>();
            video_stream_fractions = new List<byte[]>(NUM_FRACTION);
            isServer = 1;
            client_ip_list_port = 9998;
            video_client = new List<string>();
            clients = new List<List <String> >();
            playList = new List<List<string>>();
            ip_list = new List<String >();
            ip_port = new List<int>();
            conn_ip_list = new List<Connector>();


            conn_audio_data = new List<Connector>();
            conn_audio_header = new List<Connector>();
            conn_video_data = new List<Connector>();
            conn_video_header = new List<Connector>();
            conn_danmu_client = new List<Connector>();
            conn_danmu_server = new List<Connector>();

            conn_server = new Connector("", -1, 9999);
            ip_port.Add(10001);
            Enabled = false;

            self_ip = GetLocalIPAddress();
            server_ip = GetLocalIPAddress();
            ip_list.Add(self_ip);

            thread_check = new List<bool>();
        }


        public static List<String> types = new List<String>(Client.types);

        public override void run()
        {
            Thread a = new Thread(listen);
            a.Start();
        }


        public int search_last(List<String> list, String item)
        {
            int result=-1;
            for (int i = list.Count - 1; i >= 0; i++)
            {
                if (item == list[i]) { result = i; break; }
            }
            return result;
        }

        public void emit_list(Package pack)
        {
            int port_num;
            if (ip_list.IndexOf(pack.from) == -1)//newly come client
            {
                 port_num= 10001;
            }
            else
            {
                port_num = ip_port[search_last(ip_list,pack.from)];
            }
                for (int j = 0; j < ip_list.Count; j++)
                {
                    int b_port_num = ip_port[j];
                    for (int i = 0; i < types.Count; i++)
                     {
                         if (types[i]=="video_data")
                         {
                             for (int k = 0; k < NUM_FRACTION; k++)
                             {

                                 List<String> a = new List<String>();
                                 a.Add(pack.from);
                                 a.Add(ip_list[j]);
                                 a.Add(b_port_num + "");
                                 a.Add(port_num + "");
                                 a.Add(types[i]);
                                 clients.Add(a);

                                 List<String> b = new List<String>();
                                 b.Add(ip_list[j]);
                                 b.Add(pack.from);
                                 b.Add(port_num + "");
                                 b.Add(b_port_num + "");
                                 b.Add(types[i]);
                                 clients.Add(b);

                                 port_num++;
                                 b_port_num++;
                             }
                         }
                         else
                         {
                             List<String> a = new List<String>();
                             a.Add(pack.from);
                             a.Add(ip_list[j]);
                             a.Add(b_port_num + "");
                             a.Add(port_num + "");
                             a.Add(types[i]);
                             clients.Add(a);

                             List<String> b = new List<String>();
                             b.Add(ip_list[j]);
                             b.Add(pack.from);
                             b.Add(port_num + "");
                             b.Add(b_port_num + "");
                             b.Add(types[i]);
                             clients.Add(b);

                             port_num++;
                             b_port_num++;

                         }
                     }
                    ip_port[j] = b_port_num;
                }
                ip_port.Add(port_num);


            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] == self_ip || ip_list[i]==pack.from) { continue; }
                Package pack_res = new Package("ip_list");
                pack_res.connectList = clients;

                Connector conn = find_conn(conn_ip_list,ip_list[i]);
                if (conn != null) { conn.send(pack_res); }

            }

            Package change = new Package("ip_port_change");
            change.header.Add("" + client_ip_list_port);
            conn_server.send_from_listenner(change);
            
            while (conn_server.s.Connected) { }
            conn_server.s.Close();
            conn_server = new Connector("", 10000, 9999);

            Connector conn1 = new Connector(pack.from, client_ip_list_port, 9999); 
            Package pack_res1 = new Package("ip_list");
            pack_res1.connectList = clients;
            conn1.send(pack_res1);
            conn_ip_list.Add(conn1);
            client_ip_list_port--;

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
                Package pack = conn_server.recv();
                if (pack == null) { continue; }
                switch (pack.type)
                {
                    case "ip_list":
                        //broadcast clientList to all clients
                        emit_list(pack);
                        break;
                    default:
                        conn_server.disconnect();
                        break;
                }
            }
        }
    }
}
