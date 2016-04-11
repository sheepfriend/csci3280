using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;


namespace WpfApplication1
{
    
    class ThreadWithPort
    {
        public Connector conn;
        public int port_send, port_listen;
        public String ip_send;
        public int num_frac;
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

        public WaveOutReader audio_reader;

        public WaveOutStream audio_resp;

        public PMM pmm;
        public List<byte[]> pmm_data;

        public ThreadWithPort(String ip, int num1, int num2, String str)
        {
            reader = new BitmapReader();
            bitmap_resp = new BitmapStream();

            audio_reader = new WaveOutReader();
            audio_resp = new WaveOutStream();

            num_frac = 0;

            port_send = num1;
            port_listen = num2;
            ip_send = ip;
            conn = new Connector(ip_send, port_send, port_listen);

            type = str;
        }

        public ThreadWithPort(String ip, int num1, int num2, String str,int num)
        {
            reader = new BitmapReader();
            bitmap_resp = new BitmapStream();

            num_frac = num;

            audio_reader = new WaveOutReader();
            audio_resp = new WaveOutStream();

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
                case "danmu_server":
                    a = new Thread(listen_danmu_server);
                    break;
                case "danmu_client":
                    a = new Thread(listen_danmu_client);
                    break;
                case "user_exit":
                    a = new Thread(listen_exit);
                    break;
                case "pmm_ask":
                    a = new Thread(listen_pmm_ask);
                    break;
                case "pmm_resp":
                    a = new Thread(listen_pmm_resp);
                    break;
                case "search":
                    a = new Thread(listen_search);
                    break;
                case "search_resp":
                    a = new Thread(listen_search_resp);
                    break;
                default:
                    a = new Thread(def);
                    break;
            }
            a.Start();
        }

