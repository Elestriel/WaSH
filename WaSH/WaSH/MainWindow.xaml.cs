using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WaSH
{
    public partial class MainWindow : Window
    {
        // Systen Tray Components
        NotifyIcon SystemTrayIcon;
        ContextMenu SystemTrayContextMenu;

        // Hotkey Manager
        HotkeyManager HotkeyManager;
        uint HotkeyCode;
        uint HotkeyModifiers;
        uint NewHotkeyCode;
        uint NewHotkeyModifiers;

        public MainWindow()
        {
            InitializeComponent();

            // Minimize to Tray stuff
            SystemTrayContextMenu = this.FindResource("TrayContextMenu") as ContextMenu;
            SystemTrayIcon = new NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/WaSH.ico", UriKind.Relative)).Stream;
            SystemTrayIcon.Icon = new System.Drawing.Icon(iconStream);
            SystemTrayIcon.Visible = true;
            SystemTrayIcon.MouseClick += SystemTrayIcon_MouseClick;
            SystemTrayIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)args;
                    if (me.Button == MouseButtons.Left)
                    {
                        this.Show();
                        this.WindowState = WindowState.Normal;
                    }
                };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start Hotkey Manager
            HotkeyManager = new HotkeyManager(new WindowInteropHelper(this).Handle);

            bool parseHotkeyResult = TryReadSettingAsUint("HotkeyValue", out HotkeyCode);
            bool parseModifiersResult = TryReadSettingAsUint("HotkeyModifiers", out HotkeyModifiers);

            if (parseHotkeyResult && parseModifiersResult)
            {
                HotkeyManager.Listen(new WindowInteropHelper(this).Handle, HotkeyCode, HotkeyModifiers);

                UpdateText(HotkeyCode, HotkeyModifiers);
            }
        }

        private void UpdateText(uint code, uint mods)
        {
            string text = "";
            if ((mods & 0x0001) > 0) text += "Alt + ";
            if ((mods & 0x0002) > 0) text += "Control + ";
            if ((mods & 0x0004) > 0) text += "Shift + ";
            if ((mods & 0x0008) > 0) text += "Windows + ";

            Keys k = (Keys)code;
            text += k.ToString();

            HotkeyTextBox.Text = $"{text}\r\n\r\n(KeyCode: {mods}+{code})";
        }

        private void HotkeyTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotkeyManager.StopListening(new WindowInteropHelper(this).Handle);

            string keys = "";
            NewHotkeyCode = 0;
            NewHotkeyModifiers = 0;

            if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0) { NewHotkeyModifiers += 0x0001; }
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0) { NewHotkeyModifiers += 0x0002; }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0) { NewHotkeyModifiers += 0x0004; }
            if ((Keyboard.Modifiers & ModifierKeys.Windows) > 0) { NewHotkeyModifiers += 0x0008; }

            List<string> KeysToIgnore = new()
            {
                "LeftCtrl", "RightCtrl",
                "LeftShift", "RightShift",
                "LeftAlt", "RightAlt",
                "LWin", "RWin"
            };
            if (!KeysToIgnore.Contains(e.Key.ToString()))
            {
                keys += e.Key;
                Keys formsKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
                NewHotkeyCode += (uint)formsKey;
            }

            UpdateText(NewHotkeyCode, NewHotkeyModifiers);

            HotkeyTextBox.Background = new SolidColorBrush(Colors.Pink);
        }

        /// <summary>
        /// Handle minimizing to tray
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) { this.Hide(); }

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Handle tray icon interaction
        /// </summary>
        void SystemTrayIcon_MouseClick(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                SystemTrayContextMenu.PlacementTarget = sender as System.Windows.Controls.Button;
                SystemTrayContextMenu.IsOpen = true;
                this.Activate();
            }
        }

        private void PauseResumeHotkeys(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            HotkeyCode = NewHotkeyCode;
            HotkeyModifiers = NewHotkeyModifiers;
            NewHotkeyCode = 0;
            NewHotkeyModifiers = 0;
            HotkeyTextBox.Background = new SolidColorBrush(Colors.LightGreen);

            AddUpdateAppSettings("HotkeyValue", HotkeyCode);
            AddUpdateAppSettings("HotkeyModifiers", HotkeyModifiers);

            HotkeyManager.StopListening(new WindowInteropHelper(this).Handle);
            HotkeyManager.Listen(new WindowInteropHelper(this).Handle, HotkeyCode, HotkeyModifiers); ;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotkeyManager.StopListening(new WindowInteropHelper(this).Handle);
        }

        private void CloseMainWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region ConfigManager
        static bool TryReadSettingAsUint(string key, out uint result)
        {
            result = 0;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string readResult = appSettings[key] ?? "Not Found";
                if (uint.TryParse(readResult, out uint res)) 
                {
                    result = res;
                    return true;
                }
            }
            catch (ConfigurationErrorsException)
            {
                Trace.WriteLine("Error reading app settings");
            }

            return false;
        }

        static bool AddUpdateAppSettings(string key, uint value)
        {
            return AddUpdateAppSettings(key, value.ToString());
        }

        static bool AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Trace.WriteLine("Error writing app settings");
            }

            return false;
        }
        #endregion ConfigManager
    }
}