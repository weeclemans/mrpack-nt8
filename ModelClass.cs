
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;

using System.IO;
using System.Xml.Serialization;



namespace NinjaTrader.NinjaScript.Indicators.MRPack
{
	[Serializable]
	public class Instrument_Data
	{
		public string InstrumentName;
		public List<RangeProfile_Data> RangeProfilesList;
		
		public Instrument_Data()
		{}
		public Instrument_Data(string name)
		{
			InstrumentName = name;
			RangeProfilesList= new List<RangeProfile_Data>();
		}
	}
	
	
	[Serializable]
	public class RangeProfile_Data
	{
		public string Tag;
		public double Top_price;
		public double Bottom_price;
		public DateTime Left_Time;
		public DateTime Right_Time;
		public int ProfileType;
		public bool IsExtended;
		
		public RangeProfile_Data()
		{}
		public RangeProfile_Data(string tag,double topPrice, DateTime startTime, double botPrice, DateTime endTime, int type, bool extend)
		{
			Tag = tag;
			Top_price=topPrice;
			Bottom_price=botPrice;
			Left_Time=startTime;
			Right_Time=endTime;
			ProfileType = type;
			IsExtended= extend;
		}
	}
	
	
	
	public class Model
	{
		public List<Bar> ListOfBar;
		
		public CurrentBar currentBar;
		
		private Stack<Print> stackForMarketStop;
		
		public List<MarketStop> ListOfMarketStop;
		
		public HistogrammClass Histogramm;
		
		public DayProfileClass DayProfile;
		
		public DateTime MaxVerticalVolumeBar;
		
		public MRIndicator parent;
		
		public List<RangeProfile2> RangeProfiles;
		
		public List<double> TandS_AllPrints_price;
		public List<int> TandS_AllPrints_volume;
		
		public List<double> TandS_FilterPrints_price;
		public List<int> TandS_FilterPrints_volume;
		public List<DateTime> TandS_FilterPrints_time;
		
		
		private Stack<Print> stackForTickAggregator;
		private double TickAggregator_TopPrice;
		private double TickAggregator_BotPrice;
		
		public List<TickAggregatorElement> TickAggregatorElements;
		public int MaxTickAggregatorVolume;
		
		
		
		
		public bool Range_Profile_Text_OnOff;	// Edited by PD
		public Brush Range_Profile_Text_Color;	// Edited by PD
		public Brush Input_ProfileRange_Inside_Color;
		public Brush Input_ProfileRange_POC_Color;
		public bool Input_RangeProfile_BidAsk_OnOff;
		public bool Input_RangeProfile_ExtendedLine_OnOff;
		public Brush Input_ProfileRange_Border_Color;
		public Brush Input_ProfileRange_Inside_Bid_Color;
		public Brush Input_ProfileRange_Inside_Ask_Color;
		public int Profile_Text_Opacity;
		public int Profile_Opacity;
		
		public Model()
		{}
		
		public Model(MRIndicator indic)
		{
			ListOfBar = new List<Bar>();
			
			currentBar = new CurrentBar();
			
			stackForMarketStop = new Stack<Print>();
			
			ListOfMarketStop = new List<MarketStop>();
			
			Histogramm = new HistogrammClass();
			
			DayProfile = new DayProfileClass();
			
			MaxVerticalVolumeBar = new DateTime();
			
			parent = indic;
			
			RangeProfiles= new List<RangeProfile2>();
			
			TandS_AllPrints_price = new List<double>();
			TandS_AllPrints_volume = new List<int>();
			
			TandS_FilterPrints_price = new List<double>();
			TandS_FilterPrints_volume = new List<int>();
			TandS_FilterPrints_time = new List<DateTime>();
			
			stackForTickAggregator= new Stack<Print>();
			
			TickAggregatorElements = new List<TickAggregatorElement>();
			
			
		}
		
