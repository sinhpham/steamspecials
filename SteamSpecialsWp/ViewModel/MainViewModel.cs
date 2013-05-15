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
        public SteamSpecialItemViewModel(SteamSpecialItem item) {
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
                double.TryParse(NewPrice.Substring(1), out np)) {
                var per = (np - op) / op * 100;
                per = Math.Round(per);
                SalePercentage = per.ToString() + "%";
                if (per <= -75) {
                    SaleColor = new SolidColorBrush(Colors.Red);
                } else if (per <= -50) {
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
        public ObservableCollection<SteamSpecialItemViewModel> SS {
            get {
                return _ss;
            }
        }
        List<SteamSpecialItem> _itemList = new List<SteamSpecialItem>();

        public MainViewModel() {
            DataLoaded = false;
            PageNum = 1;
            IsRefreshing = false;
        }

        Random _randomId = new Random();
        bool _isRefreshing;
        bool IsRefreshing {
            get {
                return _isRefreshing;
            }
            set {
                _isRefreshing = value;
                if (_isRefreshing) {
                    ProgressBarMess = "Loading data...";
                    ShowProgressBar = true;
                } else {
                    ProgressBarMess = "";
                    ShowProgressBar = false;
                }
            }
        }
        public async void Refresh() {
            if (IsRefreshing) {
                return;
            }
            IsRefreshing = true;
            SS.Clear();
            _itemList.Clear();
            RaisePropertyChanged("InfoText");

            var wc = new SharpGIS.GZipWebClient();
            var url = "http://steamspecialsweb.apphb.com/SteamSpecials?id=" + _randomId.Next();

            string res = "";
            var downloadTask = wc.DownloadStringTaskAsync(url);

            await Task.WhenAny(downloadTask, Task.Delay(20000));

            if (downloadTask.Status == TaskStatus.RanToCompletion) {
                res = downloadTask.Result;
            } else {
                // Error downloading data.
                IsRefreshing = false;
                ProgressBarMess = "Error downloading data. Try refresh again.";
                return;
            }

            var bw = new BackgroundWorker();
            bw.DoWork += (bwSender, bwArg) => {
                var byteArray = Encoding.Unicode.GetBytes(res);
                using (var stream = new MemoryStream(byteArray)) {
                    var xs = new XmlSerializer(typeof(List<SteamSpecialItem>));
                    try {
                        _itemList = (List<SteamSpecialItem>)xs.Deserialize(stream);
                    } catch (Exception) {
                    }
                }
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    PageNum = 1;
                    IsRefreshing = false;
                });
            };
            bw.RunWorkerAsync();
        }

        public bool DataLoaded {
            get;
            set;
        }
        public double ListVerticalOffset {
            get;
            set;
        }

        bool _showProgressBar;
        public bool ShowProgressBar {
            get {
                return _showProgressBar;
            }
            set {
                _showProgressBar = value;
                RaisePropertyChanged("ShowProgressBar");
            }
        }

        string _progressbarMess;
        public string ProgressBarMess {
            get {
                return _progressbarMess;
            }
            set {
                _progressbarMess = value;
                RaisePropertyChanged("ProgressBarMess");
            }
        }

        int _pageNum;
        public int PageNum {
            get {
                return _pageNum;
            }
            set {
                if (value < 1 || (value - 1) * ItemPerPage >= _itemList.Count) {
                    return;
                }
                _pageNum = value;

                var minIdx = (PageNum - 1) * ItemPerPage;
                var maxIdx = PageNum * ItemPerPage;

                var newIdx = minIdx;
                var oldIdx = 0;
                while (newIdx < maxIdx && newIdx < _itemList.Count) {
                    var ssivm = new SteamSpecialItemViewModel(_itemList[newIdx]);
                    if (oldIdx < SS.Count) {
                        SS[oldIdx] = ssivm;
                    } else {
                        SS.Add(ssivm);
                    }
                    ++newIdx;
                    ++oldIdx;
                }
                while (oldIdx != SS.Count) {
                    SS.RemoveAt(oldIdx);
                }

                RaisePropertyChanged("InfoText");
            }
        }
        public string InfoText {
            get {
                var minIdx = (PageNum - 1) * ItemPerPage + 1;
                var maxIdx = minIdx + SS.Count - 1;
                if (maxIdx < minIdx) {
                    FooterInfoText = "";
                    return "Showing 0 - 0 of 0";
                }

                var more = _itemList.Count - maxIdx;
                if (more > 0) {
                    FooterInfoText = more.ToString() + " more";
                } else {
                    FooterInfoText = "";
                }

                return "Showing " + minIdx.ToString() + " - " + maxIdx.ToString() + " of " + _itemList.Count.ToString();
            }
        }
        string _footerInfoText;
        public string FooterInfoText {
            get {
                return _footerInfoText;
            }
            set {
                _footerInfoText = value;
                RaisePropertyChanged("FooterInfoText");
            }
        }
        static int ItemPerPage = 25;
        // Tombstone support.
        public Boolean NeedSaveState {
            get;
            set;
        }

        public class StateData
        {
            public List<SteamSpecialItem> ItemList;
            public double ListVerticalOffset;
            public int PageNum;
        }

        public StateData CurrentState {
            get {
                if (!NeedSaveState) {
                    return null;
                }
                var state = new StateData();
                state.ItemList = _itemList;
                state.ListVerticalOffset = ListVerticalOffset;
                state.PageNum = PageNum;
                return state;
            }
        }
        public void LoadFromState(StateData state) {
            NeedSaveState = true;
            _itemList = state.ItemList;
            PageNum = state.PageNum;
            ListVerticalOffset = state.ListVerticalOffset;
            DataLoaded = true;
        }
    }
}