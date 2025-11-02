// Strategy: Alert on Pivot Level Touch
// Author: Wesley (via Copilot)
// Description: Sends alerts when price hits Pivot, R1–R3, or S1–S3

using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class PivotLevelAlert : Strategy
    {
        private Pivots pivots;
        private double tolerance = 1.0; // Price proximity threshold

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "PivotLevelAlert";
                Calculate = MarketDataType.Last;
                IsOverlay = true;
            }
            else if (State == State.Configure)
            {
                pivots = Pivots(CalculateOnBarClose: false, PivotRange.Daily, HLCCalculationMode.DailyBars);
                AddChartIndicator(pivots);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1 || pivots == null)
                return;

            double price = Close[0];

            CheckLevel("Pivot", pivots.PP[0], price);
            CheckLevel("R1", pivots.R1[0], price);
            CheckLevel("R2", pivots.R2[0], price);
            CheckLevel("R3", pivots.R3[0], price);
            CheckLevel("S1", pivots.S1[0], price);
            CheckLevel("S2", pivots.S2[0], price);
            CheckLevel("S3", pivots.S3[0], price);
        }

        private void CheckLevel(string label, double level, double price)
        {
            if (Math.Abs(price - level) <= tolerance)
            {
                Alert($"Alert_{label}", Priority.High, $"Price touched {label} level: {level:F2}", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert2.wav", 10, Brushes.Yellow, Brushes.Black);
            }
        }
    }
}