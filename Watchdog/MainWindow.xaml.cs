using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using Watchdog.ViewModel;
using System.Drawing;





namespace Watchdog
{

    public partial class MainWindow : Window
    {
 


        private NotifyIcon icon;
        public MainWindow()
        {
            InitializeComponent();

            icon = new NotifyIcon();
            icon.Icon = Properties.Resources.watchdog;
            icon.Visible = true;
            icon.Text = "WatchDog";
            icon.DoubleClick +=
                delegate(object sender, EventArgs args)
                    {
                        Show();
                        WindowState = WindowState.Normal;
                    };
        }

        private void CloseProgramClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        public void StartMonitoringClick(object sender, RoutedEventArgs e)
        {
            SetValue("WatchDog", "I'm watching your System", "WatchDog - Watching");
        }

        private void SetValue(string titleText, string balloonText, string text)
        {
            icon.BalloonTipTitle = titleText;
            icon.BalloonTipText = balloonText;
            icon.Text = text;
            icon.ShowBalloonTip(1);
        }

        private void StopWatchingClick(object sender, RoutedEventArgs e)
        {
            SetValue("WatchDog", "Stopped watching your System", "WatchDog - Not watching");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            SetValue("WatchDog", "I'm still alive, just in tray", "");
        }

        private void ChangeSettings(object sender, RoutedEventArgs e)
        {
            var settings = new Settings();
            settings.Show();
        }
        public void StartMailServiceClick(object sender, RoutedEventArgs e)
        {
            SetValue("WatchDog", "MailService active", "WatchDog - Watching Mail active");
        }

    }
}

