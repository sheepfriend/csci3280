using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class media_info
    {
        private String inputList;
        public String currentPlay;
        private List<video_info> playList;
        private int currentNum;
        private int totalNum;

        //initialize object
        public media_info(String listaddr) {
            currentPlay = "";
            totalNum = 0;
            inputList = listaddr;
            playList = new List<video_info>();
            load();
        }

        //load next video
        //then mediaElement use .currentPlay to load the URI
        public void next() {
            if (totalNum == 0) {
                currentNum = 0;
                currentPlay = "";
            }
            else if (currentNum == totalNum)
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

        //load the initial input vedio list...
        public void load() {
            String line;
            System.IO.StreamReader file = new System.IO.StreamReader(inputList);
            while ((line = file.ReadLine()) != null)
            {
                video_info tmp = new video_info();
                tmp.load(line);
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
