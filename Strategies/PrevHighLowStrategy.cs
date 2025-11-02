// Strategy: Alert on Previous Day High/Low Touch
// Author: Wesley (via Copilot)
// Description: Sends alerts when price hits prior day's high or low

using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class PreviousDayHighLowAlert : Strategy
    {
        private double prevHigh;
        private double prevLow;
        private double tolerance = 1.0; // Price proximity threshold

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "PreviousDayHighLowAlert";
                Calculate = MarketDataType.Last;
                IsOverlay = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Day, 1); // Add daily series for reference
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || CurrentBar < 1)
                return;

            // Ensure daily series has enough bars
            if (BarsArray[1].Count < 2)
                return;

            // Get previous day's high and low from daily series
            prevHigh = Highs[1][1];
            prevLow = Lows[1][1];

            double price = Close[0];

            CheckLevel("Previous Day High", prevHigh, price);
            CheckLevel("Previous Day Low", prevLow, price);
        }

        private void CheckLevel(string label, double level, double price)
        {
            if (Math.Abs(price - level) <= tolerance)
            {
                Alert($"Alert_{label.Replace(" ", "")}", Priority.High,
                    $"Price touched {label}: {level:F2}",
                    NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert2.wav",
                    10, Brushes.Orange, Brushes.Black);
            }
        }
    }
}