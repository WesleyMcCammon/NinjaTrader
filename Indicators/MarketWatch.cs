using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Code;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;

// using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader8WBMInterface;

namespace NinjaTrader.Gui.NinjaScript 
{
    public class MarketWatch : AddOnBase
    {
        private NTMenuItem _myMenuItem;
        private NTMenuItem _existingMenu;
		// public static event EventHandler<PriceActionEventArgs> PriceActionUpdateEvent;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "";
                Name = "MarketWatcher";
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
                Header = "MarketWatcher",
                Style = Application.Current.TryFindResource("MainMenuItem") as Style
            };

            _existingMenu.Items.Add(_myMenuItem);
            _myMenuItem.Click += _myMenuItem_Click;
			
			// NinjaTrader.NinjaScript.Strategies.PriceActionTransfer.PropChanged += OnPropChanged;
        }

//		protected void OnPropChanged(object o, NinjaTrader.NinjaScript.Strategies.PriceActionEventArgs e) {
//			PriceActionUpdateEvent(o, e);
//		}
		
        protected override void OnWindowDestroyed(Window window)
        {
            if (_myMenuItem != null && window is ControlCenter)
            {
                if (_existingMenu != null && _existingMenu.Items.Contains(_myMenuItem))
                {
                    _existingMenu.Items.Remove(_myMenuItem);
                }

                _myMenuItem.Click -= _myMenuItem_Click;
                _myMenuItem = null;
            }
        }

        private void _myMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new MarketWatchWindow().Show()));
        }
    }

    public class MarketWatchWindow : NTWindow 
    {
        public MarketWatchWindow() 
        {
            Caption = "MarketWatch";
            Width = 1000;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var tabControl = new TabControl();
            TabControlManager.SetIsMovable(tabControl, false);
            TabControlManager.SetCanAddTabs(tabControl, false);
            TabControlManager.SetCanRemoveTabs(tabControl, false);
            tabControl.AddNTTabPage(new MarketWatchTab());

            TabControlManager.SetFactory(tabControl, new MarketWatchTabFactory());
            Content = tabControl;
        }
    }

    public class MarketWatchTabFactory : INTTabFactory
    {
        public NTWindow CreateParentWindow()
        {
            return new MarketWatchWindow();
        }

        public NTTabPage CreateTabPage(string typeName, bool isNewWindow = false)
        {
            return new MarketWatchTab();
        }
    }

    public class MarketWatchTab : NTTabPage
    {
		Label mylabel;
        public MarketWatchTab() 
        {
            Content = LoadXAML();
			// MarketWatch.PriceActionUpdateEvent += OnPriceActionUpdateEvent;
        }

        private DependencyObject LoadXAML()
        {
            try
            {
                using (var stream = GetManifestResourceStream("AddOns.MarketWatch.xaml"))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var page = System.Windows.Markup.XamlReader.Load(streamReader.BaseStream) as Page;
                        DependencyObject pageContent = null;

                        if (page != null)
                        {
                            pageContent = page.Content as DependencyObject;
							
							//PiveChangeTransfer.PropChanged += WBMPivotDiffChanged;
                            
                            // get controls here
							mylabel = LogicalTreeHelper.FindLogicalNode(page, "mylabel") as Label;
							mylabel.Content = DateTime.Now.ToLongTimeString();
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
		
        protected override string GetHeaderPart(string variable)
        {
            return "MarketWatch";
}

        protected override void Restore(XElement element)
        {
            throw new NotImplementedException();
        }

        protected override void Save(XElement element)
        {
            throw new NotImplementedException();
        }
    }
}