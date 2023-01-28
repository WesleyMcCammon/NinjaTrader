#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class WBMChartTrader : Indicator
	{
		private MenuItem buyStopMenuItem;
		private MenuItem sellStopMenuItem;
		private ChartScale chartScale;
		private Point clickPoint = new Point();
		private double convertedPrice;
		private DateTime convertedTime;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Add Stop Orders to Context Menu";
				Name = "WBMChartTrader";
				Calculate = Calculate.OnEachTick;
				IsOverlay = true;
				IsSuspendedWhileInactive = true;
			}
			else if (State == State.Historical)
			{
				if (ChartControl == null) return;
				if (ChartControl != null)
				{
					foreach (ChartScale scale in ChartPanel.Scales)
						if (scale.ScaleJustification == ScaleJustification)
							chartScale = scale;

                    ChartControl.MouseRightButtonDown += ChartControl_MouseRightButtonDown;
				}
				ChartControl.Dispatcher.InvokeAsync(new Action(() =>
				{
					ChartControl.ContextMenuOpening += ChartControl_ContextMenuOpening;
					ChartControl.ContextMenuClosing += ChartControl_ContextMenuClosing;
					buyStopMenuItem = new MenuItem { Header = "Buy Stop" };
					sellStopMenuItem = new MenuItem { Header = "Sell Stop" };
                    buyStopMenuItem.Click += BuyStopMenuItem_Click;
                    sellStopMenuItem.Click += SellStopMenuItem_Click;
				}));
			}
			else if (State == State.Terminated)
			{
				if (ChartControl == null) return;

				ChartControl.MouseRightButtonDown -= ChartControl_MouseRightButtonDown;

				if (buyStopMenuItem != null)
				{
					buyStopMenuItem.Click -= BuyStopMenuItem_Click;
				}
				if (sellStopMenuItem != null)
				{
					sellStopMenuItem.Click -= SellStopMenuItem_Click;
				}

				ChartControl.ContextMenuOpening -= ChartControl_ContextMenuOpening;
				ChartControl.ContextMenuClosing -= ChartControl_ContextMenuOpening;
			}
		}

        private void ChartControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			// convert e.GetPosition for different dpi settings
			clickPoint.X = ChartingExtensions.ConvertToHorizontalPixels(e.GetPosition(ChartControl as IInputElement).X, ChartControl.PresentationSource);
			clickPoint.Y = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);

			convertedPrice = Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY((float)clickPoint.Y));

			convertedTime = ChartControl.GetTimeBySlotIndex((int)ChartControl.GetSlotIndexByX((int)clickPoint.X));

			Draw.TextFixed(this, "priceTime", string.Format("Price: {0}, Time: {1}", convertedPrice, convertedTime), TextPosition.BottomLeft);
		}

		private void ChartControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			ChartControl chartControl = sender as ChartControl;
			if (chartControl == null) return;

			// convertedPrice
			buyStopMenuItem.Header = string.Format("Buy Stop @ {0}", convertedPrice);
			sellStopMenuItem.Header = string.Format("Sell Stop @ {0}", convertedPrice);
			if (chartControl.ContextMenu != null) chartControl.ContextMenu.Items.Add(buyStopMenuItem);
			if (chartControl.ContextMenu != null) chartControl.ContextMenu.Items.Add(sellStopMenuItem);
		}

		protected override void OnBarUpdate() { }

		private void ChartControl_ContextMenuClosing(object sender, ContextMenuEventArgs e)
		{
			if (ChartControl.ContextMenu != null && ChartControl.ContextMenu.Items.Contains(buyStopMenuItem)) ChartControl.ContextMenu.Items.Remove(buyStopMenuItem);
			if (ChartControl.ContextMenu != null && ChartControl.ContextMenu.Items.Contains(sellStopMenuItem)) ChartControl.ContextMenu.Items.Remove(sellStopMenuItem);
		}


		private void SellStopMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Print("SellStopMenuItem_Click");
		}

		private void BuyStopMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Print(string.Format("BuyStopMenuItem_Click {0}", clickPoint.Y));
			Draw.HorizontalLine(this, "tag1", 250, Brushes.LimeGreen, DashStyleHelper.Dot, 2);
			// Draw.Line(this, Guid.NewGuid().ToString(), false, 10, 1000, 0, 1001, Brushes.LimeGreen, DashStyleHelper.Dot, 2);
			//Draw.Line(this, "tag1", false, 10, 1000, 0, 1001, Brushes.LimeGreen, DashStyleHelper.Dot, 2);
			//Draw.Line(this, "Buy Stop Trigger", 10, clickPoint.Y, 1, clickPoint.Y, Brushes.LimeGreen);
			// Draw.Line(this, "tag1", true, 10, clickPoint.Y, 0, clickPoint.Y, Brushes.LimeGreen, DashStyleHelper.Dot, 2);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WBMChartTrader[] cacheWBMChartTrader;
		public WBMChartTrader WBMChartTrader()
		{
			return WBMChartTrader(Input);
		}

		public WBMChartTrader WBMChartTrader(ISeries<double> input)
		{
			if (cacheWBMChartTrader != null)
				for (int idx = 0; idx < cacheWBMChartTrader.Length; idx++)
					if (cacheWBMChartTrader[idx] != null &&  cacheWBMChartTrader[idx].EqualsInput(input))
						return cacheWBMChartTrader[idx];
			return CacheIndicator<WBMChartTrader>(new WBMChartTrader(), input, ref cacheWBMChartTrader);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WBMChartTrader WBMChartTrader()
		{
			return indicator.WBMChartTrader(Input);
		}

		public Indicators.WBMChartTrader WBMChartTrader(ISeries<double> input )
		{
			return indicator.WBMChartTrader(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WBMChartTrader WBMChartTrader()
		{
			return indicator.WBMChartTrader(Input);
		}

		public Indicators.WBMChartTrader WBMChartTrader(ISeries<double> input )
		{
			return indicator.WBMChartTrader(input);
		}
	}
}

#endregion
