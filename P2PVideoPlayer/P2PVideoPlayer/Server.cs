using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class Server
    {
        public  List<String> clients;
        public List<Tuple<String,List<String> > > playList;
        public Connector conn;
        public Server()
        {
            clients = new List<string>();
            playList = new List<Tuple<string,List<string> > >();
            conn = new Connector();
        }
        public void load_play_list(String path)
        {
            //read play list from a file in hard disk
        }
        public void emit_list(Package pack)
        {
            for (int i = 0; i < clients.Count;i++ )
            {
                Package pack_res = new Package("ip_list_info");
                pack_res.connectList = clients;
                pack_res.from = pack.to;
                pack_res.to = clients[i];
                conn.send(clients[i], 10004, pack_res);
            }
        }
        public void search_video(Package pack)
        {
            for (int i = 0; i < playList.Count; i++)
            {
                String video_name = pack.header[0].Item2;
                if (video_name == playList[i].Item1)
                {
                    // find the target video
                    Package pack_res = new Package("");
                }
            }
            //target video not find
        }
        public void listen()
        {
                Package pack = conn.recv(10010);
                switch (pack.type)
                {
                    case "ip_list":
                        //broadcast clientList to all clients
                        emit_list(pack);
                        break;
                    case "ask_video":
                        search_video(pack);
                        break;
                    case "add_video":
                        break;
                    case "connection_fail":
                        break;
                    default:
                        break;
                }
        }
    }
}
