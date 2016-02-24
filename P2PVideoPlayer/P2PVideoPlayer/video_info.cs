using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static Microsoft.VisualBasic.Interaction;

namespace WpfApplication1
{
    class video_info
    {
        public String fileName {get; set; }
        public String title { get; set; }
        public String author { get; set; }
        public String album { get; set; }
        public String path { get; set; }
        
        //initialize a video object
        public video_info()
        {
            fileName = "N/A";
            title = "N/A";
            author = "N/A";
            album = "N/A";
            path = "N/A";
        }
                    
              
        //load a line of input
        //Filename:'1.wav' 'title' 'author' 'album'
        public void load(XmlNode video) {
            foreach (XmlNode entry in video.ChildNodes)
            {
                string cont = entry.InnerText;
                switch (entry.Name)
                {
                    case "path":
                        path = cont;
                        char[] separatingChars1 = { '\\', '/' };
                        string[] Names = path.Split(separatingChars1, StringSplitOptions.RemoveEmptyEntries);
                        fileName = Names[Names.Length - 1];
                        break;
                    case "title":
                        title = cont;
                        break;
                    case "author":
                        author = cont;
                        break;
                    case "album":
                        album = cont;
                        break;
                    default:
                        break;
                }
            }
        }

        public void readFromAddr(String text)
        {
            XmlDocument doc = media_info.doc;

            char[] separatingChars1 = { '\\', '/' };
            string[] name = text.Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
            fileName = name[name.Length - 1];
            //ask for title, author, album
            title = InputBox("What is the title of the music?",fileName+": Title","N/A");
            author = InputBox("Who is the author of the music?", fileName + ": Author", "N/A");
            album = InputBox("What is the album of the music?", fileName + ": Album", "N/A");
            //XElement last = myroot.Descendants("video").Last();
            XmlNodeList l = doc.SelectNodes("/Karaoke/video", media_info.man);
            XmlNode last = l[l.Count - 1];
           
            int lastid = Int32.Parse(last.Attributes["id"].Value);
            int newid = lastid + 1;
            XmlElement newdata = doc.CreateElement("video");
            newdata.SetAttribute("id", newid.ToString());
            doc.DocumentElement.InsertAfter(newdata, last);
             
            //XDocument temp;
            //string content = "<video id=\"" + newid + "\">" +
            //                   "<path>"+text+"</path>"+
            //                  "<title>" +title+"</title>"+
            //                  "<author>" + author + "</author>"+ 
            //                  "<album>" + album + "</album>" +
            //                   "</video>";
            //temp = XDocument.Parse(content);
            //last.AddAfterSelf(new XElement(temp.ToString()));
        }

    }
}
