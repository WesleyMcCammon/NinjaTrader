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
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.Gui.NinjaScript
{
    public class PivotAddOn : AddOnBase
    {
        private NTMenuItem _myMenuItem;
        private NTMenuItem _existingMenu;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "";
                Name = "Pivots";
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
                Header = "Pivots",
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
            Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new PivotTableWindow().Show()));
        }
    }
    public class PivotTableWindow : NTWindow
    {
        public PivotTableWindow()
        {
            Caption = "Pivots";
            Width = 1000;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var tabControl = new TabControl();
            TabControlManager.SetIsMovable(tabControl, true);
            TabControlManager.SetCanAddTabs(tabControl, true);
            TabControlManager.SetCanRemoveTabs(tabControl, true);
            tabControl.AddNTTabPage(new PivotTableTab());

            TabControlManager.SetFactory(tabControl, new PivotTableTabFactory());
            Content = tabControl;
        }
    }
    public class PivotTableTabFactory : INTTabFactory
    {
        public NTWindow CreateParentWindow()
        {
            return new PivotTableWindow();
        }

        public NTTabPage CreateTabPage(string typeName, bool isNewWindow = false)
        {
            return new PivotTableTab();
        }
    }
    public class PivotTableTab : NTTabPage
    {
        public PivotTableTab()
        {
            Content = LoadXAML();
        }

        private DependencyObject LoadXAML()
        {
            try
            {
                using (var stream = GetManifestResourceStream("AddOns.PivotTableTab.xaml"))
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
                Output.Process(e.Message, PrintTo.OutputTab1);
                return null;
            }
        }
        protected override string GetHeaderPart(string variable)
        {
            return "Pivots";
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
