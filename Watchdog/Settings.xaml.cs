using System;
using System.Windows;

namespace Watchdog
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            MailToBox.Text = Properties.Settings.Default.MailTo;
            MailFromBox.Text = Properties.Settings.Default.MailFrom;
            MailSmtpPortBox.Text = Properties.Settings.Default.MailSmtpPort.ToString();
            MailPasswordBox.Password = Properties.Settings.Default.MailPassword;
            MailSmtpServerBox.Text = Properties.Settings.Default.MailSmtpServer;

            AlarmRamPercentageBox.Text = Properties.Settings.Default.AlarmRamPercentage.ToString();
            AlarmDiskFreeSpaceBox.Text = Properties.Settings.Default.AlarmDiskFreeSpace.ToString();
            AlarmIntervalBox.Text = Properties.Settings.Default.AlarmIntervalInMinutes.ToString();
        }

        private void CloseWithoutChange(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveChange(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(AlarmRamPercentageBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.AlarmRamPercentage = float.Parse(AlarmRamPercentageBox.Text);
            }
            if (String.IsNullOrEmpty(AlarmDiskFreeSpaceBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.AlarmDiskFreeSpace = int.Parse(AlarmDiskFreeSpaceBox.Text);
            }
            if (String.IsNullOrEmpty(AlarmIntervalBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.AlarmIntervalInMinutes = int.Parse(AlarmIntervalBox.Text);
            }
            if (String.IsNullOrEmpty(MailSmtpPortBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.MailSmtpPort = int.Parse(MailSmtpPortBox.Text);
            }

            if(String.IsNullOrEmpty(MailToBox.Text))
            {
            } else
            {
                Properties.Settings.Default.MailTo = MailToBox.Text;
            }
            if (String.IsNullOrEmpty(MailFromBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.MailFrom = MailFromBox.Text;
            }
            if (String.IsNullOrEmpty(MailPasswordBox.Password))
            {
            }
            else
            {
                Properties.Settings.Default.MailPassword = MailPasswordBox.Password;
            }
            if (String.IsNullOrEmpty(MailSmtpServerBox.Text))
            {
            }
            else
            {
                Properties.Settings.Default.MailSmtpServer = MailSmtpServerBox.Text;
            }

            Close();
        }
    }
}
