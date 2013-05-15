using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using SteamSpecialsWp.ViewModel;

namespace SteamSpecialsWp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage() {
            InitializeComponent();
            LittleWatson.CheckForPreviousException();
            Loaded += (o, args) => {
                var progressIndicator = SystemTray.ProgressIndicator;

                if (progressIndicator != null) {
                    return;
                }

                progressIndicator = new ProgressIndicator();
                progressIndicator.IsVisible = true;
                SystemTray.SetProgressIndicator(this, progressIndicator);

                var binding = new Binding("ShowProgressBar") { Source = MyVM };
                BindingOperations.SetBinding(
                    progressIndicator, ProgressIndicator.IsIndeterminateProperty, binding);

                binding = new Binding("ProgressBarMess") { Source = MyVM };
                BindingOperations.SetBinding(
                    progressIndicator, ProgressIndicator.TextProperty, binding);
            };
            // Set up transitions.
            leftOutSb.Completed += new EventHandler(leftOutSb_Completed);
            rightOut.Completed += new EventHandler(rightOut_Completed);
        }

        MainViewModel MyVM {
            get {
                return (MainViewModel)this.DataContext;
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            base.OnBackKeyPress(e);
            MyVM.NeedSaveState = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            ssList.SelectedIndex = -1;
            MyVM.NeedSaveState = true;
            RoutedEventHandler loadedHandler = null;
            loadedHandler = (sender, arg) => {
                ssList.Loaded -= loadedHandler;
                if (!MyVM.DataLoaded) {
                    MyVM.Refresh();
                    MyVM.DataLoaded = true;
                } else {
                    var sv = Helper.FindVisualChild<ScrollViewer>(ssList);
                    sv.ScrollToVerticalOffset(MyVM.ListVerticalOffset);
                }
            };
            ssList.Loaded += loadedHandler;
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            var sv = Helper.FindVisualChild<ScrollViewer>(ssList);
            MyVM.ListVerticalOffset = sv.VerticalOffset;
        }

        private void Appbar_Refresh(object sender, System.EventArgs e) {
            MyVM.Refresh();
        }

        private void Appbar_About(object sender, System.EventArgs e) {
            NavigationService.Navigate(new System.Uri("/View/AboutView.xaml", System.UriKind.Relative));
        }

        private void List_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 1) {
                var selectedItem = (SteamSpecialItemViewModel)e.AddedItems[0];
                var wbt = new WebBrowserTask();
                wbt.Uri = new Uri(selectedItem.Link);
                try {
                    wbt.Show();
                } catch (Exception) {

                }

            }
        }

        private void Appbar_Previous(object sender, System.EventArgs e) {
            rightOut.Begin();
        }

        void rightOut_Completed(object sender, EventArgs e) {
            var prevPageNum = MyVM.PageNum;
            MyVM.PageNum--;
            if (prevPageNum != MyVM.PageNum) {
                var sv = Helper.FindVisualChild<ScrollViewer>(ssList);
                if (sv != null) {
                    sv.ScrollToVerticalOffset(0);
                }
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                rightIn.Begin();
            });

        }

        private void Appbar_Next(object sender, System.EventArgs e) {
            leftOutSb.Begin();
        }

        void leftOutSb_Completed(object sender, EventArgs e) {
            var prevPageNum = MyVM.PageNum;
            MyVM.PageNum++;
            if (prevPageNum != MyVM.PageNum) {
                var sv = Helper.FindVisualChild<ScrollViewer>(ssList);
                if (sv != null) {
                    sv.ScrollToVerticalOffset(0);
                }
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                leftInSb.Begin();
            });
        }
    }
}
