// Strategy: Alert on 1-Minute Fair Value Gap Touch
// Author: Wesley (via Copilot)
// Description: Detects FVGs and alerts on price retrace

using System.Collections.Generic;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class FVGTouchAlert : Strategy
    {
        private struct FVG
        {
            public double Top;
            public double Bottom;
            public int BarIndex;
            public bool IsBullish;
        }

        private List<FVG> fvgList = new List<FVG>();
        private double tolerance = 0.25;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "FVGTouchAlert";
                Calculate = MarketDataType.Last;
                IsOverlay = true;
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || CurrentBar < 3)
                return;

            // Detect FVG between bar n-2 and n
            double high2 = High[2];
            double low0 = Low[0];

            double low2 = Low[2];
            double high0 = High[0];

            // Bullish FVG: High[2] < Low[0]
            if (high2 < low0)
            {
                fvgList.Add(new FVG
                {
                    Top = low0,
                    Bottom = high2,
                    BarIndex = CurrentBar,
                    IsBullish = true
                });
            }

            // Bearish FVG: Low[2] > High[0]
            if (low2 > high0)
            {
                fvgList.Add(new FVG
                {
                    Top = low2,
                    Bottom = high0,
                    BarIndex = CurrentBar,
                    IsBullish = false
                });
            }

            // Check for retrace into any FVG
            foreach (var fvg in fvgList.ToArray())
            {
                if (Close[0] >= fvg.Bottom - tolerance && Close[0] <= fvg.Top + tolerance)
                {
                    string type = fvg.IsBullish ? "Bullish" : "Bearish";
                    Alert($"FVGTouch_{fvg.BarIndex}", Priority.High,
                        $"Price retraced into {type} FVG from bar {fvg.BarIndex}",
                        NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert2.wav",
                        10, Brushes.Cyan, Brushes.Black);

                    fvgList.Remove(fvg); // Optional: remove after touch
                    break;
                }
            }
        }
    }
}