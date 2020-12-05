
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Core.FloatingPoint;

using NinjaTrader.NinjaScript.Indicators.MRPack;
#endregion

//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	public class RangeProfile2 : DrawingTool
	{
        private	const double	cursorSensitivity		= 5;
		private ChartAnchor		editingAnchor;
		
		public Model model;
		
		public int ProfileType;
		
		public bool ExtendedLine;

		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor, EndAnchor }; } }

		[Display(Order = 2)]
		public ChartAnchor EndAnchor	{ get; set; }

		/*[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolCustomLine", GroupName = "NinjaScriptLines", Order = 2)]*/
		//public Stroke LineStroke { get; set; }

		[Display(Order = 1)]
		public ChartAnchor StartAnchor	{ get; set; }

		public override bool SupportsAlerts { get { return false; } }

		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			yield return new AlertConditionItem 
			{
				Name                    = "CustomLine",
				ShouldOnlyDisplayName   = true,
			};
		}

		public override Cursor GetCursor(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, Point point)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:	return Cursors.Pen;
				case DrawingState.Moving:	return IsLocked ? Cursors.No : Cursors.SizeAll;
				case DrawingState.Editing:
					if (IsLocked)
						return Cursors.No;

					return editingAnchor == StartAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE;
				default:
					// draw move cursor if cursor is near line path anywhere
					Point startPoint = StartAnchor.GetPoint(chartControl, chartPanel, chartScale);

					ChartAnchor closest = GetClosestAnchor(chartControl, chartPanel, chartScale, cursorSensitivity, point);
					if (closest != null)
					{
						if (IsLocked)
							return Cursors.Arrow;
						return closest == StartAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE;
					}

					Point	endPoint		= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
					Point	minPoint		= startPoint;
					Point	maxPoint		= endPoint;
					
					
					
					/*Vector	totalVector	= maxPoint - minPoint;
					return MathHelper.IsPointAlongVector(point, minPoint, totalVector, cursorSensitivity) ?
						IsLocked ? Cursors.Arrow : Cursors.SizeAll : null;*/
					Vector	DelVector1	= maxPoint - new Point(minPoint.X,maxPoint.Y);
					Vector	DelVector2	= maxPoint - new Point(maxPoint.X,minPoint.Y);
					Vector	DelVector3	= minPoint - new Point(maxPoint.X,minPoint.Y);
					Vector	DelVector4	= minPoint - new Point(minPoint.X,maxPoint.Y);
					return  /*MathHelper.IsPointAlongVector(point, minPoint, totalVector, cursorSensitivity)||*/
							/*MathHelper.IsPointAlongVector(point, minPoint, DelVector1, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, minPoint, DelVector2, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, maxPoint, DelVector3, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, maxPoint, DelVector4, cursorSensitivity)*/
							MathHelper.IsPointInsideTriangle(point,minPoint,maxPoint,new Point(minPoint.X,maxPoint.Y))||
							MathHelper.IsPointInsideTriangle(point,minPoint,maxPoint,new Point(maxPoint.X,minPoint.Y))?
						IsLocked ? Cursors.Arrow : Cursors.SizeAll : null;
			}
		}
		
		public sealed override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel	chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
			Point		startPoint	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point		endPoint	= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);

			int			totalWidth	= chartPanel.W + chartPanel.X;
			int			totalHeight	= chartPanel.Y + chartPanel.H;

			//Vector strokeAdj = new Vector(Stroke.Width / 2, Stroke.Width / 2);
			//Point midPoint = startPoint + ((endPoint - startPoint) / 2);
			return new[]{ startPoint/*, midPoint*/, endPoint };
		}

		public override bool IsAlertConditionTrue(AlertConditionItem conditionItem, Condition condition, ChartAlertValue[] values, ChartControl chartControl, ChartScale chartScale)
		{
			if (values.Length < 1)
				return false;

			ChartPanel chartPanel = chartControl.ChartPanels[PanelIndex];

			// get start / end points of what is absolutely shown for our vector 
			Point lineStartPoint	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point lineEndPoint		= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);

			double minLineX = double.MaxValue;
			double maxLineX = double.MinValue; 
			foreach (Point point in new[]{lineStartPoint, lineEndPoint})
			{
				minLineX = Math.Min(minLineX, point.X);
				maxLineX = Math.Max(maxLineX, point.X);
			}
		
			// first thing, if our smallest x is greater than most recent bar, we have nothing to do yet.
			// do not try to check Y because lines could cross through stuff
			double firstBarX = values[0].ValueType == ChartAlertValueType.StaticValue ? minLineX : chartControl.GetXByTime(values[0].Time);
			double firstBarY = chartScale.GetYByValue(values[0].Value);
			
			// dont have to take extension into account as its already handled in min/max line x

			// bars completely passed our line
			if (maxLineX < firstBarX)
				return false;

			// bars not yet to our line
			if (minLineX > firstBarX)
				return false;

			// NOTE: normalize line points so the leftmost is passed first. Otherwise, our vector
			// math could end up having the line normal vector being backwards if user drew it backwards.
			// but we dont care the order of anchors, we want 'up' to mean 'up'!
			Point leftPoint		= lineStartPoint.X < lineEndPoint.X ? lineStartPoint : lineEndPoint;
			Point rightPoint	= lineEndPoint.X > lineStartPoint.X ? lineEndPoint : lineStartPoint;

			Point barPoint = new Point(firstBarX, firstBarY);
			// NOTE: 'left / right' is relative to if line was vertical. it can end up backwards too
			MathHelper.PointLineLocation pointLocation = MathHelper.GetPointLineLocation(leftPoint, rightPoint, barPoint);
			// for vertical things, think of a vertical line rotated 90 degrees to lay flat, where it's normal vector is 'up'
			switch (condition)
			{
				case Condition.Greater:			return pointLocation == MathHelper.PointLineLocation.LeftOrAbove;
				case Condition.GreaterEqual:	return pointLocation == MathHelper.PointLineLocation.LeftOrAbove || pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.Less:			return pointLocation == MathHelper.PointLineLocation.RightOrBelow;
				case Condition.LessEqual:		return pointLocation == MathHelper.PointLineLocation.RightOrBelow || pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.Equals:			return pointLocation == MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.NotEqual:		return pointLocation != MathHelper.PointLineLocation.DirectlyOnLine;
				case Condition.CrossAbove:
				case Condition.CrossBelow:
					Predicate<ChartAlertValue> predicate = v =>
					{
						double barX = chartControl.GetXByTime(v.Time);
						double barY = chartScale.GetYByValue(v.Value);
						Point stepBarPoint = new Point(barX, barY);
						MathHelper.PointLineLocation ptLocation = MathHelper.GetPointLineLocation(leftPoint, rightPoint, stepBarPoint);
						if (condition == Condition.CrossAbove)
							return ptLocation == MathHelper.PointLineLocation.LeftOrAbove;
						return ptLocation == MathHelper.PointLineLocation.RightOrBelow;
					};
					return MathHelper.DidPredicateCross(values, predicate);
			}
			
			return false;
		}

		public override bool IsVisibleOnChart(ChartControl chartControl, ChartScale chartScale, DateTime firstTimeOnChart, DateTime lastTimeOnChart)
		{
			if (DrawingState == DrawingState.Building)
				return true;

			DateTime	minTime = Core.Globals.MaxDate;
			DateTime	maxTime = Core.Globals.MinDate;

			// check at least one of our anchors is in horizontal time frame
			foreach (ChartAnchor anchor in Anchors)
			{
				if (anchor.Time < minTime)
					minTime = anchor.Time;
				if (anchor.Time > maxTime)
					maxTime = anchor.Time;
			}

			// hline extends, but otherwise try to check if line horizontally crosses through visible chart times in some way
			if(this.ExtendedLine==false)
			if (minTime > lastTimeOnChart || maxTime < firstTimeOnChart)
				return false;

			return true;
		}
		
		public override void OnCalculateMinMax()
		{
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;

			if (!IsVisible)
				return;

			// return min/max values only if something has been actually drawn
			if (Anchors.Any(a => !a.IsEditing))
				foreach (ChartAnchor anchor in Anchors)
				{
					MinValue = Math.Min(anchor.Price, MinValue);
					MaxValue = Math.Max(anchor.Price, MaxValue);
				}
		}
		
		bool IsButtonCLick=false;

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if(IsButtonCLick)
			{
				IsButtonCLick=false;
				return;
			}
			
			
			switch (DrawingState)
			{
				case DrawingState.Building:
					if (StartAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(StartAnchor);
                        StartAnchor.IsEditing = false;

						// give end anchor something to start with so we dont try to render it with bad values right away
						dataPoint.CopyDataValues(EndAnchor);
					}
					else if (EndAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(EndAnchor);
						EndAnchor.IsEditing = false;
					}
					
					// is initial building done (both anchors set)
					if (!StartAnchor.IsEditing && !EndAnchor.IsEditing)
					{
						DrawingState = DrawingState.Normal;
						IsSelected = false;
					}
					break;
				case DrawingState.Normal:
					Point point = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
					// see if they clicked near a point to edit, if so start editing
					editingAnchor = GetClosestAnchor(chartControl, chartPanel, chartScale, cursorSensitivity, point);
					
					

					if (editingAnchor != null)
					{
						editingAnchor.IsEditing = true;
						DrawingState = DrawingState.Editing;
					}
					else
					{
						if (GetCursor(chartControl, chartPanel, chartScale, point) != null)
							DrawingState = DrawingState.Moving;
						else{
						// user whiffed.
							int start_X = ((int)model.parent.ChartControl.GetXByTime(StartAnchor.Time));
							int end_X = ((int)model.parent.ChartControl.GetXByTime(EndAnchor.Time));
							
							int start_Y = chartScale.GetYByValue(StartAnchor.Price);
							int end_Y = chartScale.GetYByValue(EndAnchor.Price);
							
							int leftPosition =0;
							int topPosition =0;
							if(start_X<end_X)
								leftPosition=start_X;
							else
								leftPosition=end_X;
							if(start_Y<end_Y)
								topPosition=start_Y;
							else
								topPosition=end_Y;
							
							
							if(point.X>=leftPosition && point.X<=leftPosition+15 && point.Y>=topPosition-20 && point.Y<=topPosition-5)
							{
								model.parent.RemoveDrawObject(this.Tag);
								model.DeleteProfile(this.Tag);
								model.SaveProfiles();
								IsButtonCLick=true;
								break;
							}
							
							if(point.X>=leftPosition+20 && point.X<=leftPosition+20+15 && point.Y>=topPosition-20 && point.Y<=topPosition-5)
							{
								if(ProfileType==0)
									ProfileType=1;
								else if(ProfileType==1)
									ProfileType=2;
								else if(ProfileType==2)
									ProfileType=0;
								
								model.SaveProfiles();
								IsButtonCLick=true;
								break;
							}
							
							if(point.X>=leftPosition+40 && point.X<=leftPosition+40+15 && point.Y>=topPosition-20 && point.Y<=topPosition-5)
							{
								
								ExtendedLine = !ExtendedLine;
								model.SaveProfiles();
								IsButtonCLick=true;
								break;
							}
							
							
							IsSelected = false;
						}
					}
					break;
			}
			
			/*Point point1 = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
			
			*/
				
					
			
		}
		
		public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			
			
			
			if (IsLocked && DrawingState != DrawingState.Building)
				return;

			if (DrawingState == DrawingState.Building)
			{
				// start anchor will not be editing here because we start building as soon as user clicks, which
				// plops down a start anchor right away
				if (EndAnchor.IsEditing)
					dataPoint.CopyDataValues(EndAnchor);
			}
			else if (DrawingState == DrawingState.Editing && editingAnchor != null)
			{
                // if its a line with two anchors, update both x/y at once
                dataPoint.CopyDataValues(editingAnchor);
			}
			else if (DrawingState == DrawingState.Moving)
				foreach (ChartAnchor anchor in Anchors)
					anchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
			//lastMouseMovePoint.Value, point, chartControl, chartScale);
		}

		public override void OnMouseUp(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			// simply end whatever moving
			if (DrawingState == DrawingState.Moving || DrawingState == DrawingState.Editing)
			{
				DrawingState = DrawingState.Normal;
				model.SaveProfiles();
			}
			if (editingAnchor != null)
				editingAnchor.IsEditing = false;
			editingAnchor = null;
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			/*if (LineStroke == null)
				return;*/

			//LineStroke.RenderTarget			= RenderTarget;

			// first of all, turn on anti-aliasing to smooth out our line
			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

			ChartPanel	panel			= chartControl.ChartPanels[chartScale.PanelIndex];
			
			Point		startPoint		= StartAnchor.GetPoint(chartControl, panel, chartScale);

			// align to full pixel to avoid unneeded aliasing
			double		strokePixAdj	=	((double)(1 % 2)).ApproxCompare(0) == 0 ? 0.5d : 0d;
			Vector		pixelAdjustVec	= new Vector(strokePixAdj, strokePixAdj);

			Point					endPoint			= EndAnchor.GetPoint(chartControl, panel, chartScale);

			// convert our start / end pixel points to directx 2d vectors
			Point					startPointAdjusted	= startPoint + pixelAdjustVec;
			Point					endPointAdjusted	= endPoint + pixelAdjustVec;
			SharpDX.Vector2			startVec			= startPointAdjusted.ToVector2();
			SharpDX.Vector2			endVec				= endPointAdjusted.ToVector2();
			SharpDX.Direct2D1.Brush	tmpBrush			= chartControl.SelectionBrush;

			SharpDX.Vector2 tmpVect = startVec- endVec;
			
            // if a plain ol' line, then we're all done
            // if we're an arrow line, make sure to draw the actual line. for extended lines, only a single
            // line to extended points is drawn below, to avoid unneeded multiple DrawLine calls
            //RenderTarget.DrawLine(startVec, endVec, model.Input_ProfileRange_Inside_Color.ToDxBrush(RenderTarget), 2);
			
			RenderTarget.DrawRectangle(new SharpDX.RectangleF(endVec.X, endVec.Y, tmpVect.X, tmpVect.Y),model.Input_ProfileRange_Border_Color.ToDxBrush(RenderTarget),(float)1);
			SharpDX.Direct2D1.Brush brush1 = Brushes.Black.ToDxBrush(RenderTarget);
			brush1.Opacity = (float)0.01;
			RenderTarget.FillRectangle(new SharpDX.RectangleF(endVec.X, endVec.Y, tmpVect.X, tmpVect.Y),brush1);
			
			int firstindex = ((int)model.parent.ChartControl.GetSlotIndexByTime(StartAnchor.Time));
			int lastIndex = ((int)model.parent.ChartControl.GetSlotIndexByTime(EndAnchor.Time));
			
			
			IEnumerable<Model.Bar> bars;
			if(firstindex<=lastIndex)
			{
				bars = model.GetBarRange(firstindex, lastIndex);
			}
			else
			{
				bars = model.GetBarRange(lastIndex, firstindex);
			}
			
			int leftPosition =0;
			int topPosition =0;
			if(startVec.X<endVec.X)
				leftPosition=(int)startVec.X;
			else
				leftPosition=(int)endVec.X;
			if(startVec.Y<endVec.Y)
				topPosition=(int)startVec.Y;
			else
				topPosition=(int)endVec.Y;
			
			
			Model.HistogrammClass profile = new Model.HistogrammClass();
			
			int count=0;
			
			foreach(Model.Bar bar in bars)
			{
				IEnumerable<KeyValuePair<double, Model.Claster>> clasters;
				if(StartAnchor.Price>=EndAnchor.Price)
					clasters= bar.ListOfClasters.Where(c=>c.Key<=StartAnchor.Price && c.Key>=EndAnchor.Price);
				else
					clasters= bar.ListOfClasters.Where(c=>c.Key>=StartAnchor.Price && c.Key<=EndAnchor.Price);
				
				foreach(KeyValuePair<double, Model.Claster> claster in clasters)
				{
					profile.AddPrintToHistogramm(claster.Key,claster.Value.Volume_Ask_sum,PrintType.ASK);
					profile.AddPrintToHistogramm(claster.Key,claster.Value.Volume_Bid_sum,PrintType.BID);
				}
			}
			//textToRender+=" : "+profile.ListOfCurrentBar.Count.ToString();
			Dictionary<double, int> deltaProfile = new Dictionary<double, int>();
			double maxDeltaPrice = int.MinValue;
			int prevdelta =0;
			int volumeSum=0;
			foreach(KeyValuePair<double, Model.CurrentClaster> claster in profile.ListOfCurrentBar)
			{
				volumeSum+=claster.Value.Volume_sum;
				int delta = Math.Abs(claster.Value.Volume_Bid_sum-claster.Value.Volume_Ask_sum);
				deltaProfile.Add(claster.Key,delta);
				if(prevdelta<=delta)
				{
					prevdelta=delta;
					maxDeltaPrice = claster.Key;
				}
			}
			
		
			
			SharpDX.Direct2D1.Brush profile_Claster_Color = model.Input_ProfileRange_Inside_Color.ToDxBrush(RenderTarget);
			profile_Claster_Color.Opacity = (float)0.5;
			
			SharpDX.DirectWrite.TextFormat Claster_textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			
			
			
			
			
			
			if(ProfileType==1 || ProfileType==0)
			foreach(KeyValuePair<double, Model.CurrentClaster> claster in profile.ListOfCurrentBar)
			{
				int Y_histogramm = chartScale.GetYByValue(claster.Key)-model.Claster_Height/2;
				int vol = claster.Value.Volume_sum*(int)Math.Abs(tmpVect.X)/profile.ListOfCurrentBar[profile.pocPrice].Volume_sum;
				
				if(ProfileType==0)
				{
					if(claster.Key==profile.pocPrice)
					{
						SharpDX.Direct2D1.Brush profile_Claster_ColorPOC = model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget);
						profile_Claster_ColorPOC.Opacity = (float)0.5;
						if(ExtendedLine)
							vol = (int)ChartPanel.MaxWidth-Math.Abs(leftPosition);
						RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol,model.Claster_Height),profile_Claster_ColorPOC);
					}
					else
						RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol,model.Claster_Height),profile_Claster_Color);
					
					int text_Y = chartScale.GetYByValue(profile.pocPrice)-model.Claster_Height/2;
					int text_width = profile.pocPrice.ToString().Length*7;
					RenderTarget.DrawText(profile.pocPrice.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition-text_width, text_Y,text_width ,10),model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget));
			
				}
				if(ProfileType==1)
				{
					SharpDX.Direct2D1.Brush profile_Claster_ColorBID = Brushes.Red.ToDxBrush(RenderTarget);
					profile_Claster_ColorBID.Opacity = (float)0.5;
					SharpDX.Direct2D1.Brush profile_Claster_ColorASK = Brushes.Green.ToDxBrush(RenderTarget);
					profile_Claster_ColorASK.Opacity = (float)0.5;
					
					int vol_bid = claster.Value.Volume_Bid_sum*vol/claster.Value.Volume_sum;
					RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol_bid,model.Claster_Height),profile_Claster_ColorBID);
					RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition+vol_bid,Y_histogramm,vol-vol_bid,model.Claster_Height),profile_Claster_ColorASK);
					
					if(claster.Key==profile.pocPrice && ExtendedLine)
					{
						SharpDX.Direct2D1.Brush profile_Claster_ColorPOC = model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget);
						profile_Claster_ColorPOC.Opacity = (float)0.5;
						RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition+vol,Y_histogramm,(int)ChartPanel.MaxWidth-Math.Abs(leftPosition+vol),model.Claster_Height),profile_Claster_ColorPOC);
					}
					
					
					int text_Y = chartScale.GetYByValue(profile.pocPrice)-model.Claster_Height/2;
					int text_width = profile.pocPrice.ToString().Length*7;
					RenderTarget.DrawText(profile.pocPrice.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition-text_width, text_Y,text_width ,10),model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget));
			
				}
				
			}
			
			if(ProfileType==2)
			{
				
				foreach(KeyValuePair<double, int> claster in deltaProfile)
				{
					int Y_histogramm = chartScale.GetYByValue(claster.Key)-model.Claster_Height/2;
					SharpDX.Direct2D1.Brush profile_Claster_ColorBID = Brushes.Red.ToDxBrush(RenderTarget);
					profile_Claster_ColorBID.Opacity = (float)0.5;
					SharpDX.Direct2D1.Brush profile_Claster_ColorASK = Brushes.Green.ToDxBrush(RenderTarget);
					profile_Claster_ColorASK.Opacity = (float)0.5;
					
					int vol_delta =claster.Value*(int)Math.Abs(tmpVect.X)/(int)prevdelta;
					
					if(profile.ListOfCurrentBar[claster.Key].Volume_Bid_sum==profile.ListOfCurrentBar[claster.Key].Volume_Ask_sum)
						vol_delta=0;
					else if(profile.ListOfCurrentBar[claster.Key].Volume_Bid_sum>profile.ListOfCurrentBar[claster.Key].Volume_Ask_sum)
					{
						RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol_delta,model.Claster_Height),profile_Claster_ColorBID);
					}
					else if(profile.ListOfCurrentBar[claster.Key].Volume_Bid_sum<profile.ListOfCurrentBar[claster.Key].Volume_Ask_sum)
					{
						RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol_delta,model.Claster_Height),profile_Claster_ColorASK);
					}		
				}
				
				int text_Y = chartScale.GetYByValue(maxDeltaPrice)-model.Claster_Height/2;
				int text_width = maxDeltaPrice.ToString().Length*7;
				RenderTarget.DrawText(maxDeltaPrice.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition-text_width, text_Y,text_width ,10),model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget));
			
			}
				
				
				
			
			
			
			
			RenderTarget.DrawText("Σ "+volumeSum.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition, topPosition+Math.Abs(tmpVect.Y)+3, volumeSum.ToString().Length*8+10,10),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
			
			
			
			if(IsSelected)
			{
				
				SharpDX.Vector2 tempVector1 = new Point(leftPosition+2,topPosition+2-20).ToVector2();
				SharpDX.Vector2 tempVector2 = new Point(leftPosition-2+15,topPosition-2-20+15).ToVector2();
				RenderTarget.DrawLine(tempVector1,tempVector2, Brushes.Gray.ToDxBrush(RenderTarget), 2);
				tempVector1.X+=11;
				tempVector2.X-=11;
				RenderTarget.DrawLine(tempVector1,tempVector2, Brushes.Gray.ToDxBrush(RenderTarget), 2);
				RenderTarget.DrawRectangle(new SharpDX.RectangleF(leftPosition, topPosition-20, 15, 15),Brushes.Gray.ToDxBrush(RenderTarget),(float)1);
				
				RenderTarget.DrawRectangle(new SharpDX.RectangleF(leftPosition+20, topPosition-20, 15, 15),Brushes.Gray.ToDxBrush(RenderTarget),(float)1);
				string str = "";
				switch(ProfileType)
				{
					case 0: str="V";break;
					case 1: str="P";break;
					case 2: str="D";break;
				}
				
				SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();
				SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(fontFactory, "Segoe UI", 15);
				RenderTarget.DrawText(str,textFormat,new SharpDX.RectangleF(leftPosition+23, topPosition-23, 15, 15),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
				
				
				if(ExtendedLine)
				{
					SharpDX.Direct2D1.Brush brush = Brushes.Gray.ToDxBrush(RenderTarget);
					brush.Opacity = (float)0.5;
					RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition+42, topPosition-18, 11, 11),brush);
				}
				RenderTarget.DrawRectangle(new SharpDX.RectangleF(leftPosition+40, topPosition-20, 15, 15),Brushes.Gray.ToDxBrush(RenderTarget),(float)1);
			}
			
			
			
                return;
		}
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				//LineStroke		            = new Stroke(Brushes.DarkGray, DashStyleHelper.Solid, 2f);
				Description				    = "";
                DrawingState			    = DrawingState.Building;
				Name					    = "CustomLine";

				StartAnchor = new ChartAnchor
				{
                    IsBrowsable     = true,
                    IsEditing	    = true,
					DrawingTool	    = this,
					DisplayName     = Custom.Resource.NinjaScriptDrawingToolAnchorStart,
				};

				EndAnchor = new ChartAnchor
				{
                    IsBrowsable     = true,
					IsEditing	    = true,
					DrawingTool	    = this,
					DisplayName     = Custom.Resource.NinjaScriptDrawingToolAnchorEnd,
				};
				
			}
			else if (State == State.Terminated)
            {
				Dispose();
            }
		}
	}

	public static partial class Draw
	{
		private static T DrawRangeProfile2TypeCore<T>(NinjaScriptBase owner, bool isAutoScale, string tag,
										int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
										Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName, Model model) where T : RangeProfile2
		{
			if (owner == null)
				throw new ArgumentException("owner");

			if (string.IsNullOrWhiteSpace(tag))
				throw new ArgumentException(@"tag cant be null or empty", "tag");

			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = string.Format("{0}{1}", GlobalDrawingToolManager.GlobalDrawingToolTagPrefix, tag);

			T lineT = DrawingTool.GetByTagOrNew(owner, typeof(T), tag, templateName) as T;
			lineT.model = model;
			
			if (lineT == null)
				return null;

            if (startTime == Core.Globals.MinDate && endTime == Core.Globals.MinDate && startBarsAgo == int.MinValue && endBarsAgo == int.MinValue)
				throw new ArgumentException("bad start/end date/time");

			DrawingTool.SetDrawingToolCommonValues(lineT, tag, isAutoScale, owner, isGlobal);

			// dont nuke existing anchor refs on the instance
			ChartAnchor startAnchor;

			startAnchor				= DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
			ChartAnchor endAnchor	= DrawingTool.CreateChartAnchor(owner, endBarsAgo, endTime, endY);
			startAnchor.CopyDataValues(lineT.StartAnchor);
			endAnchor.CopyDataValues(lineT.EndAnchor);

           /* if (brush != null)
				lineT.LineStroke = new Stroke(brush, dashStyle, width);*/
			
			lineT.SetState(State.Active);
			return lineT;
		}

		// line overloads
		private static RangeProfile2 RangeProfile2(NinjaScriptBase owner, bool isAutoScale, string tag,
								int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
								Brush brush, DashStyleHelper dashStyle, int width, Model model)
		{
			return DrawRangeProfile2TypeCore<RangeProfile2>(owner, isAutoScale, tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY, brush, dashStyle, width, false, null, model);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis co-ordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value co-ordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis co-ordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value co-ordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static RangeProfile2 RangeProfile2(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return RangeProfile2(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, DashStyleHelper.Solid, 1, null);
		}
		
		
		public static RangeProfile2 RangeProfile2(NinjaScriptBase owner, string tag,  DateTime startTime, double startY, DateTime endTime, double endY, Brush brush, Model model)
		{
			return RangeProfile2(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, DashStyleHelper.Solid, 1, model);
		}
		
	/*	public static CustomLine CustomLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush, int width)
		{
			return DrawCustomLineTypeCore<CustomLine>(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, DashStyleHelper.Solid, width, false, null);
		}*/
	}
}










































































