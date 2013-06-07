using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Watchdog.Command;
using Watchdog.Helper;
using ByteConverter = Watchdog.Helper.ByteConverter;


namespace Watchdog.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _alarmTimer;
        private bool _isWatching;
        private Thread _workerThread;
        
        public MainWindowViewModel()
        {
            Properties.Settings.Default.LastLogReadedDate = DateTime.Now;
            IsAutostart = Properties.Settings.Default.Autostart;
            MailStateButtonName = "Active Mail Service";
            AvailableDriveLetters = new CollectionView(GetAvailableDrives());

            _alarmTimer = new DispatcherTimer();
            _alarmTimer.Tick += new EventHandler(SendMailTick);
            _alarmTimer.Interval = TimeSpan.FromMinutes(Properties.Settings.Default.AlarmIntervalInMinutes);
            
        }

  

        #region Get/Set

        private string _mailStateButtonName;
        public string MailStateButtonName
        {
            get { 
                if(IsMailActive)
                {
                    return "Deactivate Mail Service";
                }
                return "Activate Mail Service";
            }
            set 
            { 
                _mailStateButtonName = value;
                OnPropertyChanged("MailStateButtonName");
            }
        }

        private DriveInfo _selectedDrive;
        public DriveInfo SelectedDrive
        {
            get { return _selectedDrive; }
            set { 
                _selectedDrive = value;
                UpdateDiskOnView(value);
                OnPropertyChanged("SelectedDrive");
            }
        }

        private CollectionView _availableDriveLetters;
        public CollectionView AvailableDriveLetters
        {
            get { return _availableDriveLetters; }
            set 
            {
                _availableDriveLetters = value;
                OnPropertyChanged("AvailableDriveLetters");
            }

        }
        private bool _abortPending = true;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isMailActive;
        public bool IsMailActive 
        {
            get { return _isMailActive; }
            set 
            { 
                _isMailActive = value;
                OnPropertyChanged("MailStateButtonName");
            }
        }

        private bool _isAutostart;

        public bool IsAutostart
         {
            get
            {
                return _isAutostart;
            }
            set
            {
                _isAutostart = value;
                Properties.Settings.Default.Autostart = value;
                Properties.Settings.Default.Save();
                if(value)
                {
                    ToggleMailServiceState();
                    StartMonitoring();
                }
                OnPropertyChanged("IsAutostart");
            }
        }


        private PerformanceCounter _cpuCounter;
        private string _cpuUsage;
        public string CpuUsage
        { 
            get
            {
                return _cpuUsage;
            } 
            set
            {
                _cpuUsage = value;
                OnPropertyChanged("CpuUsage");
            }
        }

        private float GetCpuUsage()
        {
            return _cpuCounter.NextValue();
        }

        private PerformanceCounter _ramCounter;
        private string _ramUsage;
        public string RamUsage
        {
            get
            {
                return _ramUsage;
            }
            set
            {
                _ramUsage = value;
                OnPropertyChanged("RamUsage");
            }
        }

        private float GetRamUsage()
        {
            return _ramCounter.NextValue();
        }

        private long _diskUsage;
        public long DiskUsage
        {
            get
            {
                return _diskUsage;
            }
            set
            {
                _diskUsage = value;
                OnPropertyChanged("DiskUsage");
            }
        }

        private long _diskUsageBar;
        public long DiskUsageBar
        {
            get
            {
                return _diskUsageBar;
            }
            set
            {
                _diskUsageBar = value;
                OnPropertyChanged("DiskUsageBar");
            }
        }

        private long _eventLog;
        public long EventLog
        {
            get
            {
                return _eventLog;
            }
            set
            {
                _eventLog = value;
                OnPropertyChanged("EventLog");
            }
        }

