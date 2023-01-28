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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class WBMStopOrderButtons : Indicator
	{
		private System.Windows.Controls.RowDefinition addedRow1, addedRow2;
		private Gui.Chart.ChartTab chartTab;
		private Gui.Chart.Chart chartWindow;
		private System.Windows.Controls.Grid chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, upperButtonsGrid;
		private System.Windows.Controls.Button buyStopButton;
		private System.Windows.Controls.Button sellStopButton;
		private bool panelActive;
		private System.Windows.Controls.TabItem tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"ChartTrader Stop Buttons";
				Name = "WBMStopOrderButtons";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = false;
				PaintPriceMarkers = false;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateWPFControls();
					});
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						DisposeWPFControls();
					});
				}
			}
		}

		protected void CreateWPFControls()
		{
			chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			// this is the entire chart trader area grid
			chartTraderGrid = (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid = chartTraderGrid.Children[0] as System.Windows.Controls.Grid;

			// this grid is a grid i'm adding to a new row (at the bottom) in the grid that contains bid and ask prices and order controls (chartTraderButtonsGrid)
			upperButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(upperButtonsGrid, 3);

			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) }); // separator column
			upperButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// this grid is to organize stuff below
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 4);

			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) });
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// these rows will be added later, but we can create them now so they only get created once
			addedRow1 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(31) };
			addedRow2 = new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle = Application.Current.FindResource("BasicEntryButton") as Style;

			// all of the buttons are basically the same so to save lines of code I decided to use a loop over an array
			buyStopButton = new System.Windows.Controls.Button()
			{
				Content = "Buy Stop",
				Height = 30,
				Margin = new Thickness(0, 0, 0, 0),
				Padding = new Thickness(0, 0, 0, 0),
				Style = basicButtonStyle,
				Background = Brushes.SeaGreen,
				Foreground = Brushes.White
			};
			sellStopButton = new System.Windows.Controls.Button()
			{
				Content = "Sell Stop",
				Height = 30,
				Margin = new Thickness(0, 0, 0, 0),
				Padding = new Thickness(0, 0, 0, 0),
				Style = basicButtonStyle,
				Background = Brushes.IndianRed,
				Foreground = Brushes.White
			};

			buyStopButton.Click += BuyStopButton_Click;
			sellStopButton.Click += SellStopButton_Click;

			System.Windows.Controls.Grid.SetColumn(buyStopButton, 2);
			System.Windows.Controls.Grid.SetColumn(sellStopButton, 0);
			upperButtonsGrid.Children.Add(buyStopButton);
			upperButtonsGrid.Children.Add(sellStopButton);

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		public void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			if (buyStopButton != null)
				buyStopButton.Click -= BuyStopButton_Click;
			if (sellStopButton != null)
				sellStopButton.Click -= SellStopButton_Click;

			RemoveWPFControls();
		}

		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || upperButtonsGrid != null)
			{
				chartTraderButtonsGrid.Children.Remove(upperButtonsGrid);
				chartTraderButtonsGrid.RowDefinitions.Remove(addedRow1);
			}

			if (chartTraderButtonsGrid != null || lowerButtonsGrid != null)
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.RowDefinitions.Remove(addedRow2);
			}

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		public void InsertWPFControls()
		{
			if (panelActive)
				return;

			// add a new row (addedRow1) for upperButtonsGrid to the existing buttons grid
			chartTraderButtonsGrid.RowDefinitions.Add(addedRow1);
			// set our upper grid to that new panel
			System.Windows.Controls.Grid.SetRow(upperButtonsGrid, (chartTraderButtonsGrid.RowDefinitions.Count - 1));
			// and add it to the buttons grid
			chartTraderButtonsGrid.Children.Add(upperButtonsGrid);

			// add a new row (addedRow2) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow2);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, (chartTraderGrid.RowDefinitions.Count - 1));
			chartTraderGrid.Children.Add(lowerButtonsGrid);

			panelActive = true;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}

		private double GetStopOffset(Instrument instrument)
        {
			double offset = 0;
			if (Instrument.FullName.StartsWith("ES"))
			{
				offset = .75;
			}
			else if (Instrument.FullName.StartsWith("YM"))
			{
				offset = 5;
			}
			else if (Instrument.FullName.StartsWith("CL"))
			{
				offset = .03;
			}
			else if (Instrument.FullName.StartsWith("GC"))
			{
				offset = .3;
			}

			return offset;
		}
		private void SellStopButton_Click(object sender, RoutedEventArgs e)
		{			
			InstrumentSelector instrumentSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
			AccountSelector accountSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
			QuantityUpDown quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown);
			Account account = Account.All.FirstOrDefault(a => a.Name == accountSelector.SelectedAccount.Name);

			double currentBid = GetCurrentAsk();
			double offset = GetStopOffset(instrumentSelector.Instrument);
			Order entryOrder = account.CreateOrder(instrumentSelector.Instrument, OrderAction.Sell, OrderType.StopMarket,
				TimeInForce.Day, quantitySelector.Value, 0, currentBid - offset, string.Empty, "Entry", null);
			AtmStrategy.StartAtmStrategy("2-300-500", entryOrder);
		}

		private void BuyStopButton_Click(object sender, RoutedEventArgs e)
		{
			InstrumentSelector instrumentSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
			AccountSelector accountSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
			QuantityUpDown quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown);
			Account account = Account.All.FirstOrDefault(a => a.Name == accountSelector.SelectedAccount.Name);

			double currentAsk = GetCurrentAsk();
			double offset = GetStopOffset(instrumentSelector.Instrument);

			Order entryOrder = account.CreateOrder(instrumentSelector.Instrument, OrderAction.Buy, OrderType.StopMarket,
				TimeInForce.Day, quantitySelector.Value, 0, currentAsk + offset, string.Empty, "Entry", null);
			AtmStrategy.StartAtmStrategy("2-300-500", entryOrder);
		}
	}
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WBMStopOrderButtons[] cacheWBMStopOrderButtons;
		public WBMStopOrderButtons WBMStopOrderButtons()
		{
			return WBMStopOrderButtons(Input);
		}

		public WBMStopOrderButtons WBMStopOrderButtons(ISeries<double> input)
		{
			if (cacheWBMStopOrderButtons != null)
				for (int idx = 0; idx < cacheWBMStopOrderButtons.Length; idx++)
					if (cacheWBMStopOrderButtons[idx] != null &&  cacheWBMStopOrderButtons[idx].EqualsInput(input))
						return cacheWBMStopOrderButtons[idx];
			return CacheIndicator<WBMStopOrderButtons>(new WBMStopOrderButtons(), input, ref cacheWBMStopOrderButtons);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WBMStopOrderButtons WBMStopOrderButtons()
		{
			return indicator.WBMStopOrderButtons(Input);
		}

		public Indicators.WBMStopOrderButtons WBMStopOrderButtons(ISeries<double> input )
		{
			return indicator.WBMStopOrderButtons(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WBMStopOrderButtons WBMStopOrderButtons()
		{
			return indicator.WBMStopOrderButtons(Input);
		}

		public Indicators.WBMStopOrderButtons WBMStopOrderButtons(ISeries<double> input )
		{
			return indicator.WBMStopOrderButtons(input);
		}
	}
}

#endregion
