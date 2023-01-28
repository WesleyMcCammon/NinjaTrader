// Coded by Chelsea Bell. chelsea.bell@ninjatrader.com
#region Using declarations
using System.Windows;
using System.Windows.Media;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.Tools;
using System.Linq;
using NinjaTrader.Cbi;
using System.Collections.Generic;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartTraderCustomButtonsExample : Indicator
	{
		private System.Windows.Controls.RowDefinition	addedRow1, addedRow2;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, upperButtonsGrid;
		private System.Windows.Controls.Button[]		buttonsArray;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Demonstrates adding buttons to Chart Trader";
				Name						= "ChartTraderCustomButtonsExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
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

		protected void Button1Click(object sender, RoutedEventArgs e)
		{
			double currentAsk = GetCurrentAsk();
			InstrumentSelector instrumentSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
			AccountSelector accountSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
			QuantityUpDown quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown);
			Account account = Account.All.FirstOrDefault(a => a.Name == accountSelector.SelectedAccount.Name);

			Order stopOrder = new Order
			{
				Account = account,
				Instrument = instrumentSelector.Instrument,
				OrderAction = Cbi.OrderAction.Buy,
				OrderType = Cbi.OrderType.StopMarket,
				TimeInForce = Cbi.TimeInForce.Day,
				Quantity = quantitySelector.Value,
				StopPrice = currentAsk + 2				
			};

            Order entryOrder = account.CreateOrder(instrumentSelector.Instrument, OrderAction.Buy, OrderType.StopMarket,
                TimeInForce.Day, quantitySelector.Value, 0, currentAsk + 2, string.Empty, "Entry", null);
            // account.Submit(new[] { stopOrder });
            NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy("2-300-500", entryOrder);

			ForceRefresh();
		}

		protected void Button2Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 2 Clicked", TextPosition.BottomLeft, Brushes.DarkRed, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		protected void Button3Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 3 Clicked", TextPosition.BottomLeft, Brushes.DarkOrange, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		protected void Button4Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 4 Clicked", TextPosition.BottomLeft, Brushes.CadetBlue, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		protected void CreateWPFControls()
		{
			chartWindow				= Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			// this is the entire chart trader area grid
			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as System.Windows.Controls.Grid;

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
			addedRow1	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(31) };
			addedRow2	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;

			// all of the buttons are basically the same so to save lines of code I decided to use a loop over an array
			buttonsArray = new System.Windows.Controls.Button[4];

			for (int i = 0; i < 4; ++i)
			{
				buttonsArray[i]	= new System.Windows.Controls.Button()
				{
					Content			= string.Format("MyButton{0}", i + 1),
					Height			= 30,
					Margin			= new Thickness(0,0,0,0),
					Padding			= new Thickness(0,0,0,0),
					Style			= basicButtonStyle
				};

				// change colors of the buttons if you'd like. i'm going to change the first and fourth.
				if (i % 3 != 0)
				{
					buttonsArray[i].Background	= Brushes.Gray;
					buttonsArray[i].BorderBrush	= Brushes.DimGray;
				}
			}

			buttonsArray[0].Click += Button1Click;
			buttonsArray[1].Click += Button2Click;
			buttonsArray[2].Click += Button3Click;
			buttonsArray[3].Click += Button4Click;

			System.Windows.Controls.Grid.SetColumn(buttonsArray[1], 2);
			// add button3 to the lower grid
			System.Windows.Controls.Grid.SetColumn(buttonsArray[2], 0);
			// add button4 to the lower grid
			System.Windows.Controls.Grid.SetColumn(buttonsArray[3], 2);
			for (int i = 0; i < 2; ++i)
				upperButtonsGrid.Children.Add(buttonsArray[i]);
			for (int i = 2; i < 4; ++i)
				lowerButtonsGrid.Children.Add(buttonsArray[i]);

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		public void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			if (buttonsArray[0] != null)
				buttonsArray[0].Click -= Button1Click;
			if (buttonsArray[0] != null)
				buttonsArray[1].Click -= Button2Click;
			if (buttonsArray[0] != null)
				buttonsArray[2].Click -= Button3Click;
			if (buttonsArray[0] != null)
				buttonsArray[3].Click -= Button4Click;

			RemoveWPFControls();
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

		protected override void OnBarUpdate() { }

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
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartTraderCustomButtonsExample[] cacheChartTraderCustomButtonsExample;
		public ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return ChartTraderCustomButtonsExample(Input);
		}

		public ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input)
		{
			if (cacheChartTraderCustomButtonsExample != null)
				for (int idx = 0; idx < cacheChartTraderCustomButtonsExample.Length; idx++)
					if (cacheChartTraderCustomButtonsExample[idx] != null &&  cacheChartTraderCustomButtonsExample[idx].EqualsInput(input))
						return cacheChartTraderCustomButtonsExample[idx];
			return CacheIndicator<ChartTraderCustomButtonsExample>(new ChartTraderCustomButtonsExample(), input, ref cacheChartTraderCustomButtonsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return indicator.ChartTraderCustomButtonsExample(Input);
		}

		public Indicators.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderCustomButtonsExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample()
		{
			return indicator.ChartTraderCustomButtonsExample(Input);
		}

		public Indicators.ChartTraderCustomButtonsExample ChartTraderCustomButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderCustomButtonsExample(input);
		}
	}
}

#endregion
