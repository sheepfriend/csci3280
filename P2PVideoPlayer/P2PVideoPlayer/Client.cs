using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace WpfApplication1
{
    abstract class Client
    {
        public abstract BitmapStream askBitmapStream(int num);
        public abstract List<String> askVideoHeader(String name);
        
        public abstract void run();
        /* init之后不跑，run()才分配线程什么的 */

        public static String self_ip;
        public static String server_ip;
        /* 每台机器固定的ip嘛。。。*/


        public static int video_has, video_no, video_end, video;
        public static int audio_has, audio_no, audio_end, audio;
        /*
         * 立的一大堆flag
         * 作为client，正在播放的视频只能有一个
         * 这些flag是给这个视频接收用的，发给别人的时候就不用管了
         * 
         */


        public static BitmapStream video_stream;
        public static List<String> video_header;
        public static List<String> video_client;
        /* 
         * 正在播放的视频的
         * video_stream: 新来的buffer
         * video_header: header
         * video_client: 有这个视频的client的list
         * 
         */


        public static List<String> ip_list;
        public static List<int> ip_port;
        /* 
         * 为了方便查询每个ip的被占用端口的最大值+1（可用端口最小值）
         * 建立了ip_list和ip_port两个list
         * 单独的client：向所有人询问视频的时候会调用ip_list
         *              刚加入的时候是向server请求的所以这个用不到
         *              根据下面的clients表来更新ip_list
         *              ip_port就不管了
         * client和server：向所有人询问视频的时候会调用ip_list
         *                 刚加入新的client的时候同步更新clients,ip_list,ip_port
         */

        public static List<List<String>> clients;
        /* 
         * 这个东西每个传送的package都有一个，不用的时候默认是空的
         * 只在有新的client的时候package里的这项不为空，本地的会被更新
         * 
         * 每新来一个client，server会对每一组新出现的pair分配端口，
         * sender和receiver是有向的
         * 所以是每个pair每种功能分配两个端口
         * 
         * stucture of List<List<String>> clients:
         * 
         * [0] sender ip
         * [1] receiver ip
         * [2] sender port
         * [3] receiver port
         * [4] type（线程监听接口的用途）
         * 
         * 下面这个function是给定sender和receiver的ip以及type
         * 查找它们俩的端口
         * 
         */
        public static int[] find_port(String from, String to, String type)
        {
            for (int i = 0; i < Client.clients.Count; i++)
            {
                if (from == Client.clients[i][0] && to == Client.clients[i][1] && type == Client.clients[i][4])
                {
                    int[] result = {Int32.Parse(Client.clients[i][2]),
                                   Int32.Parse(Client.clients[i][3])};
                    return result;
                }
            }
            int[] result_null = { -1, -1 };
            return result_null;
        }


        public bool Enabled;
        /* Enabled默认false，当client第一次更新连接表的时候会变成true */


        public static List<Connector> conn_video_data;
        public static List<Connector> conn_audio_data;
        public static List<Connector> conn_video_header;
        public static List<Connector> conn_audio_header;
        /*
         * 用了tcp以后连接要一直保持，所以每个Connector实例只能管理一个连接
         * 监听的connector放在ThreadWithPort里面自己管理
         * 发送的当然要统一管理咯
         * 
         */
        public static Connector find_conn(List<Connector> list, String to)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ip_send == to)
                {
                    return list[i];
                }
            }
            return null;
        }

        /*
         * 更新本地列表，
         * 发现有自己是接收者的条目，拿出来创建监听线程
         * 以前监听过的创建会失败（connector在创建的时候会先连接，端口已占用）
         * 运行起来是ok的
         * 虽然原理上很粗暴
         * 
         */

        
        public void update_client_list()
        {
            if(thread_check.Count<clients.Count){
                for(int i=0;i<clients.Count;i++){
                    thread_check.Add(false);
                }
            }

            int new_conn = 0;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i][1] == self_ip && thread_check[i]==false)
                {
                    try
                    {
                        ThreadWithPort a = new ThreadWithPort(clients[i][0], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3]), clients[i][4]);
                        a.Start();
                        thread_check[i] = true;
                    }
                    catch { }
                }
                else if (clients[i][0] == self_ip && ip_list.IndexOf(clients[i][1])==-1)
                {
                    switch (clients[i][4])
                    {
                        case "video_header":
                            conn_video_header.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "video_data":
                            conn_video_data.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "audio_data":
                            conn_audio_data.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "audio_header":
                            conn_audio_header.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        default:
                            break;
                    }
                    new_conn++;
                    if (new_conn == 4)
                    {
                        new_conn = 0;
                        ip_list.Add(clients[i][1]);
                    }
                }
                Console.Out.WriteLine("\tFrom: {0}\tTo: {1}\tPort: {2}\tType: {3}", clients[i][0], clients[i][1], clients[i][2], clients[i][3]);
            }
        }


        public void update_client_list(Package pack)
        {
            clients = new List<List<String>>(pack.connectList);
            update_client_list();
        }
        

        public static List<bool> thread_check;

        //get the local ip
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }
}
