
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
	public class RangeProfile : DrawingTool
	{
        private	const double	cursorSensitivity		= 5;
		private ChartAnchor		editingAnchor;
		
		public Model model;

		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor, EndAnchor }; } }

		[Display(Order = 2)]
		public ChartAnchor EndAnchor	{ get; set; }

		/*[Display(ResourceType = typeof(Custom.Resource), Name = "RangeProfile", GroupName = "RangeProfile", Order = 2)]
		public Stroke LineStroke { get; set; }*/

		[Display(Order = 1)]
		public ChartAnchor StartAnchor	{ get; set; }

		public override bool SupportsAlerts { get { return false; } }

		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			yield return new AlertConditionItem 
			{
				Name                    = "RangeProfile",
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
					
					
					
					
					//Vector	totalVector	= maxPoint - minPoint;
					Vector	DelVector1	= maxPoint - new Point(minPoint.X,maxPoint.Y);
					Vector	DelVector2	= maxPoint - new Point(maxPoint.X,minPoint.Y);
					Vector	DelVector3	= minPoint - new Point(maxPoint.X,minPoint.Y);
					Vector	DelVector4	= minPoint - new Point(minPoint.X,maxPoint.Y);
					return  /*MathHelper.IsPointAlongVector(point, minPoint, totalVector, cursorSensitivity)||*/
							MathHelper.IsPointAlongVector(point, minPoint, DelVector1, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, minPoint, DelVector2, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, maxPoint, DelVector3, cursorSensitivity)||
							MathHelper.IsPointAlongVector(point, maxPoint, DelVector4, cursorSensitivity)?
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
			//Point midPoint = new Point((endPoint - startPoint).x,startPoint.Y);
			return new[]{ startPoint, endPoint };
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

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
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
						else
						// user whiffed.
							IsSelected = false;
					}
					break;
			}
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
				DrawingState = DrawingState.Normal;
			if (editingAnchor != null)
				editingAnchor.IsEditing = false;
			editingAnchor = null;
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			

			// first of all, turn on anti-aliasing to smooth out our line
			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

			ChartPanel	panel			= chartControl.ChartPanels[chartScale.PanelIndex];
			
			Point		startPoint		= StartAnchor.GetPoint(chartControl, panel, chartScale);

			
			Point					endPoint			= EndAnchor.GetPoint(chartControl, panel, chartScale);

			// convert our start / end pixel points to directx 2d vectors
			Point					startPointAdjusted	= startPoint /*+ pixelAdjustVec*/;
			Point					endPointAdjusted	= endPoint /*+ pixelAdjustVec*/;
			SharpDX.Vector2			startVec			= startPointAdjusted.ToVector2();
			SharpDX.Vector2			endVec				= endPointAdjusted.ToVector2();
			//SharpDX.Direct2D1.Brush	tmpBrush			= IsInHitTest ? chartControl.SelectionBrush : LineStroke.BrushDX;

			SharpDX.Vector2 tmpVect = startVec- endVec;
			
			SharpDX.Direct2D1.Brush brush = Brushes.Red.ToDxBrush(RenderTarget);
			brush.Opacity = IsSelected ? (float)0.3 : 0;
			
			RenderTarget.DrawRectangle(new SharpDX.RectangleF(endVec.X, endVec.Y, tmpVect.X, tmpVect.Y),model.Input_ProfileRange_Border_Color.ToDxBrush(RenderTarget),(float)1);
			//RenderTarget.FillRectangle(new SharpDX.RectangleF(endVec.X, endVec.Y, tmpVect.X, tmpVect.Y),brush);
			
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
		
			
			SharpDX.Direct2D1.Brush profile_Claster_Color = model.Input_ProfileRange_Inside_Color.ToDxBrush(RenderTarget);
			profile_Claster_Color.Opacity = (float)0.5;
			
			int volumeSum=0;
			
			foreach(KeyValuePair<double, Model.CurrentClaster> claster in profile.ListOfCurrentBar)
			{
				int Y_histogramm = chartScale.GetYByValue(claster.Key)-model.Claster_Height/2;
				int vol = claster.Value.Volume_sum*(int)Math.Abs(tmpVect.X)/profile.ListOfCurrentBar[profile.pocPrice].Volume_sum;
				volumeSum+=claster.Value.Volume_sum;
				if(claster.Key==profile.pocPrice)
					RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol,model.Claster_Height),model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget));
				else
					RenderTarget.FillRectangle(new SharpDX.RectangleF(leftPosition,Y_histogramm,vol,model.Claster_Height),profile_Claster_Color);
			}
			
			SharpDX.DirectWrite.TextFormat Claster_textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
			
			
			RenderTarget.DrawText("Σ "+volumeSum.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition, topPosition+Math.Abs(tmpVect.Y)+3, volumeSum.ToString().Length*8+10,10),Brushes.WhiteSmoke.ToDxBrush(RenderTarget));
			
			int text_Y = chartScale.GetYByValue(profile.pocPrice)-model.Claster_Height/2;
			int text_width = profile.pocPrice.ToString().Length*7;
			RenderTarget.DrawText(profile.pocPrice.ToString(),Claster_textFormat,new SharpDX.RectangleF(leftPosition-text_width, text_Y,text_width ,10),model.Input_ProfileRange_POC_Color.ToDxBrush(RenderTarget));
			
			
			if(false)
			{
				RenderTarget.DrawRectangle(new SharpDX.RectangleF(leftPosition, topPosition-20, 15, 15),Brushes.Gray.ToDxBrush(RenderTarget),(float)1);
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
				Name					    = "RangeProfile";

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
		private static T DrawRangeProfileTypeCore<T>(NinjaScriptBase owner, bool isAutoScale, string tag,
										int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
										Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName, Model model) where T : RangeProfile
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

            /*if (brush != null)
				lineT.LineStroke = new Stroke(brush, dashStyle, width);*/
			
			lineT.SetState(State.Active);
			return lineT;
		}

		// line overloads
		private static RangeProfile RangeProfile(NinjaScriptBase owner, bool isAutoScale, string tag,
								int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
								Brush brush, DashStyleHelper dashStyle, int width)
		{
			return DrawRangeProfileTypeCore<RangeProfile>(owner, isAutoScale, tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY, brush, dashStyle, width, false, null, null);
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
		public static RangeProfile RangeProfile(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return RangeProfile(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, DashStyleHelper.Solid, 1);
		}
		
		public static RangeProfile RangeProfile(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush, int width, Model model)
		{
			return DrawRangeProfileTypeCore<RangeProfile>(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, DashStyleHelper.Solid, width, false, null, model);
		}
	}
}
































































































































