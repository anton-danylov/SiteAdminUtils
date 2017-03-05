using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Net;
using System.Collections.ObjectModel;

namespace SiteAdminUtils.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<ViewModelBase> _allViewModels = new ObservableCollection<ViewModelBase>();
        public ObservableCollection<ViewModelBase> AllViewModels
        {
            get { return _allViewModels; }
            private set { _allViewModels = value; RaisePropertyChanged(); }
        }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set { _currentViewModel = value; RaisePropertyChanged(); }
        }


        public MainViewModel()
        {

        }

        public MainViewModel(ViewModelLocator viewModelLocator)
        {
            this._viewModelLocator = viewModelLocator;

            AllViewModels.Add(_viewModelLocator.AnalyzeAccessLogViewModel);
            AllViewModels.Add(_viewModelLocator.TestUserAgentStringsViewModel);

            CurrentViewModel = AllViewModels[0];
        }

        private ViewModelLocator _viewModelLocator;
    }
}