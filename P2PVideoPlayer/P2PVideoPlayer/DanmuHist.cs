using System;
using System.Text;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace WpfApplication1{
  class DanmuHist
{
	public List<Tuple <String, double> > hist;
	
	public DanmuHist(){
		hist = new List<Tuple <String, double> >();
	}
	
	public void addDanmu(String content, double time_interval){

		hist.Add(new Tuple <String, double>(content, time_interval));
	}
	
	public void play(Grid curtain, DanmakuCurtain dmkCurt){
		foreach(Tuple <String, double> item in hist){
			Timer timer = new Timer(item.Item2);
			timer.Elapsed += new ElapsedEventHandler(
				delegate(object source, ElapsedEventArgs e){
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
					{
						dmkCurt.Shoot(curtain,item.Item1);
					}));
				}
			);
			timer.AutoReset = false;
			timer.Enabled = true;
		}
	}
	
}
}
