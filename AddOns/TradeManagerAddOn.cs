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

namespace NinjaTrader.Gui.NinjaScript
{
    public class TradeManagerAddOn : AddOnBase
    {
        private NTMenuItem _myMenuItem;
        private NTMenuItem _existingMenu;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "";
                Name = "TradeManager";
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
                Header = "Trade Manager",
                Style = Application.Current.TryFindResource("MainMenuItem") as Style
            };

            _existingMenu.Items.Add(_myMenuItem);
            _myMenuItem.Click += _myMenuItem_Click;
        }

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
            Output.Process("Pivots", PrintTo.OutputTab1);
            Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new TradeManagerWindow().Show()));
        }
    }

    public class TradeManagerWindow : NTWindow
    {
        
        public TradeManagerWindow()
        {
            Caption = "Trade Manager";
            Width = 1000;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var tabControl = new TabControl();
            TabControlManager.SetIsMovable(tabControl, false);
            TabControlManager.SetCanAddTabs(tabControl, false);
            TabControlManager.SetCanRemoveTabs(tabControl, false);
            tabControl.AddNTTabPage(new TradeManagerTab());

            TabControlManager.SetFactory(tabControl, new TradeManagerTabFactory());
            Content = tabControl;
        }
    }
    public class TradeManagerTabFactory : INTTabFactory
    {
        public NTWindow CreateParentWindow()
        {
            return new TradeManagerWindow();
        }

        public NTTabPage CreateTabPage(string typeName, bool isNewWindow = false)
        {
            return new TradeManagerTab();
        }
    }
    public class TradeManagerTab : NTTabPage
    {
        private IList<Account> accounts = new List<Account>();
       // private string[] accountNames = { "APEX-1338-235" };



        private Grid _pendingOrderGrid;
        private StackPanel _accountListStackPanel;
        private ListBox _accountListBox;

        private bool _testOnly = false;

        public TradeManagerTab()
        {
            Content = LoadXAML();
            lock (Account.All)
            {
                Account.All.ToList().ForEach(account => {
                    if(account.ConnectionStatus == ConnectionStatus.Connected)
                    {
                        if(account.LiquidationState != LiquidationState.Disabled)
                        {
                            CheckBox accountCheckBox = new CheckBox();
                            accountCheckBox.Content = account.Name;
                            //_accountListStackPanel.Children.Add(accountCheckBox);

                            _accountListBox.Items.Add(accountCheckBox);
                            //TextBlock p = new TextBlock();
                            //p.Text = account.Name;
                            //_accountListStackPanel.Children.Add(p);

                            //_accountListStackPanel.Children.Add(new Label { Content = account.Name });
                            string output = string.Format("-{0} {1}",
                                account.Name,
                                account.LiquidationState.ToString());
                            Output.Process(output, PrintTo.OutputTab1);
                        }
                    }
                });
                //StringBuilder sb = new StringBuilder();
                //accountNames.ToList().ForEach(accountName =>
                //{
                //    accountNames.Append(accountName);
                //    accounts.Add(Account.All.Where(a => a.Name == accountName).FirstOrDefault());
                //});

                //_accountListLabel.Content = sb.ToString();
                //if (_testOnly)
                //{
                //    accounts.Clear();
                //    accounts.Add(Account.All.FirstOrDefault(a => a.Name == "Sim101"));
                //}
            }
        }

        private DependencyObject LoadXAML()
        {
            try
            {
                using (var stream = GetManifestResourceStream("AddOns.TradeManagerUI.xaml"))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        var page = System.Windows.Markup.XamlReader.Load(streamReader.BaseStream) as Page;
                        DependencyObject pageContent = null;

                        if (page != null)
                        {
                            pageContent = page.Content as DependencyObject;
                            _pendingOrderGrid = LogicalTreeHelper.FindLogicalNode(page, "PendingOrdersGrid") as Grid;
                            if (_pendingOrderGrid != null) MockPendingOrders();

                            _accountListStackPanel = LogicalTreeHelper.FindLogicalNode(page, "AccountListStackPanel") as StackPanel;
                            _accountListBox = LogicalTreeHelper.FindLogicalNode(page, "AccountListBox") as ListBox;
                        }

                        return pageContent;
                    }
                }
            }
            catch (Exception e)
            {
                Output.Process(e.Message, PrintTo.OutputTab1);
                return null;
            }
        }

        private void MockPendingOrders()
        {
            for(var x = 0; x < 5; x++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                _pendingOrderGrid.RowDefinitions.Add(rowDefinition);

                int rowIndex = _pendingOrderGrid.RowDefinitions.Count;
                Label label = new Label { Content = string.Format("EURUS{0}", rowIndex) };
                Grid.SetRow(label, rowIndex);
                Grid.SetColumn(label, 0);

                _pendingOrderGrid.Children.Add(label);

                
            }
        }
        protected override string GetHeaderPart(string variable)
        {
            return "TradeManager";
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
