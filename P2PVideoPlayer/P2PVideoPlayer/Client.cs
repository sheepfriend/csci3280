using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WpfApplication1
{
    /*
     * To use this class, please call:
     * 
     * at the beginning: run()
     * when a new video is needed: new_video(name)
     * 
     * Default port number:
     * 10000: main program
     * 10001: listener for video
     * 10002: listener for audio
     * 10003: listener for danmu
     * 10004: listener for creating new connection
     * 
     * cannot deal with danmu currently....
     * potential conflict on List<String> clients?
     * global variable for server ip?
     * 
     * server port: 10000+
     * 
     * I don't want to maintain a table for port number, thus upd is used...
     * As long as the request has sent, close the connection.
     * 
     */
    class Client
    {
        public static String self_ip;
        public static String server_ip;
        public static List<String> clients;
        public Connector conn;

        public static int video;
        public static int audio;
        public static int video_no;
        public static int audio_no;
        public static int ask;
        public static String name;

        public Client()
        {
            clients = new List<string>();
            conn = new Connector();
            self_ip = "";
            server_ip = "";
        }

        public void run()
        {            
            Thread thread10004 = new Thread(listen_main);
            Thread thread10001 = new Thread(listen_video);
            Thread thread10002 = new Thread(listen_audio);
            Thread thread10000 = new Thread(listen_ask);
            thread10000.Start();
            thread10001.Start();
            thread10002.Start();
            thread10004.Start();
        }

        public void new_video(String name_)
        {
            name = name_;
            Thread thread_ask_all = new Thread(ask_all);
        }

        public void update_client_list(Package pack)
        {
            //udpate client list and connect to other clients
            clients = new List<String>(pack.connectList);
        }

        public void listen_main()
        {
            while (true)
            {
                Package pack = conn.recv(10004);
                if (pack == null) { continue; }
                if(pack.type=="ip_list_info"){
                    update_client_list(pack);
                }
            }
        }

        public void listen_video()
        {
            while (true)
            {
                Package pack = conn.recv(10001);
                if (pack == null) { continue; }
                if(pack.type=="video_resp"){
                    /*
                     * header for video:
                     * Name: video name
                     * Number: the number of the chunk
                     * Type: .*
                     */
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    /*
                     * target path:
                     * ./src/Name/Number
                     */
                    //check if src exists
                    if (!System.IO.Directory.Exists("src"))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory("src");
                            System.IO.Directory.CreateDirectory("src/"+header[0].Item2);
                        }
                        catch(Exception e)
                        {
                            Console.Write(e);
                            return;
                        }
                    }
                    //check whether the directory for this video exists
                    if (!System.IO.Directory.Exists("src/" + header[0].Item2))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory("src/" + header[0].Item2);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                            return;
                        }
                    }
                    //write the video chunk
                    try
                    {
                        System.IO.File.WriteAllBytes("src/"+header[0].Item2+"/"+header[1].Item2+"."+header[2].Item2, pack.video);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                        return;
                    }
                }
                else if (pack.type == "ask_video")
                {
                    /*
                     * header for request:
                     * [1] Name
                     * [2] Number: check whether this number exists
                     * [3] Type: .*
                     * 
                     * if the chunk exists, return a has_audio package
                     * if not, return a no_audio package
                     */
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    if (System.IO.File.Exists("src/" + header[0].Item2 + "/" + header[1].Item2 + "." + header[2].Item2))
                    {
                        //has_audio is listened by the main thread not this thread
                        Package pack_res = new Package("has_video");
                        pack_res.header = new List<Tuple<string, string> >(pack.header);
                        pack_res.from = pack.to;
                        pack_res.to = pack.from;
                        conn.send(pack.from, 10000, pack_res);
                    }
                    else
                    {
                        //no_audio is listened by the main thread not this thread
                        Package pack_res = new Package("no_video");
                        pack_res.header = new List<Tuple<string, string> >(pack.header);
                        pack_res.from = pack.to;
                        pack_res.to = pack.from;
                        conn.send(pack.from, 10000, pack_res);
                    }
                }
                else if (pack.type == "request_video")
                {
                    /*
                     * header for request:
                     * [1] Name
                     * [2] Number: check whether this number exists
                     * [3] Type: .*
                     * 
                     * send the audio chunk to the dest addr
                     */
                    Package pack_res = new Package("video_resp");
                    pack_res.header = new List<Tuple<string, string> >(pack.header);
                    pack_res.from = pack.to;
                    pack_res.to = pack.from;
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    try
                    {
                        byte[] data = System.IO.File.ReadAllBytes("src/" + header[0].Item2 + "/" + header[1].Item2 + "." + header[2].Item2);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                    conn.send(pack.from, 10001, pack_res);
                }
            }
        }

        public void listen_audio()
        {
            while (true)
            {
                Package pack = conn.recv(10002);
                if (pack == null) { continue; }
                if (pack.type == "audio_resp")
                {
                    /*
                     * header for video:
                     * Name: video name
                     * Number: the number of the chunk
                     * Type: .*
                     */
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    /*
                     * target path:
                     * ./src/Name/Number
                     */
                    //check if src exists
                    if (!System.IO.Directory.Exists("src"))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory("src");
                            System.IO.Directory.CreateDirectory("src/" + header[0].Item2);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                            return;
                        }
                    }
                    //check whether the directory for this audio exists
                    if (!System.IO.Directory.Exists("src/" + header[0].Item2))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory("src/" + header[0].Item2);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                            return;
                        }
                    }
                    //write the audio chunk
                    try
                    {
                        System.IO.File.WriteAllBytes("src/" + header[0].Item2 + "/" + header[1].Item2 + "." + header[2].Item2, pack.audio);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                }
                else if (pack.type == "ask_audio")
                {
                    /*
                     * header for request:
                     * [1] Name
                     * [2] Number: check whether this number exists
                     * [3] Type: .*
                     * 
                     * if the chunk exists, return a has_audio package
                     * if not, return a no_audio package
                     */
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    if (System.IO.File.Exists("src/" + header[0].Item2 + "/" + header[1].Item2 + "." + header[2].Item2))
                    {
                        //has_audio is listened by the main thread not this thread
                        Package pack_res = new Package("has_audio");
                        pack_res.header = new List<Tuple<string, string> >(pack.header);
                        pack_res.from = pack.to;
                        pack_res.to = pack.from;
                        conn.send(pack.from, 10000, pack_res);
                    }
                    else
                    {
                        //no_audio is listened by the main thread not this thread
                        Package pack_res = new Package("no_audio");
                        pack_res.header = new List<Tuple<string, string> >(pack.header);
                        pack_res.from = pack.to;
                        pack_res.to = pack.from;
                        conn.send(pack.from, 10000, pack_res);
                    }
                }
                else if (pack.type == "request_audio")
                {
                    /*
                     * header for request:
                     * [1] Name
                     * [2] Number: check whether this number exists
                     * [3] Type: .*
                     * 
                     * send the audio chunk to the dest addr
                     */
                    Package pack_res = new Package("audio_resp");
                    pack_res.header = new List<Tuple<string, string> >(pack.header);
                    pack_res.from = pack.to;
                    pack_res.to = pack.from;
                    List<Tuple<String, String> > header = new List<Tuple<String, String> >(pack.header);
                    try
                    {
                        byte[] data = System.IO.File.ReadAllBytes("src/" + header[0].Item2 + "/" + header[1].Item2 + "." + header[2].Item2);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                    conn.send(pack.from, 10002, pack_res);
                }
            }
        }

        public void ask_client()
        {
            Package pack = new Package("ip_list");
            pack.from = self_ip;
            pack.to = server_ip;
            conn.send(pack.to, 10010, pack);
        }

        //send a request to ask a video chunk
        public void ask_one(int num)
        {
            video = 0;
            audio = 0;
            video_no = 0;
            audio_no = 0;
            /*
             * Procedure to ask for a video:
             * [1] send ask request to all clients in the client list
             * [2] waiting for replies of other clients(set time out)
             * [3] check the response of other clients
             * [4] repeat the request for all chunks of the video(if all other clients does not have this chunk => no more chunks, return)
             */
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i]!=self_ip){
                    //send video ask
                    Package pack = new Package("ask_video");
                    pack.from = self_ip;
                    pack.to = clients[i];
                    Tuple<String, String> name_info = new Tuple<string, string>("Name", name);
                    Tuple<String, String> number_info = new Tuple<string, string>("Number", ""+num);
                    Tuple<String, String> type_info = new Tuple<string, string>("Type", "avi");
                    pack.header.Add(name_info);
                    pack.header.Add(number_info);
                    pack.header.Add(type_info);
                    conn.send(pack.to, 10001, pack);

                    //send audio ask
                    Package pack1 = new Package("ask_audio");
                    pack1.from = self_ip;
                    pack1.to = clients[i];
                    Tuple<String, String> name_info1 = new Tuple<string, string>("Name", name);
                    Tuple<String, String> number_info1 = new Tuple<string, string>("Number", "" + num);
                    Tuple<String, String> type_info1 = new Tuple<string, string>("Type", "mp3");
                    pack1.header.Add(name_info1);
                    pack1.header.Add(number_info1);
                    pack1.header.Add(type_info1);
                    conn.send(pack1.to, 10002, pack1);
                }
            }
        }


        public void listen_ask(){
            Package pack_res;
            Package pack_response;
            while (true)
            {
                pack_res = conn.recv(10000);
                if (pack_res == null) { continue; }
                switch (pack_res.type)
                {
                    case "has_audio":
                        if (audio > 0 || pack_res.header[1].Item2!=ask+"") { break; }
                        else{
                            pack_response = new Package("request_audio");
                            pack_response.from = pack_res.to;
                            pack_response.to = pack_res.from;
                            pack_response.header = new List<Tuple<string, string> >(pack_res.header);
                            conn.send(pack_response.to, 10002, pack_response);
                            audio = 1;
                        }
                        break;
                    case "no_audio":
                        audio_no++;
                        break;
                    case "has_video":
                        if (video > 0 || pack_res.header[1].Item2 != ask + "") { break; }
                        else
                        {
                            pack_response = new Package("request_video");
                            pack_response.from = pack_res.to;
                            pack_response.to = pack_res.from;
                            pack_response.header = new List<Tuple<string, string> >(pack_res.header);
                            conn.send(pack_response.to, 10001, pack_response);
                            video = 1;
                        }
                        break;
                    case "no_video":
                        video_no++;
                        break;
                    default:
                        break;
                }
            }
        }

        public void ask_all()
        {
            ask = 0;
            while (true)
            {
                ask_one( ask);
                //wait for all response
                //potential bug: what if a connection is dead?
                while(!((video==1 && audio ==1) || (video_no != clients.Count-1) || (audio_no!=clients.Count-1))){}
                ask++;
                if ((video_no != clients.Count - 1) || (audio_no != clients.Count - 1)) { return; }
            }
        }


    }
}
