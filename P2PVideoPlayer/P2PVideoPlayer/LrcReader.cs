using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

public class LrcReader
{
    public List<List<String> > words;
    public List<List<String>> other_info;
    public String file_path;

	public LrcReader()
	{
        file_path="";
        words = new List<List<String>>();
        other_info = new List<List<String>>();
	}

    // given a path, load the lrc and put all things into words and other_info
    public void load(String path) {
        file_path = path;
        System.IO.StreamReader sr = new System.IO.StreamReader(path);
        String s;
        String content;
        String mmstring;
        String ssstring;
        int mm;
        double ss;
        while ((s = sr.ReadLine()) != null)
        {
            if (Char.IsNumber(s[1]))
            {
                do
                {
                    mmstring = s.Substring(s.IndexOf('[') + 1, s.IndexOf(':') - s.IndexOf('[') - 1);
                    mm = Int32.Parse(mmstring);

                    ssstring = s.Substring(s.IndexOf(':') + 1, s.IndexOf(']') - s.IndexOf(':') - 1);
                    ss = Double.Parse(ssstring);

                    content = s.Substring(s.LastIndexOf(']') + 1);

                    List<String> tmp = new List<String>();
                    tmp.Add(mm * 60 + ss+"");
                    tmp.Add(content);

                    words.Add(tmp);
                    s = s.Substring(s.IndexOf(']') + 1);

                } while (s.Contains('[')); //Read again if there is more than one [] on one line
            }
            else {
                do
                {
                    mmstring = s.Substring(s.IndexOf('[') + 1, s.IndexOf(':') - s.IndexOf('[') - 1);
                    ssstring = s.Substring(s.IndexOf(':') + 1, s.IndexOf(']') - s.IndexOf(':') - 1);

                    List<String> tmp = new List<String>();
                    tmp.Add(mmstring);
                    tmp.Add(ssstring);

                    other_info.Add(tmp);
                } while (s.Contains('['));
            }
        }
    }
    
    // given time, return the text for the current time
    public String get_current_text(double time){
        // currently not display contents in other_info
        String result = "";
        List<String> tmp_tuple;
        int index=0;
        if (words.Count == 0) {}
        else if (Double.Parse(words[words.Count-1][0]) <= time)
        {
            result = words[words.Count - 1][1];
        }
        else {
            do{
                tmp_tuple = (List<String>)words[index];
                result=tmp_tuple[1];
                index += 1;
            }while(words.Count>=index);
        }
        return result;
    }
}
