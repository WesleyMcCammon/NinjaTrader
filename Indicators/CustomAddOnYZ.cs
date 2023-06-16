using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Code;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.Gui.NinjaScript
{
    public class CustomAddOnYZ : AddOnBase
    {
        private NTMenuItem _myMenuItem;
        private NTMenuItem _existingMenu;

        protected override void OnStateChange()
        {
            if(State == State.SetDefaults)
            {
                Description = "";
                Name = "Order Entry";
            }
        }

        protected override void OnWindowCreated(Window window)
        {
            var controlCenterWindow = window as ControlCenter;
            if (controlCenterWindow == null)
                return;

            _existingMenu = controlCenterWindow.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;

            if (_existingMenu == null)
                return;

            _myMenuItem = new NTMenuItem
            {
                Header = "Order Entry",
                Style = Application.Current.TryFindResource("MainMenuItem") as Style
            };

            _existingMenu.Items.Add(_myMenuItem);
            _myMenuItem.Click += _myMenuItem_Click;
        }

        protected override void OnWindowDestroyed(Window window)
        {
            if(_myMenuItem != null && window is ControlCenter) 
            {
                if(_existingMenu != null && _existingMenu.Items.Contains(_myMenuItem))
                {
                    _existingMenu.Items.Remove(_myMenuItem);
                }

                _myMenuItem.Click -= _myMenuItem_Click;
                _myMenuItem = null;
            }
        }

        private void _myMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Output.Process("YZ Custom Control Click", PrintTo.OutputTab1);
            Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new YZCustomWindow().Show()));
        }
    }

    public class Message
    {
        public static void MessageShow(string text)
        {
            MessageBox.Show(text);
        }
    }

    public class YZCustomWindow : NTWindow
    {
		
        public YZCustomWindow()
        {
            Caption = "Order Entry";
            Width = 400;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var tabControl = new TabControl();
            TabControlManager.SetIsMovable(tabControl, true);
            TabControlManager.SetCanAddTabs(tabControl, true);
            TabControlManager.SetCanRemoveTabs(tabControl, true);
            tabControl.AddNTTabPage(new YZTab());

            TabControlManager.SetFactory(tabControl, new YZTabFactory());
            Content = tabControl;
        }
    }

    public class YZTab: NTTabPage
    {
		private NinjaTrader.NinjaScript.Indicators.PriorDayOHLC _PriorDayOHLC { get; set; }
		public double PrevHighDiff { get; set; }
		public double PrevLowDiff { get; set; }
        private Order entryOrder;
		
        public YZTab()
        {
            Content = LoadXAML();
			
			// _PriorDayOHLC = PriorDayOHLC();
			// Print(_PriorDayOHLC.PriorHigh[0]);

            // Find our Sim101 account
            lock (Account.All)
            {
            }
        }
        protected override string GetHeaderPart(string variable)
        {
            return "YZ Tab";
        }

        protected override void Restore(XElement element)
        {
            throw new NotImplementedException();
        }

        protected override void Save(XElement element)
        {
            throw new NotImplementedException();
        }

        private DependencyObject LoadXAML()
        {
            try
            {
                using (var stream = GetManifestResourceStream("AddOns.CustomTab.xaml"))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var page = System.Windows.Markup.XamlReader.Load(streamReader.BaseStream) as Page;
                        DependencyObject pageContent = null;

                        if (page != null)
                        {
                            pageContent = page.Content as DependencyObject;
                        }

                        return pageContent;
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private double GetMarketPrice(string instrument)
        {
            return Cbi.Instrument.GetInstrument(instrument).MarketData.Ask.Price;
        }

        private void SendOrder(Instrument instrument, OrderAction orderAction, OrderType orderType, int quantity, double stopPrice = 0.00)
        {
        }
    }

    public class YZTabFactory : INTTabFactory
    {
        public NTWindow CreateParentWindow()
        {
            return new YZCustomWindow();
        }

        public NTTabPage CreateTabPage(string typeName, bool isNewWindow = false)
        {
            return new YZTab();
        }
    }
}
