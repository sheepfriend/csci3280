using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    abstract class Client
    {
        public static int isServer = 0;
        public abstract void run();
        /* init之后不跑，run()才分配线程什么的 */

        public static String self_ip;
        public static String server_ip;
        /* 每台机器固定的ip嘛。。。*/
        /* 每台机器固定的ip嘛。。。*/
        //get the local ip


        public static String[] types = { "video_header", 
                                         "audio_header",
                                         "video_data",
                                         "audio_data",
                                         "danmu_server",
                                         "danmu_client",
                                         "user_exit",
                                         "pmm_ask",
                                         "pmm_resp",
                                         "search",
                                         "search_resp"
                                       };



        public static List<Connector> conn_video_data;
        public static List<Connector> conn_audio_data;
        public static List<Connector> conn_video_header;
        public static List<Connector> conn_audio_header;
        public static List<Connector> conn_danmu_server;
        public static List<Connector> conn_danmu_client;
        public static List<Connector> conn_user_exit;
        public static List<Connector> conn_pmm_ask;
        public static List<Connector> conn_pmm_resp;
        public static List<Connector> conn_search;
        public static List<Connector> conn_search_resp;

        public static List<String> pmm_client;
        public static int pmm_has,pmm_no;
        public static List<byte[]> pmm_data;
        public static List<String> pmm_header;
        public static int pmm_writing, pmm_finish;
        public static List<int> pmm_haha;
        public static int count;
        public static List<List<video_info>> search_result;
        public static int search_asked;
        public static int search_reply;

        public static media_info media;

        public List<List<video_info>> search_key(String key)
        {
            search_result = new List<List<video_info>>();
            search_reply = 0;
            search_asked = 0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    video_asked++;
                    Connector conn = find_conn(conn_search_resp, ip_list[i]);
                    Package pack = new Package("search");
                    pack.header.Add(key);
                    conn.send(pack);
                    search_asked++;
                }
            }

            while (search_asked > search_reply) { }

            return search_result;
        }

        public void askPMM(String filename)
        {
            pmm_no=0;
            pmm_has=0;
            pmm_finish=0;
            pmm_writing=0;
            pmm_data = new List<byte[]>();
            pmm_haha = new List<int>();
            pmm_client = new List<string>();
            count = 0;
            for (int i = 0; i < 10; i++)
            {
                pmm_data.Add(new byte[0]);
                pmm_haha.Add(0);
            }
            int pmm_asked=0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    video_asked++;
                    Connector conn = find_conn(conn_pmm_resp, ip_list[i]);
                    Package pack = new Package("ask_pmm");
                    pack.header.Add(filename);
                    conn.send(pack);
                    pmm_asked++;
                }
            }

            while (pmm_has + pmm_no < pmm_asked) { }

            if (pmm_no == pmm_asked) { return; }
            PMM pmm = new PMM(Int32.Parse(pmm_header[0]), Int32.Parse(pmm_header[1]), pmm_header[2]);

            while (count < 10)
            {
                Connector conn = find_conn(conn_pmm_resp, pmm_client[count % pmm_client.Count]);
                Package pack = new Package("request_pmm");
                pack.header.Add("" + count);
                conn.send(pack);
                while (pmm_haha[count]==0) { }
                count++;
            }

            while (true)
            {
                int tag = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (pmm_haha[i] == 0)
                    {
                        tag = 1;
                        continue;
                    }
                }
                //finish receiving all data
                if (tag == 0)
                {
                    int length = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        length += pmm_data[i].Length;
                    }
                    pmm.data = new byte[length];
                    int position=0;
                    for (int i = 0; i < 10; i++)
                    {
                        Buffer.BlockCopy(pmm_data[i], 0, pmm.data, position, pmm_data[i].Length);
                        position += pmm_data[i].Length;
                    }
                    pmm.writeToFile(Local.ref_addr+filename);
                    return;
                }
            }
        }

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
        public static int onLeave_userCount;

        public static void leave()
        {
            if (isServer == 1) { server_leave(); }
            else { client_leave(); }
        }
        public static WavFormat audio_format;
        public static WaveOutStream audio_stream;
        public static int audio_stream_count;
        public static List<String> audio_client;

        //audio就不用问有没有了，视频和音频一一对应
        public WaveOutStream askWaveOutStream(int countWaveOut)
        {
            audio_client = video_client;
            audio = 0;
            audio_writing = 0;
            audio_client.Add(server_ip);
            if (audio_client.Count == 0) { return null; }
            Connector conn = find_conn(conn_audio_header, audio_client[audio_stream_count % audio_client.Count]);
            Package pack = new Package("request_audio");
            pack.header.Add("" + countWaveOut);
            conn.send(pack);
            while (true)
            {
                if (audio == 1)
                {
                    return audio_stream;
                }
            }
        }

        /* 告诉别人这个client或者server退出了 */
        public static void client_leave()
        {
            if (isServer == 1) { return; }

            //退出的时候要收回复的。。。
            //数有多少个人给了回复
            onLeave_userCount = 0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    onLeave_userCount++;
                }
            }

            for (int i = 0; i < ip_list.Count; i++)
            {
                Package pack = new Package("client_exit");
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    Connector conn = find_conn(conn_user_exit, ip_list[i]);
                    conn.send(pack);
                }
            }


            //等待所有人回复
            while (onLeave_userCount > 0) { }
            exit_threads();
        }

        public static void server_leave()
        {
            if (isServer == 0) { return; }

            //退出的时候要收回复的。。。
            //数有多少个人给了回复
            onLeave_userCount = 0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    onLeave_userCount++;
                }
            }

            for (int i = 0; i < ip_list.Count; i++)
            {
                Package pack = new Package("server_exit");
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    Connector conn = find_conn(conn_user_exit, ip_list[i]);
                    conn.send(pack);
                }
            }

            //等待所有人回复
            while (onLeave_userCount > 0) { }
            exit_threads();
        }

        /* 别人在收到某个client的消息之后统一更新自己的clients列表和ip_list */
        //更新列表规则：变成空
        public static void update_on_client_leave(String ip)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i][0] == ip) { clients[i][0] = ""; }
                if (clients[i][1] == ip) { clients[i][1] = ""; }
            }
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] == ip) { ip_list[i] = ""; }
            }
            if (video_client.IndexOf(ip) != -1)
            {
                video_client.RemoveAt(video_client.IndexOf(ip));
            }
            List<Connector> conns = find_conns(conn_audio_data, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_video_data, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_audio_header, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_video_header, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_danmu_client, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_danmu_server, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
            conns = find_conns(conn_user_exit, ip);
            for (int i = 0; i < conns.Count; i++)
            {
                conns[i].ip_send = "";
            }
        }
        public static void exit_threads()
        {
            for (int i = 0; i < thread_check_record.Count; i++)
            {
                thread_check_record[i].a.Abort();
            }
        }


        public static int video_has, video_no, video_end, video_asked;
        public static int audio_has, audio_no, audio_end, audio_asked, audio, audio_writing;


        /* 一般弹幕就这俩了
         * askDanmuList: client：向server请求一个文件的弹幕
         *               server：读本地的弹幕文件
         *               返回DanmuList
         * addDanmu: client：向server发送一条新弹幕，server append到弹幕文件里
         *           server：直接append到弹幕文件里
         */
        public abstract DanmuList askDanmuList(String name);
        public abstract void addDamnu(int num, String content);
        public static DanmuList danmuList;


        public static List<int> video_writing, video;
        /*
         * 立的一大堆flag
         * 作为client，正在播放的视频只能有一个
         * 这些flag是给这个视频接收用的，发给别人的时候就不用管了
         * 
         */


        public static CompressedBitmapStream video_stream;

        public static List<byte[]> video_stream_fractions;
        public static int NUM_FRACTION = 1;

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

        public static List<Connector> find_conns(List<Connector> list, String to)
        {
            List<Connector> result = new List<Connector>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ip_send == to)
                {
                    result.Add(list[i]);
                }
            }
            return result;
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
            if (thread_check.Count < clients.Count)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    thread_check.Add(false);
                }
            }
            int new_conn = 0;
            int frac_count = 0;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i][1] == self_ip && clients[i][0] != "" && thread_check[i] == false)
                {
                    try
                    {
                        if (clients[i][4] == "video_data" && ip_list.IndexOf(clients[i][1]) == -1)
                        {
                            ThreadWithPort a = new ThreadWithPort(clients[i][0], Int32.Parse(clients[i][3]), Int32.Parse(clients[i][2]), clients[i][4], frac_count);
                            a.Start();
                            thread_check[i] = true;
                            frac_count++;
                            if (frac_count == NUM_FRACTION) { frac_count = 0; }
                            thread_check_record.Add(a);
                        }
                        else
                        {
                            ThreadWithPort a = new ThreadWithPort(clients[i][0], Int32.Parse(clients[i][3]), Int32.Parse(clients[i][2]), clients[i][4]);
                            a.Start();
                            thread_check[i] = true;
                            thread_check_record.Add(a);
                        }
                    }
                    catch { }
                }
                else if (clients[i][0] == self_ip && clients[i][1] != "")
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
                        case "danmu_server":
                            conn_danmu_server.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "danmu_client":
                            conn_danmu_client.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "user_exit":
                            conn_user_exit.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "pmm_ask":
                            conn_pmm_ask.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "pmm_resp":
                            conn_pmm_resp.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "search":
                            conn_search.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        case "search_resp":
                            conn_search_resp.Add(new Connector(clients[i][1], Int32.Parse(clients[i][2]), Int32.Parse(clients[i][3])));
                            break;
                        default:
                            break;
                    }
                    new_conn++;
                    if (new_conn == Client.types.Length + NUM_FRACTION - 1)
                    {
                        new_conn = 0;
                        if (ip_list.IndexOf(clients[i][1])==-1)
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
        public static List<ThreadWithPort> thread_check_record;

        public void askAudio(String name)
        {
            audio_client = video_client;
            audio_format = new WavFormat(0, 0, 0);
            //向所有有视频的人发送音频请求

            audio = 0;
            audio_writing = 0;
            foreach (String ip in audio_client)
            {
                Connector conn = find_conn(conn_audio_header, ip);
                Package pack = new Package("ask_audio");
                pack.header.Add(name);
                conn.send(pack);
            }
            while (audio == 0) { }
            //return audio_format;
        }

        public List<String> askVideoHeader(String name)
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
            video_asked = 0;
            video_header = new List<string>();
            video_client = new List<string>();
            video_has = 0;
            video_no = 0;
            for (int i = 0; i < ip_list.Count; i++)
            {
                if (ip_list[i] != self_ip && ip_list[i] != "")
                {
                    video_asked++;
                    Connector conn = find_conn(conn_video_header, ip_list[i]);
                    Package pack = new Package("ask_video");
                    pack.header.Add(name);
                    conn.send(pack);
                }
            }
            while (true)
            {
                if (video_no == video_asked )
                {
                    //没有人有这个视频
                    audio_client = video_client;
                    return null;
                }
                else if (video_has + video_no == video_asked )
                {
                    //所有人都有回复了
                    //这时video_list里面已经有所有有这个视频的client的ip了
                    audio_client = video_client;
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

        public BitmapStream askBitmapStream(int streamCount)
        {
            video_stream_fractions = new List<byte[]>(NUM_FRACTION);
            video = new List<int>(NUM_FRACTION);
            video_writing = new List<int>(NUM_FRACTION);
            for (int i = 0; i < NUM_FRACTION; i++)
            {
                video_stream_fractions.Add(new byte[0]);
                video.Add(0);
                video_writing.Add(0);
            }
            video_stream = new CompressedBitmapStream();

            //“有这个视频”名单空的，返回null
            if (video_client.Count == 0) { return null; }

            //给所有名单里的人发请求
            Connector conn = find_conn(conn_video_header, video_client[streamCount % video_client.Count]);
            Package pack = new Package("request_video");
            pack.header.Add("" + streamCount);
            conn.send(pack);

            while (true)
            {
                //等待回复
                //如果某个线程知道有回复了，就把video改成1，然后读data写到video_stream（buffer）里面
                int tag = 1;
                for (int i = 0; i < NUM_FRACTION; i++)
                {
                    if (video[i] == 0)
                    {
                        tag = 0;
                        break;
                    }
                }
                if (tag == 1)
                {
                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    for (int i = 0; i < NUM_FRACTION; i++)
                    {
                        stream.Write(video_stream_fractions[i], 0, video_stream_fractions[i].Length);
                    }
                    stream.Position = 0;

                    //解压缩一下。。。
                    /*byte[] data = stream.ToArray();
                    stream = new MemoryStream();
                    GZipStream decomp = new GZipStream(stream,CompressionMode.Decompress);
                    decomp.Read(data, 0, data.Length);
                    */

                    video_stream = (CompressedBitmapStream)formatter.Deserialize(stream);
                    return video_stream.toBitmapStream();
                }
            }
        }

    }
}
