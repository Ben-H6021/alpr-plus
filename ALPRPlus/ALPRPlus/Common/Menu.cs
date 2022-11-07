using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Rage;


namespace Stealth.Plugins.ALPRPlus.Common
{
    class Menu
    {
        public static MenuPool MenuPool;
        // Menus
        public static UIMenu CameraToggle;

        //Camera Toggle Menu
        private static UIMenuListItem EnableDriverFrontListItem;
        private static UIMenuListItem EnableDriverRearListItem;
        private static UIMenuListItem EnablePassengerFrontListItem;
        private static UIMenuListItem EnablePassengerRearListItem;
        private static UIMenuListItem EnableMobileANPRListItem;
        private static UIMenuListItem EnableMobileANPR2ListItem;
        private static UIMenuItem SaveMenuItem;
        internal static void Initialize()
        {
            MenuPool = new MenuPool();
            //Camera Toggle Menu
            CameraToggle = new UIMenu("ANPR Cam", "Camera Toggle");
            CameraToggle.AddItem(EnableDriverFrontListItem = new UIMenuListItem("Driver Front", "Toggle Driver Front Camera."));
            CameraToggle.AddItem(EnableDriverRearListItem = new UIMenuListItem("Driver Rear", "Toggle Driver Rear Camera."));
            CameraToggle.AddItem(EnablePassengerFrontListItem = new UIMenuListItem("Passenger Front", "Toggle Passenger Front Camera."));
            CameraToggle.AddItem(EnablePassengerRearListItem = new UIMenuListItem("Passenger Rear", "Toggle Passenger Rear Camera."));
            CameraToggle.AddItem((UIMenuItem)(Menu.EnableMobileANPRListItem = new UIMenuListItem("Mobile ANPR Cam", "Toggle 1st Mobile ANPR Camera.")));
            CameraToggle.AddItem((UIMenuItem)(Menu.EnableMobileANPR2ListItem = new UIMenuListItem("Mobile ANPR Cam2", "Toggle 2nd Mobile ANPR Camera.")));
            CameraToggle.AddItem(SaveMenuItem = new UIMenuItem("Save Configuration","Save Configuration to INI"));
            CameraToggle.OnItemSelect += OnOnItemSelect;
            EnableDriverFrontListItem.Collection = GetBooleanValues();
            EnableDriverRearListItem.Collection = GetBooleanValues();
            EnablePassengerFrontListItem.Collection = GetBooleanValues();
            EnablePassengerRearListItem.Collection = GetBooleanValues();
            EnableDriverFrontListItem.OnListChanged += OnBooleanListChanged;
            EnableDriverRearListItem.OnListChanged += OnBooleanListChanged;
            EnablePassengerFrontListItem.OnListChanged += OnBooleanListChanged;
            EnablePassengerRearListItem.OnListChanged += OnBooleanListChanged;
            EnableMobileANPR2ListItem.OnListChanged += OnBooleanListChanged;
            CameraToggle.MouseControlsEnabled = true;
            CameraToggle.AllowCameraMovement = false;
            SetSelectedSettings();
            MenuPool.Add(CameraToggle);
            GameFiber.StartNew(Process, $"Menu_Process-{Guid.NewGuid()}");

        }
        internal static bool process = false;
        private static void Process()
        {
            process = true;
            while (process)
            {
                if (Funcs.isKeyPressed(Config.MenuKey, Config.MenuKeyModifier))
                    {
                    CameraToggle.RefreshIndex();
                    CameraToggle.Visible = !CameraToggle.Visible;
                    }
                MenuPool.ProcessMenus();

                GameFiber.Yield();
            }
        }
        private static void SetSelectedSettings()
        {
            SetSelectedBooleanSettings();
        }
        private static void SetSelectedBooleanSettings()
        {
            EnableDriverFrontListItem.Index = Config.EnableDriverFrontCam ? 0 : 1;
            EnableDriverRearListItem.Index = Config.EnableDriverRearCam ? 0 : 1;
            EnablePassengerFrontListItem.Index = Config.EnablePassengerFrontCam ? 0 : 1;
            EnablePassengerRearListItem.Index = Config.EnablePassengerRearCam ? 0 : 1;
            EnableMobileANPRListItem.Index = Config.EnableMobileANPRCam ? 0 : 1;
            EnableMobileANPR2ListItem.Index = Config.EnableMobileANPRCam2 ? 0 : 1;
        }
        private static void OnBooleanListChanged(UIMenuItem sender, int newIndex)
        {
            if (sender == EnableDriverFrontListItem)
                Config.EnableDriverFrontCam = (bool)EnableDriverFrontListItem.SelectedValue;
            else if (sender == EnableDriverRearListItem)
                Config.EnableDriverRearCam = (bool)EnableDriverRearListItem.SelectedValue;
            else if (sender == EnablePassengerFrontListItem)
                Config.EnablePassengerFrontCam = (bool)EnablePassengerFrontListItem.SelectedValue;
            else if (sender == EnablePassengerRearListItem)
                Config.EnablePassengerRearCam = (bool)EnablePassengerRearListItem.SelectedValue;
            else if (sender == Menu.EnableMobileANPRListItem)
            {
                Config.EnableMobileANPRCam = (bool)EnableMobileANPRListItem.SelectedValue;
            }
            else
            {
                if (sender != EnableMobileANPR2ListItem)
                    return;
                Config.EnableMobileANPRCam2 = (bool)EnableMobileANPR2ListItem.SelectedValue;
            }
        }

        private static DisplayItemsCollection GetBooleanValues()
        {
            DisplayItemsCollection items = new DisplayItemsCollection();
            items.Add(true, "Activated");
            items.Add(false, "Deactivated");
            return items;
        }
        private static void OnOnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
                if (selectedItem == SaveMenuItem)
            {
                Config.SaveINI();
            }
        }

    }
}
