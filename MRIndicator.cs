#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;


using System.Collections.Generic;
using System.Diagnostics;

using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Net.Sockets;

using NinjaTrader.NinjaScript.Indicators.MRPack;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.MRPack
{
	public class MRIndicator : Indicator
	{
		
		[XmlIgnore]
		public Model indicatorModel;
		
		private System.Windows.Controls.Grid   myGrid;
		private System.Windows.Controls.Button menuButton;
		private System.Windows.Controls.Button myTAButton;
		private System.Windows.Controls.Button myMSButton;
		private System.Windows.Controls.Button myTSButton;
		private System.Windows.Controls.Button myHVButton;
		private System.Windows.Controls.Button myVVButton;
		private System.Windows.Controls.Button myDPButton;
		private System.Windows.Controls.Button myRPButton;
		private System.Windows.Controls.StackPanel stackPanel;
		
		private bool isFirstDraw = true;
		
		private double maxValue;
		private double minValue;
		
		private double LastPrice_Line;
		
		public string AnyFile0;
		
		System.Windows.Media.Brush Claster_Color;
		System.Windows.Media.Brush Claster_FilterMin_Color;
		System.Windows.Media.Brush Claster_FilterMax_Color;
		
		SharpDX.Direct2D1.Brush Claster_ColorDX;
		SharpDX.Direct2D1.Brush Claster_FilterMin_ColorDX;
		SharpDX.Direct2D1.Brush Claster_FilterMax_ColorDX;
		SharpDX.Direct2D1.Brush Input_ClasterMax_ColorDX;
		
		SharpDX.Direct2D1.Brush brush0DX;
		SharpDX.Direct2D1.Brush brush1DX;
		SharpDX.Direct2D1.Brush brush2DX;
		SharpDX.Direct2D1.Brush brush3DX;
		SharpDX.Direct2D1.Brush brush4DX;
		SharpDX.Direct2D1.Brush brush5DX;
		SharpDX.Direct2D1.Brush brush6DX;
		SharpDX.Direct2D1.Brush brush7DX;
		SharpDX.Direct2D1.Brush brush8DX;
		SharpDX.Direct2D1.Brush brush9DX;
		
		private void OnMenuButtonClick(object sender, RoutedEventArgs rea)
		{
			stackPanel.Visibility = stackPanel.Visibility==Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}
		private void OnmyTAButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("TS");
			Input_TickAggregator_OnOff=!Input_TickAggregator_OnOff;
			myTAButton.Background = Input_TickAggregator_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		private void OnmyMSButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("TS");
			Input_MS_OnOff=!Input_MS_OnOff;
			myMSButton.Background = Input_MS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		
		private void OnmyTSButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("TS");
			Input_TandS_OnOff=!Input_TandS_OnOff;
			myTSButton.Background = Input_TandS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		private void OnmyHVButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("HV");
			Input_Histogramm_OnOff=!Input_Histogramm_OnOff;
			myHVButton.Background = Input_Histogramm_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		private void OnmyVVButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("VV");
			Input_VerticalVolume_OnOff = !Input_VerticalVolume_OnOff;
			myVVButton.Background = Input_VerticalVolume_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		private void OnmyDPButtonClick(object sender, RoutedEventArgs rea)
		{
			//Alert("MS:"+"allala", Priority.High, "MarketStop", NinjaTrader.Core.Globals.InstallDir+@"\sounds\AutoTrail.wav", 2, Brushes.Black, Brushes.Yellow);
			
			Input_PocOnDay_OnOff=!Input_PocOnDay_OnOff;
			myDPButton.Background = Input_PocOnDay_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color;
			ChartControl.InvalidateVisual();
		}
		private void OnmyRPButtonClick(object sender, RoutedEventArgs rea)
		{
			//Print("RP");
			string tag = "RangeProfile0";
			if(indicatorModel.RangeProfiles.Count>0)
			{
				string tmp = indicatorModel.RangeProfiles.Last().Tag;
				int tmp1 = Int32.Parse(tmp.Split('0')[1])+1;
				tag+=tmp1.ToString();
			}
			else
			{
				tag+="1";
			}

			DateTime startTime = ChartControl.GetTimeByX(100);
			DateTime endTime = ChartControl.GetTimeByX(200);
			double top = maxValue-(maxValue-minValue)/3;
			double bot = maxValue-(maxValue-minValue)/2;
			
			CreateNewRangeProfile(tag,startTime,top,endTime,bot,0,false);
			ChartControl.InvalidateVisual();
		}
		
		
		
		public void CreateNewTickAggregatorEllipse(DateTime time, double price, Model.TickAggregatorElement data)
		{
			try
			{
			int radius = data.Volume* (Input_TickAggregator_Distance/2) / indicatorModel.MaxTickAggregatorVolume;
			if(radius<5)radius=5;
			
			
			DrawingTools.CustomEllipse elipse = Draw.CustomEllipse(this,time.ToString()+" - "+price.ToString(),time,data.TopPrice+TickSize/2, indicatorModel);
			elipse.Radius=radius;
			elipse.TickAggregatorData = data;
			
			}
			catch (Exception ex) { Print("MRPack CreateNewTickAggregatorEllipse 162: " + ex); }
		}
		
		
		public void CreateNewRangeProfile(string tag, DateTime startTime,double top,DateTime endTime,double bot, int ProfileType, bool extended)
		{
			try
			{
			//DrawingTools.RangeProfile profile = Draw.RangeProfile(this,tag,true, new DateTime(2017,12,18,10,0,0), 1.1895, new DateTime(2017,12,18,15,0,0), 1.187, Brushes.Red, 2, indicatorModel);
			//DrawingTools.RangeProfile profile = Draw.RangeProfile(this,tag,true, startTime, top, endTime, bot, Brushes.Red, 2, indicatorModel);
			DrawingTools.RangeProfile2 profile = Draw.RangeProfile2(this,tag, startTime, top, endTime, bot,Brushes.Red, indicatorModel);
			profile.IsLocked = false;
			profile.ProfileType = ProfileType;
			profile.ExtendedLine = extended;
			indicatorModel.RangeProfiles.Add(profile);
			
			int startBar = (int)ChartControl.GetSlotIndexByX(100);
			int endBar = (int)ChartControl.GetSlotIndexByX(200);
			}
			catch (Exception ex) { Print("MRPack CreateNewRangeProfile 181: " + ex); }
		}
		
		/*public static DateTime GetNetworkTime()
        {
			try{
	            const string ntpServer = "pool.ntp.org";
	            // NTP message size - 16 bytes of the digest (RFC 2030)
	            var ntpData = new byte[48];

	            //Setting the Leap Indicator, Version Number and Mode values
	            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

	            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

	            //The UDP port number assigned to NTP is 123
	            var ipEndPoint = new IPEndPoint(addresses[0], 123);
	            //NTP uses UDP
	            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
	            {
	                socket.Connect(ipEndPoint);

	                //Stops code hang if NTP is blocked
	                socket.ReceiveTimeout = 3000;

	                socket.Send(ntpData);
	                socket.Receive(ntpData);
	            }

	            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
	            //departed the server for the client, in 64-bit timestamp format."
	            const byte serverReplyTime = 40;

	            //Get the seconds part
	            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

	            //Get the seconds fraction
	            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

	            //Convert From big-endian to little-endian
	            intPart = SwapEndianness(intPart);
	            fractPart = SwapEndianness(fractPart);

	            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

	            //**UTC** time
	            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
				 return networkDateTime.ToLocalTime();
			}catch(Exception expn){
				return new DateTime();
			}
            
        }*/
		
		/*static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }	
		*/
		/*
		 static byte[] GetMd5Hash(MD5 md5Hash, string input)//Функция получения хеша из строки
        {
            byte[] data = md5Hash.ComputeHash(GetBytes(input));
            return data;
        }
		*/
		/*
		 static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
		*/
		/*
		 public static bool DSAVerifyHash(byte[] HashValue, byte[] SignedHashValue,
           DSAParameters DSAKeyInfo, string HashAlg)
        {
            bool verified = false;

            try
            {
                // Create a new instance of DSACryptoServiceProvider.
                using (DSACryptoServiceProvider DSA = new DSACryptoServiceProvider())
                {
                    // Import the key information.
                    DSA.ImportParameters(DSAKeyInfo);

                    // Create an DSASignatureDeformatter object and pass it the
                    // DSACryptoServiceProvider to transfer the private key.
                    DSASignatureDeformatter DSADeformatter = new DSASignatureDeformatter(DSA);

                    // Set the hash algorithm to the passed value.
                    DSADeformatter.SetHashAlgorithm(HashAlg);

                    // Verify signature and return the result.
                    verified = DSADeformatter.VerifySignature(HashValue, SignedHashValue);
                }
            }
            catch (CryptographicException e)
            {
                //Print(e.Message.ToString());
            }

            return verified;
        }
		*/
		
		//private DateTime prevdatePrint;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Masters of Risk";
				Name										= "MRIndicator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= false;
				
				#region Input_Claster
					Input_Claster_OnOff=true;
					Input_MaxClaster_OnOff=false;
					Input_ClasterText_OnOff=true;
					Input_Claster_BidAsk_OnOff=false;
					Input_Claster_Color=Brushes.DimGray;
					Input_ClasterMinVolume=500;
					Input_ClasterMaxVolume=2000;
					Input_Claster_Filter1_Value=1700;
					Input_Claster_Filter2_Value=1500;
					Input_Claster_Filter1_Color=Brushes.Cyan;
					Input_Claster_Filter2_Color=Brushes.Magenta;
					Input_ClasterMax_Color=Brushes.OrangeRed;
					Claster_Color = Input_Claster_Color;
					Claster_FilterMin_Color = Input_Claster_Filter2_Color;
					Claster_FilterMax_Color = Input_Claster_Filter1_Color;
				#endregion
				
				#region Vertical Volume Input Default
					Input_VerticalVolume_OnOff=true;
					Input_VerticalVolumeText_OnOff=true;
					Input_VerticalVolume_Color=Brushes.LightPink;
					Input_VerticalVolume_Min=0;
					Input_VerticalVolume_Size=100;
					Input_VerticalVolume_Filter1_Value=7000;
					Input_VerticalVolume_Filter1_Color=Brushes.Maroon;
					
				#endregion
				
				
				#region Histogramm Input Default
					Input_Histogramm_OnOff=false;
					Input_HistogrammText_OnOff=true;
					Input_HistogrammMaxVolume_OnOff=true;
					Input_Histogramm_Filter1=2000;
					Input_Histogramm_Filter2=5000;
					Input_Histogramm_Color=Brushes.DimGray;
					Input_Histogramm_Filter1_Color=Brushes.DimGray;
					Input_Histogramm_Filter2_Color=Brushes.Lavender;
					Input_Histogramm_MaxVolume_Color=Brushes.Maroon;
					Input_Histogramm_MaxSize=100;
					//Input_Histogramm_MinVolume=0;
				#endregion
				
				
				#region VPOC On Day Default
					Input_PocOnDay_OnOff=true;
					Input_PocOnDay_Color=Brushes.Maroon;
				#endregion
				
				
				#region Market Stop Default
					Input_MS_OnOff=false;
					Input_MSAlert_OnOff=true;
					Input_MS_VolumeLimit=500;
					Input_MS_Color=Brushes.DarkSlateGray;
				
				#endregion
				
				#region Buttons Default
					
					Input_ButtonsOn_Color=Brushes.Maroon;
					Input_ButtonsOff_Color=Brushes.DarkSlateGray;
				#endregion
				
				#region Profile Range Default
					Range_Profile_Text_OnOff = true;
					Range_Profile_Text_Color = Brushes.Yellow;
					Input_ProfileRange_Inside_Color=Brushes.LightSkyBlue;
					Input_ProfileRange_POC_Color=Brushes.Azure;
					//Input_RangeProfile_BidAsk_OnOff=false;
					//Input_RangeProfile_ExtendedLine_OnOff=true;
					Input_ProfileRange_Border_Color=Brushes.DimGray;
					//RangeProfileFileXML=AnyFile0;
					Input_ProfileRange_Inside_Bid_Color = Brushes.Red;
					Input_ProfileRange_Inside_Ask_Color = Brushes.Green;
					Profile_Text_Opacity = 90;
					Profile_Opacity = 50;
				#endregion
				
				
				#region Time and Sale Default
					Input_TandS_OnOff=false;
					//Input_TandS_RightMargin=80;
					Input_TandS_RightPosition=65;
					Input_TandS_TopPosition=50;
					Input_TandS_CountOrders=55;
					Input_TandS_TextSize=11;
					Input_TandS_Bid_Color=Brushes.CornflowerBlue;
					Input_TandS_Ask_Color=Brushes.IndianRed;
					Input_TandS_FilterBid=100;
					Input_TandS_FilterBid_Color=Brushes.Blue;
					Input_TandS_FilterAsk=100;
					Input_TandS_FilterAsk_Color=Brushes.Red;
					Input_OnlyFilterShow=true;
					Input_ShowFilterOnChart=true;
				#endregion
				
				#region TickAggregator DEfault
					Input_TickAggregator_OnOff=false;
					Input_TickAggregator_TickLimit=8;
					Input_TickAggregator_Delay=3000;
					Input_TickAggregator_SummLimit = 100;
					Input_TickAggregator_Range = 6;
					Input_TickAggregator_BigPrint = 100;
					Input_TickAggregator_Distance = Input_TickAggregator_SummLimit*2;
				
					Input_TickAggregator_TickShow = 8;
					Input_TickAggregator_Standart = false;
					Input_TickAggregator_AskColor = Brushes.Green;
					Input_TickAggregator_BidColor = Brushes.Red;
					Input_TickAggregator_AlertOnOff = true;
				#endregion
				
				
				#region PriceLine Default
					Input_PriceLine_OnOff=true;
					Input_PriceLine_Color=Brushes.Maroon;
				#endregion
				
				
				#region Instrument
					UseAutoTextSize             = true;
			  		TextSize                    = 200;
			    	TextOpacity                 = 10;
			    	TextBrush                   = Brushes.Gray;
				#endregion
				
				
				indicatorModel = new Model(this);
				Brush tempBrush = TextBrush.Clone();
			    tempBrush.Opacity = 1 * 0.1f;
			    TextBrush = tempBrush.Clone();
                TextBrush.Freeze();
			}
			else if (State == State.Configure)
			{
				indicatorModel = new Model(this);
				indicatorModel.Range_Profile_Text_OnOff = Range_Profile_Text_OnOff;
				indicatorModel.Range_Profile_Text_Color = Range_Profile_Text_Color;
				indicatorModel.Input_ProfileRange_Inside_Color=Input_ProfileRange_Inside_Color;
				indicatorModel.Input_ProfileRange_POC_Color=Input_ProfileRange_POC_Color;
				/*indicatorModel.Input_RangeProfile_BidAsk_OnOff=false;
				indicatorModel.Input_RangeProfile_ExtendedLine_OnOff=true;*/
				indicatorModel.Input_ProfileRange_Border_Color=Input_ProfileRange_Border_Color;
				indicatorModel.Input_ProfileRange_Inside_Bid_Color = Input_ProfileRange_Inside_Bid_Color;
				indicatorModel.Input_ProfileRange_Inside_Ask_Color = Input_ProfileRange_Inside_Ask_Color;
				indicatorModel.Profile_Text_Opacity = Profile_Text_Opacity;
				indicatorModel.Profile_Opacity = Profile_Opacity;
				indicatorModel.LoadProfiles();
				
				Brush tempBrush = TextBrush.Clone();
			    tempBrush.Opacity = 1 * 0.1f;
			    TextBrush = tempBrush.Clone();
                TextBrush.Freeze();
				
				/*DateTime d1 = new DateTime(2017,12,23,23,20,10,10);		
				DateTime d2 = new DateTime(2017,12,23,23,20,11,20);	
				TimeSpan t = d2-d1;
				Print(t.TotalMilliseconds);*/
				
				
			}
			else if (State == State.Realtime)
			{
				/*PrintListOfBar(indicatorModel.ListOfBar);
				foreach(Model.MarketStop ms in indicatorModel.ListOfMarketStop )
				{
				    Print(ms.Time+" - "+ms.Volume +" High: "+ms.Price_high+" BID: "+ms.Price_low);
				}*/
			}
			else if(State == State.Historical)
			{
					ChartControl.Dispatcher.InvokeAsync((() =>
				    {
						
				        // Grid already exists
				        if (UserControlCollection.Contains(myGrid))
				          return;
						
						
				 
				        // Add a control grid which will host our custom buttons
				        myGrid = new System.Windows.Controls.Grid
				        {
				          Name = "MyCustomGrid",
				          // Align the control to the top right corner of the chart
				          HorizontalAlignment = HorizontalAlignment.Left,
				          VerticalAlignment = VerticalAlignment.Top,
						  Margin = new Thickness(10,20,0,0)
				        };
				 
				        // Define the two columns in the grid, one for each button
				        System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
				        System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
				 
				        // Add the columns to the Grid
				        myGrid.ColumnDefinitions.Add(column1);
				        myGrid.ColumnDefinitions.Add(column2);
				 
				        // Define the custom Buy Button control object
				        menuButton = new System.Windows.Controls.Button
				        {
				          Name = "MyMenuButton",
				          Content = "Menu",
				          Foreground = Brushes.White,
							MinWidth=40,
							Width = 40,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						
						
						stackPanel = new System.Windows.Controls.StackPanel();
						stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
						//stackPanel.Margin =  new Thickness(20,0,0,0);
						stackPanel.Visibility = Visibility.Collapsed;
						
						
				 
				        // Define the custom Sell Button control object
				        myHVButton = new System.Windows.Controls.Button
				        {
				          Name = "my1Button",
				          Content = "HV",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_Histogramm_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myVVButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "VV",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_VerticalVolume_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myDPButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "DP",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_PocOnDay_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myRPButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "RP",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myTSButton = new System.Windows.Controls.Button
				        {
				          Name = "myTSButton",
				          Content = "TS",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_TandS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myMSButton = new System.Windows.Controls.Button
				        {
				          Name = "myMSButton",
				          Content = "MS",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_MS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
						myTAButton = new System.Windows.Controls.Button
				        {
				          Name = "myTAButton",
				          Content = "TA",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_TickAggregator_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(6,0,0,0),
							MinWidth=25,
							Width = 25,
							FontSize = 11.0,
							Padding = new Thickness(0, 0, 0, 0),
							Height = 18
				        };
					
						stackPanel.Children.Add(myTSButton);
						stackPanel.Children.Add(myHVButton);
						stackPanel.Children.Add(myVVButton);
						stackPanel.Children.Add(myTAButton);
						stackPanel.Children.Add(myMSButton);
						stackPanel.Children.Add(myDPButton);
						stackPanel.Children.Add(myRPButton);
				 
				        // Subscribe to each buttons click event to execute the logic we defined in OnMyButtonClick()
				        menuButton.Click += OnMenuButtonClick;
						myTAButton.Click += OnmyTAButtonClick;
						myMSButton.Click += OnmyMSButtonClick;
						myTSButton.Click += OnmyTSButtonClick;
						myHVButton.Click += OnmyHVButtonClick;
						myVVButton.Click += OnmyVVButtonClick;
						myDPButton.Click += OnmyDPButtonClick;
						myRPButton.Click += OnmyRPButtonClick;
				 
				        // Define where the buttons should appear in the grid
				        System.Windows.Controls.Grid.SetColumn(menuButton, 0);
				        System.Windows.Controls.Grid.SetColumn(stackPanel, 1);
				 
				        // Add the buttons as children to the custom grid
				        myGrid.Children.Add(menuButton);
				        myGrid.Children.Add(stackPanel);
						
				 
				        // Finally, add the completed grid to the custom NinjaTrader UserControlCollection
				        UserControlCollection.Add(myGrid);
						//UserControlCollection.Add(myCanvasParent);
				 
				    }));
				  }
				else if (State == State.Terminated)
				  {
				    if (ChartControl == null)
				        return;
					
				 	ChartControl.Properties.BarMarginRight = 8;
				    // Again, we need to use a Dispatcher to interact with the UI elements
				    ChartControl.Dispatcher.InvokeAsync((() =>
				    {
				        if (myGrid != null)
				        {
				          if (menuButton != null)
				          {
				              myGrid.Children.Remove(menuButton);
				              menuButton.Click -= OnMenuButtonClick;
				              menuButton = null;
				          }
				          if (stackPanel != null)
				          {
							  stackPanel.Children.Remove(myTAButton);
							  myTAButton=null;
							  stackPanel.Children.Remove(myMSButton);
							  myMSButton=null;
							  stackPanel.Children.Remove(myTSButton);
							  myTSButton=null;
							  stackPanel.Children.Remove(myHVButton);
							  myHVButton=null;
							  stackPanel.Children.Remove(myVVButton);
							  myVVButton=null;
							  stackPanel.Children.Remove(myDPButton);
							  myDPButton=null;
							  stackPanel.Children.Remove(myRPButton);
							  myRPButton=null;
							  
				              myGrid.Children.Remove(stackPanel);
				              //mySellButton.Click -= OnMyButtonClick2;
				              stackPanel = null;
				          }
				        }
				    }));
				  }
			}
		bool tmp =true;
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				try
				{
				PrintType printType = PrintType.NONE;
				LastPrice_Line = marketDataUpdate.Price;
				if (marketDataUpdate.Price >= marketDataUpdate.Ask)
				{
					printType = PrintType.ASK;
					indicatorModel.AddPrintToTandS(marketDataUpdate.Time,(int)marketDataUpdate.Volume,marketDataUpdate.Price);
				}
				else if (marketDataUpdate.Price <= marketDataUpdate.Bid)
				{
					printType = PrintType.BID;
					indicatorModel.AddPrintToTandS(marketDataUpdate.Time, (int)(marketDataUpdate.Volume*(-1)),marketDataUpdate.Price);
				}
				//
				
				indicatorModel.AddPrintToBar(marketDataUpdate.Price,(int)marketDataUpdate.Volume,printType);
				
				indicatorModel.AddPrintToMarketStopStack(marketDataUpdate.Time,(int)marketDataUpdate.Volume,marketDataUpdate.Price,printType);
				
				indicatorModel.Histogramm.AddPrintToHistogramm(marketDataUpdate.Price,(int)marketDataUpdate.Volume,printType);
				
				indicatorModel.DayProfile.AddPrintToHistogramm(marketDataUpdate.Price,(int)marketDataUpdate.Volume,printType);
				
				indicatorModel.AddPrintToTickAggregator(marketDataUpdate.Time,(int)marketDataUpdate.Volume,marketDataUpdate.Price,printType);
				
				
				//Print(marketDataUpdate.Time.ToString()+" - "+((int)marketDataUpdate.Volume).ToString()+" - "+marketDataUpdate.Price.ToString()+" - "+printType.ToString());
				}
				catch (Exception ex) { Print("MRPack OnMarketData 723: " + ex); }
			}
		}
		protected override void OnBarUpdate()
		{
			//Print(Time[0]);
			try
            {
			if (Time[0] == null)
			return;
			indicatorModel.CloseBar(Time[0]);
			
			if (Bars.IsFirstBarOfSession)
				indicatorModel.CloseDay();
			}
            catch (Exception ex) { Print("MRPack OnBarUpdate 738: " + ex); }	
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			try
            {
			int nb=(ChartBars.ToIndex-ChartBars.FromIndex);
			if(BarsArray[0] == null || ChartBars == null || ChartControl == null || Bars.Instrument == null || !IsVisible 
				|| chartScale.ScaleJustification != ScaleJustification.Right || CurrentBars[0] <= BarsRequiredToPlot || nb < 7 || IsInHitTest || indicatorModel == null)
				{
					return;
				}
			//Stopwatch stopWatch = new Stopwatch();
        	//stopWatch.Start();
			
			brush1DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.WhiteSmoke);
			
			if(
				//isFirstDraw 
				Input_TickAggregator_OnOff
				)
			{
				for(int i=0;i<indicatorModel.TickAggregatorElements.Count;i++)
				{
					//Print(indicatorModel.TickAggregatorElements[i].Time.ToString()+" - "+indicatorModel.TickAggregatorElements[i].Price.ToString()+" - "+indicatorModel.TickAggregatorElements[i].Volume.ToString()+" - "+indicatorModel.TickAggregatorElements[i].LowPrice.ToString()+" - "+indicatorModel.TickAggregatorElements[i].TopPrice.ToString());
					CreateNewTickAggregatorEllipse(indicatorModel.TickAggregatorElements[i].Time,indicatorModel.TickAggregatorElements[i].Price,indicatorModel.TickAggregatorElements[i]);
				
				}
			}
			if (!Input_TickAggregator_OnOff)
			{
				
				DrawingTools.CustomEllipse l1 = null;
				foreach (Gui.NinjaScript.IChartObject thisObject in ChartPanel.ChartObjects)
		  		{
					if(thisObject is NinjaTrader.NinjaScript.DrawingTools.CustomEllipse)
			  		{
						l1 = thisObject as NinjaTrader.NinjaScript.DrawingTools.CustomEllipse;
						//break;
						if (l1 != null)
						{
     						System.Reflection.BindingFlags bfObject = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    						System.Reflection.MethodInfo methodNT = typeof(ChartControl).GetMethod("RemoveDrawingTool", bfObject);
     						methodNT.Invoke(ChartControl, new Object [] { l1, false, false });
						}
					}
				}
			}
			if (!Bars.IsTickReplay)
			{
				SharpDX.Direct2D1.SolidColorBrush KBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
				SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();
				SharpDX.DirectWrite.TextFormat KTextFormat = new SharpDX.DirectWrite.TextFormat(fontFactory, "Segoe UI", 30f);
				
				RenderTarget.DrawText("Tick Replay is Disabled",KTextFormat,new SharpDX.RectangleF(50,50,1000,100),KBrush);
				
				KBrush.Dispose();
				fontFactory.Dispose();
				KTextFormat.Dispose();
				return;
			}
			
			maxValue = chartScale.MaxValue;
			minValue = chartScale.MinValue;
			
			int Claster_FilterMin_Volume;
			int Claster_FilterMax_Volume;
			
			//Brush Histogramm_Claster_Color;
			Brush Histogramm_Claster_FilterMin_Color;
			Brush Histogramm_Claster_FilterMax_Color;
			int Histogramm_Claster_FilterMin_Volume;
			int Histogramm_Claster_FilterMax_Volume;
			
			//#region Instrument
					

			   // try
			   //{
			        //int textSize    = TextSize;
			        //int cph     = ChartPanel.H;

			       // if (UseAutoTextSize)
			            //textSize = (int) (cph * 0.75f);

			       // SimpleFont simpleFont = new SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), textSize);
	                // Place the text in OnRender to adjust automatically if needed
			       // Draw.TextFixed(this, "SymbolText", Instrument.MasterInstrument.Name, TextPosition.Center, TextBrush, simpleFont, Brushes.Transparent, Brushes.Transparent, 0);
			   // }
			    //catch (Exception ex)
			    //{
			       // Print(Name + " : " + ex);
			   // }
			//#endregion
		
			indicatorModel.SetGraficDimensions(chartScale.GetYByValue(ChartPanel.MaxValue),chartScale.GetYByValue(ChartPanel.MaxValue+TickSize),
												chartControl.GetXByTime(indicatorModel.ListOfBar.ElementAt(1).Time),chartControl.GetXByTime(indicatorModel.ListOfBar.ElementAt(0).Time));
			
			SharpDX.DirectWrite.TextFormat Claster_textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			
			System.Windows.Media.SolidColorBrush vv_Color = (System.Windows.Media.SolidColorBrush)Input_VerticalVolume_Color;
			SharpDX.Direct2D1.LinearGradientBrush linearGradientBrush_VerticalVolume_Standart = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
				{
					StartPoint = new SharpDX.Vector2(0, 0),
					EndPoint = new SharpDX. Vector2(0, 0),
				},
				new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
				{
					new	SharpDX.Direct2D1. GradientStop()
					{
						Color = new SharpDX.Color(vv_Color.Color.R,vv_Color.Color.G,vv_Color.Color.B,vv_Color.Color.A),
						Position = 0,
					},
					new SharpDX.Direct2D1. GradientStop()
					{
						Color = SharpDX.Color.Black,
						Position = 1,
					}
				}));
			
				System.Windows.Media.SolidColorBrush vv_Color1=(System.Windows.Media.SolidColorBrush)Input_VerticalVolume_Filter1_Color;
				SharpDX.Direct2D1.LinearGradientBrush linearGradientBrush_VerticalVolume_Filter1 = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
				{
					StartPoint = new SharpDX.Vector2(0, 0),
					EndPoint = new SharpDX. Vector2(0, 0),
				},
				new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
				{
					new	SharpDX.Direct2D1. GradientStop()
					{
						Color = new SharpDX.Color(vv_Color1.Color.R,vv_Color1.Color.G,vv_Color1.Color.B,vv_Color1.Color.A),
						Position = 0,
					},
					new 	SharpDX.Direct2D1. GradientStop()
					{
						Color = SharpDX.Color.Black,
						Position = 1,
					}
				}));
			
			if(Input_Claster_Filter1_Value<=Input_Claster_Filter2_Value)
			{
				Claster_FilterMin_Color = Input_Claster_Filter1_Color;
				Claster_FilterMax_Color = Input_Claster_Filter2_Color;
				Claster_FilterMin_Volume = Input_Claster_Filter1_Value;
				Claster_FilterMax_Volume = Input_Claster_Filter2_Value;
			}else
			{
				Claster_FilterMin_Color = Input_Claster_Filter2_Color;
				Claster_FilterMax_Color = Input_Claster_Filter1_Color;
				Claster_FilterMin_Volume = Input_Claster_Filter2_Value;
				Claster_FilterMax_Volume = Input_Claster_Filter1_Value;
			}
			
			if(Input_Histogramm_Filter1<=Input_Histogramm_Filter2)
			{
				Histogramm_Claster_FilterMin_Color = Input_Histogramm_Filter1_Color;
				Histogramm_Claster_FilterMax_Color = Input_Histogramm_Filter2_Color;
				Histogramm_Claster_FilterMin_Volume = Input_Histogramm_Filter1;
				Histogramm_Claster_FilterMax_Volume = Input_Histogramm_Filter2;
			}else
			{
				Histogramm_Claster_FilterMin_Color = Input_Histogramm_Filter2_Color;
				Histogramm_Claster_FilterMax_Color = Input_Histogramm_Filter1_Color;
				Histogramm_Claster_FilterMin_Volume = Input_Histogramm_Filter2;
				Histogramm_Claster_FilterMax_Volume = Input_Histogramm_Filter1;
			}
			
			int MaxVV = 0;
			for(int i=ChartBars.FromIndex;i<ChartBars.ToIndex;i++)
			{
				try
				{
					if(indicatorModel.ListOfBar[i].Volume_sum>=MaxVV)
					MaxVV=indicatorModel.ListOfBar[i].Volume_sum;
				}
				catch(Exception ex)
				{
					continue;
				}
				
			}
			
			#region PriceLine Drawing
			//	LastPrice_line
				if(Input_PriceLine_OnOff){
					SharpDX.Vector2 point0 = new SharpDX.Vector2();
					SharpDX.Vector2 point1 = new SharpDX.Vector2();
				 	SharpDX.DirectWrite.TextFormat textFormatPriceLine = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
					brush0DX = Input_PriceLine_Color.ToDxBrush(RenderTarget);
					
					point0.X = chartControl.GetXByTime(indicatorModel.ListOfBar.Last().Time)+indicatorModel.Claster_Width_Max+2;
					point0.Y = chartScale.GetYByValue(LastPrice_Line);
					point1.X = ChartPanel.W;
					point1.Y = point0.Y;
					
					//RenderTarget.FillGeometry(myLineGeometry,PriceLine_Color_Brush);
					RenderTarget.DrawLine(point0, point1, brush0DX, 1);
					RenderTarget.DrawText(LastPrice_Line.ToString(),textFormatPriceLine,new SharpDX.RectangleF((point0.X)+indicatorModel.Claster_Width_Max+15,point0.Y,100,20),brush0DX);
					
					textFormatPriceLine.Dispose();
					brush0DX.Dispose();
				}
				
			#endregion
			
			bool flagForCurrentBar = false;
			for(int i=ChartBars.FromIndex;i<ChartBars.ToIndex;i++)
			{
				Model.Bar bar;
				try
				{
					bar = indicatorModel.ListOfBar[i];
				}
				catch(Exception ex)
				{
					continue;
				}
				
				int BarPositionX = chartControl.GetXByTime(bar.Time);
				if(flagForCurrentBar)
				{
					bar = indicatorModel.currentBar.GetStruct();
					BarPositionX+=indicatorModel.Claster_Width_Max;
				}
				
				if(Input_Claster_OnOff)
				{
					IEnumerable<KeyValuePair<double, Model.Claster>> clasters;// = bar.ListOfClasters.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue);
					brush2DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
					brush3DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Green);
					brush9DX = Input_Claster_Color.ToDxBrush(RenderTarget);
					if (!(bar.ListOfClasters.IsNullOrEmpty()))
					{
					if(indicatorModel.Claster_Height>1)
					{
						clasters = bar.ListOfClasters.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue);
						
					}
					else
					{
						clasters = bar.ListOfClasters.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue && (c.Value.Volume_sum>=Claster_FilterMin_Volume || c.Key==bar.PocPrice));
						
						int clasterPositionY1 = chartScale.GetYByValue(bar.ListOfClasters.First().Key)- (int)(indicatorModel.Claster_Height/2);
						int clasterPositionY2 = chartScale.GetYByValue(bar.ListOfClasters.Last().Key)+ (int)(indicatorModel.Claster_Height/2);
						
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX, clasterPositionY1 , 1, clasterPositionY2-clasterPositionY1),brush9DX);
					}
					//}
					//Print(indicatorModel.ListOfBar.Count);
					foreach(KeyValuePair<double, Model.Claster> claster in clasters)
					{
						int clasterPositionY = chartScale.GetYByValue(claster.Key);
						int clasterVolume = claster.Value.Volume_sum;
						int Claster_RealWidth=0;
						
						if(clasterVolume<=Input_ClasterMinVolume)
							Claster_RealWidth = 1;
						else if(clasterVolume>=Input_ClasterMaxVolume)
							Claster_RealWidth = indicatorModel.Claster_Width_Max;
						else{
							int Dec_RealVol_MinVol = clasterVolume-Input_ClasterMinVolume;
							int Dec_MaxVol_MinVol = Input_ClasterMaxVolume-Input_ClasterMinVolume;
							if (Dec_MaxVol_MinVol != 0)														// Edited by PD
							{	
								int Procent_DecRealVol_MinVol_Dec_MaxVol_MinVol = Dec_RealVol_MinVol*100/Dec_MaxVol_MinVol;
								Claster_RealWidth= Math.Max(1, Procent_DecRealVol_MinVol_Dec_MaxVol_MinVol*indicatorModel.Claster_Width_Max/100);
							}
						}
						
						//if(Claster_RealWidth<1) Claster_RealWidth=1;
						Claster_ColorDX = brush9DX;
						if(clasterVolume>=Claster_FilterMax_Volume)
						{
							Claster_ColorDX = Claster_FilterMax_ColorDX;
							/*if(!(indicatorModel.Claster_Height>1))
								Claster_RealWidth*=3;*/
						}
						else if(clasterVolume>=Claster_FilterMin_Volume)
						{
							Claster_ColorDX = Claster_FilterMin_ColorDX;
							/*if(!(indicatorModel.Claster_Height>1))
								Claster_RealWidth*=3;*/
						}
						
						if(claster.Key==bar.PocPrice && Input_MaxClaster_OnOff)
							Claster_ColorDX = Input_ClasterMax_ColorDX;
							
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX, clasterPositionY - (int)(indicatorModel.Claster_Height/2), Claster_RealWidth, indicatorModel.Claster_Height),Claster_ColorDX);
						
						if(Input_ClasterText_OnOff)
						{
							if(Input_Claster_BidAsk_OnOff)
							{
								string str1=claster.Value.Volume_sum.ToString()+"=";
								string str2=claster.Value.Volume_Bid_sum.ToString();
								string str3=claster.Value.Volume_Ask_sum.ToString();
								string str = str1+str2+str3;
								if(indicatorModel.Claster_Width_Max>=str.Length*8 && indicatorModel.Claster_Height>=10)
								{
									RenderTarget.DrawText(str1,Claster_textFormat,new SharpDX.RectangleF(BarPositionX,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),brush1DX);
									RenderTarget.DrawText(str2,Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),brush2DX);
									RenderTarget.DrawText("x",Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8+str2.Length*8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),brush1DX);
									RenderTarget.DrawText(str3,Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8+str2.Length*8+8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),brush3DX);
								}
							}
							else if(indicatorModel.Claster_Width_Max>=clasterVolume.ToString().Length*8 && indicatorModel.Claster_Height>=10)
								RenderTarget.DrawText(clasterVolume.ToString(),Claster_textFormat,new SharpDX.RectangleF(BarPositionX, clasterPositionY - (int)(indicatorModel.Claster_Height/2), indicatorModel.Claster_Width_Max, indicatorModel.Claster_Height),brush1DX);
						}
					}
					}
					brush2DX.Dispose();
					brush3DX.Dispose();
					brush9DX.Dispose();
				}
				
				if(Input_VerticalVolume_OnOff)
				{
					int tmp_maxVolume = MaxVV;//bars.Max(b => b.Volume_sum);
					
				if (tmp_maxVolume-1 != 0)												// Edited by PD
				{	
					int vol=bar.Volume_sum*Input_VerticalVolume_Size/tmp_maxVolume-1;
					
					int Y_VerticalVolume=chartScale.GetYByValue(chartScale.MinValue)-vol;
					int tmp_width = indicatorModel.Claster_Width_Max-2;
					if(tmp_width<1)tmp_width=1;
					
					if(bar.Volume_sum>=Input_VerticalVolume_Filter1_Value){
						linearGradientBrush_VerticalVolume_Filter1.StartPoint=new SharpDX.Vector2(0, Y_VerticalVolume);
						linearGradientBrush_VerticalVolume_Filter1.EndPoint=new SharpDX.Vector2(0, Y_VerticalVolume+vol);
						
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX - (tmp_width/2),Y_VerticalVolume,tmp_width,vol),linearGradientBrush_VerticalVolume_Filter1);
					}else{
						linearGradientBrush_VerticalVolume_Standart.StartPoint=new SharpDX.Vector2(0, Y_VerticalVolume);
						linearGradientBrush_VerticalVolume_Standart.EndPoint=new SharpDX.Vector2(0, Y_VerticalVolume+vol);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX - (tmp_width/2),Y_VerticalVolume,tmp_width,vol),linearGradientBrush_VerticalVolume_Standart);
					}
					
					if(indicatorModel.Claster_Width_Max>=bar.Volume_sum.ToString().Length*8 && Input_VerticalVolumeText_OnOff)
						RenderTarget.DrawText(bar.Volume_sum.ToString(),Claster_textFormat,new SharpDX.RectangleF(BarPositionX - (tmp_width/2),Y_VerticalVolume-15,indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),brush1DX);
				}
				}
				
				if(Input_PocOnDay_OnOff)
				{
					brush4DX = Input_PocOnDay_Color.ToDxBrush(RenderTarget);
					if(i<indicatorModel.ListOfBar.Count-1 && bar.DayPocPrice != 0 && indicatorModel.ListOfBar[i+1].DayPocPrice != 0)
					{
						SharpDX.Vector2 point0 = new SharpDX.Vector2();
						SharpDX.Vector2 point1 = new SharpDX.Vector2();
						point0.X=BarPositionX;
						point0.Y=chartScale.GetYByValue(bar.DayPocPrice);
						point1.X=BarPositionX+indicatorModel.Claster_Width_Max+2;
						point1.Y=chartScale.GetYByValue(indicatorModel.ListOfBar[i+1].DayPocPrice);
						RenderTarget.DrawLine(point0,point1,brush4DX);
						
						//NinjaTrader.Code.Output.Process(string.Format("bar.DayPocPrice: {0}, indicatorModel.Price : {1}", bar.DayPocPrice, indicatorModel.ListOfBar[i+1].DayPocPrice), PrintTo.OutputTab1);
						
					}else if(flagForCurrentBar) {
						SharpDX.Vector2 point0 = new SharpDX.Vector2();
						SharpDX.Vector2 point1 = new SharpDX.Vector2();
						point0.X=BarPositionX-indicatorModel.Claster_Width_Max-2;;
						point0.Y=chartScale.GetYByValue(indicatorModel.ListOfBar[i].DayPocPrice);
						point1.X=BarPositionX;
						point1.Y=chartScale.GetYByValue(bar.DayPocPrice);
						RenderTarget.DrawLine(point0,point1,brush4DX);
					}
					if (brush4DX != null)		{	brush4DX.Dispose();	}
				}
				
				if(i==indicatorModel.ListOfBar.Count-1 && !flagForCurrentBar)
				{
					flagForCurrentBar=true;
					i-=1;
				}
			}
				
			if(Input_MS_OnOff)
			{
				IEnumerable<Model.MarketStop> marketStops = indicatorModel.ListOfMarketStop.Where(c => c.Time>=chartControl.FirstTimePainted && c.Time<=chartControl.LastTimePainted);
				SharpDX.Direct2D1.Brush MS_brush = Input_MS_Color.ToDxBrush(RenderTarget);
				MS_brush.Opacity=(float)0.5;
				foreach(Model.MarketStop MS in marketStops)
				{
					//int MS_positionX = chartControl.GetXByBarIndex(ChartBars,(int)chartControl.GetSlotIndexByTime(MS.Time));
					int MS_positionX = chartControl.GetXByBarIndex(ChartBars, (ChartBars.GetBarIdxByTime(ChartControl, MS.Time) - 1));		//Edited by PD
					int clasterPositionY1 = chartScale.GetYByValue(MS.Price_high)- (int)(indicatorModel.Claster_Height/2);
					int clasterPositionY2 = chartScale.GetYByValue(MS.Price_low)+ (int)(indicatorModel.Claster_Height/2);
					int MS_height = clasterPositionY2-clasterPositionY1;
					
					RenderTarget.FillRectangle(new SharpDX.RectangleF(MS_positionX,clasterPositionY1,indicatorModel.Claster_Width_Max*2,MS_height),MS_brush);
					
					RenderTarget.DrawText(MS.Volume.ToString(),Claster_textFormat,new SharpDX.RectangleF(MS_positionX+indicatorModel.Claster_Width_Max,clasterPositionY1+MS_height/2,MS.Volume.ToString().Length*8,10),brush1DX);
				}
				MS_brush.Dispose();
			}
			
			if(Input_Histogramm_OnOff)
				{
					
					#region linearGradientInitialize for Histogramm
					//if(isFirstDraw){
						System.Windows.Media.SolidColorBrush histogramm_Color = (System.Windows.Media.SolidColorBrush)Input_Histogramm_Color;
					
							SharpDX.Direct2D1.LinearGradientBrush 	linearGradientBrush_Histogramm_Standart = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
								{
									StartPoint = new SharpDX.Vector2(0, 0),
									EndPoint = new SharpDX. Vector2(0, 0),
								},
								new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
								{
									new	SharpDX.Direct2D1. GradientStop()
									{
										Color = SharpDX.Color.Black,
										Position = 0,
									},
									new 	SharpDX.Direct2D1. GradientStop()
									{
										Color = new SharpDX.Color(histogramm_Color.Color.R,histogramm_Color.Color.G,histogramm_Color.Color.B,histogramm_Color.Color.A),
										Position = 1,
									}
								}));
					
						System.Windows.Media.SolidColorBrush histogramm_Color_Filter1 = (System.Windows.Media.SolidColorBrush)Histogramm_Claster_FilterMin_Color;
					
							SharpDX.Direct2D1.LinearGradientBrush	linearGradientBrush_Histogramm_FilterMin = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
								{
									StartPoint = new SharpDX.Vector2(0, 0),
									EndPoint = new SharpDX. Vector2(0, 0),
								},
								new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
								{
									new	SharpDX.Direct2D1. GradientStop()
									{
										Color = SharpDX.Color.Black,
										Position = 0,
									},
									new 	SharpDX.Direct2D1. GradientStop()
									{
										Color = new SharpDX.Color(histogramm_Color_Filter1.Color.R,histogramm_Color_Filter1.Color.G,histogramm_Color_Filter1.Color.B,histogramm_Color_Filter1.Color.A),
										Position = 1,
									}
								}));
						
						System.Windows.Media.SolidColorBrush histogramm_Color_Filter2 = (System.Windows.Media.SolidColorBrush)Histogramm_Claster_FilterMax_Color;
					
							SharpDX.Direct2D1.LinearGradientBrush	linearGradientBrush_Histogramm_FilterMax = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
								{
									StartPoint = new SharpDX.Vector2(0, 0),
									EndPoint = new SharpDX. Vector2(0, 0),
								},
								new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
								{
									new	SharpDX.Direct2D1. GradientStop()
									{
										Color = SharpDX.Color.Black,
										Position = 0,
									},
									new 	SharpDX.Direct2D1. GradientStop()
									{
										Color = new SharpDX.Color(histogramm_Color_Filter2.Color.R,histogramm_Color_Filter2.Color.G,histogramm_Color_Filter2.Color.B,histogramm_Color_Filter2.Color.A),
										Position = 1,
									}
								}));
						
						System.Windows.Media.SolidColorBrush histogramm_Color_Max = (System.Windows.Media.SolidColorBrush)Input_Histogramm_MaxVolume_Color;
					
							SharpDX.Direct2D1.LinearGradientBrush	linearGradientBrush_Histogramm_MaxColor = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
								{
									StartPoint = new SharpDX.Vector2(0, 0),
									EndPoint = new SharpDX. Vector2(0, 0),
								},
								new 	SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
								{
									new	SharpDX.Direct2D1. GradientStop()
									{
										Color = SharpDX.Color.Black,
										Position = 0,
									},
									new 	SharpDX.Direct2D1. GradientStop()
									{
										Color = new SharpDX.Color(histogramm_Color_Max.Color.R,histogramm_Color_Max.Color.G,histogramm_Color_Max.Color.B,histogramm_Color_Max.Color.A),
										Position = 1,
									}
								}));
					//}	
					#endregion
					
					IEnumerable<KeyValuePair<double, Model.CurrentClaster>> histogramm_clasters = indicatorModel.Histogramm.ListOfCurrentBar.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue);
					
					foreach(KeyValuePair<double, Model.CurrentClaster> histogramm_claster in histogramm_clasters)
					{
						int Y_histogramm = chartScale.GetYByValue(histogramm_claster.Key)-indicatorModel.Claster_Height/2;
						
					if (indicatorModel.Histogramm.ListOfCurrentBar[indicatorModel.Histogramm.pocPrice].Volume_sum != 0)																		// Edited by PD
					{	
						int vol = histogramm_claster.Value.Volume_sum*Input_Histogramm_MaxSize/indicatorModel.Histogramm.ListOfCurrentBar[indicatorModel.Histogramm.pocPrice].Volume_sum;
						
						
						if(histogramm_claster.Key==indicatorModel.Histogramm.pocPrice)
						{
							linearGradientBrush_Histogramm_MaxColor.StartPoint=new SharpDX.Vector2(0, 0);
							linearGradientBrush_Histogramm_MaxColor.EndPoint=new SharpDX.Vector2(vol, 0);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(0,Y_histogramm,vol,indicatorModel.Claster_Height),linearGradientBrush_Histogramm_MaxColor);
						}
						else if(histogramm_claster.Value.Volume_sum>=Histogramm_Claster_FilterMax_Volume)
						{
							linearGradientBrush_Histogramm_FilterMax.StartPoint=new SharpDX.Vector2(0, 0);
							linearGradientBrush_Histogramm_FilterMax.EndPoint=new SharpDX.Vector2(vol, 0);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(0,Y_histogramm,vol,indicatorModel.Claster_Height),linearGradientBrush_Histogramm_FilterMax);
						}
						else if(histogramm_claster.Value.Volume_sum>=Histogramm_Claster_FilterMin_Volume)
						{
							linearGradientBrush_Histogramm_FilterMin.StartPoint=new SharpDX.Vector2(0, 0);
							linearGradientBrush_Histogramm_FilterMin.EndPoint=new SharpDX.Vector2(vol, 0);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(0,Y_histogramm,vol,indicatorModel.Claster_Height),linearGradientBrush_Histogramm_FilterMin);
						}
						else
						{
							linearGradientBrush_Histogramm_Standart.StartPoint=new SharpDX.Vector2(0, 0);
							linearGradientBrush_Histogramm_Standart.EndPoint=new SharpDX.Vector2(vol, 0);
							RenderTarget.FillRectangle(new SharpDX.RectangleF(0,Y_histogramm,vol,indicatorModel.Claster_Height),linearGradientBrush_Histogramm_Standart);
						}
						
						if(indicatorModel.Claster_Height>=10 && Input_HistogrammText_OnOff)
							RenderTarget.DrawText(histogramm_claster.Value.Volume_sum.ToString(),Claster_textFormat,new SharpDX.RectangleF(0,Y_histogramm,histogramm_claster.Value.Volume_sum.ToString().Length*8,indicatorModel.Claster_Height),brush1DX);
					}
					}
					
					linearGradientBrush_Histogramm_Standart.Dispose();
					linearGradientBrush_Histogramm_FilterMin.Dispose();
					linearGradientBrush_Histogramm_FilterMax.Dispose();
					linearGradientBrush_Histogramm_MaxColor.Dispose();
				}
			
			if(Input_TandS_OnOff)
			{
				//chartControl.Properties.BarMarginRight=Input_TandS_RightMargin;
				int textSize=Input_TandS_TextSize;
				SharpDX.DirectWrite.TextFormat TandS_textFormat = new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory,"TimesNewRoman", textSize);
				//SharpDX.Direct2D1.Brush TandSText_Color_Brush = Brushes.WhiteSmoke.ToDxBrush(RenderTarget);
				
				brush5DX = Input_TandS_Ask_Color.ToDxBrush(RenderTarget);
				brush6DX = Input_TandS_Bid_Color.ToDxBrush(RenderTarget);
				brush7DX = Input_TandS_FilterAsk_Color.ToDxBrush(RenderTarget);
				brush8DX = Input_TandS_FilterBid_Color.ToDxBrush(RenderTarget);
				
				int i_tmp=0;
				if(Input_OnlyFilterShow)
				{
					for(int i=indicatorModel.TandS_FilterPrints_price.Count-1;(i>=indicatorModel.TandS_FilterPrints_price.Count-Input_TandS_CountOrders && i>=0);i--)
					{
						i_tmp++;
						if(indicatorModel.TandS_FilterPrints_volume[i]>0){
							string str_tmp_ask=indicatorModel.TandS_FilterPrints_volume[i]+"@"+indicatorModel.TandS_FilterPrints_price[i];
							RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),brush7DX);
						}
						else
						{
							string str_tmp_bid=(indicatorModel.TandS_FilterPrints_volume[i]*(-1)).ToString()+"@"+indicatorModel.TandS_FilterPrints_price[i];
							RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),brush8DX);
						}
					}
				}
				else
				{
					for(int i=indicatorModel.TandS_AllPrints_price.Count-1;i>=0;i--)
					{
						i_tmp++;
						if(indicatorModel.TandS_AllPrints_volume[i]>0){
						
							string str_tmp_ask=indicatorModel.TandS_AllPrints_volume[i]+"@"+indicatorModel.TandS_AllPrints_price[i];
							if(indicatorModel.TandS_AllPrints_volume[i]>=Input_TandS_FilterAsk)
								RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),brush7DX);
							else
								//RenderTarget.FillRectangle(new RectangleF(ChartPanel.W-Input_TandS_RightPosition,50+i*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.6),textSize),TandSText_Color_Brush_FilterAsk);
							RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),brush5DX);
						}
						else
						{
							string str_tmp_bid=(indicatorModel.TandS_AllPrints_volume[i]*(-1)).ToString()+"@"+indicatorModel.TandS_AllPrints_price[i];
							if(Math.Abs(indicatorModel.TandS_AllPrints_volume[i])>=Input_TandS_FilterBid)
								RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),brush8DX);
							else
								//RenderTarget.FillRectangle(new RectangleF(ChartPanel.W-Input_TandS_RightPosition,50+i*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.6),textSize),TandSText_Color_Brush_FilterBid);
							RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),brush6DX);
						}
					}
				}
				
				if(Input_ShowFilterOnChart){
					
					for(int i=0;i<indicatorModel.TandS_FilterPrints_price.Count;i++){
						
						int BP_X =chartControl.GetXByTime(indicatorModel.TandS_FilterPrints_time[i]);
						
						if(BP_X<0 || BP_X>chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex))continue;
						
						int BP_Y =chartScale.GetYByValue(indicatorModel.TandS_FilterPrints_price[i]);
						if(indicatorModel.TandS_FilterPrints_volume[i]>0)
						{
							brush7DX.Opacity=(float)0.7;
							RenderTarget.FillEllipse(new  SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(BP_X,BP_Y),3,3),brush7DX);
						}
						else
						{
							brush8DX.Opacity=(float)0.7;
							RenderTarget.FillEllipse(new  SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(BP_X,BP_Y),3,3),brush8DX);
						}
					}
				}
				if (brush5DX != null)		{	brush5DX.Dispose();	}
				if (brush6DX != null)		{	brush6DX.Dispose();	}
				if (brush7DX != null)		{	brush7DX.Dispose();	}
				if (brush8DX != null)		{	brush8DX.Dispose();	}
				TandS_textFormat.Dispose();
			}
			isFirstDraw = false;
			
			Claster_textFormat.Dispose();
			linearGradientBrush_VerticalVolume_Standart.Dispose();
			linearGradientBrush_VerticalVolume_Filter1.Dispose();
			if (brush1DX != null)		{	brush1DX.Dispose();	}
			
			//stopWatch.Stop();
			//Print("Время вывода: " + stopWatch.Elapsed.TotalMilliseconds.ToString());
          	//stopWatch.Reset();
			
			}
            catch (Exception ex) { Print("MRPack OnRender 1385: " + ex); }
		}
		
		
		
		
		
		public override void OnRenderTargetChanged()
		{
			if (Claster_ColorDX != null)
  			{
    			Claster_ColorDX.Dispose();
  			}
			
			if (Claster_FilterMin_ColorDX != null)
  			{
    			Claster_FilterMin_ColorDX.Dispose();
  			}
			
			if (Claster_FilterMax_ColorDX != null)
  			{
    			Claster_FilterMax_ColorDX.Dispose();
  			}
			
			if (RenderTarget != null)
			{
				Claster_ColorDX = Claster_Color.ToDxBrush(RenderTarget);
				Claster_FilterMin_ColorDX = Claster_FilterMin_Color.ToDxBrush(RenderTarget);
				Claster_FilterMax_ColorDX = Claster_FilterMax_Color.ToDxBrush(RenderTarget);
				Input_ClasterMax_ColorDX = Input_ClasterMax_Color.ToDxBrush(RenderTarget);
			}
		}
			
		
		#region Properties
		
			#region Claster
		
				
		
		
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Cluster Color", Order = 1, GroupName = "Cluster")]
				public Brush Input_Claster_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Claster_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Claster_Color); }
					set { Input_Claster_Color = Serialize.StringToBrush(value); }
				}
				
				[Display(Name="Cluster On/Off", Description="", Order=1, GroupName="Cluster")]
				public bool Input_Claster_OnOff
				{ get; set; }
				
				
				[Range(0, int.MaxValue)]
				[Display(Name="Filter1 volume", Description="", Order=4, GroupName="Cluster")]
				public int Input_Claster_Filter1_Value
				{ get; set; }
				
				[Range(0, int.MaxValue)]
				[Display(Name="Filter2 volume", Description="", Order=5, GroupName="Cluster")]
				public int Input_Claster_Filter2_Value
				{ get; set; }
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Filter1 Color", Order = 6, GroupName = "Cluster")]
				public Brush Input_Claster_Filter1_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Claster_Filter1_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Claster_Filter1_Color); }
					set { Input_Claster_Filter1_Color = Serialize.StringToBrush(value); }
				}
				
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Filter2 Color", Order = 7, GroupName = "Cluster")]
				public Brush Input_Claster_Filter2_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Claster_Filter2_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Claster_Filter2_Color); }
					set { Input_Claster_Filter2_Color = Serialize.StringToBrush(value); }
				}
				
				
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "MaxCluster Color", Order = 8, GroupName = "Cluster")]
				public Brush Input_ClasterMax_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_ClasterMax_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_ClasterMax_Color); }
					set { Input_ClasterMax_Color = Serialize.StringToBrush(value); }
				}
				
				[Display(Name="Max Cluster On/Off", Description="", Order=9, GroupName="Cluster")]
				public bool Input_MaxClaster_OnOff
				{ get; set; }
				
				
				[Display(Name="Text On/Off", Description="", Order=10, GroupName="Cluster")]
				public bool Input_ClasterText_OnOff
				{ get; set; }
				[Range(0, int.MaxValue)]
				[Display(Name="Min volume", Description="", Order=11, GroupName="Cluster")]
				public int Input_ClasterMinVolume
				{ get; set; }
				
				[Range(1, int.MaxValue)]
				[Display(Name="Max volume", Description="", Order=11, GroupName="Cluster")]
				public int Input_ClasterMaxVolume
				{ get; set; }
				
				[Display(Name="Bid/Ask On/Off", Description="", Order=12, GroupName="Cluster")]
				public bool Input_Claster_BidAsk_OnOff
				{ get; set; }
				
			#endregion
				
			#region VerticalVolume
				[Display(Name="Vertical volume On/Off", Description="", Order=1, GroupName="Vertical Volume")]
				public bool Input_VerticalVolume_OnOff
				{ get; set; }
				
				
				[Display(Name="Text On/Off", Description="", Order=2, GroupName="Vertical Volume")]
				public bool Input_VerticalVolumeText_OnOff
				{ get; set; }
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Color", Order = 3, GroupName = "Vertical Volume")]
				public Brush Input_VerticalVolume_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_VerticalVolume_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_VerticalVolume_Color); }
					set { Input_VerticalVolume_Color = Serialize.StringToBrush(value); }
				}
				
				[Range(0, int.MaxValue)]
				[Display(Name="Min volume", Description="", Order=4, GroupName="Vertical Volume")]
				public int Input_VerticalVolume_Min
				{ get; set; }
				
				
				[Range(0, int.MaxValue)]
				[Display(Name="Volume size", Description="", Order=5, GroupName="Vertical Volume")]
				public int Input_VerticalVolume_Size
				{ get; set; }
				
				
				[Range(0, int.MaxValue)]
				[Display(Name="Volume filter1", Description="", Order=6, GroupName="Vertical Volume")]
				public int Input_VerticalVolume_Filter1_Value
				{ get; set; }
		
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Filter1 Color", Order = 7, GroupName = "Vertical Volume")]
				public Brush Input_VerticalVolume_Filter1_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_VerticalVolume_Filter1_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_VerticalVolume_Filter1_Color); }
					set { Input_VerticalVolume_Filter1_Color = Serialize.StringToBrush(value); }
				}
			#endregion
				
			#region Histogramm Input
			
				[Display(Name="Histogramm On/Off", Description="", Order=1, GroupName="Histogramm Volume")]
				public bool Input_Histogramm_OnOff
				{ get; set; }
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Histogramm Color", Order = 2, GroupName = "Histogramm Volume")]
				public Brush Input_Histogramm_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Histogramm_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Histogramm_Color); }
					set { Input_Histogramm_Color = Serialize.StringToBrush(value); }
				}
				
				
				[Display(Name="Text On/Off", Description="", Order=3, GroupName="Histogramm Volume")]
				public bool Input_HistogrammText_OnOff
				{ get; set; }
				
				
				[Display(Name="Max Volume On/Off", Description="", Order=4, GroupName="Histogramm Volume")]
				public bool Input_HistogrammMaxVolume_OnOff
				{ get; set; }
				
				[Range(0, int.MaxValue)]
				[Display(Name="Histogramm filter1", Description="", Order=5, GroupName="Histogramm Volume")]
				public int Input_Histogramm_Filter1
				{ get; set; }
				
				[Range(0, int.MaxValue)]
				[Display(Name="Histogramm filter2", Description="", Order=6, GroupName="Histogramm Volume")]
				public int Input_Histogramm_Filter2
				{ get; set; }
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Filter1 Color", Order = 7, GroupName = "Histogramm Volume")]
				public Brush Input_Histogramm_Filter1_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Histogramm_Filter1_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Histogramm_Filter1_Color); }
					set { Input_Histogramm_Filter1_Color = Serialize.StringToBrush(value); }
				}
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Filter2 Color", Order = 8, GroupName = "Histogramm Volume")]
				public Brush Input_Histogramm_Filter2_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_HistogrammSerialize
				{
					get { return Serialize.BrushToString(Input_Histogramm_MaxVolume_Color); }
					set { Input_Histogramm_MaxVolume_Color = Serialize.StringToBrush(value); }
				}
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Max Volume Color", Order = 9, GroupName = "Histogramm Volume")]
				public Brush Input_Histogramm_MaxVolume_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_Histogramm_MaxVolume_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_Histogramm_MaxVolume_Color); }
					set { Input_Histogramm_MaxVolume_Color = Serialize.StringToBrush(value); }
				}
				
				[Range(0, int.MaxValue)]
				[Display(Name="Max Size", Description="", Order=10, GroupName="Histogramm Volume")]
				public int Input_Histogramm_MaxSize
				{ get; set; }
				
				/*[Range(0, int.MaxValue)]
				[Display(Name="Min Volume", Description="", Order=11, GroupName="Histogramm Volume")]
				public int Input_Histogramm_MinVolume
				{ get; set; }*/
			#endregion
				
			#region VPOC OnDay Input
				[Display(Name="On/Off", Description="", Order=1, GroupName="Day POC")]
				public bool Input_PocOnDay_OnOff
				{ get; set; }

				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Line Color", Order = 2, GroupName = "Day POC")]
				public Brush Input_PocOnDay_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_PocOnDay_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_PocOnDay_Color); }
					set { Input_PocOnDay_Color = Serialize.StringToBrush(value); }
				}
			
			#endregion
				
				
			#region Market Stop Input
				[Display(Name="On/Off", Description="", Order=1, GroupName="Market Stop")]
				public bool Input_MS_OnOff
				{ get; set; }

				[Range(0, int.MaxValue)]
				[Display(Name="Volume limit", Description="", Order=2, GroupName="Market Stop")]
				public int Input_MS_VolumeLimit
				{ get; set; }
				
				[Display(Name="Alert On/Off", Description="", Order=3, GroupName="Market Stop")]
				public bool Input_MSAlert_OnOff
				{ get; set; }
				
				[XmlIgnore]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Box Color", Order = 4, GroupName = "Market Stop")]
				public Brush Input_MS_Color
				{ get; set; }
				[Browsable(false)]
				public string Input_MS_ColorSerialize
				{
					get { return Serialize.BrushToString(Input_MS_Color); }
					set { Input_MS_Color = Serialize.StringToBrush(value); }
				}
		#endregion
				
		#region Buttons Input
		
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Button On Color", Order = 2, GroupName = "Buttons")]
			public Brush Input_ButtonsOn_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ButtonsOn_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ButtonsOn_Color); }
				set { Input_ButtonsOn_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Button Off Color", Order = 3, GroupName = "Buttons")]
			public Brush Input_ButtonsOff_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ButtonsOff_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ButtonsOff_Color); }
				set { Input_ButtonsOff_Color = Serialize.StringToBrush(value); }
			}
		#endregion
			
		#region Profile Range Input
			[Display(Name="Text On/Off", Description="", Order=1, GroupName="Range Profile")]
			public bool Range_Profile_Text_OnOff
			{ get; set; }
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Text Color", Order = 2, GroupName = "Range Profile")]
			public Brush Range_Profile_Text_Color
			{ get; set; }
			[Browsable(false)]
			public string Range_Profile_Text_ColorSerialize
			{
				get { return Serialize.BrushToString(Range_Profile_Text_Color); }
				set { Range_Profile_Text_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Inside Color", Order = 3, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Inside_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Inside_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Inside_Color); }
				set { Input_ProfileRange_Inside_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "POC Color", Order = 4, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_POC_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_POC_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_POC_Color); }
				set { Input_ProfileRange_POC_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Border Color", Order = 5, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Border_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Border_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Border_Color); }
				set { Input_ProfileRange_Border_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Inside Bid Color", Order = 6, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Inside_Bid_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Inside_Bid_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Inside_Bid_Color); }
				set { Input_ProfileRange_Inside_Bid_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Inside Ask Color", Order = 7, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Inside_Ask_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Inside_Ask_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Inside_Ask_Color); }
				set { Input_ProfileRange_Inside_Ask_Color = Serialize.StringToBrush(value); }
			}
			
			[Range(1, 100)]
				[Display(Name="Text Opacity", Description="", Order=8, GroupName="Range Profile")]
				public int Profile_Text_Opacity
				{ get; set; }
			
			[Range(1, 100)]
			[Display(Name="Profile Opacity", Description="", Order=9, GroupName="Range Profile")]
			public int Profile_Opacity
			{ get; set; }
			/*[Display(Name="Bid/Ask On/Off", Description="", Order=4, GroupName="Range Profile")]
			public bool Input_RangeProfile_BidAsk_OnOff
			{ get; set; }
			
			[Display(Name="Extended Line On/Off", Description="", Order=5, GroupName="Range Profile")]
			public bool Input_RangeProfile_ExtendedLine_OnOff
			{ get; set; }*/
		#endregion
				
		#region TandS Imput
			[Display(Name="On/Off", Description="", Order=1, GroupName="T&S")]
			public bool Input_TandS_OnOff
			{ get; set; }
			
			/*[Range(0, int.MaxValue)]
			[Display(Name="Right margin", Description="", Order=2, GroupName="T&S")]
			public int Input_TandS_RightMargin
			{ get; set; }*/
			
			[Range(0, int.MaxValue)]
			[Display(Name="Right position", Description="", Order=3, GroupName="T&S")]
			public int Input_TandS_RightPosition
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Top position", Description="", Order=3, GroupName="T&S")]
			public int Input_TandS_TopPosition
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Count orders", Description="", Order=4, GroupName="T&S")]
			public int Input_TandS_CountOrders
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Text Size", Description="", Order=5, GroupName="T&S")]
			public int Input_TandS_TextSize
			{ get; set; }
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Bid Order Color", Order = 6, GroupName = "T&S")]
			public Brush Input_TandS_Bid_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_TandS_Bid_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_TandS_Bid_Color); }
				set { Input_TandS_Bid_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Ask Order Color", Order = 7, GroupName = "T&S")]
			public Brush Input_TandS_Ask_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_TandS_Ask_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_TandS_Ask_Color); }
				set { Input_TandS_Ask_Color = Serialize.StringToBrush(value); }
			}
			
			[Range(0, int.MaxValue)]
			[Display(Name="Filter Bid", Description="", Order=8, GroupName="T&S")]
			public int Input_TandS_FilterBid
			{ get; set; }
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "FilterBid Color", Order = 9, GroupName = "T&S")]
			public Brush Input_TandS_FilterBid_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_TandS_FilterBid_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_TandS_FilterBid_Color); }
				set { Input_TandS_FilterBid_Color = Serialize.StringToBrush(value); }
			}
			
			[Range(0, int.MaxValue)]
			[Display(Name="Filter Ask", Description="", Order=10, GroupName="T&S")]
			public int Input_TandS_FilterAsk
			{ get; set; }
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "FilterAsk Color", Order = 11, GroupName = "T&S")]
			public Brush Input_TandS_FilterAsk_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_TandS_FilterAsk_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_TandS_FilterAsk_Color); }
				set { Input_TandS_FilterAsk_Color = Serialize.StringToBrush(value); }
			}
			//Input_OnlyFilterShow
			[Display(Name="Only filter show", Description="", Order=12, GroupName="T&S")]
			public bool Input_OnlyFilterShow
			{ get; set; }
			
			[Display(Name="Show filter on chart", Description="", Order=13, GroupName="T&S")]
			public bool Input_ShowFilterOnChart
			{ get; set; }
			
		#endregion
			
		#region TickAggregator
			[Display(Name="On/Off", Description="", Order=1, GroupName="TickAggregator")]
			public bool Input_TickAggregator_OnOff
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Tick Limit(contract)", Description="", Order=2, GroupName="TickAggregator")]
			public int Input_TickAggregator_TickLimit
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Delay(ms)", Description="", Order=3, GroupName="TickAggregator")]
			public int Input_TickAggregator_Delay
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Summ Limit(contract)", Description="", Order=3, GroupName="TickAggregator")]
			public int Input_TickAggregator_SummLimit
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Range(pips)", Description="", Order=4, GroupName="TickAggregator")]
			public int Input_TickAggregator_Range
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Big Print(contract)", Description="", Order=5, GroupName="TickAggregator")]
			public int Input_TickAggregator_BigPrint
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="Tick Show(contract)", Description="", Order=6, GroupName="TickAggregator")]
			public int Input_TickAggregator_TickShow
			{ get; set; }
			
			[Range(0, int.MaxValue)]
			[Display(Name="MaxSize", Description="", Order=7, GroupName="TickAggregator")]
			public int Input_TickAggregator_Distance
			{ get; set; }
			
			[Display(Name="Alert On/Off", Description="", Order=8, GroupName="TickAggregator")]
			public bool Input_TickAggregator_AlertOnOff
			{ get; set; }
			
			[Display(Name="Standart", Description="", Order=9, GroupName="TickAggregator")]
			public bool Input_TickAggregator_Standart
			{ get; set; }
			
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Ask Color", Order = 10, GroupName = "TickAggregator")]
			public Brush Input_TickAggregator_AskColor
			{ get; set; }
			[Browsable(false)]
			public string Input_TickAggregator_AskColorSerialize
			{
				get { return Serialize.BrushToString(Input_TickAggregator_AskColor); }
				set { Input_TickAggregator_AskColor = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Bid Color", Order = 11, GroupName = "TickAggregator")]
			public Brush Input_TickAggregator_BidColor
			{ get; set; }
			[Browsable(false)]
			public string Input_TickAggregator_BidColorSerialize
			{
				get { return Serialize.BrushToString(Input_TickAggregator_BidColor); }
				set { Input_TickAggregator_BidColor = Serialize.StringToBrush(value); }
			}
			
			
		#endregion
			
			
		#region Price Line Input
			[Display(Name="Price Line On/Off", Description="", Order=1, GroupName="Price Line")]
			public bool Input_PriceLine_OnOff
			{ get; set; }
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Price Line Color", Order = 2, GroupName = "Price Line")]
			public Brush Input_PriceLine_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_PriceLine_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_PriceLine_Color); }
				set { Input_PriceLine_Color = Serialize.StringToBrush(value); }
			}
		#endregion
			
			
			
		#region Instrument
			
	        [XmlIgnore]
	        [Display(Name = "Text color", GroupName = "Background Contract Name", Order = 0)]
	        public Brush TextBrush { get; set; }

	        [Browsable(false)]
	        public string TextBrushSerialize
	        {
	            get { return Serialize.BrushToString(TextBrush); }
	            set { TextBrush = Serialize.StringToBrush(value); }
	        }

	        //[NinjaScriptProperty]
			// [XmlIgnore]
			[Display(Name = "Auto text size?", Description="Text size adjusts to panel height.",  GroupName = "Background Contract Name", Order = 1)]
			public bool UseAutoTextSize
			{ get; set; }

	        [Range(8, 600)/*, NinjaScriptProperty*/]
			// [XmlIgnore]
	        [Display(Name = "Text size", GroupName = "Background Contract Name", Order = 2)]
	        public int TextSize
	        { get; set; }

	        [Range(0, 100)/*, NinjaScriptProperty*/]
			// [XmlIgnore]
	        [Display(Name = "Text opacity (0-100)", GroupName = "Background Contract Name", Order = 3)]
	        public int TextOpacity
	        { get; set; }

        
		#endregion
			
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MRPack.MRIndicator[] cacheMRIndicator;
		public MRPack.MRIndicator MRIndicator()
		{
			return MRIndicator(Input);
		}

		public MRPack.MRIndicator MRIndicator(ISeries<double> input)
		{
			if (cacheMRIndicator != null)
				for (int idx = 0; idx < cacheMRIndicator.Length; idx++)
					if (cacheMRIndicator[idx] != null &&  cacheMRIndicator[idx].EqualsInput(input))
						return cacheMRIndicator[idx];
			return CacheIndicator<MRPack.MRIndicator>(new MRPack.MRIndicator(), input, ref cacheMRIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MRPack.MRIndicator MRIndicator()
		{
			return indicator.MRIndicator(Input);
		}

		public Indicators.MRPack.MRIndicator MRIndicator(ISeries<double> input )
		{
			return indicator.MRIndicator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MRPack.MRIndicator MRIndicator()
		{
			return indicator.MRIndicator(Input);
		}

		public Indicators.MRPack.MRIndicator MRIndicator(ISeries<double> input )
		{
			return indicator.MRIndicator(input);
		}
	}
}

#endregion
