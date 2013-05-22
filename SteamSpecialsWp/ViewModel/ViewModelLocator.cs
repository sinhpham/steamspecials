/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:$rootnamespace$"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace SteamSpecialsWp.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public AboutViewModel About
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AboutViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            ServiceLocator.Current.GetInstance<AboutViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
        }

        // Tombstone
        public void SaveStates()
        {
            SaveObjState(Main);
            SaveObjState(About);
        }
        public void LoadStates()
        {
            LoadObjState(Main);
            LoadObjState(About);
        }

        static void SaveObjState<StateType>(IVMState<StateType> obj)
        {
            ((App)App.Current).AppState.Remove(obj.GetType().Name);
            var s = obj.CurrentState;
            if (s != null)
            {
                ((App)App.Current).AppState.Add(obj.GetType().Name, s);
            }
        }

        static void LoadObjState<StateType>(IVMState<StateType> obj)
        {
            StateType s;
            object outObj;
            if (((App)App.Current).AppState.TryGetValue(obj.GetType().Name, out outObj))
            {
                s = (StateType)outObj;
                obj.LoadFromState(s);
            }
        }
    }
}