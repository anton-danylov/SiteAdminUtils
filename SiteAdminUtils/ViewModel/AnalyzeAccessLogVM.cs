using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SiteAdminUtils.Core;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace SiteAdminUtils.ViewModel
{
    public class LogItemVM : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }


        private string _fullPath;
        public string FullPath
        {
            get { return _fullPath; }
            set { Set(ref _fullPath, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                Broadcast(null, this, nameof(IsSelected));
            }
        }
    }

    public class ApacheLogItem
    {
        public string Ip { get; set; }
        public DateTimeOffset DateOffset { get; set; }
        public string Request { get; set; }
        public int Response { get; set; }
        public int BytesSent { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        public string FromLog { get; set; }
    }

    public class AnalyzeAccessLogVM : ViewModelBase
    {
        public string Name { get; private set; } = "Analyze Access Log";

        //public static readonly string DefaultLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private const string TempFolder = "SiteAdminUtils";
        private const string LogsFolder = "Logs";

        private ApacheLogAnalyser _logAnalyser = new ApacheLogAnalyser();


        public RelayCommand DownloadLogsCommand { get; private set; }
        public RelayCommand ProcessSelectedLogsCommand { get; private set; }

        public ObservableCollection<LogItemVM> DownloadedLogItems { get; private set; } = new ObservableCollection<LogItemVM>();

        public ObservableCollection<ApacheLogItem> ProcessedLogEntries { get; private set; } = new ObservableCollection<ApacheLogItem>();


        private int _processedLinesCount;
        public int ProcessedLinesCount
        {
            get { return _processedLinesCount; }
            set { Set(ref _processedLinesCount, value); }
        }

        private DateTime _logTimeStart;
        public DateTime LogTimeStart
        {
            get { return _logTimeStart; }
            set { Set(ref _logTimeStart, value); }
        }

        private DateTime _logTimeEnd;
        public DateTime LogTimeEnd
        {
            get { return _logTimeEnd; }
            set { Set(ref _logTimeEnd, value); }
        }

        public bool? IsSelectedAllLogs
        {
            get
            {
                int selectedCount = DownloadedLogItems.Count(vm => vm.IsSelected);
                return (selectedCount == DownloadedLogItems.Count) ? (bool?)true : (selectedCount == 0) ? (bool?)false : null;
            }
            set
            {
                DownloadedLogItems.ToList().ForEach(vm => vm.IsSelected = value ?? vm.IsSelected);
            }
        }


        public string TempFolderPath => Path.Combine(Path.GetTempPath(), TempFolder);
        public string LogsFolderPath => Path.Combine(TempFolderPath, LogsFolder);


        public AnalyzeAccessLogVM()
        {
            MessengerInstance.Register<PropertyChangedMessage<LogItemVM>>(this, m =>
            {
                RaisePropertyChanged(nameof(IsSelectedAllLogs));
            });

            DownloadLogsCommand = new RelayCommand(DownloadLogsFromServer, () => true);
            ProcessSelectedLogsCommand = new RelayCommand(ProcessSelectedLogs, () => true);

            AutoMapper.Mapper.Initialize(c => c.CreateMap<Core.ApacheLogEntry, ApacheLogItem>());
        
            FillDownloadedLogItems();
        }

        private void ProcessSelectedLogs()
        {
            _logAnalyser.Reset();

            var selectedFiles = DownloadedLogItems.Where(vm => vm.IsSelected).Select(vm => vm.FullPath);

            foreach (var file in selectedFiles)
            {
                _logAnalyser.ProcessFile(file);
            }

            ProcessedLinesCount = _logAnalyser.ItemsCount;
            LogTimeStart = _logAnalyser.ParsedLogEntries.Min(le => le.DateOffset).ToLocalTime().DateTime;
            LogTimeEnd = _logAnalyser.ParsedLogEntries.Max(le => le.DateOffset).ToLocalTime().DateTime;



            //var errors =_logAnalyser.ParsedLogEntries.Where(le => le.Response == 500).ToList();

            //foreach (var entry in _logAnalyser.ParsedLogEntries)
            //{
            //    ProcessedLogEntries.Add(AutoMapper.Mapper.Map<ApacheLogItem>(entry));
            //}
        }

        public void ClearTempFolder(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
        }

        private void FillDownloadedLogItems()
        {
            DownloadedLogItems.Clear();


            Directory.EnumerateFiles(LogsFolderPath)
            .Select(p => new LogItemVM() { FullPath = p, Name = Path.GetFileName(p), IsSelected = true })
            .ToList()
            .ForEach(vm => DownloadedLogItems.Add(vm));
        }

        public void DownloadLogsFromServer()
        {
            var url = ConfigurationManager.AppSettings["ArchivedLogsUrl"];
            var uri = new Uri(url);

            if (!Directory.Exists(TempFolderPath))
            {
                Directory.CreateDirectory(TempFolderPath);
            }
            else
            {
                ClearTempFolder(TempFolderPath);
            }

            var archivePath = Path.Combine(TempFolderPath, "logs.zip");

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) => { /*e.ProgressPercentage;*/ };
                client.DownloadFile(uri, archivePath);
            }

            if (!Directory.Exists(LogsFolderPath))
            {
                Directory.CreateDirectory(LogsFolderPath);
            }
            else
            {
                ClearTempFolder(LogsFolderPath);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, LogsFolderPath);

            FillDownloadedLogItems();
        }

    }
}