/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SteamSpecialsWp.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  
  OR (WPF only):
  
  xmlns:vm="clr-namespace:SteamSpecialsWp.ViewModel"
  DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
*/

namespace SteamSpecialsWp.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// Use the <strong>mvvmlocatorproperty</strong> snippet to add ViewModels
    /// to this locator.
    /// </para>
    /// <para>
    /// In Silverlight and WPF, place the ViewModelLocatorTemplate in the App.xaml resources:
    /// </para>
    /// <code>
    /// &lt;Application.Resources&gt;
    ///     &lt;vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SteamSpecialsWp.ViewModel"
    ///                                  x:Key="Locator" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    /// <para>
    /// Then use:
    /// </para>
    /// <code>
    /// DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
    /// </code>
    /// <para>
    /// You can also use Blend to do all this with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// <para>
    /// In <strong>*WPF only*</strong> (and if databinding in Blend is not relevant), you can delete
    /// the Main property and bind to the ViewModelNameStatic property instead:
    /// </para>
    /// <code>
    /// xmlns:vm="clr-namespace:SteamSpecialsWp.ViewModel"
    /// DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
    /// </code>
    /// </summary>
    public class ViewModelLocator
    {
        private static MainViewModel _main;
        private static AboutViewModel _aboutViewModel;

        /// <summary>
        /// Gets the AboutViewModel property.
        /// </summary>
        public static AboutViewModel AboutViewModelStatic {
            get {
                if (_aboutViewModel == null) {
                    CreateAboutViewModel();
                }

                return _aboutViewModel;
            }
        }

        /// <summary>
        /// Gets the AboutViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AboutViewModel AboutViewModel {
            get {
                return AboutViewModelStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the AboutViewModel property.
        /// </summary>
        public static void ClearAboutViewModel() {
            _aboutViewModel.Cleanup();
            _aboutViewModel = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the AboutViewModel property.
        /// </summary>
        public static void CreateAboutViewModel() {
            if (_aboutViewModel == null) {
                _aboutViewModel = new AboutViewModel();
            }
        }


        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator() {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view models
            ////}
            ////else
            ////{
            ////    // Create run time view models
            ////}

            CreateMain();
            CreateAboutViewModel();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        public static MainViewModel MainStatic {
            get {
                if (_main == null) {
                    CreateMain();
                }

                return _main;
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main {
            get {
                return MainStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Main property.
        /// </summary>
        public static void ClearMain() {
            _main.Cleanup();
            _main = null;
        }

        /// <summary>
        /// Provides a deterministic way to create the Main property.
        /// </summary>
        public static void CreateMain() {
            if (_main == null) {
                _main = new MainViewModel();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup() {
            ClearMain();
            ClearAboutViewModel();
        }

        // Tombstone
        public void SaveStates() {
            SaveObjState(_main);
            SaveObjState(_aboutViewModel);
        }
        public void LoadStates() {
            LoadObjState(_main);
            LoadObjState(_aboutViewModel);
        }

        static void SaveObjState<StateType>(IVMState<StateType> obj) {
            ((App)App.Current).AppState.Remove(obj.GetType().Name);
            var s = obj.CurrentState;
            if (s != null) {
                ((App)App.Current).AppState.Add(obj.GetType().Name, s);
            }
        }

        static void LoadObjState<StateType>(IVMState<StateType> obj) {
            StateType s;
            object outObj;
            if (((App)App.Current).AppState.TryGetValue(obj.GetType().Name, out outObj)) {
                s = (StateType)outObj;
                obj.LoadFromState(s);
            }
        }

        static void ClearObjState<StateType>(IVMState<StateType> obj) {

        }
    }
}