#endregion 

        #region commands

        private DelegateCommand _startMonitoringCommand;
        public ICommand StartMonitoringCommand
        {
            get
            {
                if (_startMonitoringCommand == null)
                {
                    _startMonitoringCommand = new DelegateCommand(StartMonitoring);
                }
                return _startMonitoringCommand;
            }
        }

        private DelegateCommand _stopMonitoringCommand;
        public ICommand StopMonitoringCommand
        {
            get
            {
                if (_stopMonitoringCommand == null)
                {
                    _stopMonitoringCommand = new DelegateCommand(StopMonitoring);
                }
                return _stopMonitoringCommand;
            }
        }

        private DelegateCommand _changeMailserviceStateCommand;
        public ICommand ChangeMailserviceStateStateCommand
        {
            get
            {
                if (_changeMailserviceStateCommand == null)
                {
                    _changeMailserviceStateCommand = new DelegateCommand(ToggleMailServiceState);
                }
                return _changeMailserviceStateCommand;
            }
        }
        #endregion 
        

        public void StartMonitoring()
        {
            _isWatching = true;
            _abortPending = false;
            _workerThread = new Thread(MonitoringThreadMain);
            _workerThread.Start();
        }

        private void MonitoringThreadMain()
        {
            InitialisePerformanceCounter();
            
            while (!_abortPending)
            {
                CpuUsage = GetCpuUsage().ToString("0.00");
                RamUsage = GetRamUsage().ToString("0.00");
                
                Thread.Sleep(500);
                
                if (_isMailActive)
                {
                    if(CheckForAlertOrNot())
                    {
                        try
                        {
                            PrepareMailSending();
                        }
                        catch (Exception sendMailFailed)
                        {

                            CantSendMail(sendMailFailed.ToString());
                        }
                    }
                    if(!_alarmTimer.IsEnabled)
                    {
                        _alarmTimer.Start();
                        try
                        {
                            PrepareMailSending();
                        }
                        catch (Exception sendMailFailed)
                        {
                            CantSendMail(sendMailFailed.ToString());
                        }
                    }
                }
            }
        }

        private static void CantSendMail(string sendMailFailed)
        {
            MessageBox.Show(sendMailFailed);
        }

        private void SendMailTick(object sender, EventArgs e)
        {
            try
            {
                PrepareMailSending();
            }
            catch (Exception sendMailFailed)
            {
                CantSendMail(sendMailFailed.ToString());
            }
        }

        public void UpdateDiskOnView(DriveInfo driver)
        {
            try
            {
                DiskUsage = ByteConverter.GetGigaBytesFromBytes(driver.TotalFreeSpace);
                DiskUsageBar = 100 - (100 * (long)driver.TotalFreeSpace / driver.TotalSize);
            }
            catch (Exception)
            {
                DiskUsage = 0;
            }
        }

        private void InitialisePerformanceCounter()
        {
            _cpuCounter = new PerformanceCounter();
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";

            _ramCounter = new PerformanceCounter();
            _ramCounter.CounterName = "% Committed Bytes in Use";
            _ramCounter.CategoryName = "Memory";
        }

        private void PrepareMailSending()
        {
            String alarmText;
            if (CheckForAlertOrNot())
            {
                alarmText = Properties.Settings.Default.AlarmText;
               
            } else
            {
                alarmText = Properties.Settings.Default.FineText;
            }
            MailSender.Send(_ramCounter.NextValue(), _cpuCounter.NextValue(), GetAvailableDrives(), alarmText, GetLastEventLogs());
            Properties.Settings.Default.LastLogReadedDate = DateTime.Now;
        }

        private List<DriveInfo> GetAvailableDrives()
        {
            var listOfDrives = new List<DriveInfo>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady && drive.DriveType.ToString() == "Fixed")
                {
                    listOfDrives.Add(drive);
                }
            }
            
            return listOfDrives;
        }

        private bool CheckForAlertOrNot()
        {

            List<DriveInfo> drivesWithLessSpace = ExtractLessSpaceDrives();
            List<EventLogEntry> lastEventLogList = GetLastEventLogs();

            return _ramCounter.NextValue() > Properties.Settings.Default.AlarmRamPercentage || drivesWithLessSpace.Count > 0 || (lastEventLogList.Count > 0);
        }

        private List<DriveInfo> ExtractLessSpaceDrives()
        {
            var drivesWithLessSpace = new List<DriveInfo>();
            foreach (DriveInfo testDrive in GetAvailableDrives())
            {
                if(ByteConverter.GetGigaBytesFromBytes(testDrive.TotalFreeSpace) <= Properties.Settings.Default.AlarmDiskFreeSpace)
                {
                    drivesWithLessSpace.Add(testDrive);
                }
                
            }
            return drivesWithLessSpace;
        }

        private static List<EventLogEntry> GetLastEventLogs()
        {
            var myLog = new EventLog("System");
            var entryCollection = myLog.Entries;
            return new ErrorOrWarningFilter().GetFilteredList(entryCollection);
        }

        private string _eventLogMessage;
        private DriveInfo[] _drive;
            

        public string EventLogMessage
        {
            get
            {
                return _eventLogMessage;
            }
            set
            {
                _eventLogMessage = value;
                OnPropertyChanged("EventLogMessage");
            }
        }

       
        public void ToggleMailServiceState()
        {
            IsMailActive = !IsMailActive;
        }

      

        public void StopMonitoring()
        {
            IsMailActive = false;
            MailStateButtonName = "Active Mail Service";
            _isWatching = false;
            _abortPending = true;

            var terminated = _workerThread.Join(TimeSpan.FromSeconds(2));
            if (!terminated)
                _workerThread.Abort();

            CpuUsage = "0";
            RamUsage = "0";
        }

        
    }
}
