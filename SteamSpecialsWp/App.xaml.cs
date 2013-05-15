using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SteamSpecialsWp.ViewModel;
using System.Windows.Media;

namespace SteamSpecialsWp
{
    public partial class App : Application
    {

        // Easy access to the root frame
        public TransitionFrame RootFrame { get; private set; }

        public IDictionary<string, object> AppState {
            get {
                return PhoneApplicationService.Current.State;
            }
        }
        // Constructor
        public App() {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        public const string ApiKeyValue = "TR5NIY1VT5F3Z8YV8RSY";

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e) {
            FlurryWP7SDK.Api.StartSession(ApiKeyValue);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e) {
            if (!e.IsApplicationInstancePreserved) {
                // Need to restore from tombstone.
                FlurryWP7SDK.Api.StartSession(ApiKeyValue);
                var x = App.Current.Resources["Locator"] as ViewModelLocator;
                x.LoadStates();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e) {
            var x = App.Current.Resources["Locator"] as ViewModelLocator;
            x.SaveStates();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e) {
            ViewModelLocator.Cleanup();
        }

        // Code to execute if a navigation fails
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {
            LittleWatson.ReportException(e.Exception, "");
            if (System.Diagnostics.Debugger.IsAttached) {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            LittleWatson.ReportException(e.ExceptionObject, "");
            if (System.Diagnostics.Debugger.IsAttached) {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication() {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame() {
                Background = new SolidColorBrush(Colors.Transparent)
            };
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }

    public class LittleWatson
    {
        const string filename = "LittleWatson.txt";

        internal static void ReportException(Exception ex, string extra) {
            try {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename))) {
                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            } catch (Exception) {
            }
        }

        internal static void CheckForPreviousException() {
            try {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
                    if (store.FileExists(filename)) {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None))) {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null) {
                    if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it?", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "phamansinh@gmail.com";
                        email.Subject = "[SteamSpecial] Auto-generated problem report";
                        email.Body = contents;
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                        email.Show();
                    }
                }
            } catch (Exception) {
            } finally {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store) {
            try {
                store.DeleteFile(filename);
            } catch (Exception) {
            }
        }
    }
}