        public void listen_search() {
            while (true)
            {
                Package pack = conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "search_result")
                {
                    MemoryStream stream = new MemoryStream(pack.data);
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;
                    Client.search_result.Add((List<video_info>)formatter.Deserialize(stream));
                    Client.search_reply++;
                }
            }
        }

        public void listen_search_resp()
        {
            while (true)
            {
                Package pack = conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "search")
                {
                    List<video_info> result = Utils.search_list(pack.header[0],Client.media);
                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, result);
                    byte[] data = stream.ToArray();

                    Package pack_resp=new Package("search_result");
                    Connector conn_resp=Client.find_conn(Client.conn_search,pack.from);
                    pack_resp.data=data;
                    conn_resp.send(pack_resp);
                }
            }

        }

        public void listen_pmm_ask()
        {
            while (true)
            {
                Package pack = conn.recv();
                if (pack == null) { continue;}
                if (pack.type == "resp_pmm")
                {
                    int num = Int32.Parse(pack.header[0]);
                    Client.pmm_data[num] = pack.data;
                    Client.pmm_haha[num] = 1;
                }
                else if (pack.type == "has_pmm")
                {
                    Client.pmm_has += 1;
                    Client.pmm_client.Add(pack.from);
                    if (Client.pmm_writing == 0)
                    {
                        Client.pmm_writing = 1;
                        Client.pmm_header = pack.header;
                        Client.pmm_finish = 1;
                    }
                }
                else if (pack.type == "no_pmm")
                {
                    Client.pmm_no += 1;
                }
            }
        }

        public void listen_pmm_resp()
        {
            while (true)
            {

                Package pack = conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "ask_pmm")
                {
                    if (Local.exist(pack.header[0]))
                    {
                        pmm = PMM.loadFile(pack.header[0]);
                        Package pack_resp = new Package("has_pmm");
                        Connector conn_resp = Client.find_conn(Client.conn_pmm_ask, pack.from);
                        pack_resp.header.Add("" + pmm.width);
                        pack_resp.header.Add("" + pmm.height);
                        pack_resp.header.Add(pmm.method);

                        pmm_data = new List<byte[]>();
                        int position = 0;
                        int len = pmm.data.Length / 10;

                        for (int i = 0; i < 9; i++)
                        {
                            pmm_data.Add(new byte[len]);
                            Buffer.BlockCopy(pmm.data, position, pmm_data[i], 0, len);
                            position += len;
                        }
                        pmm_data.Add(new byte[pmm.data.Length - position]);
                        Buffer.BlockCopy(pmm.data, position, pmm_data[9], 0, pmm.data.Length - position);

                        conn_resp.send(pack_resp);
                    }
                    else
                    {
                        Package pack_resp = new Package("no_pmm");
                        Connector conn_resp = Client.find_conn(Client.conn_pmm_ask, pack.from);
                        conn_resp.send(pack_resp);
                    }
                }
                else if (pack.type == "request_pmm")
                {
                    Package pack_resp = new Package("resp_pmm");
                    Connector conn_resp = Client.find_conn(Client.conn_pmm_ask, pack.from);
                    pack_resp.header.Add(pack.header[0]);
                    pack_resp.data = pmm_data[Int32.Parse(pack.header[0])];
                    conn_resp.send(pack_resp);
                }
            }
        }

        public void listen_exit()
        {
            while (true)
            {
                Package pack = conn.recv();
                if (pack == null) { continue; }
                if (pack.type == "client_exit")
                {
                    //更新本地clients列表和ip_list
                    //发送exit_count消息给xxxx

                    Connector conn_resp = Client.find_conn(Client.conn_user_exit, pack.from);
                    
                    Package pack_resp = new Package("exit_count");
                    conn_resp.send(pack_resp);

                    Client.update_on_client_leave(pack.from);

                }
                else if (pack.type == "server_exit")
                {
                    //发送exit_count消息给xxxx
                    //其实想直接退出的
                    //就想想

                    Connector conn_resp = Client.find_conn(Client.conn_user_exit, pack.from);
                    
                    Package pack_resp = new Package("exit_count");
                    conn_resp.send(pack_resp);

                    Client.update_on_client_leave(pack.from);

                }
                else if (pack.type == "exit_count")
                {
                    //要退出的程序收到这个以后减小记数
                    Client.onLeave_userCount--;
                }
            }
        }
        
        public void def()
        {
            Console.Out.WriteLine("Error: wrong type of thread: {0}", type);
            return;
        }

        public void listen_video_header()
        {
            while (true)
            {
                Console.Out.WriteLine("listen_video_header is running...");
                Package pack = conn.recv();
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
                    else
                    {
                        Package pack_resp = new Package("no_video");
                        Connector conn_resp = Client.find_conn(Client.conn_video_data, pack.from);
                        conn_resp.send(pack_resp);
                    }
                }
                else if (pack.type == "request_video" && num_frac==0)
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
                        List<Connector> conn_resp = Client.find_conns(Client.conn_video_data, pack.from);

                        conn_resp[0].send(pack_resp);
                    }
                    else
                    {
                        BitmapStream bitmapStream = reader.loadBitmapStream_count(Int32.Parse(header[0]));

                        CompressedBitmapStream comp_bitmapStream = new CompressedBitmapStream(bitmapStream);

                        //把object serialize成memoryStream,再到byte[]
                        MemoryStream stream = new MemoryStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, comp_bitmapStream);

                        byte[] data = stream.ToArray();

                        //byte[] m = Compress.compress(data);

                        List<byte[]> data_list = new List<byte[]>();

                        int offset=(int)data.Length / Client.NUM_FRACTION;

                        for (int i = 0; i < Client.NUM_FRACTION; i++)
                        {
                            byte[] tmp_data;
                            if(i<Client.NUM_FRACTION-1){
                                tmp_data= new byte[offset];
                            }
                            else{
                                tmp_data = new byte[data.Length - (Client.NUM_FRACTION - 1) * offset];
                            }
                            System.Buffer.BlockCopy(data, i* offset, tmp_data, 0, tmp_data.Length);
                            data_list.Add(tmp_data);
                        }

                        //发回去

                        List<Connector> conn_resp = Client.find_conns(Client.conn_video_data, pack.from);

                        if (conn_resp.Count == 0) { return; }

                        for (int i = 0; i < Client.NUM_FRACTION; i++)
                        {
                            Package pack_resp = new Package("video_resp");
                            pack_resp.data = data_list[i];
                            if (conn_resp[i].ip_send == "") { return; }
                            send_thread a = new send_thread(conn_resp[i],pack_resp);
                            a.start();
                        }
                    }
                }
            }
        }

        class send_thread
        {
            public Connector conn;
            public Package pack;
            public send_thread(Connector conn1, Package pack1)
            {
                conn = conn1;
                pack = pack1;
            }
            public void send()
            {
                conn.send(pack);
            }
            public void start()
            {
                Thread a = new Thread(send);
                a.Start();
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
                    if (Client.video[num_frac] == 0 && Client.video_writing[num_frac] == 0)
                    {
                        Client.video_writing[num_frac] = 1;
                        Client.video_stream_fractions[num_frac] = pack.data;
                        Client.video[num_frac] = 1;
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

                }

            }
        }

        //只有server在监听这个=。=
        public void listen_danmu_server() {
            if (Client.isServer == 0) { return; }
            while (true)
            {
                Package pack=conn.recv();
                if(pack==null){continue;}
                if(pack.type=="danmu_ask"){
                    /*
                     * header:
                     * [0]filename
                     * 
                     * return:
                     * data -> serialized danmulist
                     */
                    DanmuList danmuList = new DanmuList();

                    String[] tmp = pack.header[0].Split('\\');

                    danmuList.readFromFile(tmp[tmp.Length-1]);

                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, danmuList);

                    byte[] data = stream.ToArray();

                    Package pack_resp = new Package("danmu_resp");
                    pack_resp.data = data;
                    Connector conn_resp = Client.find_conn(Client.conn_danmu_client, pack.from);

                    conn_resp.send(pack_resp);
                }
                else if (pack.type == "danmu_add")
                {
                    /*
                     * header:
                     * [0] filename
                     * [1] num
                     * [2] content
                     * 
                     * no need to resp
                     */
                    DanmuList.appendDanmu(Int32.Parse(pack.header[1]), pack.header[2], pack.header[0]);
                }
            }
        }

        //只有client在监听这个=。=
        public void listen_danmu_client() {
            if (Client.isServer == 1) { return; }
            while (true)
            {
                Package pack = conn.recv();
                if (pack.type == "danmu_resp")
                {
                    MemoryStream stream = new MemoryStream(pack.data);
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;
                    Client.danmuList = (DanmuList)formatter.Deserialize(stream);
                }
            }
        }
        
        public void listen_audio_header() {
            while (true)
            {
                Package pack = conn.recv();
                if (pack.type == "request_audio")
                {
                    /*
                     * header
                     * [0] count
                     * 
                     */
                    audio_resp = audio_reader.loadWaveOutStream(Int32.Parse(pack.header[0]));
                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, audio_resp);
                    byte[] data = stream.ToArray();

                    Package pack_resp = new Package("audio_resp");
                    pack_resp.data = data;
                    Connector conn_resp = Client.find_conn(Client.conn_audio_data, pack.from);
                    if (conn_resp != null) {
                        conn_resp.send(pack_resp);
                    }
                }
                else if (pack.type == "ask_audio")
                {
                    /*
                     * header
                     * [0] count
                     * 
                     * data
                     * format
                     * 
                     */
                    /*audio_reader.loadFile(Local.ref_addr + @"audio\" + pack.header[0]);
                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, audio_reader.format);
                    byte[] data = stream.ToArray();
                    */
                    byte[] data = new byte[0]; 
                    try
                    {
                         data = File.ReadAllBytes(pack.header[0]);
                    }
                    catch
                    {   
                    }

                    Package pack_resp = new Package("audio_format");
                    pack_resp.data = data;
                    pack_resp.header.Add( pack.header[0]);
                    Connector conn_resp = Client.find_conn(Client.conn_audio_data, pack.from);
                    conn_resp.send(pack_resp);
                }
            }
        }
        public void listen_audio_data() {
            while (true)
            {
                Package pack = conn.recv();
                if (pack.type == "audio_resp")
                {
                    MemoryStream stream = new MemoryStream(pack.data);
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;
                    
                    if (Client.audio_writing == 0 && Client.audio==0)
                    {
                        Client.audio_writing = 1;
                        Client.audio_stream = (WaveOutStream)formatter.Deserialize(stream);
                        Client.audio = 1;
                    }
                }
                else if (pack.type == "audio_format")
                {
                    MemoryStream stream = new MemoryStream(pack.data);
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;

                    
                    if (Client.audio_writing == 0 && Client.audio == 0)
                    {
                        Client.audio_writing = 1;
                        //Client.audio_format = (WavFormat)formatter.Deserialize(stream);
                        if (pack.data.Length == 0) { Client.audio = 1; }
                        File.WriteAllBytes(pack.header[0], pack.data);
                        Client.audio = 1; 
                    }
                    
                }
            }
        }
    }
}
