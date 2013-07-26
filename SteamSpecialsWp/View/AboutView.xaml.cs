using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Reflection;
using SteamSpecialsWp.ViewModel;

namespace SteamSpecialsWp
{
    public partial class AboutView : PhoneApplicationPage
    {
        public AboutView() {
            InitializeComponent();
            var parts = Assembly.GetExecutingAssembly().FullName.Split(',');
            _version = parts[1].Split('=')[1];
            versionTextBlock.Text = "Version " + _version;
        }

        AboutViewModel MyVM {
            get {
                return (AboutViewModel)this.DataContext;
            }
        }

        string _version;

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            base.OnBackKeyPress(e);
            MyVM.NeedSaveState = false;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            MyVM.NeedSaveState = true;
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            contentTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void EmailMeClicked(object sender, System.Windows.RoutedEventArgs e) {
            var ect = new EmailComposeTask();
            ect.Subject = string.Format("[Steam Specials {0}]", _version);
            ect.Body = contentTextBox.Text;
            ect.To = "phamansinh@gmail.com";
            try {
                ect.Show();
            } catch (Exception) {

            }
            
        }

        private void RateButton_Clicked(object sender, System.Windows.RoutedEventArgs e) {
            var mrt = new MarketplaceReviewTask();
            try {
                mrt.Show();
            } catch (Exception) {

            }
        }
    }
}
