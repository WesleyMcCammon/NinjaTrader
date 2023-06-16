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
	public class WBMPrevHLDiff : Indicator
	{
		private PriorDayOHLC _PriorDayOHLC { get; set; }		
		public double PrevHighDiff { get; set; }
		public double PrevLowDiff { get; set; }
		private string _instrument { get; set; }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "WBMPrevHLDiff";
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
				AddPlot(Brushes.Green, "PrevHighDiff");
				AddPlot(Brushes.LightGreen,	"PrevLowDiff");
			}
            else if(State == State.DataLoaded)
            {
				_PriorDayOHLC = PriorDayOHLC();
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
			var highDiff = _PriorDayOHLC.PriorHigh[0] - Close[0];
			var lowDiff = _PriorDayOHLC.PriorLow[0] - Close[0];
			Values[0][0] = lowDiff;
			Values[1][0] = highDiff;	
					
			if(!string.IsNullOrEmpty(_instrument)) 
			{	
				PrevOHLC prevOHLC = new PrevOHLC
				{
					instrument = _instrument,
					type = "PrevOHLC",
					tickSize = 1/TickSize,
					bid = GetCurrentBid(),
					ask = GetCurrentAsk(),
					data = new PrevOHLCLevel {
						previousDayHigh = _PriorDayOHLC.PriorHigh[0],
						previousDayLow = _PriorDayOHLC.PriorLow[0],
						previousDayHighDiff = _PriorDayOHLC.PriorHigh[0] - Close[0],
						previousDayLowDiff = _PriorDayOHLC.PriorLow[0] - Close[0]
					}
				};
				
				string message = JsonConvert.SerializeObject(prevOHLC);
				NinjaTrader8WBMInterface.RabbitMQPublish.Send("nt8_prevohlc", message);
			}
		}
	}
	
	public class PrevOHLC
	{
		public string instrument;
		public string type;
		public double tickSize;
		public double bid;
		public double ask;
		public PrevOHLCLevel data;
	}
	
	public class PrevOHLCLevel 
	{
		public double previousDayHigh;
		public double previousDayLow;
		public double previousDayHighDiff;
		public double previousDayLowDiff;
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WBMPrevHLDiff[] cacheWBMPrevHLDiff;
		public WBMPrevHLDiff WBMPrevHLDiff()
		{
			return WBMPrevHLDiff(Input);
		}

		public WBMPrevHLDiff WBMPrevHLDiff(ISeries<double> input)
		{
			if (cacheWBMPrevHLDiff != null)
				for (int idx = 0; idx < cacheWBMPrevHLDiff.Length; idx++)
					if (cacheWBMPrevHLDiff[idx] != null &&  cacheWBMPrevHLDiff[idx].EqualsInput(input))
						return cacheWBMPrevHLDiff[idx];
			return CacheIndicator<WBMPrevHLDiff>(new WBMPrevHLDiff(), input, ref cacheWBMPrevHLDiff);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WBMPrevHLDiff WBMPrevHLDiff()
		{
			return indicator.WBMPrevHLDiff(Input);
		}

		public Indicators.WBMPrevHLDiff WBMPrevHLDiff(ISeries<double> input )
		{
			return indicator.WBMPrevHLDiff(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WBMPrevHLDiff WBMPrevHLDiff()
		{
			return indicator.WBMPrevHLDiff(Input);
		}

		public Indicators.WBMPrevHLDiff WBMPrevHLDiff(ISeries<double> input )
		{
			return indicator.WBMPrevHLDiff(input);
		}
	}
}

#endregion
