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
        public List<video_info> playList { get; set; }
        public int currentNum {get; set; }
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
        public void select_play(String item)
        {
            int index = -1;
            for (int i = 0; i < playList.Count; i++)
            {
                if (item == playList[i].fileName) { index= i; }
            }
            if (index < 0 || index >= totalNum) { return; }
            else
            {
                currentNum = index;
                currentPlay = playList[currentNum].path;
            }
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
         public List<String> print() {
            List<String> result = new List<String>();
            int i;
            for (i = 0; i < totalNum; i++) {
                result.Add(playList[i].fileName);
            }
            return result;
        }

        public void delete(int currentnum)
        {
            video_info to_delete_vid = playList[currentnum];
            //remove from document
            XmlNode to_delete = doc.SelectSingleNode(String.Format("/Karaoke/video[@id='{0}']",to_delete_vid.id), man);
            to_delete.ParentNode.RemoveChild(to_delete);
            //remove from list
            playList.Remove(to_delete_vid);
            totalNum--;
        }
    }
}
