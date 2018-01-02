using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SiteAdminUtils.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        public string FilePath { get; set; }
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
        public RelayCommand LoadItemsToDataGridCommand { get; private set; }

        

        public ObservableCollection<LogItemVM> DownloadedLogItems { get; private set; } = new ObservableCollection<LogItemVM>();

        public ObservableCollection<ApacheLogItem> ProcessedLogEntries { get; private set; } = new ObservableCollection<ApacheLogItem>();


        private string _aroundTime;
        public string AroundTime
        {
            get { return _aroundTime; }
            set { Set(ref _aroundTime, value); }
        }

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

        private bool _isDownloading;
        public bool IsDownloading
        {
            get { return _isDownloading; }
            private set
            {
                _isDownloading = value;
                DownloadLogsCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get { return _isProcessing; }
            private set
            {
                _isProcessing = value;
                ProcessSelectedLogsCommand.RaiseCanExecuteChanged();
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

            DownloadLogsCommand = new RelayCommand(DownloadLogsFromServer, () => !IsDownloading);
            ProcessSelectedLogsCommand = new RelayCommand(ProcessSelectedLogs, () => !IsProcessing);
            LoadItemsToDataGridCommand = new RelayCommand(LoadItemsToDataGrid, () => true);

            AutoMapper.Mapper.Initialize(c => c.CreateMap<Core.ApacheLogEntry, ApacheLogItem>());

            AroundTime = "07/03/2017 13:05:01 -05:00";

            FillDownloadedLogItems();
        }

        private void LoadItemsToDataGrid()
        {
            IEnumerable<ApacheLogEntry> selectedEntries = _logAnalyser.ParsedLogEntries;

            if (!String.IsNullOrEmpty(AroundTime))
            {
                string format = "dd/MM/yyyy HH:mm:ss zzz";

                DateTimeOffset aroundTime = DateTimeOffset.ParseExact(AroundTime, format, CultureInfo.InvariantCulture);

                selectedEntries = _logAnalyser.ParsedLogEntries
                    .Where(le => (le.DateOffset - aroundTime).Duration().TotalMinutes < 10);
            }
            else
            {
                selectedEntries = _logAnalyser.ParsedLogEntries.Where(le => le.Response == 500);
                //selectedEntries = _logAnalyser.ParsedLogEntries.GroupBy(le => le.Ip);
                selectedEntries = _logAnalyser.ParsedLogEntries.Where(le => le.Request.Contains(".pdf"));



            }

            foreach (var entry in selectedEntries)
            {
                var vm = AutoMapper.Mapper.Map<ApacheLogItem>(entry);

                //vm.DateOffset = vm.DateOffset.ToLocalTime();

                ProcessedLogEntries.Add(vm);
            }
        }

        private async void ProcessSelectedLogs()
        {
            IsProcessing = true;

            _logAnalyser.Reset();

            var selectedFiles = DownloadedLogItems.Where(vm => vm.IsSelected).Select(vm => vm.FullPath);


            await Task.WhenAll(selectedFiles.Select(file => _logAnalyser.ProcessFileAsync(file)));

            

            ProcessedLinesCount = _logAnalyser.ItemsCount;
            LogTimeStart = _logAnalyser.ParsedLogEntries.Min(le => le.DateOffset).ToLocalTime().DateTime;
            LogTimeEnd = _logAnalyser.ParsedLogEntries.Max(le => le.DateOffset).ToLocalTime().DateTime;

            IsProcessing = false;
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

            if (Directory.Exists(LogsFolderPath))
            {
                Directory.EnumerateFiles(LogsFolderPath)
                .Select(p => new LogItemVM() { FullPath = p, Name = Path.GetFileName(p), IsSelected = true })
                .ToList()
                .ForEach(vm => DownloadedLogItems.Add(vm));
            }
        }

        public async void DownloadLogsFromServer()
        {
            IsDownloading = true;

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
                await client.DownloadFileTaskAsync(uri, archivePath);
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

            IsDownloading = false;
        }

    }
}