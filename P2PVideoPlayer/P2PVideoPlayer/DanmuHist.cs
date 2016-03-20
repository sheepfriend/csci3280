using System;
using System.Text;
using System.IO;
using System.Timers;

namespace WpfApplication1{
  class DanmuHist
{
	public List<Tuple <String, double> > hist;
	
	public DanmuHist(){
		hist = new List<Tuple <String, double> >();
	}
	
	public void addDanmu(String content, double time_interval){
		hist.Add(Tuple <String, double>(content, interval));
	}
	
	public void play(Grid curtain, DanmakuCurtain dmkCurt){
		for(Tuple <String, double> item in hist){
			timer = new System.Timers.Timer(item.Item2);
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
