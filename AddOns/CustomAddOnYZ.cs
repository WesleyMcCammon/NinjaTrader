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
        private Order entryOrder;

        private Button _btnESBuyStop;
        private Button _btnESSellStop;
        private Button _btnYMBuyStop;
        private Button _btnYMSellStop;
        private Button _btnCLBuyStop;
        private Button _btnCLSellStop;
        private Button _btnGCBuyStop;
        private Button _btnGCSellStop;
        private Label _lblAtmStrategy;
        private CheckBox _cbAccount1;
        private CheckBox _cbAccount2;
        private CheckBox _cbAccount3;
        private Button _btnESBuyMarket;
        private Button _btnESSellMarket;
        private Button _btnYMBuyMarket;
        private Button _btnYMSellMarket;
        private Button _btnCLBuyMarket;
        private Button _btnCLSellMarket;
        private Button _btnGCBuyMarket;
        private Button _btnGCSellMarket;

        private IList<Account> accounts = new List<Account>();
        private string[] accountNames = { "LL012241-603", "LL012241-604", "LL012241-605" }; 
        private string ES_CONTRACT = "ES 09-22";
        private string YM_CONTRACT = "YM 09-22";
        private string CL_CONTRACT = "CL 10-22";
        private string GC_CONTRACT = "GC 12-22";
        private double ES_OFFSET = .75;
        private double YM_OFFSET = 3;
        private double GC_OFFSET = .2;
        private double CL_OFFSET = .04;

        private bool _testOnly = false;

        public YZTab()
        {
            Content = LoadXAML();

            // Find our Sim101 account
            lock (Account.All)
            {
                accountNames.ToList().ForEach(accountName =>
                {
                    accounts.Add(Account.All.Where(a => a.Name == accountName).FirstOrDefault());
                });

                if(_testOnly)
                {
                    accounts.Clear();
                    accounts.Add(Account.All.FirstOrDefault(a => a.Name == "Sim101"));
                }
                // account = Account.All.FirstOrDefault(a => a.Name == "Sim101");
            }

            //var sb = new StringBuilder();
            //Account.All.ToList().ForEach(a => { sb.AppendLine(a.Name); });
            //Output.Process(sb.ToString(), PrintTo.OutputTab1);
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

                            _btnESBuyStop = LogicalTreeHelper.FindLogicalNode(page, "btnESBuyStop") as Button;
                            _btnESSellStop = LogicalTreeHelper.FindLogicalNode(page, "btnESSellStop") as Button;
                            _btnYMBuyStop = LogicalTreeHelper.FindLogicalNode(page, "btnYMBuyStop") as Button;
                            _btnYMSellStop = LogicalTreeHelper.FindLogicalNode(page, "btnYMSellStop") as Button;
                            _btnCLBuyStop = LogicalTreeHelper.FindLogicalNode(page, "btnCLBuyStop") as Button;
                            _btnCLSellStop = LogicalTreeHelper.FindLogicalNode(page, "btnCLSellStop") as Button;
                            _btnGCBuyStop = LogicalTreeHelper.FindLogicalNode(page, "btnGCBuyStop") as Button;
                            _btnGCSellStop = LogicalTreeHelper.FindLogicalNode(page, "btnGCSellStop") as Button;

                            _btnESBuyMarket = LogicalTreeHelper.FindLogicalNode(page, "btnESBuyMarket") as Button;
                            _btnESSellMarket = LogicalTreeHelper.FindLogicalNode(page, "btnESSellMarket") as Button;
                            _btnYMBuyMarket = LogicalTreeHelper.FindLogicalNode(page, "btnYMBuyMarket") as Button;
                            _btnYMSellMarket = LogicalTreeHelper.FindLogicalNode(page, "btnYMSellMarket") as Button;
                            _btnCLBuyMarket = LogicalTreeHelper.FindLogicalNode(page, "btnCLBuyMarket") as Button;
                            _btnCLSellMarket = LogicalTreeHelper.FindLogicalNode(page, "btnCLSellMarket") as Button;
                            _btnGCBuyMarket = LogicalTreeHelper.FindLogicalNode(page, "btnGCBuyMarket") as Button;
                            _btnGCSellMarket = LogicalTreeHelper.FindLogicalNode(page, "btnGCSellMarket") as Button;

                            _lblAtmStrategy = LogicalTreeHelper.FindLogicalNode(page, "lblAtmStrategy") as Label;

                            if (_lblAtmStrategy != null)
                            {
                                _lblAtmStrategy.Content = "2-300-500";
                            }

                            #region ES
                            if (_btnESBuyStop != null)
                            {
                                _btnESBuyStop.Content = string.Format("Buy Stop {0}", ES_CONTRACT);
                                _btnESBuyStop.Click += btnESBuyStopClick;
                            }

                            if (_btnESSellStop != null)
                            {
                                _btnESSellStop.Content = string.Format("Sell Stop {0}", ES_CONTRACT);
                                _btnESSellStop.Click += btnESSellStopClick;
                            }

                            if(_btnESBuyMarket != null)
                            {
                                _btnESBuyMarket.Content = string.Format("Buy Market {0}", ES_CONTRACT);
                                _btnESBuyMarket.Click += _btnESBuyMarket_Click;
                            }

                            if (_btnESSellMarket != null)
                            {
                                _btnESSellMarket.Content = string.Format("Sell Market {0}", ES_CONTRACT);
                                _btnESSellMarket.Click += _btnESSellMarket_Click;
                            }
                            #endregion

                            #region YM
                            if (_btnYMBuyStop != null)
                            {
                                _btnYMBuyStop.Content = string.Format("Buy Stop {0}", YM_CONTRACT);
                                _btnYMBuyStop.Click += btnYMBuyStopClick;
                            }

                            if(_btnYMBuyMarket != null)
                            {
                                _btnYMBuyMarket.Content = string.Format("Buy Market {0}", YM_CONTRACT);
                                _btnYMBuyMarket.Click += _btnYMBuyMarket_Click;
                            }

                            if (_btnYMSellStop != null)
                            {
                                _btnYMSellStop.Content = string.Format("Sell Stop {0}", YM_CONTRACT);
                                _btnYMSellStop.Click += btnYMSellStopClick;
                            }

                            if(_btnYMSellMarket != null)
                            {
                                _btnYMSellMarket.Content = string.Format("Sell Market {0}", YM_CONTRACT);
                                _btnYMSellMarket.Click += _btnYMSellMarket_Click;
                            }
                            #endregion

                            #region CL
                            if (_btnCLBuyStop != null)
                            {
                                _btnCLBuyStop.Content = string.Format("Buy Stop {0}", CL_CONTRACT);
                                _btnCLBuyStop.Click += btnCLBuyStopClick;
                            }

                            if (_btnCLSellStop != null)
                            {
                                _btnCLSellStop.Content = string.Format("Sell Stop {0}", CL_CONTRACT);
                                _btnCLSellStop.Click += btnCLSellStopClick;
                            }

                            if(_btnCLBuyMarket != null)
                            {
                                _btnCLBuyMarket.Content = string.Format("Buy Market {0}", CL_CONTRACT);
                                _btnCLBuyMarket.Click += _btnCLBuyMarket_Click;
                            }

                            if (_btnCLSellMarket != null)
                            {
                                _btnCLSellMarket.Content = string.Format("Sell Market {0}", CL_CONTRACT);
                                _btnCLSellMarket.Click += _btnCLSellMarket_Click;
                            }
                            #endregion

                            #region GC
                            if (_btnGCBuyStop != null)
                            {
                                _btnGCBuyStop.Content = string.Format("Buy Stop {0}", GC_CONTRACT);
                                _btnGCBuyStop.Click += btnGCBuyStopClick;
                            }

                            if (_btnGCSellStop != null)
                            {
                                _btnGCSellStop.Content = string.Format("Sell Stop {0}", GC_CONTRACT);
                                _btnGCSellStop.Click += btnGCSellStopClick;
                            }

                            if(_btnGCBuyMarket != null)
                            {
                                _btnGCBuyMarket.Content = string.Format("Buy Market {0}", GC_CONTRACT);
                                _btnGCBuyMarket.Click += _btnGCBuyMarket_Click;
                            }

                            if(_btnGCSellMarket != null)
                            {
                                _btnGCSellMarket.Content = string.Format("Sell Market {0}", GC_CONTRACT);
                                _btnGCSellMarket.Click += _btnGCSellMarket_Click;
                            }
                            #endregion
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
            accounts.ToList().ForEach(account =>
            {                
                entryOrder = account.CreateOrder(instrument, orderAction, orderType, TimeInForce.Day, quantity, 0, stopPrice, 
                    string.Empty, "Entry", null);

                // Submits our entry order with the ATM strategy named "myAtmStrategyName"
                NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy("2-300-500", entryOrder);
            });
        }

        #region ES Click Events
        private void btnESBuyStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(ES_CONTRACT);
            double stopPrice = marketPrice + ES_OFFSET;

            SendOrder(Instrument.GetInstrument(ES_CONTRACT), OrderAction.Buy, OrderType.StopMarket, 2, stopPrice);
        }

        private void btnESSellStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(ES_CONTRACT);
            double stopPrice = marketPrice - ES_OFFSET;

            SendOrder(Instrument.GetInstrument(ES_CONTRACT), OrderAction.Sell, OrderType.StopMarket, 2, stopPrice);
        }

        private void _btnESBuyMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(ES_CONTRACT), OrderAction.Buy, OrderType.Market, 2);
        }

        private void _btnESSellMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(ES_CONTRACT), OrderAction.Sell, OrderType.Market, 2);
        }

        #endregion

        #region YM Click Events
        private void btnYMBuyStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(YM_CONTRACT);
            double stopPrice = marketPrice + YM_OFFSET;

            SendOrder(Instrument.GetInstrument(YM_CONTRACT), OrderAction.Buy, OrderType.StopMarket, 2, stopPrice);
        }

        private void btnYMSellStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(YM_CONTRACT);
            double stopPrice = marketPrice - YM_OFFSET;

            SendOrder(Instrument.GetInstrument(YM_CONTRACT), OrderAction.Sell, OrderType.StopMarket, 2, stopPrice);
        }

        private void _btnYMBuyMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(YM_CONTRACT), OrderAction.Buy, OrderType.Market, 2);
        }

        private void _btnYMSellMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(YM_CONTRACT), OrderAction.Sell, OrderType.Market, 2);
        }

        #endregion

        #region CL Click Events
        private void btnCLBuyStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(CL_CONTRACT);
            double stopPrice = marketPrice + CL_OFFSET;

            SendOrder(Instrument.GetInstrument(CL_CONTRACT), OrderAction.Buy, OrderType.StopMarket, 2, stopPrice);
        }
        private void btnCLSellStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(CL_CONTRACT);
            double stopPrice = marketPrice - CL_OFFSET;

            SendOrder(Instrument.GetInstrument(CL_CONTRACT), OrderAction.Sell, OrderType.StopMarket, 2, stopPrice);
        }

        private void _btnCLSellMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(CL_CONTRACT), OrderAction.Buy, OrderType.Market, 2);
        }

        private void _btnCLBuyMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(CL_CONTRACT), OrderAction.Sell, OrderType.Market, 2);
        }

        #endregion

        #region GC Click Events

        private void btnGCBuyStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(GC_CONTRACT);
            double stopPrice = marketPrice + GC_OFFSET;

            SendOrder(Instrument.GetInstrument(GC_CONTRACT), OrderAction.Buy, OrderType.StopMarket, 2, stopPrice);
        }
        private void btnGCSellStopClick(object sender, RoutedEventArgs e)
        {
            double marketPrice = GetMarketPrice(GC_CONTRACT);
            double stopPrice = marketPrice - GC_OFFSET;

            SendOrder(Instrument.GetInstrument(GC_CONTRACT), OrderAction.Sell, OrderType.StopMarket, 2, stopPrice);
        }
        private void _btnGCBuyMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(GC_CONTRACT), OrderAction.Buy, OrderType.Market, 2);
        }
        private void _btnGCSellMarket_Click(object sender, RoutedEventArgs e)
        {
            SendOrder(Instrument.GetInstrument(GC_CONTRACT), OrderAction.Sell, OrderType.Market, 2);
        }

        #endregion
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