		public void DeleteProfile(string tag)
		{
			RangeProfiles.RemoveAll(p=>p.Tag==tag);
		}
		
		
		public void LoadProfiles()
		{
			if(File.Exists("RangeProfiles.xml"))
			{
				List<Instrument_Data> lst = new List<Instrument_Data>();
				XmlSerializer formatter = new XmlSerializer(typeof(List<Instrument_Data>));
				Instrument_Data instrument;
				using (FileStream fs = new FileStream("RangeProfiles.xml", FileMode.OpenOrCreate))
	            {
	                lst = (List<Instrument_Data>)formatter.Deserialize(fs);
					
					if(lst.Exists(i=>i.InstrumentName==parent.Instrument.FullName))
						instrument = lst.Find(i=>i.InstrumentName==parent.Instrument.FullName);
					else
						instrument = new Instrument_Data(parent.Instrument.FullName);
					
					foreach(RangeProfile_Data profile in instrument.RangeProfilesList)
					{
						parent.CreateNewRangeProfile(profile.Tag, profile.Left_Time,profile.Top_price,profile.Right_Time,profile.Bottom_price, profile.ProfileType,profile.IsExtended);
					}
				}
			}
		}
		
		public void SaveProfiles()
		{
			
			List<RangeProfile_Data> profiles = new List<RangeProfile_Data>();
			foreach(DrawingTools.RangeProfile2 profile in RangeProfiles)
			{
				RangeProfile_Data RP = new RangeProfile_Data(profile.Tag,
														profile.StartAnchor.Price,
														profile.StartAnchor.Time,
														profile.EndAnchor.Price,
														profile.EndAnchor.Time,
														profile.ProfileType,
														profile.ExtendedLine);
				profiles.Add(RP);
			}
			
			
			if(File.Exists("RangeProfiles.xml"))
			{
				List<Instrument_Data> lst = new List<Instrument_Data>();
				XmlSerializer formatter = new XmlSerializer(typeof(List<Instrument_Data>));
				Instrument_Data instrument;
				using (FileStream fs = new FileStream("RangeProfiles.xml", FileMode.OpenOrCreate))
	            {
	                lst = (List<Instrument_Data>)formatter.Deserialize(fs);
					
					
					if(lst.Exists(i=>i.InstrumentName==parent.Instrument.FullName))
					{
						instrument = lst.Find(i=>i.InstrumentName==parent.Instrument.FullName);
						lst.Remove(instrument);
					}
					else
						instrument = new Instrument_Data(parent.Instrument.FullName);
						
					
					
					instrument.RangeProfilesList = profiles;
					lst.Add(instrument);
					
				}
				
				using (FileStream fs = new FileStream("RangeProfiles.xml", FileMode.Create))
	            {
	               formatter.Serialize(fs, lst);
	            }
			}
			else
			{
				List<Instrument_Data> lst = new List<Instrument_Data>();
				XmlSerializer formatter = new XmlSerializer(typeof(List<Instrument_Data>));
				Instrument_Data instrument = new Instrument_Data(parent.Instrument.FullName);
				lst.Add(instrument);
				using (FileStream fs = new FileStream("RangeProfiles.xml", FileMode.Create))
	            {
	               formatter.Serialize(fs, lst);
	            }
			}
		}
		
		
		
		public bool CloseBar(DateTime date)
		{
			
			currentBar.Time = date;
			ListOfBar.Add(currentBar.GetStruct());
			
			/*if(!(ListOfBar.ContainsKey(MaxVerticalVolumeBar)))
				MaxVerticalVolumeBar = date;
			
			if(ListOfBar[date].Volume_sum>=ListOfBar[MaxVerticalVolumeBar].Volume_sum)
				MaxVerticalVolumeBar = date;*/
				
			
			currentBar.Clear();
			return true;
		}
		
		public void CloseDay()
		{
			DayProfile.Clear();
		}
		
		public void AddPrintToTickAggregator(DateTime time, int volume, double price, PrintType printType)
		{
			if(volume>=parent.Input_TickAggregator_TickLimit)
			{
				if(stackForTickAggregator.Count==0)
				{
					stackForTickAggregator.Push(new Print(time, volume, price, printType));
					TickAggregator_TopPrice = TickAggregator_BotPrice = price;
					return;
				}
				Print p  = stackForTickAggregator.Peek();
				
				TimeSpan t = time - p.Time;
				if(t.TotalMilliseconds>parent.Input_TickAggregator_Delay)
				{
					SumTickAggregatorStack();
					AddPrintToTickAggregator(time,volume,price, printType);
					return;
				}
				
				if(price>TickAggregator_TopPrice)
				{
					if(((int)((price-TickAggregator_BotPrice)/parent.TickSize))>parent.Input_TickAggregator_Range)
					{
						SumTickAggregatorStack();
						AddPrintToTickAggregator(time,volume,price, printType);
						return;
					}
				}
				if(price<TickAggregator_BotPrice)
				{
					if(((int)((TickAggregator_TopPrice-price)/parent.TickSize))>parent.Input_TickAggregator_Range)
					{
						SumTickAggregatorStack();
						AddPrintToTickAggregator(time,volume,price, printType);
						return;
					}
				}
				
				
				
				stackForTickAggregator.Push(new Print(time, volume, price, printType));
				if(price>TickAggregator_TopPrice)TickAggregator_TopPrice=price;
				if(price<TickAggregator_BotPrice)TickAggregator_BotPrice=price;
				
			}
		}
		
