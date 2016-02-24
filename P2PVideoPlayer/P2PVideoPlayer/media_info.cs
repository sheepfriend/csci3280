using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WpfApplication1
{
    class media_info
    {
        private String inputList;
        public String currentPlay { get; set;}
        public List<video_info> playList { get; }
        private int currentNum;
        private int totalNum;
        public static XmlDocument doc { set; get; }
        public static XmlNamespaceManager man { set; get; }
        //initialize object
        public media_info(String listaddr) {
            currentPlay = "";
            totalNum = 0;
            inputList = listaddr;
            playList = new List<video_info>();
            load();
        }
        ~media_info()
        {
            doc.Save(inputList);
            
        }
        //load next video
        //then mediaElement use .currentPlay to load the URI
        public void next() {
            if (totalNum == 0) {
                currentNum = 0;
                currentPlay = "";
            }
            else if (currentNum == totalNum-1)
            {
                currentNum = 0;
                currentPlay = playList[currentNum].path;
            }
            else {
                currentNum += 1;
                currentPlay = playList[currentNum].path;
            }
        }

        //load next video
        //then mediaElement use .currentPlay to load the URI
        public void pre()
        {
            if (totalNum == 0)
            {
                currentNum = 0;
                currentPlay = "";
            }
            else if (currentNum == 0)
            {
                currentNum = totalNum - 1;
                currentPlay = playList[currentNum].path;
            }
            else
            {
                currentNum -= 1;
                currentPlay = playList[currentNum].path;
            }
        }

        //add a new vedio into the list
        //may need deduplication?
        public void add(String path) {
            video_info tmp = new video_info();
            tmp.readFromAddr(path);
            playList.Add(tmp);
            totalNum += 1;
        }

        //load the initial input video list...
        public void load() {
            //String line;
            //System.IO.StreamReader file = new System.IO.StreamReader(inputList);
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
                playList.Add(tmp);
                totalNum += 1;
            }
            
            if(totalNum>0)
            {
                currentNum = 0;
                currentPlay = playList[currentNum].path;
            }
        }

        //return a string of info
        public String print() {
            String result = "";
            int i;
            for (i = 0; i < totalNum; i++) {
                result += playList[i].fileName + "\n";
            }
            return result;
        }
    }
}
