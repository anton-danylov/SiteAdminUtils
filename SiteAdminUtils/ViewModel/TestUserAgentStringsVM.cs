using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Net;

namespace SiteAdminUtils.ViewModel
{
    public class TestUserAgentStringsVM : ViewModelBase
    {
        public string Name { get; set; } = "Test User Agent Strings";


        public RelayCommand StartCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }

        CancellationTokenSource _cancelTokenSource = null;

        private bool _isExecuting = false;
        public bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                _isExecuting = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            }
        }

        private string _allUserAgents;
        public string AllUserAgents
        {
            get { return _allUserAgents; }
            set
            {
                _allUserAgents = value;
                RaisePropertyChanged();
            }
        }

        private string _forbiddenUserAgents;
        public string ForbiddenUserAgents
        {
            get { return _forbiddenUserAgents; }
            set
            {
                _forbiddenUserAgents = value;
                RaisePropertyChanged();
            }
        }

        private string _currentUserAgent;
        public string CurrentUserAgent
        {
            get { return _currentUserAgent; }
            set
            {
                _currentUserAgent = value;
                RaisePropertyChanged();
            }
        }

        private string _siteUrl;
        public string SiteUrl
        {
            get { return _siteUrl; }
            set
            {
                _siteUrl = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        private int _currentProgress;
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                RaisePropertyChanged();
            }
        }


        public TestUserAgentStringsVM()
        {
            StartCommand = new RelayCommand(OnStart, () => !IsExecuting && !String.IsNullOrEmpty(SiteUrl));
            StopCommand = new RelayCommand(OnStop, () => IsExecuting);
        }

        private void OnStop()
        {
            _cancelTokenSource?.Cancel();
        }


        private void CheckUserAgents(string siteUrl, string[] agents, CancellationToken token
            , IProgress<int> progress
            , IProgress<string> currentItem
            , IProgress<string> reportForbidden)
        {
            int agentsCount = agents.Length;

            for (int i = 0; i < agentsCount; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                string currentAgent = agents[i].Trim('\"', ' ');

                try
                {
                    currentItem.Report(currentAgent);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(siteUrl);
                    request.UserAgent = currentAgent;

                    try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                reportForbidden.Report($"{currentAgent} : {response.StatusCode}");
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        reportForbidden.Report($"{currentAgent} : {(ex.Response as HttpWebResponse)?.StatusCode}");
                    }
                    


                    progress.Report((i * 100) / agentsCount);
                }
                catch (Exception ex)
                {
                    reportForbidden.Report($"Unexpected Error on {currentAgent} : {ex.Message}");
                }
                
            }
        }


        private async void OnStart()
        {
            IsExecuting = true;
            ForbiddenUserAgents = null;
            _cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancelTokenSource.Token;

            var progress = new Progress<int>((p) => { CurrentProgress = p; });
            var currentItem = new Progress<string>((s) => { CurrentUserAgent = s; });
            var reportForbidden = new Progress<string>((s) => { ForbiddenUserAgents += s + Environment.NewLine; });

            var allAgents = AllUserAgents.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                await Task.Factory.StartNew(() => CheckUserAgents(_siteUrl, allAgents, token, progress, currentItem, reportForbidden), token);
            }
            finally
            {
                IsExecuting = false;
                CurrentProgress = 0;
                CurrentUserAgent = null;
            }
        }
    }
}