		public void SumTickAggregatorStack()
		{
			int sum = 0;//stackForTickAggregator.Sum(p=>p.Volume);
			int sumAsk = 0;
			int sumBid = 0;
			List<Print> tmp = stackForTickAggregator.ToList();
			foreach(Print p in tmp)
			{
				if(p.Type==PrintType.ASK) sumAsk+=p.Volume;
				if(p.Type==PrintType.BID) sumBid+=p.Volume;
				sum+=p.Volume;
			}
			
			if(sum>=parent.Input_TickAggregator_SummLimit)
			{
				Print l = stackForTickAggregator.Peek();
				List<Print> SortedList = tmp.OrderBy(o=>o.Volume).ToList();
				SortedList.Reverse();
				if(SortedList[0].Volume>=parent.Input_TickAggregator_BigPrint)
				{
					TickAggregatorElement t = new TickAggregatorElement(l.Time,sum,sumAsk,sumBid,l.Price,TickAggregator_BotPrice,TickAggregator_TopPrice,SortedList);
					TickAggregatorElements.Add(t);
					if(sum>=MaxTickAggregatorVolume)
						MaxTickAggregatorVolume=sum;
					if(parent.State==State.Realtime)
					{
						parent.CreateNewTickAggregatorEllipse(l.Time,l.Price,t);
						if(parent.Input_TickAggregator_AlertOnOff){
							//parent.PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\AutoTrail.wav");
							parent.Alert("MS:"+l.Time.ToString(), Priority.High, "TiCkAggregator", NinjaTrader.Core.Globals.InstallDir+@"\sounds\AutoTrail.wav", 2, Brushes.Black, Brushes.Yellow);
						}
					}
				}
				
			}
			stackForTickAggregator.Clear();
		}
		
		
		
		public void AddPrintToBar(double price, int volume, PrintType typeOfPrint)
		{
			if(currentBar.ListOfCurrentBar.ContainsKey(price))
			{
				currentBar.ListOfCurrentBar[price].AddPrintOnClaster(volume, typeOfPrint);
			}else{
				CurrentClaster claster = new CurrentClaster();
				currentBar.ListOfCurrentBar.Add(price,claster);
				currentBar.ListOfCurrentBar[price].AddPrintOnClaster(volume, typeOfPrint);
			}
			if(currentBar.pocPrice==0)
			{
				currentBar.pocPrice =  price;
			}
			else
			{
				if(currentBar.pocPrice != price && currentBar.ListOfCurrentBar[price].Volume_sum>=currentBar.ListOfCurrentBar[currentBar.pocPrice].Volume_sum)
				{
					currentBar.pocPrice = price;
				}
			}
			currentBar.dayPocPrice = DayProfile.pocPrice;
		}
		
		
		public void AddPrintToMarketStopStack(DateTime time, int volume, double price, PrintType printType)
		{
			if(stackForMarketStop.Count!=0)
			{
				Print p = stackForMarketStop.Peek();
				TimeSpan ts = timeDelta(p.Time, time);
				
				if(!(ts.TotalSeconds<1))
				{
					int sum = stackForMarketStop.Sum(pr => pr.Volume);
					if(sum>=parent.Input_MS_VolumeLimit)
					{
						double minPrice = stackForMarketStop.Min(pr => pr.Price);
						double maxPrice = stackForMarketStop.Max(pr => pr.Price);
						ListOfMarketStop.Add(new MarketStop(p.Time,sum,minPrice,maxPrice));
						if(parent.Input_MSAlert_OnOff){
							//parent.PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\AutoTrail.wav");
							parent.Alert("MS:"+p.Time.ToString(), Priority.High, "MarketStop", NinjaTrader.Core.Globals.InstallDir+@"\sounds\AutoTrail.wav", 2, Brushes.Black, Brushes.Yellow);
						}
					}
					stackForMarketStop.Clear();
				}
			}
			stackForMarketStop.Push(new Print(time,volume,price,printType));
		}
		
