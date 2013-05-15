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

    public class SteamSpecialItem
    {
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string Link { get; set; }
        public string OldPrice { get; set; }
        public string NewPrice { get; set; }
        public string MetaScore { get; set; }
        public string TypeImg { get; set; }
        public string PlatformImg { get; set; }
        public string Cat_Release { get; set; }
    }

    public class SteamSpecialItemViewModel
    {
        public SteamSpecialItemViewModel(SteamSpecialItem item)
        {
            Name = item.Name;
            ImgUrl = item.ImgUrl;
            Link = item.Link;
            OldPrice = item.OldPrice ?? "$-1";
            NewPrice = item.NewPrice ?? "$-1";
            MetaScore = item.MetaScore;
            TypeImg = item.TypeImg;
            PlatformImg = item.PlatformImg;
            Cat_Release = item.Cat_Release;

            SalePercentage = "";
            SaleColor = ((SolidColorBrush)((App)App.Current).Resources["PhoneForegroundBrush"]);

            double op;
            double np;
            if (double.TryParse(OldPrice.Substring(1), out op) &&
                double.TryParse(NewPrice.Substring(1), out np))
            {
                var per = (np - op) / op * 100;
                per = Math.Round(per);
                SalePercentage = per.ToString() + "%";
                if (per <= -75)
                {
                    SaleColor = new SolidColorBrush(Colors.Red);
                }
                else if (per <= -50)
                {
                    SaleColor = new SolidColorBrush(Colors.Green);
                }
            }
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

        public string SalePercentage { get; set; }
        public SolidColorBrush SaleColor { get; set; }
    }

    public class MainViewModel : ViewModelBase, IVMState<MainViewModel.StateData>
    {
        readonly ObservableCollection<SteamSpecialItemViewModel> _ss = new ObservableCollection<SteamSpecialItemViewModel>();
        public ObservableCollection<SteamSpecialItemViewModel> SS
        {
            get
            {
                return _ss;
            }
        }
        List<SteamSpecialItem> _itemList = new List<SteamSpecialItem>();

        public MainViewModel()
        {
            DataLoaded = false;
            CurrPageNum = 1;
            MaxPageNum = 1;
            IsRefreshing = false;
        }

        Random _randomId = new Random();
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
            SS.Clear();
            _itemList.Clear();
            RaisePropertyChanged("InfoText");

            var wc = new SharpGIS.GZipWebClient();
            var url = "http://store.steampowered.com/search/?sort_by=Name&sort_order=ASC&specials=1&page=" + CurrPageNum;

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

            _itemList.Clear();

            SSParser.ParseDealPage(htmlDoc, _itemList);
            foreach (var item in _itemList)
            {
                SS.Add(new SteamSpecialItemViewModel(item));
            }
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
            public List<SteamSpecialItem> ItemList;
            public double ListVerticalOffset;
            public int CurrPageNum;
            public int MaxPageNum;
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
                state.ItemList = _itemList;
                state.ListVerticalOffset = ListVerticalOffset;
                state.CurrPageNum = CurrPageNum;
                state.MaxPageNum = MaxPageNum;
                return state;
            }
        }

        public void LoadFromState(StateData state)
        {
            NeedSaveState = true;
            _itemList = state.ItemList;
            CurrPageNum = state.CurrPageNum;
            MaxPageNum = state.MaxPageNum;
            ListVerticalOffset = state.ListVerticalOffset;
            DataLoaded = true;
        }
    }
}