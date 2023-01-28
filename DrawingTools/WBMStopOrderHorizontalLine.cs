//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;

#endregion

namespace NinjaTrader.NinjaScript.DrawingTools
{


	/// <summary>
	/// Represents an interface that exposes information regarding a HorizontalLineCustom IDrawingTool.
	/// </summary>
	public class HorizontalLineCustom : DrawingTool
	{
		// this line class takes care of all stock line types, so we use this to keep track
		// of what kind of line instances this is. Localization is not needed because it's not visible on ui
		protected enum ChartLineType2
		{
			HorizontalLineCustom,
		}

		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor, EndAnchor }; } }
		[Display(Order = 2)]
		public ChartAnchor	EndAnchor		{ get; set; }
		[Display(Order = 1)]
		public ChartAnchor StartAnchor		{ get; set; }

		public override object Icon			{ get { return Gui.Tools.Icons.DrawLineTool; } }

		[CLSCompliant(false)]
		protected		SharpDX.Direct2D1.PathGeometry		ArrowPathGeometry;
		private	const	double								cursorSensitivity		= 15;
		private			ChartAnchor							editingAnchor;

		[Browsable(false)]
		[XmlIgnore]
		protected ChartLineType2 HorizontalLineCustomType { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptGeneral", Name = "NinjaScriptDrawingToolLine", Order = 99)]
		public Stroke Stroke { get; set; }

		public override bool SupportsAlerts { get { return true; } }

		public override void OnCalculateMinMax()
		{
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;

			if (!IsVisible)
				return;

			// make sure to set good min/max values on single click lines as well, in case anchor left in editing
			if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
				MinValue = MaxValue = Anchors.First().Price;
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
					if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
						return Cursors.SizeAll;
					return editingAnchor == StartAnchor ? Cursors.SizeNESW : Cursors.SizeNWSE;
				default:
					// draw move cursor if cursor is near line path anywhere
					Point startPoint = StartAnchor.GetPoint(chartControl, chartPanel, chartScale);

					if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
					{
						// just go by single axis since we know the entire lines position
						if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom && Math.Abs(point.Y - startPoint.Y) <= cursorSensitivity)
							return IsLocked ? Cursors.Arrow : Cursors.SizeAll;
						return null;
					}

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

					Vector	totalVector	= maxPoint - minPoint;
					return MathHelper.IsPointAlongVector(point, minPoint, totalVector, cursorSensitivity) ?
						IsLocked ? Cursors.Arrow : Cursors.SizeAll : null;
			}
		}

		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			yield return new AlertConditionItem
			{
				Name = "WBMStopOrderHorizontalLine",
				ShouldOnlyDisplayName = true
			};
		}

		public sealed override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel	chartPanel	= chartControl.ChartPanels[chartScale.PanelIndex];
			Point		startPoint	= StartAnchor.GetPoint(chartControl, chartPanel, chartScale);
			Point		endPoint	= EndAnchor.GetPoint(chartControl, chartPanel, chartScale);

			int			totalWidth	= chartPanel.W + chartPanel.X;
			int			totalHeight	= chartPanel.Y + chartPanel.H;

			if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
				return new[] { new Point(chartPanel.X, startPoint.Y), new Point(totalWidth / 2d, startPoint.Y), new Point(totalWidth, startPoint.Y) };

			//Vector strokeAdj = new Vector(Stroke.Width / 2, Stroke.Width / 2);
			Point midPoint = startPoint + ((endPoint - startPoint) / 2);
			return new[]{ startPoint, midPoint, endPoint };
		}

		public override bool IsAlertConditionTrue(AlertConditionItem conditionItem, Condition condition, ChartAlertValue[] values, ChartControl chartControl, ChartScale chartScale)
		{
			if (values.Length < 1)
				return false;
			ChartPanel chartPanel = chartControl.ChartPanels[PanelIndex];
			// h line and v line have much more simple alert handling
			if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
			{
				double barVal	= values[0].Value;
				double lineVal	= conditionItem.Offset.Calculate(StartAnchor.Price, AttachedTo.Instrument);

				switch (condition)
				{
					case Condition.Equals:			return barVal.ApproxCompare(lineVal) == 0;
					case Condition.NotEqual:		return barVal.ApproxCompare(lineVal) != 0;
					case Condition.Greater:			return barVal > lineVal;
					case Condition.GreaterEqual:	return barVal >= lineVal;
					case Condition.Less:			return barVal < lineVal;
					case Condition.LessEqual:		return barVal <= lineVal;
					case Condition.CrossAbove:
					case Condition.CrossBelow:
						Predicate<ChartAlertValue> predicate = v =>
						{
							if (condition == Condition.CrossAbove)
								return v.Value > lineVal;
							return v.Value < lineVal;
						};
						return MathHelper.DidPredicateCross(values, predicate);
				}
				return false;
			}

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

			// check offscreen vertically. make sure to check the line doesnt cut through the scale, so check both are out
			if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom && (StartAnchor.Price < chartScale.MinValue || StartAnchor.Price > chartScale.MaxValue) && !IsAutoScale)
				return false; // horizontal line only has one anchor to whiff

			return true;
		}

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:
                
					if (StartAnchor.IsEditing)
					{
                        Print(string.Format("StartAnchor.IsEditing {0}", DateTime.Now.ToLongTimeString()));
						dataPoint.CopyDataValues(StartAnchor);
						StartAnchor.IsEditing = false;

						// these lines only need one anchor, so stop editing end anchor too
						if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
							EndAnchor.IsEditing = false;

						// give end anchor something to start with so we dont try to render it with bad values right away
						dataPoint.CopyDataValues(EndAnchor);
					}
					else if (EndAnchor.IsEditing)
					{
                        Print(string.Format("StartAnchor.IsEditing {0}", DateTime.Now.ToLongTimeString()));
						dataPoint.CopyDataValues(EndAnchor);
						EndAnchor.IsEditing = false;
					}

					// is initial building done (both anchors set)
					if (!StartAnchor.IsEditing && !EndAnchor.IsEditing)
					{
                        Print(string.Format("OnMouseDown both anchors set {0}", DateTime.Now.ToLongTimeString()));
						DrawingState = DrawingState.Normal;
						IsSelected = false;
					}
					break;
				case DrawingState.Normal:
					Point point = dataPoint.GetPoint(chartControl, chartPanel, chartScale);
                
                    Print(string.Format("Click on Horizontal Line {0}", DateTime.Now.ToLongTimeString()));

					// see if they clicked near a point to edit, if so start editing
					if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
					{
						if (GetCursor(chartControl, chartPanel, chartScale, point) == null)
							IsSelected = false;
						else
						{
							// we dont care here, since we're moving just one anchor
							editingAnchor = StartAnchor;
						}
					}
					
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
				
					// horizontal line only needs Y value updated
					editingAnchor.Price = dataPoint.Price;
					EndAnchor.Price		= dataPoint.Price;

			}
			else if (DrawingState == DrawingState.Moving)
				foreach (ChartAnchor anchor in Anchors)
					// only move anchor values as needed depending on line type
					if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
						anchor.MoveAnchorPrice(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
				
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
			if (Stroke == null)
				return;

			Stroke.RenderTarget									= RenderTarget;

			SharpDX.Direct2D1.AntialiasMode	oldAntiAliasMode	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			ChartPanel						panel				= chartControl.ChartPanels[chartScale.PanelIndex];
			Point							startPoint			= StartAnchor.GetPoint(chartControl, panel, chartScale);

			// align to full pixel to avoid unneeded aliasing
			double							strokePixAdj		= ((double)(Stroke.Width % 2)).ApproxCompare(0) == 0 ? 0.5d : 0d;
			Vector							pixelAdjustVec		= new Vector(strokePixAdj, strokePixAdj);

			if (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom)
			{
				// horizontal and vertical line only need single anchor (StartAnchor) to draw
				// so just go by panel bounds. Keep in mind the panel may not start at 0
				Point startAdj	= (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom ? new Point(panel.X, startPoint.Y) : new Point(startPoint.X, panel.Y)) + pixelAdjustVec;
				Point endAdj	= (HorizontalLineCustomType == ChartLineType2.HorizontalLineCustom ? new Point(panel.X + panel.W, startPoint.Y) : new Point(startPoint.X, panel.Y + panel.H)) + pixelAdjustVec;
				RenderTarget.DrawLine(startAdj.ToVector2(), endAdj.ToVector2(), Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
				return;
			}

			Point					endPoint			= EndAnchor.GetPoint(chartControl, panel, chartScale);

			// convert our start / end pixel points to directx 2d vectors
			Point					endPointAdjusted	= endPoint + pixelAdjustVec;
			SharpDX.Vector2			endVec				= endPointAdjusted.ToVector2();
			Point					startPointAdjusted	= startPoint + pixelAdjustVec;
			SharpDX.Vector2			startVec			= startPointAdjusted.ToVector2();
			SharpDX.Direct2D1.Brush	tmpBrush			= IsInHitTest ? chartControl.SelectionBrush : Stroke.BrushDX;


			// we have a line type with extensions (ray / extended line) or additional drawing needed
			// create a line vector to easily calculate total length
			Vector lineVector = endPoint - startPoint;
			lineVector.Normalize();


			Point minPoint = startPointAdjusted;
			Point maxPoint = GetExtendedPoint(chartControl, panel, chartScale, StartAnchor, EndAnchor);//GetExtendedPoint(startPoint, endPoint); //

			RenderTarget.DrawLine(minPoint.ToVector2(), maxPoint.ToVector2(), tmpBrush, Stroke.Width, Stroke.StrokeStyle);
			
			
			RenderTarget.AntialiasMode	= oldAntiAliasMode;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				HorizontalLineCustomType					= ChartLineType2.HorizontalLineCustom;
				Name						= "WBMStopOrderHorizontalLine";
				DrawingState				= DrawingState.Building;

				EndAnchor					= new ChartAnchor
				{
					IsEditing		= true,
					DrawingTool		= this,
					DisplayName		= Custom.Resource.NinjaScriptDrawingToolAnchorEnd,
					IsBrowsable		= true
				};

				StartAnchor			= new ChartAnchor
				{
					IsEditing		= true,
					DrawingTool		= this,
					DisplayName		= Custom.Resource.NinjaScriptDrawingToolAnchorStart,
					IsBrowsable		= true
				};

				// a normal line with both end points has two anchors
				Stroke						= new Stroke(Brushes.IndianRed, DashStyleHelper.DashDotDot, 1f);
			}
			else if (State == State.Terminated)
			{
				// release any device resources
				Dispose();
			}
		}
	}

	public static partial class Draw
	{
		private static T DrawLineTypeCore2<T>(NinjaScriptBase owner, bool isAutoScale, string tag,
										int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
										Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName) where T : HorizontalLineCustom
		{
			if (owner == null)
				throw new ArgumentException("owner");

			if (string.IsNullOrWhiteSpace(tag))
				throw new ArgumentException(@"tag cant be null or empty", "tag");

			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = string.Format("{0}{1}", GlobalDrawingToolManager.GlobalDrawingToolTagPrefix, tag);

			T lineT = DrawingTool.GetByTagOrNew(owner, typeof(T), tag, templateName) as T;

			if (lineT == null)
				return null;

			if (lineT is VerticalLine)
			{
				if (startTime == Core.Globals.MinDate && startBarsAgo == int.MinValue)
					throw new ArgumentException("missing vertical line time / bars ago");
			}
			else if (lineT is HorizontalLine)
			{
				if (startY.ApproxCompare(double.MinValue) == 0)
					throw new ArgumentException("missing horizontal line Y");
			}
			else if (startTime == Core.Globals.MinDate && endTime == Core.Globals.MinDate && startBarsAgo == int.MinValue && endBarsAgo == int.MinValue)
				throw new ArgumentException("bad start/end date/time");

			DrawingTool.SetDrawingToolCommonValues(lineT, tag, isAutoScale, owner, isGlobal);

			// dont nuke existing anchor refs on the instance
			ChartAnchor startAnchor;

			// check if its one of the single anchor lines
			if (lineT is HorizontalLine || lineT is VerticalLine)
			{
				startAnchor = DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
				startAnchor.CopyDataValues(lineT.StartAnchor);
			}
			else
			{
				startAnchor				= DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
				ChartAnchor endAnchor	= DrawingTool.CreateChartAnchor(owner, endBarsAgo, endTime, endY);
				startAnchor.CopyDataValues(lineT.StartAnchor);
				endAnchor.CopyDataValues(lineT.EndAnchor);
			}

			if (brush != null)
				lineT.Stroke = new Stroke(brush, dashStyle, width) { RenderTarget = lineT.Stroke.RenderTarget };

			lineT.SetState(State.Active);
			return lineT;
		}

	
		// horizontal line overloads
		private static HorizontalLineCustom HorizontalLineCore2(NinjaScriptBase owner, bool isAutoScale, string tag,
												double y, Brush brush, DashStyleHelper dashStyle, int width)
		{
            
			return DrawLineTypeCore2<HorizontalLineCustom>(owner, isAutoScale, tag, 0, Core.Globals.MinDate, y, 0, Core.Globals.MinDate,
											y, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, double y, Brush brush)
		{
			return HorizontalLineCore2(owner, false, tag, y, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, double y, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return HorizontalLineCore2(owner, false, tag, y, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, double y, Brush brush, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				HorizontalLineCore2(owner, false, tag, y, brush, DashStyleHelper.Solid, 1));
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, double y, Brush brush,
													DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				HorizontalLineCore2(owner, false, tag, y, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, double y, bool isGlobal, string templateName)
		{
			return DrawLineTypeCore2<HorizontalLineCustom>(owner, false, tag, int.MinValue, Core.Globals.MinDate, y, int.MinValue, Core.Globals.MinDate,
											y, null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, bool isAutoScale, double y, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return HorizontalLineCore2(owner, isAutoScale, tag, y, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoscale">if set to <c>true</c> [is autoscale].</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static HorizontalLineCustom HorizontalLineCustom(NinjaScriptBase owner, string tag, bool isAutoscale, double y, Brush brush, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				HorizontalLineCore2(owner, isAutoscale, tag, y, brush, DashStyleHelper.Solid, 1));
		}
	}
}
