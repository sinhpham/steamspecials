using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using HtmlAgilityPack;

namespace SteamSpecialsWp.ViewModel
{
    public class SteamSpecialItemViewModel
    {
        public SteamSpecialItemViewModel()
        {

        }

        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string Link { get; set; }
        public string OldPrice { get; set; }
        public string NewPrice { get; set; }
        public string MetaScore { get; set; }
        public string TypeImg { get; set; }
        public string PlatformImg { get; set; }
        public string Cat_Release { get; set; }

        public string SalePercentage
        {
            get
            {
                double op;
                double np;
                if (OldPrice != null && NewPrice != null &&
                    double.TryParse(OldPrice.Substring(1), out op) &&
                    double.TryParse(NewPrice.Substring(1), out np))
                {
                    var per = (np - op) / op * 100;
                    per = Math.Round(per);
                    return per.ToString() + "%";
                }
                return "";
            }
        }

        public SolidColorBrush SaleColor
        {
            get
            {
                double op;
                double np;
                if (OldPrice != null && NewPrice != null &&
                    double.TryParse(OldPrice.Substring(1), out op) &&
                    double.TryParse(NewPrice.Substring(1), out np))
                {
                    var per = (np - op) / op * 100;
                    per = Math.Round(per);
                    if (per <= -75)
                    {
                        return new SolidColorBrush(Colors.Red);
                    }
                    else if (per <= -50)
                    {
                        return new SolidColorBrush(Colors.Green);
                    }
                }

                return ((SolidColorBrush)((App)App.Current).Resources["PhoneForegroundBrush"]);
            }
        }
    }

    public class MainViewModel : ViewModelBase, IVMState<MainViewModel.StateData>
    {
        public MainViewModel()
        {
            DataLoaded = false;
            CurrPageNum = 1;
            MaxPageNum = 1;
            IsRefreshing = false;

            SortBy = new List<string>() {
                "Name",
                "Price",
                "Metascore"
            };
            _selectedSortBy = "Name";

            SortOrder = new List<string>() {
                "Asc",
                "Desc"
            };
            _selectedSortOrder = "Asc";
        }

        readonly ObservableCollection<SteamSpecialItemViewModel> _ssList = new ObservableCollection<SteamSpecialItemViewModel>();
        public ObservableCollection<SteamSpecialItemViewModel> SSList
        {
            get
            {
                return _ssList;
            }
        }

        public List<string> SortBy { get; set; }
        private string _selectedSortBy;
        public string SelectedSortBy
        {
            get
            {
                return _selectedSortBy;
            }
            set
            {
                if (_selectedSortBy == value)
                {
                    return;
                }

                _selectedSortBy = value;
                RaisePropertyChanged("SelectedSortBy");
                Refresh();
            }
        }

        public List<string> SortOrder { get; set; }
        private string _selectedSortOrder;
        public string SelectedSortOrder
        {
            get
            {
                return _selectedSortOrder;
            }
            set
            {
                if (_selectedSortOrder == value)
                {
                    return;
                }

                _selectedSortOrder = value;
                RaisePropertyChanged("SelectedSortOrder");
                Refresh();
            }
        }

        bool _isRefreshing;
        bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                if (_isRefreshing)
                {
                    ProgressBarMess = "Loading data...";
                    ShowProgressBar = true;
                }
                else
                {
                    ProgressBarMess = "";
                    ShowProgressBar = false;
                }
            }
        }

        public async void Refresh()
        {
            if (IsRefreshing)
            {
                return;
            }

            if (CurrPageNum < 1)
            {
                CurrPageNum = 1;
            }
            else if (CurrPageNum > MaxPageNum)
            {
                CurrPageNum = MaxPageNum;
            }

            IsRefreshing = true;
            InfoText = "";
            SSList.Clear();
            RaisePropertyChanged("InfoText");

            var wc = new SharpGIS.GZipWebClient();
            var url = Helper.SSUrl(SelectedSortBy, SelectedSortOrder, CurrPageNum);

            string res = "";
            var downloadTask = wc.DownloadStringTaskAsync(url);

            await Task.WhenAny(downloadTask, Task.Delay(20000));

            if (downloadTask.Status == TaskStatus.RanToCompletion)
            {
                res = downloadTask.Result;
            }
            else
            {
                // Error downloading data.
                IsRefreshing = false;
                ProgressBarMess = "Error downloading data. Try refresh again.";
                return;
            }


            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);

            MaxPageNum = Math.Max(SSParser.ParseNumberOfPages(htmlDoc), CurrPageNum);
            var newInfoText = SSParser.ParseInfoText(htmlDoc);

            SSParser.ParseDealPage(htmlDoc, SSList);

            InfoText = newInfoText;
            IsRefreshing = false;
        }

        public bool DataLoaded
        {
            get;
            set;
        }
        public double ListVerticalOffset
        {
            get;
            set;
        }

        bool _showProgressBar;
        public bool ShowProgressBar
        {
            get
            {
                return _showProgressBar;
            }
            set
            {
                _showProgressBar = value;
                RaisePropertyChanged("ShowProgressBar");
            }
        }

        string _progressbarMess;
        public string ProgressBarMess
        {
            get
            {
                return _progressbarMess;
            }
            set
            {
                _progressbarMess = value;
                RaisePropertyChanged("ProgressBarMess");
            }
        }

        public int CurrPageNum { get; set; }

        private int _mpn;
        public int MaxPageNum
        {
            get
            {
                return _mpn;
            }
            set
            {
                _mpn = value;
            }
        }

        private string _infoText;
        public string InfoText
        {
            get
            {
                return _infoText;
            }
            set
            {
                _infoText = value;
                RaisePropertyChanged("InfoText");
            }
        }

        string _footerInfoText;
        public string FooterInfoText
        {
            get
            {
                return _footerInfoText;
            }
            set
            {
                _footerInfoText = value;
                RaisePropertyChanged("FooterInfoText");
            }
        }

        // Tombstone support.
        public Boolean NeedSaveState
        {
            get;
            set;
        }

        public class StateData
        {

            public double ListVerticalOffset;
            public int CurrPageNum;
            public int MaxPageNum;
            public List<SteamSpecialItemViewModel> SSList;
        }

        public StateData CurrentState
        {
            get
            {
                if (!NeedSaveState)
                {
                    return null;
                }
                var state = new StateData();

                state.ListVerticalOffset = ListVerticalOffset;
                state.CurrPageNum = CurrPageNum;
                state.MaxPageNum = MaxPageNum;
                state.SSList = new List<SteamSpecialItemViewModel>();
                foreach (var ssvm in SSList)
                {
                    state.SSList.Add(ssvm);
                }
                return state;
            }
        }

        public void LoadFromState(StateData state)
        {
            NeedSaveState = true;

            CurrPageNum = state.CurrPageNum;
            MaxPageNum = state.MaxPageNum;

            foreach (var ssvm in state.SSList)
            {
                SSList.Add(ssvm);
            }
            ListVerticalOffset = state.ListVerticalOffset;
            DataLoaded = true;
        }
    }
}