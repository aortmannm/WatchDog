using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Watchdog.Helper;

namespace Watchdog
{
    class MailSender
    {
        public static void Send(float ram, float cpu, List<DriveInfo> diskSummary, string subject, List<EventLogEntry> lastLogs)
        {
            MailAddress to = new MailAddress(Properties.Settings.Default.MailTo);
            MailAddress from = new MailAddress(Properties.Settings.Default.MailFrom);
            MailMessage mail = new MailMessage(Properties.Settings.Default.MailFrom, Properties.Settings.Default.MailTo);
           
            mail.Subject = subject;
            StringBuilder builder = new StringBuilder();
            builder.Append("Your CPU Usage: " +cpu+ "\n\nYour RAM Usage: " +ram+ "\n\n");

            foreach (DriveInfo disksToWrite in diskSummary)
            {
                builder.AppendLine(string.Format("{0} {1} GB Free Space of {2} GB Total Size;\n", disksToWrite.Name, ByteConverter.GetGigaBytesFromBytes(disksToWrite.TotalFreeSpace), ByteConverter.GetGigaBytesFromBytes(disksToWrite.TotalSize)));
            }

            foreach (EventLogEntry logsToWrite in lastLogs)
            {
                builder.AppendLine(string.Format("{0}; Event ID: {1}; {2}; {3};", logsToWrite.TimeGenerated, logsToWrite.InstanceId, logsToWrite.EntryType,
                                             logsToWrite.Message));
            }
            builder.AppendLine("\n\n" + subject);
            mail.Body = builder.ToString();
            SmtpClient smtp = new SmtpClient();
            smtp.Host = Properties.Settings.Default.MailSmtpServer;
            smtp.Port = Properties.Settings.Default.MailSmtpPort;

            smtp.Credentials = new NetworkCredential(
                Properties.Settings.Default.MailFrom, Properties.Settings.Default.MailPassword);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}
