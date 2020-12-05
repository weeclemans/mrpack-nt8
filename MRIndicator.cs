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
		
		bool key=false;
		bool isTrial=false;
		//bool isFirstDraw=false;
		bool isConnection=true;
		bool keyFileFound=true;
		bool isfullVersion=false;
		StreamReader SR;
		

		
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
			
			int radius = data.Volume* (Input_TickAggregator_Distance/2) / indicatorModel.MaxTickAggregatorVolume;
			if(radius<5)radius=5;
			
			
			DrawingTools.CustomEllipse elipse = Draw.CustomEllipse(this,time.ToString()+" - "+price.ToString(),time,data.TopPrice+TickSize/2, indicatorModel);
			elipse.Radius=radius;
			elipse.TickAggregatorData = data;
		}
		
		
		public void CreateNewRangeProfile(string tag, DateTime startTime,double top,DateTime endTime,double bot, int ProfileType, bool extended)
		{
			
			
			//DrawingTools.RangeProfile profile = Draw.RangeProfile(this,tag,true, new DateTime(2017,12,18,10,0,0), 1.1895, new DateTime(2017,12,18,15,0,0), 1.187, Brushes.Red, 2, indicatorModel);
			//DrawingTools.RangeProfile profile = Draw.RangeProfile(this,tag,true, startTime, top, endTime, bot, Brushes.Red, 2, indicatorModel);
			DrawingTools.RangeProfile2 profile = Draw.RangeProfile2(this,tag, startTime, top, endTime, bot,Brushes.Red, indicatorModel);
			profile.IsLocked = false;
			profile.ProfileType = ProfileType;
			profile.ExtendedLine = extended;
			indicatorModel.RangeProfiles.Add(profile);
			//profile.IsLocked = false;
			
			int startBar = (int)ChartControl.GetSlotIndexByX(100);
			int endBar = (int)ChartControl.GetSlotIndexByX(200);
			
			
			
		}
		
		public static DateTime GetNetworkTime()
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
            
        }
		
		static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }	
		
		
		 static byte[] GetMd5Hash(MD5 md5Hash, string input)//Функция получения хеша из строки
        {
            byte[] data = md5Hash.ComputeHash(GetBytes(input));
            return data;
        }
		 static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
		
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
		
		
		
		private DateTime prevdatePrint;
		
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
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
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
					Input_ProfileRange_Inside_Color=Brushes.LightSkyBlue;
					Input_ProfileRange_POC_Color=Brushes.Azure;
					//Input_RangeProfile_BidAsk_OnOff=false;
					//Input_RangeProfile_ExtendedLine_OnOff=true;
					Input_ProfileRange_Border_Color=Brushes.DimGray;
					//RangeProfileFileXML=AnyFile0;
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
				
				#region lalalal
						byte[] HashValueProcessor = new byte[20];
						byte[] SignedHashValue = new byte[40];
			            string source = Cbi.License.MachineId;
						DateTime serverTime=new DateTime();
						//try{
							serverTime=GetNetworkTime();
						if(serverTime.ToString()=="01.01.0001 0:00:00")isConnection=false;

						string endTime="";
						//Print(AnyFile0);
						try{
							//Print("Key File Path:"+KeyFile_path);
							String textfile=AnyFile0;
							SR = new StreamReader(textfile, System.Text.Encoding.Unicode, true);
						}catch(Exception e){
							Print(e);
							keyFileFound=false;
						}
			            string line;
			            int z = 0;
			            DSAParameters publicKeyInfo1 = new DSAParameters();
						
						
						int tmpByteArray_Counter = 780;
			            byte[] tmpByteArray_G = new byte[] { 137,194,139,245,126,171,150,140,132,96,7,211,183,120,206,200,71,111,112,89,164,249,155,19,161,237,37,100,97,10,108,50,63,80,5,129,201,131,185,163,228,113,215,98,75,192,119,105,207,204,240,6,243,202,13,72,240,58,151,74,205,221,169,107,118,83,240,36,23,187,229,249,119,32,100,108,6,200,55,8,119,150,193,25,64,251,95,136,229,95,21,1,209,65,36,191,16,170,82,139,196,157,63,185,212,107,146,105,13,147,112,52,174,130,195,103,38,184,152,28,202,198,147,135,232,185,113,157};
			            byte[] tmpByteArray_J = new byte[] { 186,190,193,65,73,127,195,42,18,193,92,31,243,69,12,39,18,63,36,72,76,87,223,13,22,30,74,157,45,107,252,215,215,76,36,79,75,246,52,166,3,123,209,130,184,60,116,14,105,142,168,16,214,64,162,82,224,64,135,200,48,15,170,33,18,116,34,91,17,70,29,161,196,147,20,118,86,154,62,130,127,190,134,175,19,30,120,94,187,5,228,199,93,151,68,46,76,110,181,186,217,56,23,88,233,128,208,214};
			            byte[] tmpByteArray_P = new byte[] { 169,165,221,101,15,137,155,103,207,27,194,147,240,73,174,190,198,196,90,128,72,28,202,213,110,11,210,97,94,58,77,155,16,247,105,180,157,42,32,9,146,139,184,254,86,61,109,2,32,188,237,123,61,58,63,161,81,49,176,245,90,134,135,120,42,153,2,35,150,66,84,197,149,234,202,134,106,246,231,71,230,141,250,13,129,87,54,26,76,185,138,69,99,114,149,15,17,7,142,252,45,174,109,69,167,55,130,12,250,168,200,3,119,17,159,8,164,146,222,182,57,152,57,13,244,75,108,135};
			            byte[] tmpByteArray_Q = new byte[] { 232,143,238,30,251,119,205,173,166,9,174,16,54,168,58,90,61,82,191,137};
			            byte[] tmpByteArray_Seed = new byte[] { 210,221,87,192,220,167,99,254,127,52,51,125,171,19,177,71,26,18,200,40};
			            byte[] tmpByteArray_Y = new byte[] {15,189,51,109,142,25,215,80,145,24,156,51,106,30,23,120,146,233,61,34,136,69,116,133,114,225,149,9,3,229,92,54,163,210,232,3,229,213,101,124,16,89,90,185,232,137,56,238,76,95,46,18,167,74,249,106,7,26,220,150,219,95,100,11,200,40,94,235,58,1,180,170,246,197,29,52,194,219,110,170,2,61,119,155,231,215,90,27,44,48,154,21,159,203,191,238,171,146,78,65,243,107,23,247,166,110,243,231,216,88,90,252,148,65,245,220,57,153,50,116,48,6,183,56,30,173,186,248};
								
						
						publicKeyInfo1.Counter = tmpByteArray_Counter;
			            publicKeyInfo1.G = tmpByteArray_G;
			            publicKeyInfo1.J = tmpByteArray_J;
			            publicKeyInfo1.P = tmpByteArray_P;
			            publicKeyInfo1.Q = tmpByteArray_Q;
			            publicKeyInfo1.Seed = tmpByteArray_Seed;
			            publicKeyInfo1.Y = tmpByteArray_Y;

						while (keyFileFound && (line = SR.ReadLine()) != null )
			            {
							if (z == 0)
			                {
			                    string[] str_mas = line.Split('/');
			                    List<byte> byte_tmp = new List<byte>();
								string str="";
			                    for (int i = 0; i < str_mas.Length - 1; i++){ byte_tmp.Add(Convert.ToByte(str_mas[i]));str+=Convert.ToChar(Convert.ToByte(str_mas[i]));}
			                   // publicKeyInfo1.Y = byte_tmp.ToArray();
								endTime=str;
								//Print(endTime);
			                }
			                if (z == 1)
			                {
			                    string[] str_mas = line.Split('/');
			                    List<byte> byte_tmp = new List<byte>();
			                    for (int i = 0; i < str_mas.Length - 1; i++) byte_tmp.Add(Convert.ToByte(str_mas[i]));
			                    SignedHashValue = byte_tmp.ToArray();
			                }
			                z++;
			            }
						
						if(keyFileFound){
							SR.Close();
							
							//Print("kyky");
							using (MD5 md5Hash = MD5.Create())
				            {
				                GetMd5Hash(md5Hash, source+endTime).CopyTo(HashValueProcessor, 0);//Получаем хэш введенного номера процессора и записываем его в массив ьайт размером 20 байт.
				            }
							
							
							//Print(myDate.Day.ToString());
							bool tmp = DSAVerifyHash(HashValueProcessor, SignedHashValue, publicKeyInfo1, "SHA1");
				            if (tmp)
				            {
				               // Print("YES");
								key=true;
								//Print("--"+endTime);
								if(endTime=="00:00:0000 00:00:00"){
									//key=true;
									isfullVersion=true;
									isTrial=false;
								}else{
									isfullVersion=false;
									DateTime endDateTime = DateTime.ParseExact(endTime, "dd:MM:yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
									if(serverTime<=endDateTime){
										isTrial=true;
									}
									else{
										isTrial=false;	
									}
								}
				            }
				            else
				            {
								key=false;
				            }
						}
					#endregion
				
				indicatorModel = new Model(this);
				indicatorModel.Input_ProfileRange_Inside_Color=Input_ProfileRange_Inside_Color;
				indicatorModel.Input_ProfileRange_POC_Color=Input_ProfileRange_POC_Color;
				/*indicatorModel.Input_RangeProfile_BidAsk_OnOff=false;
				indicatorModel.Input_RangeProfile_ExtendedLine_OnOff=true;*/
				indicatorModel.Input_ProfileRange_Border_Color=Input_ProfileRange_Border_Color;
				
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
						  Margin = new Thickness(20,20,0,0)
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
							MinWidth=70,
							Width = 70
				        };
						
						
						stackPanel = new System.Windows.Controls.StackPanel();
						stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
						//stackPanel.Margin =  new Thickness(20,0,0,0);
						
						
						
				 
				        // Define the custom Sell Button control object
				        myHVButton = new System.Windows.Controls.Button
				        {
				          Name = "my1Button",
				          Content = "HV",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_Histogramm_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myVVButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "VV",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_VerticalVolume_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myDPButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "DP",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_PocOnDay_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myRPButton = new System.Windows.Controls.Button
				        {
				          Name = "my2Button",
				          Content = "RP",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myTSButton = new System.Windows.Controls.Button
				        {
				          Name = "myTSButton",
				          Content = "TS",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_TandS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myMSButton = new System.Windows.Controls.Button
				        {
				          Name = "myMSButton",
				          Content = "MS",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_MS_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
				        };
						myTAButton = new System.Windows.Controls.Button
				        {
				          Name = "myTAButton",
				          Content = "TA",
				          Foreground = Brushes.WhiteSmoke,
				          Background = Input_TickAggregator_OnOff ? Input_ButtonsOn_Color : Input_ButtonsOff_Color,
						  Margin =  new Thickness(10,0,0,0),
							MinWidth=40,
							Width = 40
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
				if(!(key && ((isTrial && isConnection) || isfullVersion)))
				{return;}
				
				
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
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			//Print(Time[0]);
			indicatorModel.CloseBar(Time[0]);
			
			if (Bars.IsFirstBarOfSession)
				indicatorModel.CloseDay();
				
		}
		
		
		
		
		
		
		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if(isFirstDraw)
			{
				for(int i=0;i<indicatorModel.TickAggregatorElements.Count;i++)
				{
					//Print(indicatorModel.TickAggregatorElements[i].Time.ToString()+" - "+indicatorModel.TickAggregatorElements[i].Price.ToString()+" - "+indicatorModel.TickAggregatorElements[i].Volume.ToString()+" - "+indicatorModel.TickAggregatorElements[i].LowPrice.ToString()+" - "+indicatorModel.TickAggregatorElements[i].TopPrice.ToString());
					CreateNewTickAggregatorEllipse(indicatorModel.TickAggregatorElements[i].Time,indicatorModel.TickAggregatorElements[i].Price,indicatorModel.TickAggregatorElements[i]);
				}
			}
			
			if(!(key && ((isTrial && isConnection) || isfullVersion)))
			{
				#region Comands
				SharpDX.Direct2D1.SolidColorBrush KBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
				SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();
				SharpDX.DirectWrite.TextFormat KTextFormat = new SharpDX.DirectWrite.TextFormat(fontFactory, "Segoe UI", 50f);
						
				if(isConnection){
					if(keyFileFound){
						if(key){
							if(!isfullVersion){
								if(isTrial){
									//RenderTarget.DrawText("That is trial version",KTextFormat,new RectangleF(100,100,1000,100),KBrush);
								}else{
									RenderTarget.DrawText("Trial is end",KTextFormat,new SharpDX.RectangleF(100,100,1000,100),KBrush);
								}	
							}
						}else{
							RenderTarget.DrawText("This is not a license",KTextFormat,new SharpDX.RectangleF(100,100,1000,100),KBrush);
						}
					}else{
						RenderTarget.DrawText("Key file not found",KTextFormat,new SharpDX.RectangleF(100,100,1000,100),KBrush);
					}	
				}else{
					if(!isfullVersion){
						RenderTarget.DrawText("Connection lost",KTextFormat,new SharpDX.RectangleF(100,100,1000,100),KBrush);
					}
				}
			#endregion
				return;
			}
				
			
			
			
			maxValue = chartScale.MaxValue;
			minValue = chartScale.MinValue;
			/*Stopwatch stopWatch = new Stopwatch();
        	stopWatch.Start();*/
			Brush Claster_Color;
			Brush Claster_FilterMin_Color;
			Brush Claster_FilterMax_Color;
			int Claster_FilterMin_Volume;
			int Claster_FilterMax_Volume;
			
			Brush Histogramm_Claster_Color;
			Brush Histogramm_Claster_FilterMin_Color;
			Brush Histogramm_Claster_FilterMax_Color;
			int Histogramm_Claster_FilterMin_Volume;
			int Histogramm_Claster_FilterMax_Volume;
			
			
			
			#region Instrument
					if( Bars == null || ChartControl == null || Bars.Instrument == null || !IsVisible)
				{
					return;
				}

			    try
			    {
			        int textSize    = TextSize;
			        int cph     = ChartPanel.H;

			        if (UseAutoTextSize)
			            textSize = (int) (cph * 0.75f);

			        SimpleFont simpleFont = new SimpleFont(ChartControl.Properties.LabelFont.Family.ToString(), textSize);
	                // Place the text in OnRender to adjust automatically if needed
			        Draw.TextFixed(this, "SymbolText", Instrument.MasterInstrument.Name, TextPosition.Center, TextBrush, simpleFont, Brushes.Transparent, Brushes.Transparent, 0);
			    }
			    catch (Exception ex)
			    {
			        Print(Name + " : " + ex);
			    }
			#endregion
		
			
			
			
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
					SharpDX.Direct2D1.Brush PriceLine_Color_Brush = Input_PriceLine_Color.ToDxBrush(RenderTarget);
					
					point0.X = chartControl.GetXByTime(indicatorModel.ListOfBar.Last().Time)+indicatorModel.Claster_Width_Max+2;
					point0.Y = chartScale.GetYByValue(LastPrice_Line);
					point1.X = ChartPanel.W;
					point1.Y = point0.Y;
				 
					
					//RenderTarget.FillGeometry(myLineGeometry,PriceLine_Color_Brush);
					RenderTarget.DrawLine(point0, point1, PriceLine_Color_Brush, 1);
					RenderTarget.DrawText(LastPrice_Line.ToString(),textFormatPriceLine,new SharpDX.RectangleF((point0.X)+indicatorModel.Claster_Width_Max+15,point0.Y,100,20),PriceLine_Color_Brush);
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
					if(indicatorModel.Claster_Height>1)
					{
						clasters = bar.ListOfClasters.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue);
					}
					else
					{
						clasters = bar.ListOfClasters.Where(c => c.Key<=ChartPanel.MaxValue && c.Key>=ChartPanel.MinValue && (c.Value.Volume_sum>=Claster_FilterMin_Volume || c.Key==bar.PocPrice));
						
						int clasterPositionY1 = chartScale.GetYByValue(bar.ListOfClasters.First().Key)- (int)(indicatorModel.Claster_Height/2);
						int clasterPositionY2 = chartScale.GetYByValue(bar.ListOfClasters.Last().Key)+ (int)(indicatorModel.Claster_Height/2);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX, clasterPositionY1 , 1, clasterPositionY2-clasterPositionY1),Input_Claster_Color.ToDxBrush(RenderTarget));
					}
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
							int Procent_DecRealVol_MinVol_Dec_MaxVol_MinVol = Dec_RealVol_MinVol*100/Dec_MaxVol_MinVol;
							Claster_RealWidth=Procent_DecRealVol_MinVol_Dec_MaxVol_MinVol*indicatorModel.Claster_Width_Max/100;
						}
						
						if(Claster_RealWidth<1) Claster_RealWidth=1;
						
						
						Claster_Color = Input_Claster_Color;
						if(clasterVolume>=Claster_FilterMax_Volume)
						{
							Claster_Color = Claster_FilterMax_Color;
							/*if(!(indicatorModel.Claster_Height>1))
								Claster_RealWidth*=3;*/
						}
						else if(clasterVolume>=Claster_FilterMin_Volume)
						{
							Claster_Color = Claster_FilterMin_Color;
							/*if(!(indicatorModel.Claster_Height>1))
								Claster_RealWidth*=3;*/
						}
						
						if(claster.Key==bar.PocPrice && Input_MaxClaster_OnOff)
							Claster_Color = Input_ClasterMax_Color;
							
						
							
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX, clasterPositionY - (int)(indicatorModel.Claster_Height/2), Claster_RealWidth, indicatorModel.Claster_Height),Claster_Color.ToDxBrush(RenderTarget));
						
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
									RenderTarget.DrawText(str1,Claster_textFormat,new SharpDX.RectangleF(BarPositionX,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
									RenderTarget.DrawText(str2,Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),Brushes.Red.ToDxBrush(RenderTarget));
									RenderTarget.DrawText("+",Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8+str2.Length*8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
									RenderTarget.DrawText(str3,Claster_textFormat,new SharpDX.RectangleF(BarPositionX+str1.Length*8+str2.Length*8+8,clasterPositionY - (int)(indicatorModel.Claster_Height/2),indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),Brushes.Green.ToDxBrush(RenderTarget));
								}

							}
							else if(indicatorModel.Claster_Width_Max>=clasterVolume.ToString().Length*8 && indicatorModel.Claster_Height>=10)
								RenderTarget.DrawText(clasterVolume.ToString(),Claster_textFormat,new SharpDX.RectangleF(BarPositionX, clasterPositionY - (int)(indicatorModel.Claster_Height/2), indicatorModel.Claster_Width_Max, indicatorModel.Claster_Height),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
						}
					}
				}
					
				
				if(Input_VerticalVolume_OnOff)
				{
							
					int tmp_maxVolume = MaxVV;//bars.Max(b => b.Volume_sum);
					
					int vol=bar.Volume_sum*Input_VerticalVolume_Size/tmp_maxVolume-1;
					
					int Y_VerticalVolume=chartScale.GetYByValue(chartScale.MinValue)-vol;
					int tmp_width = indicatorModel.Claster_Width_Max-2;
					if(tmp_width<1)tmp_width=1;
					
					if(bar.Volume_sum>=Input_VerticalVolume_Filter1_Value){
						linearGradientBrush_VerticalVolume_Filter1.StartPoint=new SharpDX.Vector2(0, Y_VerticalVolume);
						linearGradientBrush_VerticalVolume_Filter1.EndPoint=new SharpDX.Vector2(0, Y_VerticalVolume+vol);
						
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX,Y_VerticalVolume,tmp_width,vol),linearGradientBrush_VerticalVolume_Filter1);
					}else{
						linearGradientBrush_VerticalVolume_Standart.StartPoint=new SharpDX.Vector2(0, Y_VerticalVolume);
						linearGradientBrush_VerticalVolume_Standart.EndPoint=new SharpDX.Vector2(0, Y_VerticalVolume+vol);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(BarPositionX,Y_VerticalVolume,tmp_width,vol),linearGradientBrush_VerticalVolume_Standart);
					}
					
					if(indicatorModel.Claster_Width_Max>=bar.Volume_sum.ToString().Length*8 && Input_VerticalVolumeText_OnOff)
						RenderTarget.DrawText(bar.Volume_sum.ToString(),Claster_textFormat,new SharpDX.RectangleF(BarPositionX,Y_VerticalVolume-15,indicatorModel.Claster_Width_Max,indicatorModel.Claster_Height),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
					
				}
				
				if(Input_PocOnDay_OnOff)
				{
					if(i<indicatorModel.ListOfBar.Count-1)
					{
						SharpDX.Vector2 point0 = new SharpDX.Vector2();
						SharpDX.Vector2 point1 = new SharpDX.Vector2();
						point0.X=BarPositionX;
						point0.Y=chartScale.GetYByValue(bar.DayPocPrice);
						point1.X=BarPositionX+indicatorModel.Claster_Width_Max+2;
						point1.Y=chartScale.GetYByValue(indicatorModel.ListOfBar[i+1].DayPocPrice);
						RenderTarget.DrawLine(point0,point1,Input_PocOnDay_Color.ToDxBrush(RenderTarget));
					}else if(flagForCurrentBar) {
						SharpDX.Vector2 point0 = new SharpDX.Vector2();
						SharpDX.Vector2 point1 = new SharpDX.Vector2();
						point0.X=BarPositionX-indicatorModel.Claster_Width_Max-2;;
						point0.Y=chartScale.GetYByValue(indicatorModel.ListOfBar[i].DayPocPrice);
						point1.X=BarPositionX;
						point1.Y=chartScale.GetYByValue(bar.DayPocPrice);
						RenderTarget.DrawLine(point0,point1,Input_PocOnDay_Color.ToDxBrush(RenderTarget));
					}
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
				foreach(Model.MarketStop MS in marketStops)
				{
					int MS_positionX = chartControl.GetXByBarIndex(ChartBars,(int)chartControl.GetSlotIndexByTime(MS.Time));
					int clasterPositionY1 = chartScale.GetYByValue(MS.Price_high)- (int)(indicatorModel.Claster_Height/2);
					int clasterPositionY2 = chartScale.GetYByValue(MS.Price_low)+ (int)(indicatorModel.Claster_Height/2);
					int MS_height = clasterPositionY2-clasterPositionY1;
					SharpDX.Direct2D1.Brush MS_brush = Input_MS_Color.ToDxBrush(RenderTarget);
					MS_brush.Opacity=(float)0.5;
					RenderTarget.FillRectangle(new SharpDX.RectangleF(MS_positionX,clasterPositionY1,indicatorModel.Claster_Width_Max*2,MS_height),MS_brush);
					
					RenderTarget.DrawText(MS.Volume.ToString(),Claster_textFormat,new SharpDX.RectangleF(MS_positionX+indicatorModel.Claster_Width_Max,clasterPositionY1+MS_height/2,MS.Volume.ToString().Length*8,10),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
					
				}
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
							RenderTarget.DrawText(histogramm_claster.Value.Volume_sum.ToString(),Claster_textFormat,new SharpDX.RectangleF(0,Y_histogramm,histogramm_claster.Value.Volume_sum.ToString().Length*8,indicatorModel.Claster_Height),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
					
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
				
				SharpDX.Direct2D1.Brush TandSText_Color_Brush_Ask = Input_TandS_Ask_Color.ToDxBrush(RenderTarget);
				SharpDX.Direct2D1.Brush TandSText_Color_Brush_Bid = Input_TandS_Bid_Color.ToDxBrush(RenderTarget);
				
				SharpDX.Direct2D1.Brush TandSText_Color_Brush_FilterAsk = Input_TandS_FilterAsk_Color.ToDxBrush(RenderTarget);
				SharpDX.Direct2D1.Brush TandSText_Color_Brush_FilterBid = Input_TandS_FilterBid_Color.ToDxBrush(RenderTarget);
				
				int i_tmp=0;
				if(Input_OnlyFilterShow)
				{
					for(int i=indicatorModel.TandS_FilterPrints_price.Count-1;(i>=indicatorModel.TandS_FilterPrints_price.Count-Input_TandS_CountOrders && i>=0);i--)
					{
						i_tmp++;
						if(indicatorModel.TandS_FilterPrints_volume[i]>0){
							string str_tmp_ask=indicatorModel.TandS_FilterPrints_volume[i]+"@"+indicatorModel.TandS_FilterPrints_price[i];
							RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_FilterAsk);
						}
						else
						{
							string str_tmp_bid=(indicatorModel.TandS_FilterPrints_volume[i]*(-1)).ToString()+"@"+indicatorModel.TandS_FilterPrints_price[i];
							RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_FilterBid);
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
								RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_FilterAsk);
							else
								//RenderTarget.FillRectangle(new RectangleF(ChartPanel.W-Input_TandS_RightPosition,50+i*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.6),textSize),TandSText_Color_Brush_FilterAsk);
							RenderTarget.DrawText(str_tmp_ask,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_ask.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_Ask);
						}
						else
						{
							string str_tmp_bid=(indicatorModel.TandS_AllPrints_volume[i]*(-1)).ToString()+"@"+indicatorModel.TandS_AllPrints_price[i];
							if(Math.Abs(indicatorModel.TandS_AllPrints_volume[i])>=Input_TandS_FilterBid)
								RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_FilterBid);
							else
								//RenderTarget.FillRectangle(new RectangleF(ChartPanel.W-Input_TandS_RightPosition,50+i*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.6),textSize),TandSText_Color_Brush_FilterBid);
							RenderTarget.DrawText(str_tmp_bid,TandS_textFormat,new SharpDX.RectangleF(ChartPanel.W-Input_TandS_RightPosition,Input_TandS_TopPosition+i_tmp*(textSize),str_tmp_bid.ToString().Length*(float)(textSize*0.8),textSize),TandSText_Color_Brush_Bid);
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
							TandSText_Color_Brush_FilterAsk.Opacity=(float)0.7;
							RenderTarget.FillEllipse(new  SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(BP_X,BP_Y),3,3),TandSText_Color_Brush_FilterAsk);
						}
						else
						{
							TandSText_Color_Brush_FilterBid.Opacity=(float)0.7;
							RenderTarget.FillEllipse(new  SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(BP_X,BP_Y),3,3),TandSText_Color_Brush_FilterBid);
						}
					}
				}
				
				
			}
			
			
			
			
			
			isFirstDraw = false;
			
			/*stopWatch.Stop();
	        // Get the elapsed time as a TimeSpan value.
	        TimeSpan ts = stopWatch.Elapsed;
			Print(ts);*/
			
			
				
			Claster_textFormat.Dispose();
			linearGradientBrush_VerticalVolume_Standart.Dispose();
			linearGradientBrush_VerticalVolume_Filter1.Dispose();
		
		}
		
		
		
		#region Properties
		
			#region Claster
		
				[Gui.PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Any Files | *.txt")]
				[Display(ResourceType = typeof(Custom.Resource), Name = "Key File", Order = 1, GroupName = "Cluster")]
				[Browsable(true)]
				public string AnyFile0
				{
					get; set;
				}
		
		
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
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Inside Color", Order = 1, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Inside_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Inside_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Inside_Color); }
				set { Input_ProfileRange_Inside_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "POC Color", Order = 2, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_POC_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_POC_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_POC_Color); }
				set { Input_ProfileRange_POC_Color = Serialize.StringToBrush(value); }
			}
			
			[XmlIgnore]
			[Display(ResourceType = typeof(Custom.Resource), Name = "Border Color", Order = 3, GroupName = "Range Profile")]
			public Brush Input_ProfileRange_Border_Color
			{ get; set; }
			[Browsable(false)]
			public string Input_ProfileRange_Border_ColorSerialize
			{
				get { return Serialize.BrushToString(Input_ProfileRange_Border_Color); }
				set { Input_ProfileRange_Border_Color = Serialize.StringToBrush(value); }
			}
			
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
		public MRPack.MRIndicator MRIndicator(int textSize, int textOpacity)
		{
			return MRIndicator(Input, textSize, textOpacity);
		}

		public MRPack.MRIndicator MRIndicator(ISeries<double> input, int textSize, int textOpacity)
		{
			if (cacheMRIndicator != null)
				for (int idx = 0; idx < cacheMRIndicator.Length; idx++)
					if (cacheMRIndicator[idx] != null && cacheMRIndicator[idx].TextSize == textSize && cacheMRIndicator[idx].TextOpacity == textOpacity && cacheMRIndicator[idx].EqualsInput(input))
						return cacheMRIndicator[idx];
			return CacheIndicator<MRPack.MRIndicator>(new MRPack.MRIndicator(){ TextSize = textSize, TextOpacity = textOpacity }, input, ref cacheMRIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MRPack.MRIndicator MRIndicator(int textSize, int textOpacity)
		{
			return indicator.MRIndicator(Input, textSize, textOpacity);
		}

		public Indicators.MRPack.MRIndicator MRIndicator(ISeries<double> input , int textSize, int textOpacity)
		{
			return indicator.MRIndicator(input, textSize, textOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MRPack.MRIndicator MRIndicator(int textSize, int textOpacity)
		{
			return indicator.MRIndicator(Input, textSize, textOpacity);
		}

		public Indicators.MRPack.MRIndicator MRIndicator(ISeries<double> input , int textSize, int textOpacity)
		{
			return indicator.MRIndicator(input, textSize, textOpacity);
		}
	}
}

#endregion
