using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Watchdog.Helper
{
    class ErrorOrWarningFilter
    {
        public List<EventLogEntry> GetFilteredList(EventLogEntryCollection entryCollection)
        {
            var resultList = new List<EventLogEntry>();
            foreach (EventLogEntry entry in entryCollection)
            {
                if (IsWarningOrErrorAndInTimeOfRunning(entry))
                {
                    resultList.Add(entry);
                }
            }
            return resultList;
        }

        private bool IsWarningOrErrorAndInTimeOfRunning(EventLogEntry entry)
        {
            var errorType = entry.EntryType.ToString();
            return (IsErrorOrWarning(errorType) && WasNotAlreadySent(entry) && isNotInFuture(entry));
        }

        private bool isNotInFuture(EventLogEntry entry)
        {
            return (DateTime.Compare(entry.TimeGenerated, DateTime.Now) < 0);
        }

        private static bool WasNotAlreadySent(EventLogEntry entry)
        {
            return (DateTime.Compare(entry.TimeGenerated, Properties.Settings.Default.LastLogReadedDate) > 0);
        }

        private static bool IsErrorOrWarning(string errorType)
        {
            return (errorType.Equals("Error") || errorType.Equals("Warning"));
        }
    }
}

    