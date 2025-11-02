// Strategy: Swing Level Trader
// Author: Wesley (modular logic for clarity and reuse)

using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class SwingLevelTrader : Strategy
    {
        private Swing swing;
        private int swingStrength = 5;
        private double lastSwingHigh = double.NaN;
        private double lastSwingLow = double.NaN;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SwingLevelTrader";
                Calculate = MarketDataType.Last;
                IsOverlay = true;
                IsInstantiatedOnEachOptimizationIteration = false;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 30); // 30-minute bars
            }
            else if (State == State.DataLoaded)
            {
                swing = Swing(BarsArray[1], swingStrength);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 1 || CurrentBar < swingStrength) return;

            // Capture new swing levels
            double swingHigh = swing.SwingHigh[swingStrength];
            double swingLow = swing.SwingLow[swingStrength];

            if (!double.IsNaN(swingHigh) && swingHigh != lastSwingHigh)
            {
                lastSwingHigh = swingHigh;
                Draw.HorizontalLine(this, "High_" + CurrentBar, swingHigh, Brushes.DarkRed);
            }

            if (!double.IsNaN(swingLow) && swingLow != lastSwingLow)
            {
                lastSwingLow = swingLow;
                Draw.HorizontalLine(this, "Low_" + CurrentBar, swingLow, Brushes.DarkGreen);
            }

            // Trade logic on primary series
            if (BarsInProgress == 0)
            {
                // Sell on bearish swing high touch
                if (Position.MarketPosition != MarketPosition.Short && Close[0] >= lastSwingHigh)
                {
                    EnterShort(1, "SwingShort");
                }

                // Buy on bullish swing low touch
                if (Position.MarketPosition != MarketPosition.Long && Close[0] <= lastSwingLow)
                {
                    EnterLong(1, "SwingLong");
                }
            }
        }
    }
}