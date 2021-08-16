
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

using System.ComponentModel;

using NinjaTrader.NinjaScript.Indicators.MRPack;
#endregion

//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	//[CLSCompliant(false)]
	
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CustomEllipse : DrawingTool
	{
        private	const double	cursorSensitivity		= 15;
		private ChartAnchor		editingAnchor;

		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor}; } }
		
		public int Radius;
		
		public Model model;
		
		public Model.TickAggregatorElement TickAggregatorData;
		
		SharpDX.Direct2D1.Brush brush0DX;
		SharpDX.Direct2D1.Brush brush1DX;
		SharpDX.Direct2D1.Brush brush2DX;
		SharpDX.Direct2D1.Brush brush3DX;
		SharpDX.Direct2D1.Brush brush4DX;
		SharpDX.Direct2D1.Brush brush5DX;
		SharpDX.Direct2D1.Brush brush6DX;
		SharpDX.Direct2D1.Brush brush7DX;
		
		//public bool tempInput;

		/*[Display(Order = 2)]
		public ChartAnchor EndAnchor	{ get; set; }*/

		/*[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolCustomLine", GroupName = "NinjaScriptLines", Order = 2)]
		public Stroke LineStroke { get; set; }*/

		[Display(Order = 1)]
		public ChartAnchor StartAnchor	{ get; set; }

		public override bool SupportsAlerts { get { return false; } }

		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			yield return new AlertConditionItem 
			{
				Name                    = "CECustomLine",
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
					
					int Position_Y1 = chartScale.GetYByValue(TickAggregatorData.TopPrice)-model.Claster_Height/2;
					int Position_Y2 = chartScale.GetYByValue(TickAggregatorData.LowPrice);
					int Delta_Y = Position_Y2-Position_Y1+model.Claster_Height/2;
					if(Delta_Y<1)Delta_Y = model.Claster_Height;
					
					
					int max = model.parent.Input_TickAggregator_Distance;
					
					int Position_X1 = chartControl.GetXByTime(TickAggregatorData.Time);
					int Delta_X = TickAggregatorData.Volume * max / model.MaxTickAggregatorVolume;
					
					

					//Point	endPoint		= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);
					Point	TopLeft		= new Point(Position_X1,Position_Y1);
					Point	TopRight	= new Point(Position_X1+Delta_X,Position_Y1);
					Point	BotRight	= new Point(Position_X1+Delta_X,Position_Y1+Delta_Y);
					Point	BotLeft		= new Point(Position_X1,Position_Y1+Delta_Y);
					//Point	maxPoint		= endPoint;
					
					Vector	totalVector	= startPoint - point;
					
					if(model.parent.Input_TickAggregator_Standart)
					{
						return Math.Abs(totalVector.Length)<=Radius+cursorSensitivity?
								IsLocked ? Cursors.Arrow : Cursors.SizeAll : null;
					}
					else
					{
						return 	MathHelper.IsPointInsideTriangle(point,TopLeft,TopRight,BotLeft)||
								MathHelper.IsPointInsideTriangle(point,BotLeft,TopRight,BotRight)?
								IsLocked ? Cursors.Arrow : Cursors.SizeAll : null;
					}
					
			}
		}
		
		public sealed override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel	chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
			Point		startPoint	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			//Point		endPoint	= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);

			int			totalWidth	= chartPanel.W + chartPanel.X;
			int			totalHeight	= chartPanel.Y + chartPanel.H;

			//Vector strokeAdj = new Vector(Stroke.Width / 2, Stroke.Width / 2);
			//Point midPoint = startPoint + ((endPoint - startPoint) / 2);
			return new[]{ startPoint};
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
			
			if(!model.parent.Input_TickAggregator_OnOff)
			{
				return false;
				
			}
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
			IsLocked=true;
			switch (DrawingState)
			{
				case DrawingState.Building:
					if (StartAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(StartAnchor);
                        StartAnchor.IsEditing = false;

						// give end anchor something to start with so we dont try to render it with bad values right away
						//dataPoint.CopyDataValues(EndAnchor);
					}
					
					// is initial building done (both anchors set)
					if (!StartAnchor.IsEditing)
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
							IsSelected = false;
					}
					break;
			}
		}

		/*public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if (IsLocked && DrawingState != DrawingState.Building)
				return;

			if (DrawingState == DrawingState.Building)
			{
				// start anchor will not be editing here because we start building as soon as user clicks, which
				// plops down a start anchor right away
				
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
		}*/

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
		try
		{
			if (!model.parent.Input_TickAggregator_OnOff)
			{
				Dispose();
				return;
			}
			if (model != null)
			{
           	
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

			ChartPanel	panel			= chartControl.ChartPanels[chartScale.PanelIndex];
			
			Point		startPoint		= StartAnchor.GetPoint(chartControl, panel, chartScale);

			// align to full pixel to avoid unneeded aliasing
			double		strokePixAdj	=	((double)(1 % 2)).ApproxCompare(0) == 0 ? 0.5d : 0d;
			Vector		pixelAdjustVec	= new Vector(strokePixAdj, strokePixAdj);

			//Point					endPoint			= EndAnchor.GetPoint(chartControl, panel, chartScale);

			// convert our start / end pixel points to directx 2d vectors
			Point					startPointAdjusted	= startPoint + pixelAdjustVec;
			
			SharpDX.Vector2			startVec			= startPointAdjusted.ToVector2();
			
            // if a plain ol' line, then we're all done
            // if we're an arrow line, make sure to draw the actual line. for extended lines, only a single
            // line to extended points is drawn below, to avoid unneeded multiple DrawLine calls
           // RenderTarget.DrawLine(startVec, endVec, tmpBrush, LineStroke.Width, LineStroke.StrokeStyle);
			brush0DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.WhiteSmoke);
			brush1DX = model.parent.Input_TickAggregator_AskColor.ToDxBrush(RenderTarget);
			brush1DX.Opacity = (float)0.3;
			brush2DX = model.parent.Input_TickAggregator_BidColor.ToDxBrush(RenderTarget);
			brush2DX.Opacity = (float)0.3;
			brush3DX = model.parent.Input_TickAggregator_AskColor.ToDxBrush(RenderTarget);
			brush4DX = model.parent.Input_TickAggregator_BidColor.ToDxBrush(RenderTarget);
			brush5DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Black);
			brush5DX.Opacity = (float)0.01;
			brush6DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.White);
			brush6DX.Opacity = (float)0.3;
			brush7DX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.White);
			
			SharpDX.Direct2D1.Ellipse el = new SharpDX.Direct2D1.Ellipse(startVec,Radius,Radius);
			
			int Position_Y1 = chartScale.GetYByValue(TickAggregatorData.TopPrice)-model.Claster_Height/2;
			int Position_Y2 = chartScale.GetYByValue(TickAggregatorData.LowPrice);
			int Delta_Y = Position_Y2-Position_Y1+model.Claster_Height/2;
			if(Delta_Y<1)Delta_Y = model.Claster_Height;
			
			
			int max = model.parent.Input_TickAggregator_Distance;
			
			int Position_X1 = chartControl.GetXByTime(TickAggregatorData.Time);
			int Delta_X = TickAggregatorData.Volume * max / model.MaxTickAggregatorVolume;
			int Delta_X_Ask = TickAggregatorData.Volume_Ask * Delta_X /100;
			int Delta_X_Bid = Delta_X - Delta_X_Ask;
			
			
			
			if(IsSelected)
			{
				SharpDX.DirectWrite.TextFormat Claster_textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
				string str = "Volume: " + TickAggregatorData.Volume.ToString()+" =";//+" - "+TickAggregatorData.Volume_Ask.ToString()+" - "+TickAggregatorData.Volume_Bid.ToString();
				//string str ="";
				bool tmpFirst=true;
				foreach(Model.Print p in TickAggregatorData.PrintList)
				{
					if(p.Volume>=model.parent.Input_TickAggregator_TickShow)
					{
						if(tmpFirst)
						{
							str+=" "+p.Volume;
							tmpFirst= false;
						}
						else
							str+=" + "+p.Volume;
					}
					else
						break;
				}
				string str1 = "Time: "+TickAggregatorData.Time.ToLongTimeString()+" MaxPrice: "+TickAggregatorData.TopPrice.ToString()+" MinPrice: "+TickAggregatorData.LowPrice.ToString()+" Ask: "+TickAggregatorData.Volume_Ask.ToString()+"% Bid:"+TickAggregatorData.Volume_Bid.ToString()+"% Delta: "+TickAggregatorData.Volume_Delta.ToString()+"%";
				
				
				
				if(model.parent.Input_TickAggregator_Standart)
				{
					RenderTarget.DrawText(str1,Claster_textFormat,new SharpDX.RectangleF(startVec.X+5, startVec.Y-Radius-23,str1.Length*8 ,10),brush0DX);
					RenderTarget.DrawText(str,Claster_textFormat,new SharpDX.RectangleF(startVec.X+5, startVec.Y-Radius-12,str.Length*8 ,10),brush0DX);
				}
				else
				{
					RenderTarget.DrawText(str1,Claster_textFormat,new SharpDX.RectangleF(startVec.X+5, Position_Y1-23,str1.Length*8 ,10),brush0DX);
					RenderTarget.DrawText(str,Claster_textFormat,new SharpDX.RectangleF(startVec.X+5, Position_Y1-12,str.Length*8 ,10),brush0DX);
				}
				Claster_textFormat.Dispose();
			}
			
			if(model.parent.Input_TickAggregator_Standart)
			{
				if(TickAggregatorData.Volume_Ask>TickAggregatorData.Volume_Bid)
				{
					RenderTarget.DrawEllipse(el,brush3DX);
					if(IsSelected)
					{
						RenderTarget.FillEllipse(el, brush1DX);
					}
				}
				else if(TickAggregatorData.Volume_Ask<TickAggregatorData.Volume_Bid)
				{
					RenderTarget.DrawEllipse(el,brush4DX);
					if(IsSelected)
					{
						RenderTarget.FillEllipse(el, brush2DX);
					}
				}
				else if(TickAggregatorData.Volume_Ask==TickAggregatorData.Volume_Bid)
				{
					RenderTarget.DrawEllipse(el,brush7DX);
					if(IsSelected)
					{
						RenderTarget.FillEllipse(el, brush6DX);
					}
				}
				
				if(!IsSelected)
				{
					RenderTarget.FillEllipse(el, brush5DX);
				}
			}
			else
			{
				RenderTarget.FillRectangle(new SharpDX.RectangleF(Position_X1, Position_Y1 , Delta_X_Ask,Delta_Y),brush1DX );
				RenderTarget.FillRectangle(new SharpDX.RectangleF(Position_X1+Delta_X_Ask, Position_Y1 , Delta_X_Bid,Delta_Y), brush2DX);
			}
				RenderTarget.AntialiasMode = oldAntialiasMode;
				if (brush0DX != null)		{	brush0DX.Dispose();	}
				if (brush1DX != null)		{	brush1DX.Dispose();	}
				if (brush2DX != null)		{	brush2DX.Dispose();	}
				if (brush3DX != null)		{	brush3DX.Dispose();	}
				if (brush4DX != null)		{	brush4DX.Dispose();	}
				if (brush5DX != null)		{	brush5DX.Dispose();	}
				if (brush6DX != null)		{	brush6DX.Dispose();	}
				if (brush7DX != null)		{	brush7DX.Dispose();	}
			}
		}
			catch (Exception ex) { Print("MR CustomEllipse 417: " + ex); }
			return;
		}
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				//LineStroke		            = new Stroke(Brushes.DarkGray, DashStyleHelper.Solid, 2f);
				Description				    = "";
                DrawingState			    = DrawingState.Building;
				Name					    = "CECustomLine";

				StartAnchor = new ChartAnchor
				{
                    IsBrowsable     = true,
                    IsEditing	    = true,
					DrawingTool	    = this,
					DisplayName     = Custom.Resource.NinjaScriptDrawingToolAnchorStart,
				};
				
				/*EndAnchor = new ChartAnchor
				{
                    IsBrowsable     = true,
					IsEditing	    = true,
					DrawingTool	    = this,
					DisplayName     = Custom.Resource.NinjaScriptDrawingToolAnchorEnd,
				};*/
			
			}
			
			else if (State == State.Terminated)
            {
				Dispose();
            }
		}
	}
	
	public static partial class Draw
	{
		private static T DrawCustomEllipseTypeCore<T>(NinjaScriptBase owner, bool isAutoScale, string tag,
										int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
										Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName, Model model) where T : CustomEllipse
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
			//endAnchor.CopyDataValues(lineT.EndAnchor);

            /*if (brush != null)
				lineT.LineStroke = new Stroke(brush, dashStyle, width);*/
			
			lineT.SetState(State.Active);
			return lineT;
		}

		// line overloads
		private static CustomEllipse CustomEllipse(NinjaScriptBase owner, bool isAutoScale, string tag,
								int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
								Brush brush, DashStyleHelper dashStyle, int width, Model model)
		{
			return DrawCustomEllipseTypeCore<CustomEllipse>(owner, isAutoScale, tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY, brush, dashStyle, width, false, null, model);
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
		public static CustomEllipse CustomEllipse(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return CustomEllipse(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, DashStyleHelper.Solid, 1, null);
		}
		
		public static CustomEllipse CustomEllipse(NinjaScriptBase owner, string tag,  DateTime startTime, double startY, Model model)
		{
			return CustomEllipse(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, new DateTime(), 0, null, DashStyleHelper.Solid, 1, model);
		}
	}
}


































































































