		public List<Bar> GetBarRange(int firstTime, int lastTime)
		{
			Bar b = currentBar.GetStruct();
			List<Bar> tmp;
			//return this.ListOfBar.Where(bar => bar.Key>=firstTime && bar.Key<=lastTime);
			if(firstTime<0)firstTime=0;
			if(lastTime<0)lastTime=0;
			int count=lastTime-firstTime;
			if(lastTime>this.ListOfBar.Count)
				count=this.ListOfBar.Count-firstTime;
			if(firstTime>this.ListOfBar.Count) 
			{
				firstTime=this.ListOfBar.Count-1;
				count =0;
			}
			if(firstTime==lastTime)
				count=0;
			
			tmp = this.ListOfBar.GetRange(firstTime, count);
			
			if(lastTime>this.ListOfBar.Count && firstTime<=this.ListOfBar.Count-1)
				tmp.Add(this.currentBar.GetStruct());
				
			return tmp;
		}
		
		
		public void AddPrintToTandS(DateTime time,int volume, double price)
		{
			TandS_AllPrints_price.Add(price);
			TandS_AllPrints_volume.Add(volume);
			if(TandS_AllPrints_price.Count>parent.Input_TandS_CountOrders)
			{
				TandS_AllPrints_price.RemoveAt(0);
				TandS_AllPrints_volume.RemoveAt(0);
			}
			
			if((volume>0 && volume>=parent.Input_TandS_FilterAsk)||(volume<0 && volume*(-1)>=parent.Input_TandS_FilterBid))
			{
				TandS_FilterPrints_price.Add(price);
				TandS_FilterPrints_volume.Add(volume);
				TandS_FilterPrints_time.Add(time);
			}
			
		}
		
		
		public TimeSpan timeDelta(DateTime prev, DateTime next)
		{
				DateTime d1 = new DateTime(next.Year,next.Month,next.Day,
											next.Hour,next.Minute,next.Second);
				DateTime d2 = new DateTime(prev.Year,prev.Month,prev.Day,
											prev.Hour,prev.Minute,prev.Second);
			return d1-d2;
		}
		
		
		
		public int Claster_Height;
		public int Claster_Width_Max;
		public void SetGraficDimensions(int cena1Y, int cena2Y, int bar1X, int bar2X)
		{
			int priceDelta = cena1Y-cena2Y;
			Claster_Height = priceDelta % 2 == 0 ? priceDelta - 1 : priceDelta - 2;
			if(Claster_Height<1) Claster_Height=1;
			//if(priceDelta==3) Claster_Height=3;
			
			
			int barDelta = bar1X-bar2X;
			Claster_Width_Max = barDelta -1;
			if(Claster_Width_Max<1) Claster_Width_Max=1;
			
			
		}
		
		
	
		
		public class CurrentBar
		{
			public SortedDictionary<double,CurrentClaster> ListOfCurrentBar;
			
			public double pocPrice;
			
			public double dayPocPrice;
			
			public DateTime Time;
			
			public CurrentBar()
			{
				ListOfCurrentBar = new SortedDictionary<double,CurrentClaster>();	
			}
			
			public Bar GetStruct()
			{
				Bar b = new Bar(this);
				return b;
			}
			
			public void Clear()
			{
				this.ListOfCurrentBar.Clear();
				this.pocPrice = 0;
				this.dayPocPrice = 0;
			}
			
		}
		
		
		public class HistogrammClass : CurrentBar
		{
			public void AddPrintToHistogramm(double price, int volume, PrintType typeOfPrint)
			{
				if(this.ListOfCurrentBar.ContainsKey(price))
				{
					this.ListOfCurrentBar[price].AddPrintOnClaster(volume, typeOfPrint);
				}else{
					CurrentClaster claster = new CurrentClaster();
					this.ListOfCurrentBar.Add(price,claster);
					this.ListOfCurrentBar[price].AddPrintOnClaster(volume, typeOfPrint);
				}
				if(this.pocPrice==0)
				{
					this.pocPrice =  price;
				}
				else
				{
					if(this.pocPrice != price && this.ListOfCurrentBar[price].Volume_sum>=this.ListOfCurrentBar[this.pocPrice].Volume_sum)
					{
						this.pocPrice = price;
					}
				}
			}
		}
		
