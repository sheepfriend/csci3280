using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace WpfApplication1
{
    class media_info
    {
        private String inputList;
        public video_info currentPlay { get; set;}
        public static XmlDocument doc { set; get; }
        public static XmlNamespaceManager man { set; get; }
        private Dictionary<String, video_info> _name_to_list;
        public Dictionary<String,video_info> name_to_list { get { return _name_to_list; } } 
        //initialize object
        public media_info(String listaddr) {
            currentPlay = null;
            _name_to_list = new Dictionary<string, video_info>();
            inputList = listaddr;
            load();
        }

        ~media_info()
        {
            doc.Save(inputList);
        }

        public void select_play(String item)
        {
            currentPlay = name_to_list[item];
        }
      

        //add a new vedio into the list
  
        public void add(String path) {
            video_info tmp = null;
            char[] separatingChars1 = { '\\', '/' };
            string[] Names = path.Split(separatingChars1, StringSplitOptions.RemoveEmptyEntries);
            string fileName = Names[Names.Length - 1];
            //deduplication goes first
            foreach (video_info myvideo in name_to_list.Values)
            {
                if(myvideo.fileName == fileName)
                {
                    tmp = myvideo;
                    break;
                }
            }
            //may be a real new one
            if(tmp == null)
                tmp = new video_info();
            tmp.readFromAddr(path);
            name_to_list.Add(tmp.fileName, tmp);
            
        }

        //load the initial input video list...
        public void load() {
            doc = new XmlDocument();
            doc.Load(inputList);
            man = new XmlNamespaceManager(doc.NameTable);
            man.AddNamespace("kara", "list");
            XmlElement root = doc.DocumentElement;
            XmlNodeList videos = root.SelectNodes("/Karaoke/video", man);

            foreach (XmlNode video in videos)
            {
                video_info tmp = new video_info();
                tmp.load(video);
                name_to_list.Add(tmp.fileName, tmp);
            }
            
        }

        //return a string of info
         public HashSet<String> print() {
            HashSet<String> result = new HashSet<String>();
            
            foreach(video_info item in name_to_list.Values) { 
                result.Add(item.fileName);
            }
            return result;
        }

        public void delete(string filename)
        {

            video_info to_delete_vid = name_to_list[filename];
            //remove from document
            XmlNode to_delete = doc.SelectSingleNode(String.Format("/Karaoke/video[@id='{0}']",to_delete_vid.id), man);
            to_delete.ParentNode.RemoveChild(to_delete);
            //remove from list
            name_to_list.Remove(to_delete_vid.fileName);
            
        }
    }
}
