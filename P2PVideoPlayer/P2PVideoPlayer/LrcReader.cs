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
    public List<Tuple<Double, String> > words;
    public List<Tuple<String, String>> other_info;
    public String file_path;

	public LrcReader()
	{
        file_path="";
        words = new List<Tuple<Double, String>>();
        other_info = new List<Tuple<String, String>>();
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

                    words.Add(new Tuple<Double, String>(mm * 60 + ss, content));
                    s = s.Substring(s.IndexOf(']') + 1);

                } while (s.Contains('[')); //Read again if there is more than one [] on one line
            }
            else {
                do
                {
                    mmstring = s.Substring(s.IndexOf('[') + 1, s.IndexOf(':') - s.IndexOf('[') - 1);
                    ssstring = s.Substring(s.IndexOf(':') + 1, s.IndexOf(']') - s.IndexOf(':') - 1);

                    other_info.Add(new Tuple<String, String>(mmstring, ssstring));
                } while (s.Contains('['));
            }
        }
    }
    
    // given time, return the text for the current time
    public String get_current_text(double time){
        // currently not display contents in other_info
        String result = "";
        Tuple<Double,String> tmp_tuple;
        int index=0;
        if (words.Count == 0) {}
        else if (((Tuple<Double, String>)words.Last()).Item1 <= time)
        {
            result = ((Tuple<Double, String>)words.Last()).Item2;
        }
        else {
            do{
                tmp_tuple=(Tuple<Double,String>)words.ElementAt(index);
                result=tmp_tuple.Item2;
                index += 1;
            }while(words.Count>=index);
        }
        return result;
    }
}