		public class DayProfileClass : HistogrammClass
		{
			
		}
		
		
		
		public class CurrentClaster
		{
			public int Volume_sum;
			public int Volume_Ask_sum;
			public int Volume_Bid_sum;
			
			public void AddPrintOnClaster(int volume, PrintType typeOfPrint)
			{
				switch(typeOfPrint)
				{
					case PrintType.ASK:
						{
							this.Volume_sum+=volume;
							this.Volume_Ask_sum+=volume;
						}break;
					case PrintType.BID:
						{
							this.Volume_sum+=volume;
							this.Volume_Bid_sum+=volume;
						}break;
				}
			}
			
			public Claster GetStruct()
			{
				Claster claster = new Claster();
				claster.Volume_sum = this.Volume_sum;
				claster.Volume_Ask_sum = this.Volume_Ask_sum;
				claster.Volume_Bid_sum = this.Volume_Bid_sum;
				return claster;
			}
		}
		
		
		public struct Bar
		{
			public int Volume_sum;
			public int Volume_Ask_sum;
			public int Volume_Bid_sum;
			public double PocPrice;
			public double DayPocPrice;
			public DateTime Time;
			
			public SortedDictionary<double, Claster> ListOfClasters;
			
			public Bar(CurrentBar curBar)
			{
				ListOfClasters = new SortedDictionary<double,Claster>();
				int Volume_sum_tmp = 0;
				int Volume_Ask_sum_tmp = 0;
				int Volume_Bid_sum_tmp = 0;
				foreach(KeyValuePair<double, Model.CurrentClaster> kvp in curBar.ListOfCurrentBar)
				{
					ListOfClasters.Add(kvp.Key,kvp.Value.GetStruct());
					Volume_sum_tmp+=kvp.Value.Volume_sum;
					Volume_Ask_sum_tmp+=kvp.Value.Volume_Ask_sum;
					Volume_Bid_sum_tmp+=kvp.Value.Volume_Bid_sum;
				}
				Volume_sum = Volume_sum_tmp;
				Volume_Ask_sum = Volume_Ask_sum_tmp;
				Volume_Bid_sum = Volume_Bid_sum_tmp;
				PocPrice = curBar.pocPrice;
				DayPocPrice = curBar.dayPocPrice;
				Time = curBar.Time;
			}
				
		}
		
		public struct Claster
		{
			public int Volume_sum;
			public int Volume_Ask_sum;
			public int Volume_Bid_sum;
		}
		
		public struct Print
		{
			public DateTime Time;
			public int Volume;
			public double Price;
			public PrintType Type;
			
			public Print(DateTime time, int volume, double price, PrintType type)
			{
				Time = time;
				Volume = volume;
				Price = price;
				Type = type;
			}
		}
		
		public struct MarketStop
		{
			public DateTime Time;
			public int Volume;
			public double Price_low;
			public double Price_high;
			
			public MarketStop(DateTime time,int volume, double price_low, double price_high)
			{
				Time = time;
				Volume = volume;
				Price_low = price_low;
				Price_high = price_high;
			}
		}
	
		public struct TickAggregatorElement
		{
			public DateTime Time;
			public int Volume;
			public int Volume_Ask;
			public int Volume_Bid;
			public int Volume_Delta;
			public double Price;
			public double LowPrice;
			public double TopPrice;
			public List<Print> PrintList;
			
			public TickAggregatorElement(DateTime time,int volume,int ask, int bid, double price, double lowPrice, double topPrice,List<Print> printList)
			{
				Time = time;
				Volume = volume;
				Price = price;
				LowPrice=lowPrice;
				TopPrice=topPrice;
				Volume_Ask = ask*100/volume;
				Volume_Bid = 100-Volume_Ask;
				Volume_Delta = Math.Abs(Volume_Ask-Volume_Bid);
				PrintList = printList;
			}
		}
	
	}
	
	public enum PrintType
	{
		ASK,
		BID,
		NONE
	}
}

