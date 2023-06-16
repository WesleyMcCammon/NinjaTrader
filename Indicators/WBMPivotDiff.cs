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
#endregion
using NinjaTrader8WBMInterface;
using Newtonsoft.Json;

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class WBMPivotDiff : Indicator
	{
		private Pivots _Pivots;
		private string _instrument;
		public double R1Diff { get; set; }
		public double R2Diff { get; set; }
		public double R3Diff { get; set; }
		public double PpDiff { get; set; }
		public double S1Diff { get; set; }
		public double S2Diff { get; set; }
		public double S3Diff { get; set; }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "WBMPivotDiff";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				AddPlot(Brushes.IndianRed, "R3Diff");
				AddPlot(Brushes.Salmon,	"R2Diff");
				AddPlot(Brushes.LightSalmon, "R1Diff");
				AddPlot(Brushes.Goldenrod, "PpDiff");
				AddPlot(Brushes.SeaGreen, "S1Diff");
				AddPlot(Brushes.Green, "S2Diff");
				AddPlot(Brushes.LightGreen,	"S3Diff");
			}
            else if(State == State.DataLoaded)
            {
				_Pivots = Pivots(PivotRange.Daily, HLCCalculationMode.CalcFromIntradayData, 0, 0, 0, 20);
	            ChartControl.Dispatcher.InvokeAsync((Action)(() =>
	            {
					InstrumentSelector instrumentSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
					Print(instrumentSelector.Instrument.FullName);
					_instrument = instrumentSelector.Instrument.FullName;
//					AccountSelector accountSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
//					Account account = Account.All.FirstOrDefault(a => a.Name == accountSelector.SelectedAccount.Name);

//					double currentBid = GetCurrentBid();
//					double priceOffset = GetStopTicks(instrumentSelector.Instrument) / GetTicksPerPoint(instrumentSelector.Instrument);

//	                Print(priceOffset);
//					Print(currentBid);
//	                Print(currentBid - priceOffset);


//	                double stopPrice = currentBid - priceOffset;
//	                Order entryOrder = account.CreateOrder(instrumentSelector.Instrument, OrderAction.Sell, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, 
//						ChartControl.OwnerChart.ChartTrader.Quantity, 0, stopPrice, Guid.NewGuid().ToString(), "Entry", Core.Globals.MaxDate, null);
//	                AtmStrategy.StartAtmStrategy(ChartControl.OwnerChart.ChartTrader.AtmStrategy.Template, entryOrder);
				}));
            }
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1) return;
			var ticks = 1/TickSize;
			
			if(_Pivots[0] > 0) {
				Values[0][0] = _Pivots.R3[0] - Close[0];
				Values[1][0] = _Pivots.R2[0] - Close[0];
				Values[2][0] = _Pivots.R1[0] - Close[0];
				Values[3][0] = _Pivots[0] - Close[0];
				Values[4][0] = _Pivots.S1[0] - Close[0];
				Values[5][0] = _Pivots.S2[0] - Close[0];
				Values[6][0] = _Pivots.S3[0] - Close[0];
			}
			
			if(!string.IsNullOrEmpty(_instrument)) {	
				var pivotData = new PivotData
				{
					instrument = _instrument,
					type = "pivot",
					tickSize = 1/TickSize,
					bid = GetCurrentBid(),
					ask = GetCurrentAsk(),
					data = new PivotLevel[] {
						new PivotLevel { name = "R3", value = _Pivots.R3[0], difference = (_Pivots.R3[0] - Close[0]) },
						new PivotLevel { name = "R2", value = _Pivots.R2[0], difference = (_Pivots.R2[0] - Close[0]) },
						new PivotLevel { name = "R1", value = _Pivots.R1[0], difference = (_Pivots.R1[0] - Close[0]) },
						new PivotLevel { name = "Pp", value = _Pivots[0], difference = (_Pivots[0] - Close[0]) },
						new PivotLevel { name = "S1", value = _Pivots.S1[0], difference = (_Pivots.S1[0] - Close[0]) },
						new PivotLevel { name = "S2", value = _Pivots.S1[0], difference = (_Pivots.S1[0] - Close[0]) },
						new PivotLevel { name = "S3", value = _Pivots.S3[0], difference = (_Pivots.S3[0] - Close[0]) }
					}
				};
				
				string message = JsonConvert.SerializeObject(pivotData);
				NinjaTrader8WBMInterface.RabbitMQPublish.Send("nt8_pivotdiff", message);
			}
		}
	}
	
	public class PivotLevel {
		public string name;
		public double value;
		public double difference;	
	}
	public class PivotData {
		public string instrument;
		public string type;
		public double tickSize;
		public double bid;
		public double ask;
		public PivotLevel[] data;
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WBMPivotDiff[] cacheWBMPivotDiff;
		public WBMPivotDiff WBMPivotDiff()
		{
			return WBMPivotDiff(Input);
		}

		public WBMPivotDiff WBMPivotDiff(ISeries<double> input)
		{
			if (cacheWBMPivotDiff != null)
				for (int idx = 0; idx < cacheWBMPivotDiff.Length; idx++)
					if (cacheWBMPivotDiff[idx] != null &&  cacheWBMPivotDiff[idx].EqualsInput(input))
						return cacheWBMPivotDiff[idx];
			return CacheIndicator<WBMPivotDiff>(new WBMPivotDiff(), input, ref cacheWBMPivotDiff);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WBMPivotDiff WBMPivotDiff()
		{
			return indicator.WBMPivotDiff(Input);
		}

		public Indicators.WBMPivotDiff WBMPivotDiff(ISeries<double> input )
		{
			return indicator.WBMPivotDiff(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WBMPivotDiff WBMPivotDiff()
		{
			return indicator.WBMPivotDiff(Input);
		}

		public Indicators.WBMPivotDiff WBMPivotDiff(ISeries<double> input )
		{
			return indicator.WBMPivotDiff(input);
		}
	}
}

#endregion
